using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Api.Servicios;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Servicios.Concurrencia;

namespace ImperiosEnGuerra.Tests;

public class ColaEntrenamientoTests
{
    [Test]
    public async Task DosOrdenes_MismoCentro_SeProcesanEnCola()
    {
        Partida partida =
            CrearPartida();

        var estado =
            new EstadoPartidaService();

        estado.EstablecerPartida(
            partida);

        using var gestor =
            new GestorProcesosConcurrentes();

        var servicio =
            new ServicioAccionesConcurrentes(
                estado,
                gestor,
                TimeSpan.FromMilliseconds(20));

        ProcesoConcurrente primero =
            servicio.IniciarEntrenamiento(
                Request("Guerrero"));

        ProcesoConcurrente segundo =
            servicio.IniciarEntrenamiento(
                Request("Arquero"));

        await Task.WhenAll(
            primero.Finalizacion,
            segundo.Finalizacion);

        Assert.That(
            partida.JugadorHumano.Unidades.Count,
            Is.EqualTo(2));

        CentroUrbano centro =
            partida.JugadorHumano.Edificios
                .OfType<CentroUrbano>()
                .Single();

        Assert.That(
            centro.ColaEntrenamiento,
            Is.Empty);
    }

    [Test]
    public async Task CancelarOrdenEnCola_ReembolsaYNoBloqueaLaSiguiente()
    {
        Partida partida =
            CrearPartida();

        var estado =
            new EstadoPartidaService();

        estado.EstablecerPartida(
            partida);

        using var gestor =
            new GestorProcesosConcurrentes();

        var servicio =
            new ServicioAccionesConcurrentes(
                estado,
                gestor,
                TimeSpan.FromMilliseconds(100));

        int oroInicial =
            partida.JugadorHumano.Recursos
                .ObtenerCantidad(TipoRecurso.Oro);

        ProcesoConcurrente primero =
            servicio.IniciarEntrenamiento(
                Request("Monje"));

        ProcesoConcurrente segundo =
            servicio.IniciarEntrenamiento(
                Request("Guerrero"));

        servicio.Cancelar(
            primero.Id);

        await Task.WhenAll(
            primero.Finalizacion,
            segundo.Finalizacion);

        Assert.That(
            partida.JugadorHumano.Unidades.Count,
            Is.EqualTo(1));

        Assert.That(
            partida.JugadorHumano.Recursos
                .ObtenerCantidad(TipoRecurso.Oro),
            Is.LessThan(oroInicial));
    }

    [Test]
    public void EntrenamientoNoCompleto_NoCreaUnidadAntesDeTiempo()
    {
        Partida partida =
            CrearPartida();

        var estado =
            new EstadoPartidaService();

        estado.EstablecerPartida(
            partida);

        ResultadoAccion encolado =
            estado.EncolarEntrenamiento(
                Request("Guerrero"),
                out Guid entrenamientoId,
                out Coordenada centro);

        Assert.That(encolado.Exito, Is.True);

        for (int i = 0; i < 9; i++)
        {
            ResultadoProgresoEntrenamiento progreso =
                estado.AvanzarEntrenamiento(
                    centro,
                    entrenamientoId,
                    10);

            Assert.That(progreso.Exito, Is.True);
            Assert.That(progreso.Terminado, Is.False);
        }

        Assert.That(
            partida.JugadorHumano.Unidades,
            Is.Empty);

        ResultadoSpawnEntrenamiento spawn =
            estado.CompletarEntrenamientoConSpawn(
                centro,
                entrenamientoId,
                "Guerrero");

        Assert.That(spawn.Exito, Is.False);
        Assert.That(
            spawn.Mensaje,
            Does.Contain("todavía"));
        Assert.That(
            partida.JugadorHumano.Unidades,
            Is.Empty);
    }

    [Test]
    public void SinEspacioParaSpawn_DevuelveFalloControlado()
    {
        Partida partida =
            CrearPartida();

        var estado =
            new EstadoPartidaService();

        estado.EstablecerPartida(
            partida);

        ResultadoAccion encolado =
            estado.EncolarEntrenamiento(
                Request("Arquero"),
                out Guid entrenamientoId,
                out Coordenada centro);

        Assert.That(encolado.Exito, Is.True);

        for (int x = 0;
             x < partida.JugadorHumano.Mapa.Ancho;
             x++)
        {
            for (int y = 0;
                 y < partida.JugadorHumano.Mapa.Alto;
                 y++)
            {
                partida.JugadorHumano.Mapa
                    .ObtenerCasilla(x, y)
                    .Ocupar();
            }
        }

        for (int i = 0; i < 10; i++)
        {
            ResultadoProgresoEntrenamiento progreso =
                estado.AvanzarEntrenamiento(
                    centro,
                    entrenamientoId,
                    10);

            Assert.That(progreso.Exito, Is.True);
        }

        ResultadoSpawnEntrenamiento spawn =
            estado.CompletarEntrenamientoConSpawn(
                centro,
                entrenamientoId,
                "Arquero");

        Assert.That(spawn.Exito, Is.False);
        Assert.That(
            spawn.Mensaje,
            Does.Contain("casilla libre"));
        Assert.That(
            partida.JugadorHumano.Unidades,
            Is.Empty);

        Assert.That(
            estado.CancelarEntrenamientoCola(
                centro,
                entrenamientoId),
            Is.True);
    }

    private static Partida CrearPartida()
    {
        var mapa =
            new Mapa(8, 8);

        var humano =
            new Jugador(
                "Humano",
                TipoJugador.Humano,
                mapa,
                new RecursosJugador());

        var maquina =
            new Jugador(
                "Maquina",
                TipoJugador.Maquina,
                mapa,
                new RecursosJugador());

        humano.Recursos.Agregar(
            TipoRecurso.Oro,
            500);
        humano.Recursos.Agregar(
            TipoRecurso.Madera,
            500);
        humano.Recursos.Agregar(
            TipoRecurso.Comida,
            500);

        humano.AgregarEdificio(
            new CentroUrbano(
                new Coordenada(3, 3)));

        mapa.ObtenerCasilla(3, 3)
            .Ocupar();

        return new Partida(
            humano,
            maquina);
    }

    private static EntrenarRequest Request(
        string tipo)
    {
        return new EntrenarRequest
        {
            EdificioOrigen =
                new CoordenadaRequest
                {
                    X = 3,
                    Y = 3
                },
            TipoUnidad = tipo,
            Destino =
                new CoordenadaRequest
                {
                    X = 6,
                    Y = 6
                }
        };
    }
}
