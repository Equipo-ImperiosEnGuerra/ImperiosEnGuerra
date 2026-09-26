using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Tests;

public class OperacionPasoRecoleccionTests
{
    private Mapa mapa;
    private Partida partida;
    private Aldeano aldeano;
    private Recurso recurso;
    private OperacionPasoRecoleccion operacion;

    [SetUp]
    public void Preparar()
    {
        mapa = new Mapa(6, 6);

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
                new Coordenada(1, 1),
                5);

        recurso =
            new Recurso(
                TipoRecurso.Oro,
                new Coordenada(2, 1),
                20);

        partida.JugadorHumano.AgregarUnidad(
            aldeano);

        mapa.ColocarRecurso(
            recurso);

        Assert.That(
            aldeano.IntentarIniciarOrden(
                TipoAccionJuego.Recolectar),
            Is.True);

        operacion =
            new OperacionPasoRecoleccion();
    }

    [Test]
    public void PasoValido_AumentaCargaPeroNoSaldo()
    {
        int saldoInicial =
            partida.JugadorHumano.Recursos
                .ObtenerCantidad(
                    TipoRecurso.Oro);

        var resultado =
            operacion.Ejecutar(
                partida,
                aldeano.Id,
                recurso.Coordenada,
                2);

        Assert.That(
            resultado.Exito,
            Is.True);

        Assert.That(
            resultado.CantidadExtraida,
            Is.EqualTo(2));

        Assert.That(
            aldeano.CargaActual,
            Is.EqualTo(2));

        Assert.That(
            recurso.CantidadRestante,
            Is.EqualTo(18));

        Assert.That(
            partida.JugadorHumano.Recursos
                .ObtenerCantidad(
                    TipoRecurso.Oro),
            Is.EqualTo(saldoInicial));
    }

    [Test]
    public void VariosPasos_RespetanCapacidad()
    {
        operacion.Ejecutar(
            partida,
            aldeano.Id,
            recurso.Coordenada,
            3);

        var segundo =
            operacion.Ejecutar(
                partida,
                aldeano.Id,
                recurso.Coordenada,
                3);

        Assert.That(
            segundo.Exito,
            Is.True);

        Assert.That(
            segundo.CantidadExtraida,
            Is.EqualTo(2));

        Assert.That(
            segundo.CapacidadCompleta,
            Is.True);

        Assert.That(
            aldeano.CargaActual,
            Is.EqualTo(5));
    }

    [Test]
    public void AldeanoNoAdyacente_Falla()
    {
        var mapaLejano = new Mapa(6, 6);

        var partidaLejana =
            new Partida(
                new Jugador(
                    "Humano",
                    TipoJugador.Humano,
                    mapaLejano,
                    new RecursosJugador()),
                new Jugador(
                    "Máquina",
                    TipoJugador.Maquina,
                    mapaLejano,
                    new RecursosJugador()));

        var lejano =
            new Aldeano(
                new Coordenada(0, 0));

        var nodo =
            new Recurso(
                TipoRecurso.Oro,
                new Coordenada(4, 4),
                10);

        partidaLejana.JugadorHumano
            .AgregarUnidad(
                lejano);

        mapaLejano.ColocarRecurso(
            nodo);

        lejano.IntentarIniciarOrden(
            TipoAccionJuego.Recolectar);

        var resultado =
            operacion.Ejecutar(
                partidaLejana,
                lejano.Id,
                nodo.Coordenada,
                1);

        Assert.That(
            resultado.Exito,
            Is.False);

        Assert.That(
            resultado.Mensaje,
            Does.Contain("junto"));
    }
}
