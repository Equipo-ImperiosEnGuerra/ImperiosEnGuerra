using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Api.Servicios;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Tests;

public class EntrenamientoTests
{
    private Partida partida;
    private Mapa mapa;
    private CentroUrbano centroHumano;
    private CentroUrbano centroMaquina;
    private OperacionEntrenamiento operacion;

    [SetUp]
    public void Preparar()
    {
        mapa = new Mapa(6, 6);

        partida = new Partida(
            new Jugador(
                "Humano",
                TipoJugador.Humano,
                mapa,
                new RecursosJugador()),
            new Jugador(
                "Máquina",
                TipoJugador.Maquina,
                mapa,
                new RecursosJugador()));

        centroHumano =
            new CentroUrbano(new Coordenada(1, 1));

        centroMaquina =
            new CentroUrbano(new Coordenada(4, 4));

        partida.JugadorHumano
            .AgregarEdificio(centroHumano);

        partida.JugadorMaquina
            .AgregarEdificio(centroMaquina);

        mapa.ObtenerCasilla(1, 1).Ocupar();
        mapa.ObtenerCasilla(4, 4).Ocupar();

        partida.JugadorHumano.Recursos.Agregar(TipoRecurso.Oro, 100);
        partida.JugadorHumano.Recursos.Agregar(TipoRecurso.Madera, 100);
        partida.JugadorHumano.Recursos.Agregar(TipoRecurso.Comida, 100);

        partida.JugadorMaquina.Recursos.Agregar(TipoRecurso.Oro, 100);
        partida.JugadorMaquina.Recursos.Agregar(TipoRecurso.Madera, 100);
        partida.JugadorMaquina.Recursos.Agregar(TipoRecurso.Comida, 100);

        operacion = new OperacionEntrenamiento();
    }

    [Test]
    public void EntrenamientoValido_CreaUnidadYOcupaCasilla()
    {
        int unidadesIniciales =
            partida.JugadorHumano.Unidades.Count;

        var solicitud =
            new SolicitudEntrenamiento(
                new Coordenada(1, 1),
                "Guerrero",
                new Coordenada(2, 2));

        ResultadoAccion resultado =
            operacion.Ejecutar(
                partida,
                solicitud);

        Assert.That(resultado.Exito, Is.True);

        Assert.That(
            partida.JugadorHumano.Unidades.Count,
            Is.EqualTo(unidadesIniciales + 1));

        Assert.That(
            partida.JugadorHumano.Unidades[^1],
            Is.TypeOf<Guerrero>());

        Assert.That(
            mapa.ObtenerCasilla(2, 2).EstaOcupada,
            Is.True);
    }

    [Test]
    public void EdificioMaquina_EntrenamientoValido_CreaUnidadPropia()
    {
        ResultadoAccion resultado =
            operacion.Ejecutar(
                partida,
                new SolicitudEntrenamiento(
                    new Coordenada(4, 4),
                    "Guerrero",
                    new Coordenada(2, 2)));

        Assert.That(
            resultado.Exito,
            Is.True,
            resultado.Mensaje);

        Assert.That(
            partida.JugadorMaquina.Unidades.Count,
            Is.EqualTo(1));

        Assert.That(
            partida.JugadorMaquina.Unidades[0],
            Is.TypeOf<Guerrero>());
    }

    [Test]
    public void EdificioHumanoInexistente_Falla()
    {
        ResultadoAccion resultado =
            operacion.Ejecutar(
                partida,
                new SolicitudEntrenamiento(
                    new Coordenada(3, 3),
                    "Guerrero",
                    new Coordenada(2, 2)));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(
            resultado.Mensaje,
            Does.Contain("edificio"));
    }

    [Test]
    public void TipoUnidadNoPermitido_Falla()
    {
        ResultadoAccion resultado =
            operacion.Ejecutar(
                partida,
                new SolicitudEntrenamiento(
                    new Coordenada(1, 1),
                    "Dragon",
                    new Coordenada(2, 2)));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(
            resultado.Mensaje,
            Does.Contain("permitido"));
    }

    [Test]
    public void AparicionFueraDelMapa_Falla()
    {
        ResultadoAccion resultado =
            operacion.Ejecutar(
                partida,
                new SolicitudEntrenamiento(
                    new Coordenada(1, 1),
                    "Arquero",
                    new Coordenada(8, 8)));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(
            resultado.Mensaje,
            Does.Contain("fuera"));
    }

    [Test]
    public void AparicionOcupada_Falla()
    {
        mapa.ObtenerCasilla(2, 2).Ocupar();

        ResultadoAccion resultado =
            operacion.Ejecutar(
                partida,
                new SolicitudEntrenamiento(
                    new Coordenada(1, 1),
                    "Lancero",
                    new Coordenada(2, 2)));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(
            resultado.Mensaje,
            Does.Contain("disponible"));
    }

    [Test]
    public void Servicio_EntrenamientoValido()
    {
        var servicio =
            new EstadoPartidaService();

        servicio.EstablecerPartida(partida);

        var request = new EntrenarRequest
        {
            EdificioOrigen = new CoordenadaRequest
            {
                X = 1,
                Y = 1
            },

            TipoUnidad = "Arquero",

            Destino = new CoordenadaRequest
            {
                X = 2,
                Y = 2
            }
        };

        ResultadoAccion resultado =
            servicio.Entrenar(request);

        Assert.That(resultado.Exito, Is.True);

        Assert.That(
            partida.JugadorHumano.Unidades[^1],
            Is.TypeOf<Arquero>());
    }

    [Test]
    public void Servicio_EntrenamientoMaquina_UsaRecursosYJugadorPropios()
    {
        var servicio =
            new EstadoPartidaService();

        servicio.EstablecerPartida(
            partida);

        int comidaHumanoAntes =
            partida.JugadorHumano.Recursos
                .ObtenerCantidad(
                    TipoRecurso.Comida);

        int comidaMaquinaAntes =
            partida.JugadorMaquina.Recursos
                .ObtenerCantidad(
                    TipoRecurso.Comida);

        var request =
            new EntrenarRequest
            {
                EdificioOrigen =
                    new CoordenadaRequest
                    {
                        X = 4,
                        Y = 4
                    },

                TipoUnidad =
                    "Guerrero",

                Destino =
                    new CoordenadaRequest
                    {
                        X = 3,
                        Y = 3
                    }
            };

        ResultadoAccion resultado =
            servicio.Entrenar(
                request);

        Assert.That(
            resultado.Exito,
            Is.True,
            resultado.Mensaje);

        Assert.That(
            partida.JugadorMaquina.Unidades.Count,
            Is.EqualTo(1));

        Assert.That(
            partida.JugadorHumano.Unidades,
            Is.Empty);

        Assert.That(
            partida.JugadorHumano.Recursos
                .ObtenerCantidad(
                    TipoRecurso.Comida),
            Is.EqualTo(
                comidaHumanoAntes));

        Assert.That(
            partida.JugadorMaquina.Recursos
                .ObtenerCantidad(
                    TipoRecurso.Comida),
            Is.LessThan(
                comidaMaquinaAntes));
    }

    [Test]
    public void Servicio_SolicitudNula_Falla()
    {
        var servicio =
            new EstadoPartidaService();

        servicio.EstablecerPartida(partida);

        ResultadoAccion resultado =
            servicio.Entrenar(null);

        Assert.That(resultado.Exito, Is.False);
    }
}