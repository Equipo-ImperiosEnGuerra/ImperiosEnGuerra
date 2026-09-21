using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recoleccion;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Tests;

public class PlanificadorAproximacionRecursoTests
{
    private Mapa mapa;
    private Partida partida;
    private Aldeano aldeano;
    private Recurso recurso;
    private PlanificadorAproximacionRecurso planificador;

    [SetUp]
    public void Preparar()
    {
        mapa =
            new Mapa(7, 7);

        partida =
            new Partida(
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

        aldeano =
            new Aldeano(
                new Coordenada(1, 1));

        partida.JugadorHumano
            .AgregarUnidad(
                aldeano);

        recurso =
            new Recurso(
                TipoRecurso.Oro,
                new Coordenada(4, 1),
                20);

        Assert.That(
            mapa.ColocarRecurso(
                recurso),
            Is.True);

        planificador =
            new PlanificadorAproximacionRecurso();
    }

    [Test]
    public void AldeanoLejano_EligeCasillaAdyacenteSinEntrarAlRecurso()
    {
        Coordenada origen =
            aldeano.Coordenada;

        ResultadoAproximacionRecurso resultado =
            planificador.Preparar(
                partida,
                new SolicitudRecoleccion(
                    aldeano.Id,
                    recurso.Coordenada));

        Assert.That(
            resultado.Exito,
            Is.True);

        Assert.That(
            Distancia(
                resultado.PuntoInteraccion,
                recurso.Coordenada),
            Is.EqualTo(1));

        Assert.That(
            Coincide(
                resultado.PuntoInteraccion,
                recurso.Coordenada),
            Is.False);

        Assert.That(
            resultado.Pasos,
            Is.Not.Empty);

        Assert.That(
            aldeano.Coordenada,
            Is.SameAs(origen));
    }

    [Test]
    public void AldeanoYaAdyacente_RutaVacia()
    {
        var mapaAdyacente =
            new Mapa(7, 7);

        var partidaAdyacente =
            new Partida(
                new Jugador(
                    "Humano",
                    TipoJugador.Humano,
                    mapaAdyacente,
                    new RecursosJugador()),
                new Jugador(
                    "Máquina",
                    TipoJugador.Maquina,
                    mapaAdyacente,
                    new RecursosJugador()));

        var aldeanoAdyacente =
            new Aldeano(
                new Coordenada(3, 1));

        partidaAdyacente.JugadorHumano
            .AgregarUnidad(
                aldeanoAdyacente);

        var recursoAdyacente =
            new Recurso(
                TipoRecurso.Oro,
                new Coordenada(4, 1),
                20);

        mapaAdyacente.ColocarRecurso(
            recursoAdyacente);

        ResultadoAproximacionRecurso resultado =
            planificador.Preparar(
                partidaAdyacente,
                new SolicitudRecoleccion(
                    aldeanoAdyacente.Id,
                    recursoAdyacente.Coordenada));

        Assert.That(
            resultado.Exito,
            Is.True);

        Assert.That(
            resultado.Pasos,
            Is.Empty);

        Assert.That(
            resultado.PuntoInteraccion.X,
            Is.EqualTo(3));

        Assert.That(
            resultado.PuntoInteraccion.Y,
            Is.EqualTo(1));
    }

    [Test]
    public void LadoMasCercanoBloqueado_EligeOtraCasilla()
    {
        partida.JugadorHumano.AgregarEdificio(
            new CentroUrbano(
                new Coordenada(3, 1)));

        ResultadoAproximacionRecurso resultado =
            planificador.Preparar(
                partida,
                new SolicitudRecoleccion(
                    aldeano.Id,
                    recurso.Coordenada));

        Assert.That(
            resultado.Exito,
            Is.True);

        Assert.That(
            Coincide(
                resultado.PuntoInteraccion,
                new Coordenada(3, 1)),
            Is.False);

        Assert.That(
            Distancia(
                resultado.PuntoInteraccion,
                recurso.Coordenada),
            Is.EqualTo(1));
    }

    [Test]
    public void RecursoSinCasillasAdyacentesTransitables_Falla()
    {
        foreach ((int x, int y) in new[]
        {
            (3, 1),
            (5, 1),
            (4, 0),
            (4, 2)
        })
        {
            mapa.ObtenerCasilla(
                x,
                y)
                .CambiarTransitabilidad(
                    false);
        }

        ResultadoAproximacionRecurso resultado =
            planificador.Preparar(
                partida,
                new SolicitudRecoleccion(
                    aldeano.Id,
                    recurso.Coordenada));

        Assert.That(
            resultado.Exito,
            Is.False);

        Assert.That(
            resultado.Mensaje,
            Does.Contain("accesible"));

        Assert.That(
            resultado.Reintentable,
            Is.True);
    }

    [Test]
    public void RecursoAgotado_Falla()
    {
        recurso.Extraer(
            recurso.CantidadRestante);

        ResultadoAproximacionRecurso resultado =
            planificador.Preparar(
                partida,
                new SolicitudRecoleccion(
                    aldeano.Id,
                    recurso.Coordenada));

        Assert.That(
            resultado.Exito,
            Is.False);

        Assert.That(
            resultado.Mensaje,
            Does.Contain("agotado"));

        Assert.That(
            resultado.Reintentable,
            Is.False);
    }

    private static int Distancia(
        Coordenada primera,
        Coordenada segunda)
    {
        return Math.Abs(
                   primera.X -
                   segunda.X)
               +
               Math.Abs(
                   primera.Y -
                   segunda.Y);
    }

    private static bool Coincide(
        Coordenada primera,
        Coordenada segunda)
    {
        return primera.X == segunda.X &&
               primera.Y == segunda.Y;
    }
}
