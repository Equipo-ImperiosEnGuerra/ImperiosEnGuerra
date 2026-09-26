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

    [Test]
    public void Planificador_ConDosAldeanosYComida_EntrenaTercerAldeano()
    {
        Partida partida =
            CrearPartidaEstrategica(
                aldeanos: 2,
                centros: 1,
                oro: 0,
                madera: 0,
                comida: 10);

        DecisionMaquina decision =
            new PlanificadorDecisionMaquina()
                .Preparar(
                    partida);

        Assert.That(
            decision.Tipo,
            Is.EqualTo(
                TipoDecisionMaquina.Entrenar));

        Assert.That(
            decision.TipoUnidad,
            Is.EqualTo(
                nameof(Aldeano)));
    }

    [Test]
    public void Planificador_ConEconomiaBase_ConstruyeSegundoCentro()
    {
        Partida partida =
            CrearPartidaEstrategica(
                aldeanos: 3,
                centros: 1,
                oro: 20,
                madera: 50,
                comida: 0);

        DecisionMaquina decision =
            new PlanificadorDecisionMaquina()
                .Preparar(
                    partida);

        Assert.That(
            decision.Tipo,
            Is.EqualTo(
                TipoDecisionMaquina.Construir));

        Assert.That(
            decision.TipoEdificio,
            Is.EqualTo(
                nameof(CentroUrbano)));

        Assert.That(
            decision.UnidadId,
            Is.Not.EqualTo(
                Guid.Empty));

        Assert.That(
            decision.Objetivo,
            Is.Not.Null);
    }

    [Test]
    public void Planificador_SinExpansionPendiente_EntrenaPrimeraUnidadMilitar()
    {
        Partida partida =
            CrearPartidaEstrategica(
                aldeanos: 3,
                centros: 2,
                oro: 5,
                madera: 0,
                comida: 15);

        DecisionMaquina decision =
            new PlanificadorDecisionMaquina()
                .Preparar(
                    partida);

        Assert.That(
            decision.Tipo,
            Is.EqualTo(
                TipoDecisionMaquina.Entrenar));

        Assert.That(
            decision.TipoUnidad,
            Is.EqualTo(
                nameof(Guerrero)));
    }

    [Test]
    public async Task EjecutarPaso_EstrategiaEntrenamiento_CreaTercerAldeano()
    {
        Partida partida =
            CrearPartidaEstrategica(
                aldeanos: 2,
                centros: 1,
                oro: 0,
                madera: 0,
                comida: 10);

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
                TimeSpan.FromMilliseconds(10));

        ProcesoConcurrente? proceso =
            maquina.EjecutarPaso();

        Assert.That(
            proceso,
            Is.Not.Null);

        await proceso!.Finalizacion;

        Assert.That(
            partida.JugadorMaquina.Unidades
                .OfType<Aldeano>()
                .Count(),
            Is.EqualTo(3));
    }

    [Test]
    public async Task EjecutarPaso_EstrategiaConstruccion_CreaSegundoCentro()
    {
        Partida partida =
            CrearPartidaEstrategica(
                aldeanos: 3,
                centros: 1,
                oro: 20,
                madera: 50,
                comida: 0);

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
                TimeSpan.FromMilliseconds(10));

        ProcesoConcurrente? proceso =
            maquina.EjecutarPaso();

        Assert.That(
            proceso,
            Is.Not.Null);

        await proceso!.Finalizacion;

        Assert.That(
            partida.JugadorMaquina.Edificios
                .OfType<CentroUrbano>()
                .Count(),
            Is.EqualTo(2));
    }

    [Test]
    public void Planificador_MilitarLejano_DecideAproximarse()
    {
        Partida partida =
            CrearPartidaCombate(
                out Guerrero maquina,
                out Guerrero humano);

        DecisionMaquina decision =
            new PlanificadorDecisionMaquina()
                .Preparar(partida);

        Assert.That(
            decision.Tipo,
            Is.EqualTo(TipoDecisionMaquina.Mover));

        Assert.That(
            decision.UnidadId,
            Is.EqualTo(maquina.Id));

        Assert.That(
            decision.ObjetivoUnidadId,
            Is.EqualTo(humano.Id));

        Assert.That(
            decision.Objetivo,
            Is.Not.Null);
    }

    [Test]
    public async Task EjecutarDosPasos_MueveYLuegoPreparaAtaque()
    {
        Partida partida =
            CrearPartidaCombate(
                out Guerrero maquina,
                out Guerrero humano);

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

        using var ia =
            new ServicioJugadorMaquina(
                estado,
                acciones,
                TimeSpan.FromMilliseconds(10));

        ProcesoConcurrente? movimiento =
            ia.EjecutarPaso();

        Assert.That(
            movimiento,
            Is.Not.Null);

        await movimiento!.Finalizacion;

        Assert.That(
            Math.Abs(
                maquina.Coordenada.X -
                humano.Coordenada.X)
            +
            Math.Abs(
                maquina.Coordenada.Y -
                humano.Coordenada.Y),
            Is.EqualTo(1));

        ProcesoConcurrente? ataque =
            ia.EjecutarPaso();

        Assert.That(
            ataque,
            Is.Not.Null);

        await ataque!.Finalizacion;

        Assert.That(
            acciones.IntentarObtenerResultado(
                ataque.Id,
                out ResultadoProcesoConcurrente resultado),
            Is.True);

        Assert.That(
            resultado.Resultado?.Exito,
            Is.True,
            resultado.Resultado?.Mensaje);

        Assert.That(
            resultado.Resultado?.Mensaje,
            Does.Contain("destruido"));

        Assert.That(
            humano.VidaActual,
            Is.EqualTo(0));

        Assert.That(
            partida.JugadorHumano.Unidades,
            Does.Not.Contain(humano));
    }

    [Test]
    public void Planificador_NoPersigueAldeanosConUnSoloMilitar()
    {
        var mapa =
            new Mapa(10, 10);

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

        maquina.AgregarUnidad(
            new Guerrero(
                new Coordenada(1, 1)));

        humano.AgregarUnidad(
            new Aldeano(
                new Coordenada(5, 1)));

        Partida partida =
            new Partida(
                humano,
                maquina);

        DecisionMaquina decision =
            new PlanificadorDecisionMaquina()
                .Preparar(
                    partida);

        Assert.That(
            decision.Tipo,
            Is.EqualTo(
                TipoDecisionMaquina.Patrullar));

        Assert.That(
            decision.Objetivo,
            Is.Not.Null);

        Assert.That(
            decision.ObjetivoUnidadId,
            Is.EqualTo(
                Guid.Empty));
    }

    [Test]
    public async Task EjecutarPaso_Patrulla_MueveMilitarSinObjetivoDeCombate()
    {
        var mapa =
            new Mapa(10, 10);

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

        var patrullero =
            new Guerrero(
                new Coordenada(3, 3));

        maquina.AgregarUnidad(
            patrullero);

        humano.AgregarUnidad(
            new Aldeano(
                new Coordenada(8, 8)));

        Partida partida =
            new Partida(
                humano,
                maquina);

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

        using var ia =
            new ServicioJugadorMaquina(
                estado,
                acciones,
                TimeSpan.FromMilliseconds(10));

        int origenX =
            patrullero.Coordenada.X;

        int origenY =
            patrullero.Coordenada.Y;

        ProcesoConcurrente? proceso =
            ia.EjecutarPaso();

        Assert.That(
            proceso,
            Is.Not.Null);

        await proceso!.Finalizacion;

        Assert.That(
            patrullero.Coordenada.X != origenX ||
            patrullero.Coordenada.Y != origenY,
            Is.True);

        Assert.That(
            patrullero.OrdenActiva,
            Is.Null);
    }

    [Test]
    public void Planificador_EnFaseEconomica_NoIniciaCombate()
    {
        var mapa =
            new Mapa(10, 10);

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
                new Coordenada(8, 8)));

        maquina.AgregarUnidad(
            new Guerrero(
                new Coordenada(1, 1)));

        maquina.AgregarUnidad(
            new Guerrero(
                new Coordenada(2, 1)));

        mapa.ObtenerCasilla(
                8,
                8)
            .Ocupar();

        var partida =
            new Partida(
                humano,
                maquina);

        DecisionMaquina decision =
            new PlanificadorDecisionMaquina()
                .Preparar(
                    partida,
                    permitirCombate: false);

        Assert.That(
            decision.Tipo,
            Is.Not.EqualTo(
                TipoDecisionMaquina.Atacar));

        Assert.That(
            decision.Tipo,
            Is.Not.EqualTo(
                TipoDecisionMaquina.Mover),
            "Durante la gracia inicial no debe comenzar una aproximación ofensiva.");

        Assert.That(
            decision.Tipo,
            Is.EqualTo(
                TipoDecisionMaquina.Patrullar));
    }

    [Test]
    public void Planificador_ConDosMilitaresYSinMilitaresEnemigos_AsaltaCentro()
    {
        var mapa =
            new Mapa(10, 10);

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

        var centro =
            new CentroUrbano(
                new Coordenada(8, 8));

        humano.AgregarEdificio(
            centro);

        maquina.AgregarUnidad(
            new Guerrero(
                new Coordenada(1, 1)));

        maquina.AgregarUnidad(
            new Guerrero(
                new Coordenada(2, 1)));

        mapa.ObtenerCasilla(8, 8).Ocupar();

        Partida partida =
            new Partida(
                humano,
                maquina);

        DecisionMaquina decision =
            new PlanificadorDecisionMaquina()
                .Preparar(
                    partida);

        Assert.That(
            decision.Tipo,
            Is.EqualTo(
                TipoDecisionMaquina.Mover));

        Assert.That(
            decision.ObjetivoUnidadId,
            Is.EqualTo(
                centro.Id));
    }

    [Test]
    public async Task EjecutarPaso_NoIniciaDosCombatesDeMaquinaEnParalelo()
    {
        var mapa =
            new Mapa(10, 10);

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

        var maquinaUno =
            new Guerrero(
                new Coordenada(1, 1));

        var maquinaDos =
            new Guerrero(
                new Coordenada(1, 3));

        var humanoUno =
            new Guerrero(
                new Coordenada(2, 1));

        var humanoDos =
            new Guerrero(
                new Coordenada(2, 3));

        maquina.AgregarUnidad(
            maquinaUno);

        maquina.AgregarUnidad(
            maquinaDos);

        humano.AgregarUnidad(
            humanoUno);

        humano.AgregarUnidad(
            humanoDos);

        Partida partida =
            new Partida(
                humano,
                maquina);

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
                TimeSpan.FromMilliseconds(100));

        using var ia =
            new ServicioJugadorMaquina(
                estado,
                acciones,
                TimeSpan.FromMilliseconds(10));

        ProcesoConcurrente? primero =
            ia.EjecutarPaso();

        Assert.That(
            primero,
            Is.Not.Null);

        ProcesoConcurrente? segundo =
            ia.EjecutarPaso();

        Assert.That(
            segundo,
            Is.Null,
            "La Máquina debe conservar un único frente de combate mientras el primero siga activo.");

        await primero!.Finalizacion;

        Assert.That(
            acciones.IntentarObtenerResultado(
                primero.Id,
                out ResultadoProcesoConcurrente resultado),
            Is.True);

        Assert.That(
            resultado.Resultado?.Exito,
            Is.True,
            resultado.Resultado?.Mensaje);
    }

    private static Partida CrearPartidaCombate(
        out Guerrero maquina,
        out Guerrero humano)
    {
        var mapa =
            new Mapa(10, 10);

        var jugadorHumano =
            new Jugador(
                "Humano",
                TipoJugador.Humano,
                mapa,
                new RecursosJugador());

        var jugadorMaquina =
            new Jugador(
                "Máquina",
                TipoJugador.Maquina,
                mapa,
                new RecursosJugador());

        jugadorMaquina.AgregarEdificio(
            new CentroUrbano(
                new Coordenada(0, 0)));

        jugadorMaquina.AgregarEdificio(
            new CentroUrbano(
                new Coordenada(8, 8)));

        jugadorHumano.AgregarEdificio(
            new CentroUrbano(
                new Coordenada(9, 9)));

        mapa.ObtenerCasilla(0, 0).Ocupar();
        mapa.ObtenerCasilla(8, 8).Ocupar();
        mapa.ObtenerCasilla(9, 9).Ocupar();

        maquina =
            new Guerrero(
                new Coordenada(1, 1));

        humano =
            new Guerrero(
                new Coordenada(5, 1));

        jugadorMaquina.AgregarUnidad(
            maquina);

        jugadorHumano.AgregarUnidad(
            humano);

        return new Partida(
            jugadorHumano,
            jugadorMaquina);
    }

    private static Partida CrearPartidaEstrategica(
        int aldeanos,
        int centros,
        int oro,
        int madera,
        int comida)
    {
        var mapaHumano =
            new Mapa(10, 10);

        var mapaMaquina =
            new Mapa(10, 10);

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
                new Coordenada(9, 9)));

        mapaHumano
            .ObtenerCasilla(9, 9)
            .Ocupar();

        for (int i = 0;
             i < centros;
             i++)
        {
            Coordenada posicion =
                i == 0
                    ? new Coordenada(0, 0)
                    : new Coordenada(8, 8);

            maquina.AgregarEdificio(
                new CentroUrbano(
                    posicion));

            mapaMaquina
                .ObtenerCasilla(
                    posicion.X,
                    posicion.Y)
                .Ocupar();
        }

        Coordenada[] posicionesAldeanos =
        {
            new Coordenada(1, 0),
            new Coordenada(2, 0),
            new Coordenada(3, 0),
            new Coordenada(4, 0)
        };

        for (int i = 0;
             i < aldeanos;
             i++)
        {
            maquina.AgregarUnidad(
                new Aldeano(
                    posicionesAldeanos[i]));
        }

        maquina.Recursos.Agregar(
            TipoRecurso.Oro,
            oro);

        maquina.Recursos.Agregar(
            TipoRecurso.Madera,
            madera);

        maquina.Recursos.Agregar(
            TipoRecurso.Comida,
            comida);

        return new Partida(
            humano,
            maquina);
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
