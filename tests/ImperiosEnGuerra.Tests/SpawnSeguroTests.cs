using System;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using NUnit.Framework;

namespace ImperiosEnGuerra.Tests;

public class SpawnSeguroTests
{
    [Test]
    public void SpawnJuntoACentro_EvitaBordeCuandoExisteAlternativaInterior()
    {
        var mapa =
            new Mapa(6, 6);

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
                new Coordenada(1, 1)));

        maquina.AgregarEdificio(
            new CentroUrbano(
                new Coordenada(5, 5)));

        mapa.ObtenerCasilla(1, 1).Ocupar();
        mapa.ObtenerCasilla(5, 5).Ocupar();

        var partida =
            new Partida(
                humano,
                maquina);

        Coordenada spawn =
            new BuscadorCasillaSpawn()
                .Buscar(
                    partida,
                    TipoJugador.Humano,
                    new Coordenada(1, 1));

        Assert.That(spawn, Is.Not.Null);

        Assert.That(
            spawn.X,
            Is.GreaterThan(0));

        Assert.That(
            spawn.Y,
            Is.GreaterThan(0));

        Assert.That(
            spawn.X,
            Is.LessThan(mapa.Ancho - 1));

        Assert.That(
            spawn.Y,
            Is.LessThan(mapa.Alto - 1));
    }
}
