using System;
using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Api.Servicios;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Tests;

public class RecoleccionTests
{
    private Partida partida;
    private Mapa mapa;
    private Aldeano aldeano;
    private OperacionRecoleccion operacion;

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

        aldeano = new Aldeano(new Coordenada(1, 1));
        partida.JugadorHumano.AgregarUnidad(aldeano);

        mapa.ColocarRecurso(
            new Recurso(
                TipoRecurso.Oro,
                new Coordenada(4, 5)));

        operacion = new OperacionRecoleccion();
    }

    [Test]
    public void AldeanoHumanoYRecursoValido_PreparaRecoleccion()
    {
        Coordenada posicionInicial = aldeano.Coordenada;

        int oroInicial = partida.JugadorHumano.Recursos
            .ObtenerCantidad(TipoRecurso.Oro);

        var solicitud = new SolicitudRecoleccion(
            aldeano.Id,
            new Coordenada(4, 5));

        ResultadoAccion resultado =
            operacion.Ejecutar(partida, solicitud);

        Assert.That(resultado.Exito, Is.True);
        Assert.That(
            solicitud.Tipo,
            Is.EqualTo(TipoAccionJuego.Recolectar));
        Assert.That(resultado.Mensaje, Does.Contain("Oro"));
        Assert.That(aldeano.Coordenada, Is.SameAs(posicionInicial));
        Assert.That(aldeano.Disponible, Is.True);

        Assert.That(
            partida.JugadorHumano.Recursos
                .ObtenerCantidad(TipoRecurso.Oro),
            Is.EqualTo(oroInicial));
    }

    [Test]
    public void UnidadQueNoEsAldeano_Falla()
    {
        var guerrero = new Guerrero(new Coordenada(2, 2));
        partida.JugadorHumano.AgregarUnidad(guerrero);

        ResultadoAccion resultado = operacion.Ejecutar(
            partida,
            new SolicitudRecoleccion(
                guerrero.Id,
                new Coordenada(4, 5)));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(resultado.Mensaje, Does.Contain("Aldeano"));
    }

    [Test]
    public void AldeanoMaquinaYRecursoValido_PreparaRecoleccion()
    {
        var mapaMaquina = new Mapa(6, 6);

        var maquina =
            new Jugador(
                "Máquina",
                TipoJugador.Maquina,
                mapaMaquina,
                new RecursosJugador());

        var aldeanoMaquina =
            new Aldeano(
                new Coordenada(1, 1));

        maquina.AgregarUnidad(
            aldeanoMaquina);

        mapaMaquina.ColocarRecurso(
            new Recurso(
                TipoRecurso.Oro,
                new Coordenada(4, 5)));

        partida =
            new Partida(
                partida.JugadorHumano,
                maquina);

        ResultadoAccion resultado =
            operacion.Ejecutar(
                partida,
                new SolicitudRecoleccion(
                    aldeanoMaquina.Id,
                    new Coordenada(4, 5)));

        Assert.That(
            resultado.Exito,
            Is.True,
            resultado.Mensaje);

        Assert.That(
            resultado.Mensaje,
            Does.Contain("Oro"));
    }

    [Test]
    public void AldeanoNoDisponible_Falla()
    {
        aldeano.MarcarNoDisponible();

        ResultadoAccion resultado = operacion.Ejecutar(
            partida,
            new SolicitudRecoleccion(
                aldeano.Id,
                new Coordenada(4, 5)));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(resultado.Mensaje, Does.Contain("disponible"));
    }

    [Test]
    public void IdInexistente_Falla()
    {
        ResultadoAccion resultado = operacion.Ejecutar(
            partida,
            new SolicitudRecoleccion(
                Guid.NewGuid(),
                new Coordenada(4, 5)));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(resultado.Mensaje, Is.Not.Empty);
    }

    [Test]
    public void PosicionSinRecurso_Falla()
    {
        ResultadoAccion resultado = operacion.Ejecutar(
            partida,
            new SolicitudRecoleccion(
                aldeano.Id,
                new Coordenada(3, 3)));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(resultado.Mensaje, Does.Contain("recurso"));
    }

    [TestCase(-1, 0)]
    [TestCase(0, -1)]
    [TestCase(6, 0)]
    [TestCase(0, 6)]
    public void ObjetivoFueraDelMapa_Falla(int x, int y)
    {
        ResultadoAccion resultado = operacion.Ejecutar(
            partida,
            new SolicitudRecoleccion(
                aldeano.Id,
                new Coordenada(x, y)));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(resultado.Mensaje, Does.Contain("fuera"));
    }

    [Test]
    public void ObjetivoNulo_Falla()
    {
        ResultadoAccion resultado = operacion.Ejecutar(
            partida,
            new SolicitudRecoleccion(
                aldeano.Id,
                null));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(resultado.Mensaje, Is.Not.Empty);
    }

    [Test]
    public void TipoRecursoInvalido_Falla()
    {
        mapa.ColocarRecurso(
            new Recurso(
                (TipoRecurso)999,
                new Coordenada(3, 3)));

        ResultadoAccion resultado = operacion.Ejecutar(
            partida,
            new SolicitudRecoleccion(
                aldeano.Id,
                new Coordenada(3, 3)));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(resultado.Mensaje, Does.Contain("válido"));
    }

    [Test]
    public void SinPartidaOSolicitud_Falla()
    {
        Assert.That(
            operacion.Ejecutar(
                null,
                new SolicitudRecoleccion(
                    aldeano.Id,
                    new Coordenada(4, 5))).Exito,
            Is.False);

        Assert.That(
            operacion.Ejecutar(partida, null).Exito,
            Is.False);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("no-es-guid")]
    public void Transporte_IdInvalido_Falla(string id)
    {
        EstadoPartidaService servicio = CrearServicio();

        RecolectarRequest request = CrearRequest();
        request.AldeanoId = id;

        ResultadoAccion resultado =
            servicio.IniciarRecoleccion(request);

        Assert.That(resultado.Exito, Is.False);
        Assert.That(resultado.Mensaje, Does.Contain("Guid"));
    }

    [Test]
    public void Transporte_SolicitudUObjetivoNulos_Falla()
    {
        EstadoPartidaService servicio = CrearServicio();

        Assert.That(
            servicio.IniciarRecoleccion(null).Exito,
            Is.False);

        RecolectarRequest request = CrearRequest();
        request.Objetivo = null;

        Assert.That(
            servicio.IniciarRecoleccion(request).Exito,
            Is.False);
    }

    [Test]
    public void ServicioSinPartida_Falla()
    {
        var servicio = new EstadoPartidaService();

        ResultadoAccion resultado =
            servicio.IniciarRecoleccion(CrearRequest());

        Assert.That(resultado.Exito, Is.False);
        Assert.That(
            resultado.Mensaje,
            Is.EqualTo("No hay una partida activa."));
    }

    [Test]
    public void Servicio_PreparaRecoleccionValida()
    {
        EstadoPartidaService servicio = CrearServicio();

        ResultadoAccion resultado =
            servicio.IniciarRecoleccion(CrearRequest());

        Assert.That(resultado.Exito, Is.True);
        Assert.That(resultado.Mensaje, Does.Contain("Oro"));
    }

    private EstadoPartidaService CrearServicio()
    {
        var servicio = new EstadoPartidaService();
        servicio.EstablecerPartida(partida);
        return servicio;
    }

    private RecolectarRequest CrearRequest()
    {
        return new RecolectarRequest
        {
            AldeanoId = aldeano.Id.ToString("D"),
            Objetivo = new CoordenadaRequest
            {
                X = 4,
                Y = 5
            }
        };
    }
}