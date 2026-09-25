using ImperiosEnGuerra.Api.Servicios;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Combate;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;
using ImperiosEnGuerra.Servicios.Concurrencia;

namespace ImperiosEnGuerra.Tests;

public class ReaccionAutomaticaTests
{
    private Mapa mapa;
    private Jugador humano;
    private Jugador maquina;
    private Partida partida;
    private PlanificadorReaccionAutomatica planificador;

    [SetUp]
    public void Preparar()
    {
        mapa =
            new Mapa(12, 12);

        humano =
            new Jugador(
                "Humano",
                TipoJugador.Humano,
                mapa,
                new RecursosJugador());

        maquina =
            new Jugador(
                "Máquina",
                TipoJugador.Maquina,
                mapa,
                new RecursosJugador());

        partida =
            new Partida(
                humano,
                maquina);

        planificador =
            new PlanificadorReaccionAutomatica();
    }

    [Test]
    public void SoldadoIdle_DetectaEnemigoCercano_YPreparaAtaque()
    {
        var guerrero =
            new Guerrero(
                new Coordenada(1, 1));

        var enemigo =
            new Guerrero(
                new Coordenada(4, 1));

        humano.AgregarUnidad(
            guerrero);

        maquina.AgregarUnidad(
            enemigo);

        var reacciones =
            planificador.Preparar(
                partida);

        Assert.That(
            reacciones.Count,
            Is.EqualTo(1));

        Assert.That(
            reacciones[0].Tipo,
            Is.EqualTo(
                TipoReaccionAutomatica.Atacar));

        Assert.That(
            reacciones[0].UnidadId,
            Is.EqualTo(
                guerrero.Id));

        Assert.That(
            reacciones[0].ObjetivoId,
            Is.EqualTo(
                enemigo.Id));
    }

    [Test]
    public void Soldado_NoPersigueEnemigoFueraDelRadio()
    {
        humano.AgregarUnidad(
            new Guerrero(
                new Coordenada(1, 1)));

        maquina.AgregarUnidad(
            new Guerrero(
                new Coordenada(9, 9)));

        var reacciones =
            planificador.Preparar(
                partida);

        Assert.That(
            reacciones,
            Is.Empty);
    }

    [Test]
    public void OrdenManual_ImpideReaccionAutomatica()
    {
        var guerrero =
            new Guerrero(
                new Coordenada(1, 1));

        var enemigo =
            new Guerrero(
                new Coordenada(2, 1));

        humano.AgregarUnidad(
            guerrero);

        maquina.AgregarUnidad(
            enemigo);

        Assert.That(
            guerrero.IntentarIniciarOrden(
                TipoAccionJuego.Mover),
            Is.True);

        var reacciones =
            planificador.Preparar(
                partida);

        Assert.That(
            reacciones,
            Is.Empty);
    }

    [Test]
    public void Monje_DetectaAliadoHerido_YPreparaCuracion()
    {
        var monje =
            new Monje(
                new Coordenada(1, 1));

        var aliado =
            new Guerrero(
                new Coordenada(2, 1));

        aliado.RecibirDanio(
            30);

        humano.AgregarUnidad(
            monje);

        humano.AgregarUnidad(
            aliado);

        var reacciones =
            planificador.Preparar(
                partida);

        Assert.That(
            reacciones.Count,
            Is.EqualTo(1));

        Assert.That(
            reacciones[0].Tipo,
            Is.EqualTo(
                TipoReaccionAutomatica.Curar));

        Assert.That(
            reacciones[0].UnidadId,
            Is.EqualTo(
                monje.Id));

        Assert.That(
            reacciones[0].ObjetivoId,
            Is.EqualTo(
                aliado.Id));
    }

    [Test]
    public void Monje_DetectaAliadoHeridoFueraDeAlcanceDeCuracion()
    {
        var monje =
            new Monje(
                new Coordenada(1, 1));

        var aliado =
            new Guerrero(
                new Coordenada(5, 1));

        aliado.RecibirDanio(
            30);

        humano.AgregarUnidad(
            monje);

        humano.AgregarUnidad(
            aliado);

        var reacciones =
            planificador.Preparar(
                partida);

        Assert.That(
            reacciones.Count,
            Is.EqualTo(1));

        Assert.That(
            reacciones[0].Tipo,
            Is.EqualTo(
                TipoReaccionAutomatica.Curar));

        Assert.That(
            reacciones[0].ObjetivoId,
            Is.EqualTo(
                aliado.Id));
    }

    [Test]
    public void Monje_NoCuraAliadoConVidaCompleta()
    {
        humano.AgregarUnidad(
            new Monje(
                new Coordenada(1, 1)));

        humano.AgregarUnidad(
            new Guerrero(
                new Coordenada(2, 1)));

        var reacciones =
            planificador.Preparar(
                partida);

        Assert.That(
            reacciones,
            Is.Empty);
    }

    [Test]
    public void AldeanoIdle_PreparaMovimientoLigero()
    {
        var aldeano =
            new Aldeano(
                new Coordenada(5, 5));

        humano.AgregarUnidad(
            aldeano);

        var reacciones =
            planificador.Preparar(
                partida);

        Assert.That(
            reacciones.Count,
            Is.EqualTo(1));

        Assert.That(
            reacciones[0].Tipo,
            Is.EqualTo(
                TipoReaccionAutomatica.MoverIdle));

        Assert.That(
            reacciones[0].UnidadId,
            Is.EqualTo(
                aldeano.Id));

        Assert.That(
            reacciones[0].Destino,
            Is.Not.Null);

        int distancia =
            Math.Max(
                Math.Abs(
                    aldeano.Coordenada.X -
                    reacciones[0].Destino.X),
                Math.Abs(
                    aldeano.Coordenada.Y -
                    reacciones[0].Destino.Y));

        Assert.That(
            distancia,
            Is.InRange(1, 2));
    }

    [Test]
    public void AldeanoConOrdenReal_NoRecibeMovimientoIdle()
    {
        var aldeano =
            new Aldeano(
                new Coordenada(5, 5));

        humano.AgregarUnidad(
            aldeano);

        Assert.That(
            aldeano.IntentarIniciarOrden(
                TipoAccionJuego.Recolectar),
            Is.True);

        var reacciones =
            planificador.Preparar(
                partida);

        Assert.That(
            reacciones,
            Is.Empty);

        aldeano.CompletarOrden();
    }

    [Test]
    public async Task OrdenManual_CancelaDeambulacionAutomatica()
    {
        var aldeano =
            new Aldeano(
                new Coordenada(5, 5));

        humano.AgregarUnidad(
            aldeano);

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
                TimeSpan.FromSeconds(2));

        using var reacciones =
            new ServicioReaccionesAutomaticas(
                estado,
                acciones,
                TimeSpan.FromMilliseconds(20));

        int iniciadas =
            reacciones.EjecutarPaso();

        Assert.That(
            iniciadas,
            Is.EqualTo(1));

        Assert.That(
            aldeano.OrdenActiva,
            Is.Null,
            "La deambulación idle no debe mostrarse como una orden activa.");

        Assert.That(
            aldeano.Disponible,
            Is.True,
            "El paseo idle no debe bloquear Recolectar, Construir o Mover.");

        reacciones.PrepararOrdenManual(
            aldeano.Id);

        Assert.That(
            SpinWait.SpinUntil(
                () =>
                    !aldeano.OrdenActiva.HasValue,
                TimeSpan.FromSeconds(1)),
            Is.True);

        Assert.That(
            aldeano.Disponible,
            Is.True);
    }

    [Test]
    public void UnidadSuspendida_NoReiniciaReaccionAutomatica()
    {
        var guerrero =
            new Guerrero(
                new Coordenada(1, 1));

        var enemigo =
            new Guerrero(
                new Coordenada(2, 1));

        humano.AgregarUnidad(
            guerrero);

        maquina.AgregarUnidad(
            enemigo);

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

        using var reacciones =
            new ServicioReaccionesAutomaticas(
                estado,
                acciones,
                TimeSpan.FromMilliseconds(20));

        reacciones.SuspenderUnidad(
            guerrero.Id);

        int iniciadas =
            reacciones.EjecutarPaso();

        Assert.That(
            iniciadas,
            Is.Zero);

        reacciones.ReanudarUnidad(
            guerrero.Id);

        int reanudadas =
            reacciones.EjecutarPaso();

        Assert.That(
            reanudadas,
            Is.EqualTo(1));
    }

    [Test]
    public void Servicio_EjecutarPaso_LanzaReaccionConcurrente()
    {
        var guerrero =
            new Guerrero(
                new Coordenada(1, 1));

        var enemigo =
            new Guerrero(
                new Coordenada(2, 1));

        humano.AgregarUnidad(
            guerrero);

        maquina.AgregarUnidad(
            enemigo);

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
                TimeSpan.FromMilliseconds(20));

        using var reacciones =
            new ServicioReaccionesAutomaticas(
                estado,
                acciones,
                TimeSpan.FromMilliseconds(20));

        int iniciadas =
            reacciones.EjecutarPaso();

        Assert.That(
            iniciadas,
            Is.EqualTo(1));
    }
}
