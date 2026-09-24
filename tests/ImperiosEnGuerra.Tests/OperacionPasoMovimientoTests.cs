using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Tests;

public class OperacionPasoMovimientoTests
{
    private Mapa mapa;
    private Partida partida;
    private Aldeano unidad;
    private OperacionPasoMovimiento operacion;

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

        operacion =
            new OperacionPasoMovimiento();
    }

    [Test]
    public void OrdenMovimientoActiva_AvanzaUnaCasilla()
    {
        Assert.That(
            unidad.IntentarIniciarOrden(
                TipoAccionJuego.Mover),
            Is.True);

        ResultadoAccion resultado =
            operacion.Ejecutar(
                partida,
                unidad.Id,
                new Coordenada(2, 1));

        Assert.That(
            resultado.Exito,
            Is.True);

        Assert.That(
            unidad.Coordenada.X,
            Is.EqualTo(2));

        Assert.That(
            unidad.Coordenada.Y,
            Is.EqualTo(1));

        Assert.That(
            unidad.Estado,
            Is.EqualTo(
                EstadoUnidad.Moviendo));
    }

    [Test]
    public void SinOrdenMovimiento_NoAvanza()
    {
        var origen = unidad.Coordenada;

        ResultadoAccion resultado =
            operacion.Ejecutar(
                partida,
                unidad.Id,
                new Coordenada(2, 1));

        Assert.That(
            resultado.Exito,
            Is.False);

        Assert.That(
            unidad.Coordenada,
            Is.SameAs(origen));
    }

    [Test]
    public void PasoDiagonal_NoAvanza()
    {
        unidad.IntentarIniciarOrden(
            TipoAccionJuego.Mover);

        var origen = unidad.Coordenada;

        ResultadoAccion resultado =
            operacion.Ejecutar(
                partida,
                unidad.Id,
                new Coordenada(2, 2));

        Assert.That(
            resultado.Exito,
            Is.False);

        Assert.That(
            unidad.Coordenada,
            Is.SameAs(origen));
    }

    [Test]
    public void PasoConUnidadAliada_Avanza()
    {
        unidad.IntentarIniciarOrden(
            TipoAccionJuego.Mover);

        partida.JugadorHumano.AgregarUnidad(
            new Guerrero(
                new Coordenada(2, 1)));

        ResultadoAccion resultado =
            operacion.Ejecutar(
                partida,
                unidad.Id,
                new Coordenada(2, 1));

        Assert.That(
            resultado.Exito,
            Is.True,
            resultado.Mensaje);

        Assert.That(
            unidad.Coordenada.X,
            Is.EqualTo(2));

        Assert.That(
            unidad.Coordenada.Y,
            Is.EqualTo(1));
    }

    [Test]
    public void PasoBloqueadoPorEntidad_NoAvanza()
    {
        unidad.IntentarIniciarOrden(
            TipoAccionJuego.Mover);

        partida.JugadorMaquina.AgregarUnidad(
            new Guerrero(
                new Coordenada(2, 1)));

        var origen = unidad.Coordenada;

        ResultadoAccion resultado =
            operacion.Ejecutar(
                partida,
                unidad.Id,
                new Coordenada(2, 1));

        Assert.That(
            resultado.Exito,
            Is.False);

        Assert.That(
            unidad.Coordenada,
            Is.SameAs(origen));
    }
}
