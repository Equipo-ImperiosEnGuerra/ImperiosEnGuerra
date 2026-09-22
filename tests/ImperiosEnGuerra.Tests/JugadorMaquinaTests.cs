using ImperiosEnGuerra.Api.Servicios;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.IA;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;
using ImperiosEnGuerra.Servicios.Concurrencia;

namespace ImperiosEnGuerra.Tests;

public class JugadorMaquinaTests
{
    [Test]
    public void Planificador_EligeParejaDisponibleMasCercana()
    {
        Partida partida =
            CrearPartidaSeparada(
                out _,
                out Aldeano cercano,
                out Aldeano lejano,
                out Recurso recursoCercano,
                out _);

        DecisionMaquina decision =
            new PlanificadorDecisionMaquina()
                .Preparar(partida);

        Assert.That(
            decision.Tipo,
            Is.EqualTo(TipoDecisionMaquina.Recolectar));

        Assert.That(
            decision.UnidadId,
            Is.EqualTo(cercano.Id));

        Assert.That(
            decision.Objetivo.X,
            Is.EqualTo(recursoCercano.Coordenada.X));

        Assert.That(
            decision.Objetivo.Y,
            Is.EqualTo(recursoCercano.Coordenada.Y));

        DecisionMaquina excluyendo =
            new PlanificadorDecisionMaquina()
                .Preparar(
                    partida,
                    new[] { cercano.Id });

        Assert.That(
            excluyendo.UnidadId,
            Is.EqualTo(lejano.Id));
    }

    [Test]
    public async Task EjecutarPaso_RecolectaConAldeanoMaquinaYDepositaEnMaquina()
    {
        Partida partida =
            CrearPartidaSeparada(
                out Mapa mapaMaquina,
                out Aldeano cercano,
                out _,
                out Recurso recursoCercano,
                out _);

        var estado =
            new EstadoPartidaService();

        estado.EstablecerPartida(partida);

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
                TimeSpan.FromMilliseconds(10));

        int oroHumanoAntes =
            partida.JugadorHumano.Recursos
                .ObtenerCantidad(TipoRecurso.Oro);

        ProcesoConcurrente? proceso =
            maquina.EjecutarPaso();

        Assert.That(proceso, Is.Not.Null);

        await proceso!.Finalizacion;

        Assert.That(
            partida.JugadorMaquina.Recursos
                .ObtenerCantidad(recursoCercano.Tipo),
            Is.GreaterThan(0));

        Assert.That(
            partida.JugadorHumano.Recursos
                .ObtenerCantidad(TipoRecurso.Oro),
            Is.EqualTo(oroHumanoAntes));

        Assert.That(
            recursoCercano.Agotado,
            Is.True);

        Assert.That(
            cercano.Disponible,
            Is.True);

        Assert.That(
            mapaMaquina,
            Is.SameAs(partida.JugadorMaquina.Mapa));
    }

    [Test]
    public void CicloAutomatico_SePuedeIniciarYDetenerSinDuplicarlo()
    {
        Partida partida =
            CrearPartidaSeparada(
                out _,
                out _,
                out _,
                out _,
                out _);

        var estado =
            new EstadoPartidaService();

        estado.EstablecerPartida(partida);

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
                TimeSpan.FromMilliseconds(10));

        Assert.That(maquina.Iniciar(), Is.True);
        Assert.That(maquina.Iniciar(), Is.False);
        Assert.That(maquina.Activo, Is.True);
        Assert.That(maquina.Detener(), Is.True);

        Assert.That(
            SpinWait.SpinUntil(
                () => !maquina.Activo,
                TimeSpan.FromSeconds(1)),
            Is.True);
    }

    private static Partida CrearPartidaSeparada(
        out Mapa mapaMaquina,
        out Aldeano cercano,
        out Aldeano lejano,
        out Recurso recursoCercano,
        out Recurso recursoLejano)
    {
        var mapaHumano =
            new Mapa(8, 8);

        mapaMaquina =
            new Mapa(8, 8);

        var humano =
            new Jugador(
                "Humano",
                TipoJugador.Humano,
                mapaHumano,
                new RecursosJugador());

        var maquina =
            new Jugador(
                "Máquina",
                TipoJugador.Maquina,
                mapaMaquina,
                new RecursosJugador());

        humano.AgregarEdificio(
            new CentroUrbano(
                new Coordenada(0, 0)));

        maquina.AgregarEdificio(
            new CentroUrbano(
                new Coordenada(0, 0)));

        mapaHumano.ObtenerCasilla(0, 0).Ocupar();
        mapaMaquina.ObtenerCasilla(0, 0).Ocupar();

        cercano =
            new Aldeano(
                new Coordenada(1, 0));

        lejano =
            new Aldeano(
                new Coordenada(7, 7));

        maquina.AgregarUnidad(cercano);
        maquina.AgregarUnidad(lejano);

        recursoCercano =
            new Recurso(
                TipoRecurso.Oro,
                new Coordenada(2, 0),
                10);

        recursoLejano =
            new Recurso(
                TipoRecurso.Madera,
                new Coordenada(6, 7),
                10);

        Assert.That(
            mapaMaquina.ColocarRecurso(recursoCercano),
            Is.True);

        Assert.That(
            mapaMaquina.ColocarRecurso(recursoLejano),
            Is.True);

        return new Partida(
            humano,
            maquina);
    }
}
