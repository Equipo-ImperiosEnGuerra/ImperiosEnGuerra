using ImperiosEnGuerra.Api.Mapeadores;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Combate;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.IA;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;
using NUnit.Framework;

namespace ImperiosEnGuerra.Tests;

public class CuatroJugadoresTests
{
    [Test]
    public void PartidaMultijugador_ExponeHumanoYTresMaquinas()
    {
        Partida partida =
            CrearPartidaCuatro();

        Assert.That(
            partida.Jugadores,
            Has.Count.EqualTo(4));

        Assert.That(
            partida.JugadoresMaquina,
            Has.Count.EqualTo(3));

        Assert.That(
            partida.JugadorMaquina,
            Is.SameAs(
                partida.JugadoresMaquina[0]));

        Assert.That(
            partida.SonAliados(
                partida.JugadoresMaquina[0],
                partida.JugadoresMaquina[1]),
            Is.True);

        Assert.That(
            partida.SonEnemigos(
                partida.JugadorHumano,
                partida.JugadoresMaquina[2]),
            Is.True);
    }

    [Test]
    public void InicializadorCuatroJugadores_ComparteMapaYRecursosFisicos()
    {
        Partida partida =
            CrearPartidaCuatro();

        Mapa mapa =
            partida.JugadorHumano.Mapa;

        Assert.That(
            partida.Jugadores.All(
                jugador =>
                    ReferenceEquals(
                        jugador.Mapa,
                        mapa)),
            Is.True);

        Assert.That(
            mapa.Recursos,
            Has.Count.EqualTo(9));

        Assert.That(
            partida.Jugadores.All(
                jugador =>
                    jugador.Edificios
                        .OfType<CentroUrbano>()
                        .Count() == 1),
            Is.True);

        Assert.That(
            partida.Jugadores.All(
                jugador =>
                    jugador.Unidades.Count ==
                    ConfiguracionInicioPartida
                        .AldeanosInicialesPredeterminados),
            Is.True);

        Assert.That(
            partida.Jugadores
                .Select(
                    jugador =>
                        jugador.Recursos)
                .Distinct()
                .Count(),
            Is.EqualTo(4));
    }

    [Test]
    public void EliminarUnaIa_NoFinalizaPartida()
    {
        Partida partida =
            CrearPartidaCuatro();

        Jugador maquina =
            partida.JugadoresMaquina[0];

        EliminarCentro(
            maquina);

        EvaluacionVictoria evaluacion =
            new EvaluadorVictoria()
                .Evaluar(
                    partida,
                    maquina);

        Assert.That(
            evaluacion.HayVictoria,
            Is.True);

        Assert.That(
            partida.Finalizada,
            Is.False);
    }

    [Test]
    public void ConquistarIa_CreaCentroUrbanoParaHumano()
    {
        Partida partida =
            CrearPartidaCuatro();

        Jugador maquina =
            partida.JugadoresMaquina[0];

        EliminarCentro(
            maquina);

        var atacante =
            new Guerrero(
                new Coordenada(
                    6,
                    6));

        var defensor =
            new Guerrero(
                new Coordenada(
                    7,
                    6));

        partida.JugadorHumano
            .AgregarUnidad(
                atacante);

        maquina.AgregarUnidad(
            defensor);

        partida.JugadorHumano.Mapa
            .ObtenerCasilla(
                6,
                6)
            .Ocupar();

        maquina.Mapa
            .ObtenerCasilla(
                7,
                6)
            .Ocupar();

        int centrosAntes =
            partida.JugadorHumano
                .Edificios
                .OfType<CentroUrbano>()
                .Count();

        var ataque =
            new OperacionAtaque();

        while (maquina.Unidades.Any(
                   unidad =>
                       unidad.Id ==
                       defensor.Id))
        {
            ResultadoAccion resultado =
                ataque.Ejecutar(
                    partida,
                    new SolicitudAtaque(
                        atacante.Id,
                        defensor.Id));

            Assert.That(
                resultado.Exito,
                Is.True,
                resultado.Mensaje);
        }

        CentroUrbano[] centros =
            partida.JugadorHumano
                .Edificios
                .OfType<CentroUrbano>()
                .ToArray();

        Assert.That(
            centros,
            Has.Length.EqualTo(
                centrosAntes + 1));

        Assert.That(
            centros.Any(
                centro =>
                    centro.Coordenada.X == 7 &&
                    centro.Coordenada.Y == 6),
            Is.True);

        Assert.That(
            partida.Finalizada,
            Is.False,
            "Conquistar una sola IA no debe terminar la partida.");
    }

    [Test]
    public void IaYaEliminada_DestruirAldeanoResidual_NoCreaOtroCentroConquista()
    {
        Partida partida =
            CrearPartidaCuatro();

        Jugador maquina =
            partida.JugadoresMaquina[0];

        EliminarCentro(
            maquina);

        var atacante =
            new Guerrero(
                new Coordenada(
                    6,
                    6));

        var defensorMilitar =
            new Guerrero(
                new Coordenada(
                    7,
                    6));

        var aldeanoResidual =
            new Aldeano(
                new Coordenada(
                    6,
                    7));

        partida.JugadorHumano
            .AgregarUnidad(
                atacante);

        maquina.AgregarUnidad(
            defensorMilitar);

        maquina.AgregarUnidad(
            aldeanoResidual);

        partida.JugadorHumano.Mapa
            .ObtenerCasilla(
                6,
                6)
            .Ocupar();

        maquina.Mapa
            .ObtenerCasilla(
                7,
                6)
            .Ocupar();

        maquina.Mapa
            .ObtenerCasilla(
                6,
                7)
            .Ocupar();

        var ataque =
            new OperacionAtaque();

        while (maquina.Unidades.Any(
                   unidad =>
                       unidad.Id ==
                       defensorMilitar.Id))
        {
            ResultadoAccion resultado =
                ataque.Ejecutar(
                    partida,
                    new SolicitudAtaque(
                        atacante.Id,
                        defensorMilitar.Id));

            Assert.That(
                resultado.Exito,
                Is.True,
                resultado.Mensaje);
        }

        int centrosTrasConquista =
            partida.JugadorHumano
                .Edificios
                .OfType<CentroUrbano>()
                .Count();

        Assert.That(
            centrosTrasConquista,
            Is.EqualTo(
                2),
            "La primera IA derrotada debe crear exactamente un Centro Urbano de conquista.");

        while (maquina.Unidades.Any(
                   unidad =>
                       unidad.Id ==
                       aldeanoResidual.Id))
        {
            ResultadoAccion resultado =
                ataque.Ejecutar(
                    partida,
                    new SolicitudAtaque(
                        atacante.Id,
                        aldeanoResidual.Id));

            Assert.That(
                resultado.Exito,
                Is.True,
                resultado.Mensaje);
        }

        int centrosDespuesDelAldeano =
            partida.JugadorHumano
                .Edificios
                .OfType<CentroUrbano>()
                .Count();

        Assert.That(
            centrosDespuesDelAldeano,
            Is.EqualTo(
                centrosTrasConquista),
            "Una IA ya derrotada no puede generar Centros Urbanos adicionales al perder Aldeanos residuales.");
    }

    [Test]
    public void IaEliminada_DejaDeTomarDecisiones()
    {
        Partida partida =
            CrearPartidaCuatro();

        Jugador maquina =
            partida.JugadoresMaquina[0];

        EliminarCentro(
            maquina);

        DecisionMaquina decision =
            new PlanificadorDecisionMaquina()
                .Preparar(
                    partida,
                    maquina);

        Assert.That(
            decision.Tipo,
            Is.EqualTo(
                TipoDecisionMaquina.Ninguna));
    }

    [Test]
    public void EliminarLasTresIas_DaVictoriaAlHumano()
    {
        Partida partida =
            CrearPartidaCuatro();

        var evaluador =
            new EvaluadorVictoria();

        foreach (Jugador maquina
                 in partida.JugadoresMaquina)
        {
            EliminarCentro(
                maquina);

            evaluador.Evaluar(
                partida,
                maquina);
        }

        Assert.That(
            partida.Finalizada,
            Is.True);

        Assert.That(
            partida.Ganador,
            Is.SameAs(
                partida.JugadorHumano));
    }

    [Test]
    public void Mapper_ExponeCuatroFaccionesConColoresLogicos()
    {
        Partida partida =
            CrearPartidaCuatro();

        var respuesta =
            PartidaEstadoMapper.Convertir(
                partida);

        Assert.That(
            respuesta.Jugadores,
            Has.Count.EqualTo(4));

        Assert.That(
            respuesta.Jugadores
                .Select(
                    jugador =>
                        jugador.Faccion),
            Is.EqualTo(
                new[]
                {
                    "Azul",
                    "Morada",
                    "Verde",
                    "Amarilla"
                }));
    }

    [Test]
    public void PlanificadorIa_PuedeDecidirParaLaTerceraMaquina()
    {
        Partida partida =
            CrearPartidaCuatro();

        Jugador amarilla =
            partida.JugadoresMaquina[2];

        DecisionMaquina decision =
            new PlanificadorDecisionMaquina()
                .Preparar(
                    partida,
                    amarilla);

        Assert.That(
            decision.Tipo,
            Is.Not.EqualTo(
                TipoDecisionMaquina.Ninguna));

        if (decision.EdificioOrigen != null)
        {
            Assert.That(
                amarilla.Edificios.Any(
                    edificio =>
                        edificio.Coordenada.X ==
                            decision.EdificioOrigen.X &&
                        edificio.Coordenada.Y ==
                            decision.EdificioOrigen.Y),
                Is.True);
        }

        if (decision.UnidadId != Guid.Empty)
        {
            Assert.That(
                amarilla.Unidades.Any(
                    unidad =>
                        unidad.Id ==
                        decision.UnidadId),
                Is.True);
        }
    }

    private static Partida CrearPartidaCuatro()
    {
        var mapa =
            new Mapa(
                15,
                15);

        var recursos =
            new List<Recurso>
            {
                new Recurso(
                    TipoRecurso.Oro,
                    new Coordenada(4, 2)),
                new Recurso(
                    TipoRecurso.Oro,
                    new Coordenada(10, 2)),
                new Recurso(
                    TipoRecurso.Oro,
                    new Coordenada(7, 10)),

                new Recurso(
                    TipoRecurso.Madera,
                    new Coordenada(2, 5)),
                new Recurso(
                    TipoRecurso.Madera,
                    new Coordenada(7, 5)),
                new Recurso(
                    TipoRecurso.Madera,
                    new Coordenada(12, 8)),

                new Recurso(
                    TipoRecurso.Comida,
                    new Coordenada(5, 8)),
                new Recurso(
                    TipoRecurso.Comida,
                    new Coordenada(9, 9)),
                new Recurso(
                    TipoRecurso.Comida,
                    new Coordenada(4, 12))
            };

        return new InicializadorPartida()
            .CrearCuatroJugadores(
                "Jugador",
                mapa,
                new Coordenada(1, 1),
                recursos,
                new[]
                {
                    (
                        "CPU Morada",
                        new Coordenada(13, 13)
                    ),
                    (
                        "CPU Verde",
                        new Coordenada(13, 1)
                    ),
                    (
                        "CPU Amarilla",
                        new Coordenada(1, 13)
                    )
                });
    }

    private static void EliminarCentro(
        Jugador jugador)
    {
        CentroUrbano centro =
            jugador.Edificios
                .OfType<CentroUrbano>()
                .Single();

        Assert.That(
            jugador.EliminarEdificio(
                centro),
            Is.True);

        jugador.Mapa
            .ObtenerCasilla(
                centro.Coordenada.X,
                centro.Coordenada.Y)
            .Liberar();
    }
}
