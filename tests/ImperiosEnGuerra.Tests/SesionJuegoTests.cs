using ImperiosEnGuerra.Api.Servicios;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Servicios.Concurrencia;

namespace ImperiosEnGuerra.Tests;

public class SesionJuegoTests
{
    [Test]
    public void ActivarYPausar_ControlaServiciosDeSimulacion()
    {
        Partida partida =
            CrearPartidaMinima();

        var estado =
            new EstadoPartidaService();

        estado.EstablecerPartida(
            partida);

        using var gestor =
            new GestorProcesosConcurrentes();

        var acciones =
            new ServicioAccionesConcurrentes(
                estado,
                gestor,
                TimeSpan.Zero);

        using var maquina =
            new ServicioJugadorMaquina(
                estado,
                acciones,
                TimeSpan.FromSeconds(5));

        using var reacciones =
            new ServicioReaccionesAutomaticas(
                estado,
                acciones,
                TimeSpan.FromSeconds(5));

        using var sesion =
            new ServicioSesionJuego(
                estado,
                acciones,
                maquina,
                reacciones,
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(1));

        Assert.That(
            sesion.Activar(),
            Is.True);

        Assert.That(
            sesion.Activa,
            Is.True);

        Assert.That(
            maquina.Activo,
            Is.True);

        Assert.That(
            reacciones.Activo,
            Is.True);

        Assert.That(
            sesion.Pausar(),
            Is.True);

        Assert.That(
            SpinWait.SpinUntil(
                () =>
                    !maquina.Activo &&
                    !reacciones.Activo,
                TimeSpan.FromSeconds(1)),
            Is.True);

        Assert.That(
            sesion.Activa,
            Is.False);
    }

    [Test]
    public void SinLatido_SesionSePausaAutomaticamente()
    {
        Partida partida =
            CrearPartidaMinima();

        var estado =
            new EstadoPartidaService();

        estado.EstablecerPartida(
            partida);

        using var gestor =
            new GestorProcesosConcurrentes();

        var acciones =
            new ServicioAccionesConcurrentes(
                estado,
                gestor,
                TimeSpan.Zero);

        using var maquina =
            new ServicioJugadorMaquina(
                estado,
                acciones,
                TimeSpan.FromSeconds(5));

        using var reacciones =
            new ServicioReaccionesAutomaticas(
                estado,
                acciones,
                TimeSpan.FromSeconds(5));

        using var sesion =
            new ServicioSesionJuego(
                estado,
                acciones,
                maquina,
                reacciones,
                TimeSpan.FromMilliseconds(80),
                TimeSpan.FromMilliseconds(20));

        Assert.That(
            sesion.Activar(),
            Is.True);

        Assert.That(
            SpinWait.SpinUntil(
                () => !sesion.Activa,
                TimeSpan.FromSeconds(2)),
            Is.True,
            "La sesión debe detenerse si Unity deja de enviar snapshots.");

        Assert.That(
            SpinWait.SpinUntil(
                () =>
                    !maquina.Activo &&
                    !reacciones.Activo,
                TimeSpan.FromSeconds(1)),
            Is.True);
    }

    [Test]
    public void RegistrarLatido_MantieneSesionActiva()
    {
        Partida partida =
            CrearPartidaMinima();

        var estado =
            new EstadoPartidaService();

        estado.EstablecerPartida(
            partida);

        using var gestor =
            new GestorProcesosConcurrentes();

        var acciones =
            new ServicioAccionesConcurrentes(
                estado,
                gestor,
                TimeSpan.Zero);

        using var maquina =
            new ServicioJugadorMaquina(
                estado,
                acciones,
                TimeSpan.FromSeconds(5));

        using var reacciones =
            new ServicioReaccionesAutomaticas(
                estado,
                acciones,
                TimeSpan.FromSeconds(5));

        using var sesion =
            new ServicioSesionJuego(
                estado,
                acciones,
                maquina,
                reacciones,
                TimeSpan.FromMilliseconds(120),
                TimeSpan.FromMilliseconds(20));

        Assert.That(
            sesion.Activar(),
            Is.True);

        for (int i = 0;
             i < 5;
             i++)
        {
            Thread.Sleep(40);

            Assert.That(
                sesion.RegistrarLatido(),
                Is.True);
        }

        Assert.That(
            sesion.Activa,
            Is.True);
    }

    private static Partida CrearPartidaMinima()
    {
        var mapa =
            new Mapa(
                6,
                6);

        var humano =
            new Jugador(
                "Humano",
                TipoJugador.Humano,
                mapa,
                new RecursosJugador());

        var maquina =
            new Jugador(
                "CPU",
                TipoJugador.Maquina,
                mapa,
                new RecursosJugador());

        humano.AgregarEdificio(
            new CentroUrbano(
                new Coordenada(
                    0,
                    0)));

        maquina.AgregarEdificio(
            new CentroUrbano(
                new Coordenada(
                    5,
                    5)));

        mapa.ObtenerCasilla(
                0,
                0)
            .Ocupar();

        mapa.ObtenerCasilla(
                5,
                5)
            .Ocupar();

        return new Partida(
            humano,
            maquina);
    }
}
