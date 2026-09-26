using ImperiosEnGuerra.Api.Servicios;
using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;
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

        using var regeneracion =
            new ServicioRegeneracionRecursos(
                estado,
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(1),
                7);

        using var sesion =
            new ServicioSesionJuego(
                estado,
                acciones,
                maquina,
                reacciones,
                regeneracion,
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
            regeneracion.Activo,
            Is.True);

        Assert.That(
            sesion.Pausar(),
            Is.True);

        Assert.That(
            SpinWait.SpinUntil(
                () =>
                    !maquina.Activo &&
                    !reacciones.Activo &&
                    !regeneracion.Activo,
                TimeSpan.FromSeconds(1)),
            Is.True);

        Assert.That(
            sesion.Activa,
            Is.False);
    }

    [Test]
    public async Task PausaTemporal_DetieneWorkerYReanudaLaMismaOrden()
    {
        Partida partida =
            CrearPartidaMinima();

        var aldeano =
            new Aldeano(
                new Coordenada(
                    1,
                    0));

        partida.JugadorHumano.AgregarUnidad(
            aldeano);

        partida.JugadorHumano.Mapa
            .ObtenerCasilla(
                1,
                0)
            .Ocupar();

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
                TimeSpan.FromMilliseconds(
                    120));

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

        using var regeneracion =
            new ServicioRegeneracionRecursos(
                estado,
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(1),
                7);

        using var sesion =
            new ServicioSesionJuego(
                estado,
                acciones,
                maquina,
                reacciones,
                regeneracion,
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(1));

        Assert.That(
            sesion.Activar(),
            Is.True);

        Assert.That(
            sesion.PausarTemporal(),
            Is.True);

        ProcesoConcurrente proceso =
            acciones.IniciarMovimiento(
                new MoverUnidadRequest
                {
                    UnidadId =
                        aldeano.Id.ToString(
                            "D"),

                    Destino =
                        new CoordenadaRequest
                        {
                            X = 2,
                            Y = 0
                        }
                });

        Thread.Sleep(
            250);

        Assert.That(
            aldeano.Coordenada.X,
            Is.EqualTo(
                1),
            "El worker no debe modificar el Modelo mientras el menú de pausa está activo.");

        Assert.That(
            sesion.ReanudarTemporal(),
            Is.True);

        await proceso.Finalizacion;

        Assert.That(
            aldeano.Coordenada.X,
            Is.EqualTo(
                2));

        Assert.That(
            acciones.PausadoTemporalmente,
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

        using var regeneracion =
            new ServicioRegeneracionRecursos(
                estado,
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(1),
                7);

        using var sesion =
            new ServicioSesionJuego(
                estado,
                acciones,
                maquina,
                reacciones,
                regeneracion,
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
                    !reacciones.Activo &&
                    !regeneracion.Activo,
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

        using var regeneracion =
            new ServicioRegeneracionRecursos(
                estado,
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(1),
                7);

        using var sesion =
            new ServicioSesionJuego(
                estado,
                acciones,
                maquina,
                reacciones,
                regeneracion,
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
