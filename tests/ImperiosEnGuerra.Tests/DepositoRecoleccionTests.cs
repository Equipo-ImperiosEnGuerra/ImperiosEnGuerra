using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Tests;

public class DepositoRecoleccionTests
{
    [Test]
    public void DepositoValido_VaciaCargaYAumentaSaldo()
    {
        var mapa = new Mapa(6, 6);
        var humano = new Jugador(
            "Humano",
            TipoJugador.Humano,
            mapa,
            new RecursosJugador());
        var maquina = new Jugador(
            "Maquina",
            TipoJugador.Maquina,
            mapa,
            new RecursosJugador());
        var partida = new Partida(humano, maquina);

        var aldeano =
            new Aldeano(new Coordenada(2, 1), 5);

        var centro =
            new CentroUrbano(new Coordenada(1, 1));

        var recurso =
            new Recurso(
                TipoRecurso.Madera,
                new Coordenada(3, 1),
                20);

        humano.AgregarUnidad(aldeano);
        humano.AgregarEdificio(centro);
        mapa.ColocarRecurso(recurso);

        aldeano.RecolectarDesde(recurso, 5);

        var resultado =
            new OperacionDepositoRecoleccion()
                .Ejecutar(
                    partida,
                    aldeano.Id,
                    centro.Coordenada);

        Assert.That(resultado.Exito, Is.True);
        Assert.That(resultado.CantidadDepositada, Is.EqualTo(5));
        Assert.That(aldeano.CargaActual, Is.Zero);
        Assert.That(aldeano.TipoCarga, Is.Null);
        Assert.That(
            humano.Recursos.ObtenerCantidad(TipoRecurso.Madera),
            Is.EqualTo(5));
    }

    [Test]
    public void DepositoLejano_NoVaciaCarga()
    {
        var mapa = new Mapa(6, 6);
        var humano = new Jugador(
            "Humano",
            TipoJugador.Humano,
            mapa,
            new RecursosJugador());
        var maquina = new Jugador(
            "Maquina",
            TipoJugador.Maquina,
            mapa,
            new RecursosJugador());
        var partida = new Partida(humano, maquina);

        var aldeano =
            new Aldeano(new Coordenada(5, 5), 5);
        var centro =
            new CentroUrbano(new Coordenada(1, 1));
        var recurso =
            new Recurso(
                TipoRecurso.Oro,
                new Coordenada(4, 5),
                20);

        humano.AgregarUnidad(aldeano);
        humano.AgregarEdificio(centro);
        mapa.ColocarRecurso(recurso);

        aldeano.RecolectarDesde(recurso, 5);

        var resultado =
            new OperacionDepositoRecoleccion()
                .Ejecutar(
                    partida,
                    aldeano.Id,
                    centro.Coordenada);

        Assert.That(resultado.Exito, Is.False);
        Assert.That(aldeano.CargaActual, Is.EqualTo(5));
        Assert.That(
            humano.Recursos.ObtenerCantidad(TipoRecurso.Oro),
            Is.Zero);
    }
}
