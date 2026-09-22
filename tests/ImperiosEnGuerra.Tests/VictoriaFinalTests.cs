using System;
using System.IO;
using System.Threading.Tasks;
using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Api.Servicios;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;
using ImperiosEnGuerra.Servicios;
using ImperiosEnGuerra.Servicios.Concurrencia;
using NUnit.Framework;

namespace ImperiosEnGuerra.Tests;

public class VictoriaFinalTests
{
    [Test]
    public void ReglaAnd_SinMilitaresPeroConCentro_NoFinaliza()
    {
        Partida partida =
            CrearEscenarioHumanoAtaca(
                out Guerrero atacante,
                out Guerrero militarMaquina,
                out _);

        ResultadoAccion resultado =
            new OperacionAtaque().Ejecutar(
                partida,
                new SolicitudAtaque(
                    atacante.Id,
                    militarMaquina.Id));

        Assert.That(resultado.Exito, Is.True, resultado.Mensaje);
        Assert.That(partida.Finalizada, Is.False);
        Assert.That(partida.Ganador, Is.Null);
    }

    [Test]
    public void ReglaAnd_SinCentroPeroConMilitar_NoFinaliza()
    {
        Partida partida =
            CrearEscenarioHumanoAtaca(
                out Guerrero atacante,
                out _,
                out CentroUrbano centroMaquina);

        ResultadoAccion resultado =
            new OperacionAtaque().Ejecutar(
                partida,
                new SolicitudAtaque(
                    atacante.Id,
                    centroMaquina.Id));

        Assert.That(resultado.Exito, Is.True, resultado.Mensaje);
        Assert.That(partida.Finalizada, Is.False);
        Assert.That(partida.Ganador, Is.Null);
    }

    [Test]
    public void ReglaAnd_SinCentroYSinMilitares_DeclaraGanadorHumano()
    {
        Partida partida =
            CrearEscenarioHumanoAtaca(
                out Guerrero atacante,
                out Guerrero militarMaquina,
                out CentroUrbano centroMaquina);

        ResultadoAccion primerImpacto =
            new OperacionAtaque().Ejecutar(
                partida,
                new SolicitudAtaque(
                    atacante.Id,
                    militarMaquina.Id));

        Assert.That(primerImpacto.Exito, Is.True);
        Assert.That(partida.Finalizada, Is.False);

        ResultadoAccion segundoImpacto =
            new OperacionAtaque().Ejecutar(
                partida,
                new SolicitudAtaque(
                    atacante.Id,
                    centroMaquina.Id));

        Assert.That(segundoImpacto.Exito, Is.True, segundoImpacto.Mensaje);
        Assert.That(partida.Finalizada, Is.True);
        Assert.That(partida.Ganador, Is.SameAs(partida.JugadorHumano));
        Assert.That(partida.MotivoFinalizacion, Does.Contain("Regla AND"));
    }

    [Test]
    public void EstadoPartida_Finaliza_GuardaResultadoYRechazaNuevasOrdenes()
    {
        string directorio =
            Path.Combine(
                Path.GetTempPath(),
                "ImperiosEnGuerraVictoria_" +
                Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(directorio);

        try
        {
            var archivos =
                new ServicioArchivos(
                    directorio);

            var estado =
                new EstadoPartidaService(
                    archivos);

            Partida partida =
                CrearEscenarioHumanoAtaca(
                    out Guerrero atacante,
                    out Guerrero militarMaquina,
                    out CentroUrbano centroMaquina);

            estado.EstablecerPartida(
                partida);

            Assert.That(
                estado.Atacar(
                    Ataque(
                        atacante.Id,
                        militarMaquina.Id)).Exito,
                Is.True);

            Assert.That(
                estado.Atacar(
                    Ataque(
                        atacante.Id,
                        centroMaquina.Id)).Exito,
                Is.True);

            string ruta =
                Path.Combine(
                    directorio,
                    "resultado_final.txt");

            Assert.That(
                File.Exists(ruta),
                Is.True);

            string contenido =
                File.ReadAllText(
                    ruta);

            Assert.That(
                contenido,
                Does.Contain("ReglaVictoria=AND"));

            Assert.That(
                contenido,
                Does.Contain("GanadorTipo=Humano"));

            Assert.That(
                contenido,
                Does.Contain("GanadorNombre=Humano"));

            ResultadoAccion posterior =
                estado.MoverUnidad(
                    new MoverUnidadRequest
                    {
                        UnidadId =
                            atacante.Id.ToString("D"),

                        Destino =
                            new CoordenadaRequest
                            {
                                X = 3,
                                Y = 1
                            }
                    });

            Assert.That(
                posterior.Exito,
                Is.False);

            Assert.That(
                posterior.Mensaje,
                Does.Contain("finalizó"));
        }
        finally
        {
            if (Directory.Exists(directorio))
                Directory.Delete(directorio, true);
        }
    }

    [Test]
    public async Task Finalizacion_CancelaWorkerConcurrentePendiente()
    {
        Partida partida =
            CrearEscenarioHumanoAtaca(
                out Guerrero atacante,
                out Guerrero militarMaquina,
                out CentroUrbano centroMaquina);

        var aldeano =
            new Aldeano(
                new Coordenada(0, 1));

        partida.JugadorHumano.AgregarUnidad(
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
                TimeSpan.FromSeconds(10));

        ProcesoConcurrente movimiento =
            acciones.IniciarMovimiento(
                new MoverUnidadRequest
                {
                    UnidadId =
                        aldeano.Id.ToString("D"),

                    Destino =
                        new CoordenadaRequest
                        {
                            X = 5,
                            Y = 5
                        }
                });

        Assert.That(
            estado.Atacar(
                Ataque(
                    atacante.Id,
                    militarMaquina.Id)).Exito,
            Is.True);

        Assert.That(
            estado.Atacar(
                Ataque(
                    atacante.Id,
                    centroMaquina.Id)).Exito,
            Is.True);

        await movimiento.Finalizacion;

        Assert.That(
            acciones.IntentarObtenerResultado(
                movimiento.Id,
                out ResultadoProcesoConcurrente resultado),
            Is.True);

        Assert.That(
            resultado.Estado,
            Is.EqualTo(
                EstadoProcesoConcurrente.Cancelado));
    }

    [Test]
    public void ReglaAnd_PuedeDeclararGanadoraALaMaquina()
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

        var atacante =
            new Guerrero(
                new Coordenada(1, 1));

        var militarHumano =
            new Guerrero(
                new Coordenada(2, 1));

        var centroHumano =
            new CentroUrbano(
                new Coordenada(1, 2));

        maquina.AgregarUnidad(
            atacante);

        humano.AgregarUnidad(
            militarHumano);

        humano.AgregarEdificio(
            centroHumano);

        mapa.ObtenerCasilla(1, 2).Ocupar();

        var partida =
            new Partida(
                humano,
                maquina);

        var operacion =
            new OperacionAtaque();

        Assert.That(
            operacion.Ejecutar(
                partida,
                new SolicitudAtaque(
                    atacante.Id,
                    militarHumano.Id)).Exito,
            Is.True);

        Assert.That(
            operacion.Ejecutar(
                partida,
                new SolicitudAtaque(
                    atacante.Id,
                    centroHumano.Id)).Exito,
            Is.True);

        Assert.That(
            partida.Finalizada,
            Is.True);

        Assert.That(
            partida.Ganador,
            Is.SameAs(
                maquina));
    }

    private static Partida CrearEscenarioHumanoAtaca(
        out Guerrero atacante,
        out Guerrero militarMaquina,
        out CentroUrbano centroMaquina)
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

        atacante =
            new Guerrero(
                new Coordenada(1, 1));

        militarMaquina =
            new Guerrero(
                new Coordenada(2, 1));

        centroMaquina =
            new CentroUrbano(
                new Coordenada(1, 2));

        humano.AgregarUnidad(
            atacante);

        maquina.AgregarUnidad(
            militarMaquina);

        maquina.AgregarEdificio(
            centroMaquina);

        mapa.ObtenerCasilla(1, 2).Ocupar();

        return new Partida(
            humano,
            maquina);
    }

    private static AtacarRequest Ataque(
        Guid atacanteId,
        Guid objetivoId)
    {
        return new AtacarRequest
        {
            AtacanteId =
                atacanteId.ToString("D"),

            ObjetivoId =
                objetivoId.ToString("D")
        };
    }
}
