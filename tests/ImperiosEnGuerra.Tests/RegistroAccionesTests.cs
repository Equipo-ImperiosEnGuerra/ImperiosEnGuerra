using System;
using System.IO;
using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Api.Servicios;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;
using ImperiosEnGuerra.Servicios;

namespace ImperiosEnGuerra.Tests;

public class RegistroAccionesTests
{
    private string directorioTemporal;

    [SetUp]
    public void Preparar()
    {
        directorioTemporal = Path.Combine(
            Path.GetTempPath(),
            "ImperiosEnGuerraRegistro_" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(directorioTemporal);
    }

    [TearDown]
    public void Limpiar()
    {
        if (Directory.Exists(directorioTemporal))
            Directory.Delete(directorioTemporal, true);
    }

    [Test]
    public void Servicio_RegistraAccionesBaseYRechazo()
    {
        var archivos = new ServicioArchivos(directorioTemporal);
        var servicio = new EstadoPartidaService(archivos);
        Partida partida = CrearPartida(out Aldeano aldeano, out Guerrero guerrero, out Lancero enemigo);

        servicio.EstablecerPartida(partida);

        Assert.That(
            servicio.MoverUnidad(
                new MoverUnidadRequest
                {
                    UnidadId = guerrero.Id.ToString("D"),
                    Destino = Coordenada(3, 3)
                }).Exito,
            Is.True);

        Assert.That(
            servicio.IniciarRecoleccion(
                new RecolectarRequest
                {
                    AldeanoId = aldeano.Id.ToString("D"),
                    Objetivo = Coordenada(1, 0)
                }).Exito,
            Is.True);

        Assert.That(
            servicio.Construir(
                new ConstruirRequest
                {
                    AldeanoId = aldeano.Id.ToString("D"),
                    TipoEdificio = "CentroUrbano",
                    Destino = Coordenada(4, 4)
                }).Exito,
            Is.True);

        Assert.That(
            servicio.Entrenar(
                new EntrenarRequest
                {
                    EdificioOrigen = Coordenada(0, 0),
                    TipoUnidad = "Arquero",
                    Destino = Coordenada(5, 5)
                }).Exito,
            Is.True);

        Assert.That(
            servicio.Atacar(
                new AtacarRequest
                {
                    AtacanteId = guerrero.Id.ToString("D"),
                    ObjetivoId = enemigo.Id.ToString("D")
                }).Exito,
            Is.True);

        ResultadoAccion rechazo = servicio.Atacar(
            new AtacarRequest
            {
                AtacanteId = guerrero.Id.ToString("D"),
                ObjetivoId = aldeano.Id.ToString("D")
            });

        Assert.That(rechazo.Exito, Is.False);

        string log = File.ReadAllText(
            Path.Combine(directorioTemporal, "log_partida.txt"));

        Assert.That(log, Does.Contain("PARTIDA|EXITO|"));
        Assert.That(log, Does.Contain("MOVER|EXITO|"));
        Assert.That(log, Does.Contain("RECOLECTAR|EXITO|"));
        Assert.That(log, Does.Contain("CONSTRUIR|EXITO|"));
        Assert.That(log, Does.Contain("ENTRENAR|EXITO|"));
        Assert.That(log, Does.Contain("ATACAR|EXITO|"));
        Assert.That(log, Does.Contain("ATACAR|RECHAZADO|"));
        Assert.That(log, Does.Contain(rechazo.Mensaje));
    }

    [Test]
    public void ErrorDeIoEnLog_NoRompeLaPartidaNiLaAccion()
    {
        Directory.CreateDirectory(
            Path.Combine(directorioTemporal, "log_partida.txt"));

        var archivos = new ServicioArchivos(directorioTemporal);
        var servicio = new EstadoPartidaService(archivos);
        Partida partida = CrearPartida(out _, out Guerrero guerrero, out _);

        Assert.DoesNotThrow(
            () => servicio.EstablecerPartida(partida));

        ResultadoAccion resultado = null;

        Assert.DoesNotThrow(
            () =>
                resultado = servicio.MoverUnidad(
                    new MoverUnidadRequest
                    {
                        UnidadId = guerrero.Id.ToString("D"),
                        Destino = Coordenada(3, 3)
                    }));

        Assert.That(resultado, Is.Not.Null);
        Assert.That(resultado.Exito, Is.True);
    }

    private static Partida CrearPartida(
        out Aldeano aldeano,
        out Guerrero guerrero,
        out Lancero enemigo)
    {
        var mapa = new Mapa(10, 10);

        var humano = new Jugador(
            "Humano",
            TipoJugador.Humano,
            mapa,
            new RecursosJugador());

        var maquina = new Jugador(
            "Máquina",
            TipoJugador.Maquina,
            mapa,
            new RecursosJugador());

        var centroHumano = new CentroUrbano(new Coordenada(0, 0));
        var centroMaquina = new CentroUrbano(new Coordenada(9, 9));

        humano.AgregarEdificio(centroHumano);
        maquina.AgregarEdificio(centroMaquina);

        mapa.ObtenerCasilla(0, 0).Ocupar();
        mapa.ObtenerCasilla(9, 9).Ocupar();

        Assert.That(
            mapa.ColocarRecurso(
                new Recurso(
                    TipoRecurso.Oro,
                    new Coordenada(1, 0))),
            Is.True);

        aldeano = new Aldeano(new Coordenada(1, 1));
        guerrero = new Guerrero(new Coordenada(2, 1));
        enemigo = new Lancero(new Coordenada(8, 8));

        humano.AgregarUnidad(aldeano);
        humano.AgregarUnidad(guerrero);
        maquina.AgregarUnidad(enemigo);

        return new Partida(humano, maquina);
    }

    private static CoordenadaRequest Coordenada(int x, int y)
    {
        return new CoordenadaRequest
        {
            X = x,
            Y = y
        };
    }
}
