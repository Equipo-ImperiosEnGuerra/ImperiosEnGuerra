using System;
using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Api.Servicios;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Tests;

public class ConstruccionTests
{
    private Partida partida;
    private Mapa mapa;
    private Aldeano aldeano;
    private OperacionConstruccion operacion;

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

        partida.JugadorHumano.Recursos.Agregar(TipoRecurso.Oro, 100);
        partida.JugadorHumano.Recursos.Agregar(TipoRecurso.Madera, 100);
        partida.JugadorHumano.Recursos.Agregar(TipoRecurso.Comida, 100);

        partida.JugadorHumano.AgregarUnidad(aldeano);

        operacion = new OperacionConstruccion();
    }

    [Test]
    public void ConstruccionValida_CreaEdificioYOcupaCasilla()
    {
        int edificiosIniciales =
            partida.JugadorHumano.Edificios.Count;

        var solicitud = new SolicitudConstruccion(
            aldeano.Id,
            "CentroUrbano",
            new Coordenada(3, 3));

        ResultadoAccion resultado =
            operacion.Ejecutar(partida, solicitud);

        Assert.That(resultado.Exito, Is.True);

        Assert.That(
            partida.JugadorHumano.Edificios.Count,
            Is.EqualTo(edificiosIniciales + 1));

        Assert.That(
            partida.JugadorHumano.Edificios[^1],
            Is.TypeOf<CentroUrbano>());

        Assert.That(
            mapa.ObtenerCasilla(3, 3).EstaOcupada,
            Is.True);
    }

    [Test]
    public void CoordenadaFueraDelMapa_Falla()
    {
        ResultadoAccion resultado =
            operacion.Ejecutar(
                partida,
                new SolicitudConstruccion(
                    aldeano.Id,
                    "CentroUrbano",
                    new Coordenada(8, 8)));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(resultado.Mensaje, Does.Contain("fuera"));
    }

    [Test]
    public void CasillaOcupada_Falla()
    {
        mapa.ObtenerCasilla(3, 3).Ocupar();

        ResultadoAccion resultado =
            operacion.Ejecutar(
                partida,
                new SolicitudConstruccion(
                    aldeano.Id,
                    "CentroUrbano",
                    new Coordenada(3, 3)));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(resultado.Mensaje, Does.Contain("disponible"));
    }

    [Test]
    public void AldeanoMaquina_Falla()
    {
        var aldeanoMaquina =
            new Aldeano(new Coordenada(2, 2));

        partida.JugadorMaquina
            .AgregarUnidad(aldeanoMaquina);

        ResultadoAccion resultado =
            operacion.Ejecutar(
                partida,
                new SolicitudConstruccion(
                    aldeanoMaquina.Id,
                    "CentroUrbano",
                    new Coordenada(3, 3)));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(resultado.Mensaje, Does.Contain("máquina"));
    }

    [Test]
    public void TipoEdificioNoPermitido_Falla()
    {
        ResultadoAccion resultado =
            operacion.Ejecutar(
                partida,
                new SolicitudConstruccion(
                    aldeano.Id,
                    "CastilloInventado",
                    new Coordenada(3, 3)));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(resultado.Mensaje, Does.Contain("permitido"));
    }

    [Test]
    public void Servicio_ConstruccionValida()
    {
        var servicio = new EstadoPartidaService();
        servicio.EstablecerPartida(partida);

        var request = new ConstruirRequest
        {
            AldeanoId = aldeano.Id.ToString("D"),
            TipoEdificio = "CentroUrbano",
            Destino = new CoordenadaRequest
            {
                X = 3,
                Y = 3
            }
        };

        ResultadoAccion resultado =
            servicio.Construir(request);

        Assert.That(resultado.Exito, Is.True);
        Assert.That(
            mapa.ObtenerCasilla(3, 3).EstaOcupada,
            Is.True);
    }

    [Test]
    public void Servicio_IdInvalido_Falla()
    {
        var servicio = new EstadoPartidaService();
        servicio.EstablecerPartida(partida);

        var request = new ConstruirRequest
        {
            AldeanoId = "no-es-guid",
            TipoEdificio = "CentroUrbano",
            Destino = new CoordenadaRequest
            {
                X = 3,
                Y = 3
            }
        };

        ResultadoAccion resultado =
            servicio.Construir(request);

        Assert.That(resultado.Exito, Is.False);
        Assert.That(resultado.Mensaje, Does.Contain("Guid"));
    }
}