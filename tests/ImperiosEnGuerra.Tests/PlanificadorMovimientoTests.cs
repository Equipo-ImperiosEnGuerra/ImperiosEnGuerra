using System.Linq;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Movimiento;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Tests;

public class PlanificadorMovimientoTests
{
    private Mapa mapa;
    private Partida partida;
    private Aldeano unidad;
    private PlanificadorMovimiento planificador;

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

        unidad =
            new Aldeano(
                new Coordenada(1, 1));

        partida.JugadorHumano
            .AgregarUnidad(unidad);

        planificador =
            new PlanificadorMovimiento();
    }

    [Test]
    public void EdificioIntermedio_SeRodea()
    {
        partida.JugadorHumano.AgregarEdificio(
            new CentroUrbano(
                new Coordenada(2, 1)));

        ResultadoPlanMovimiento resultado =
            planificador.Preparar(
                partida,
                new SolicitudMovimiento(
                    unidad.Id,
                    new Coordenada(3, 1)));

        Assert.That(
            resultado.Exito,
            Is.True);

        Assert.That(
            resultado.Pasos.Any(
                paso =>
                    paso.X == 2 &&
                    paso.Y == 1),
            Is.False);

        Assert.That(
            resultado.Pasos.Count,
            Is.EqualTo(4));
    }

    [Test]
    public void UnidadEnemigaIntermedia_SeRodea()
    {
        partida.JugadorMaquina.AgregarUnidad(
            new Guerrero(
                new Coordenada(2, 1)));

        ResultadoPlanMovimiento resultado =
            planificador.Preparar(
                partida,
                new SolicitudMovimiento(
                    unidad.Id,
                    new Coordenada(3, 1)));

        Assert.That(
            resultado.Exito,
            Is.True);

        Assert.That(
            resultado.Pasos.Any(
                paso =>
                    paso.X == 2 &&
                    paso.Y == 1),
            Is.False);
    }

    [Test]
    public void UnidadEncerradaPorEntidades_RutaImposible()
    {
        partida.JugadorHumano.AgregarEdificio(
            new CentroUrbano(
                new Coordenada(0, 1)));

        partida.JugadorHumano.AgregarEdificio(
            new CentroUrbano(
                new Coordenada(2, 1)));

        partida.JugadorMaquina.AgregarUnidad(
            new Guerrero(
                new Coordenada(1, 0)));

        partida.JugadorMaquina.AgregarUnidad(
            new Guerrero(
                new Coordenada(1, 2)));

        ResultadoPlanMovimiento resultado =
            planificador.Preparar(
                partida,
                new SolicitudMovimiento(
                    unidad.Id,
                    new Coordenada(4, 4)));

        Assert.That(
            resultado.Exito,
            Is.False);

        Assert.That(
            resultado.Mensaje,
            Does.Contain("ruta"));
    }

    [Test]
    public void Preparar_NoMueveLaUnidad()
    {
        var origen = unidad.Coordenada;

        ResultadoPlanMovimiento resultado =
            planificador.Preparar(
                partida,
                new SolicitudMovimiento(
                    unidad.Id,
                    new Coordenada(4, 1)));

        Assert.That(
            resultado.Exito,
            Is.True);

        Assert.That(
            unidad.Coordenada,
            Is.SameAs(origen));
    }
}
