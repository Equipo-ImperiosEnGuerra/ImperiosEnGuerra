using System;
using System.IO;
using System.Linq;
using System.Threading;
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

        AtacarHastaEliminarUnidad(
            partida,
            atacante.Id,
            militarMaquina.Id,
            partida.JugadorMaquina);

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

        AtacarHastaEliminarEdificio(
            partida,
            atacante.Id,
            centroMaquina.Id,
            partida.JugadorMaquina);

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

        AtacarHastaEliminarUnidad(
            partida,
            atacante.Id,
            militarMaquina.Id,
            partida.JugadorMaquina);

        Assert.That(partida.Finalizada, Is.False);

        AtacarHastaEliminarEdificio(
            partida,
            atacante.Id,
            centroMaquina.Id,
            partida.JugadorMaquina);

        Assert.That(partida.Finalizada, Is.True);
        Assert.That(partida.Ganador, Is.SameAs(partida.JugadorHumano));
        Assert.That(
            partida.MotivoFinalizacion,
            Does.Not.Contain("AND"));

        Assert.That(
            partida.MotivoFinalizacion,
            Does.Contain("Centros Urbanos"));
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

            estado.EstablecerPartida(partida);

            AtacarHastaEliminarConServicio(
                estado,
                atacante.Id,
                militarMaquina.Id,
                () => partida.JugadorMaquina.Unidades.Any(u => u.Id == militarMaquina.Id));

            AtacarHastaEliminarConServicio(
                estado,
                atacante.Id,
                centroMaquina.Id,
                () => partida.JugadorMaquina.Edificios.Any(e => e.Id == centroMaquina.Id));

            string ruta =
                Path.Combine(
                    directorio,
                    "resultado_final.txt");

            Assert.That(File.Exists(ruta), Is.True);

            string contenido =
                File.ReadAllText(ruta);

            Assert.That(contenido, Does.Contain("ReglaVictoria=AND"));
            Assert.That(contenido, Does.Contain("GanadorTipo=Humano"));

            ResultadoAccion posterior =
                estado.MoverUnidad(
                    new MoverUnidadRequest
                    {
                        UnidadId = atacante.Id.ToString("D"),
                        Destino = new CoordenadaRequest
                        {
                            X = 3,
                            Y = 1
                        }
                    });

            Assert.That(posterior.Exito, Is.False);
            Assert.That(posterior.Mensaje, Does.Contain("finalizó"));
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
                    UnidadId = aldeano.Id.ToString("D"),
                    Destino = new CoordenadaRequest
                    {
                        X = 5,
                        Y = 5
                    }
                });

        Assert.That(
            SpinWait.SpinUntil(
                () => aldeano.OrdenActiva == TipoAccionJuego.Mover,
                TimeSpan.FromSeconds(1)),
            Is.True);

        AtacarHastaEliminarConServicio(
            estado,
            atacante.Id,
            militarMaquina.Id,
            () => partida.JugadorMaquina.Unidades.Any(u => u.Id == militarMaquina.Id));

        AtacarHastaEliminarConServicio(
            estado,
            atacante.Id,
            centroMaquina.Id,
            () => partida.JugadorMaquina.Edificios.Any(e => e.Id == centroMaquina.Id));

        await movimiento.Finalizacion;

        Assert.That(
            acciones.IntentarObtenerResultado(
                movimiento.Id,
                out ResultadoProcesoConcurrente resultado),
            Is.True);

        Assert.That(
            resultado.Estado,
            Is.EqualTo(EstadoProcesoConcurrente.Cancelado));
    }

    [Test]
    public void ObtenerEstado_ReevaluaDerrotaSiCondicionYaSeCumplio()
    {
        var mapa =
            new Mapa(
                6,
                6);

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

        // Un Aldeano no cuenta como unidad militar para la condición final.
        humano.AgregarUnidad(
            new Aldeano(
                new Coordenada(
                    1,
                    1)));

        var centroHumano =
            new CentroUrbano(
                new Coordenada(
                    0,
                    0));

        var centroMaquina =
            new CentroUrbano(
                new Coordenada(
                    5,
                    5));

        humano.AgregarEdificio(
            centroHumano);

        maquina.AgregarEdificio(
            centroMaquina);

        mapa.ObtenerCasilla(
                0,
                0)
            .Ocupar();

        mapa.ObtenerCasilla(
                5,
                5)
            .Ocupar();

        var partida =
            new Partida(
                humano,
                maquina);

        var estado =
            new EstadoPartidaService();

        estado.EstablecerPartida(
            partida);

        // Simula una condición terminal que ya ocurrió en una partida real,
        // pero cuya notificación de combate no llegó al cliente.
        humano.EliminarEdificio(
            centroHumano);

        mapa.ObtenerCasilla(
                0,
                0)
            .Liberar();

        EstadoPartidaResponse respuesta =
            estado.ObtenerEstado();

        Assert.That(
            partida.Finalizada,
            Is.True);

        Assert.That(
            partida.Ganador,
            Is.SameAs(
                maquina));

        Assert.That(
            respuesta.Estado,
            Is.EqualTo(
                "finalizada"));
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

        maquina.AgregarUnidad(atacante);
        humano.AgregarUnidad(militarHumano);
        humano.AgregarEdificio(centroHumano);

        mapa.ObtenerCasilla(1, 2).Ocupar();

        var partida =
            new Partida(
                humano,
                maquina);

        AtacarHastaEliminarUnidad(
            partida,
            atacante.Id,
            militarHumano.Id,
            humano);

        AtacarHastaEliminarEdificio(
            partida,
            atacante.Id,
            centroHumano.Id,
            humano);

        Assert.That(partida.Finalizada, Is.True);
        Assert.That(partida.Ganador, Is.SameAs(maquina));
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

        humano.AgregarUnidad(atacante);
        maquina.AgregarUnidad(militarMaquina);
        maquina.AgregarEdificio(centroMaquina);

        mapa.ObtenerCasilla(1, 2).Ocupar();

        return new Partida(
            humano,
            maquina);
    }

    private static void AtacarHastaEliminarUnidad(
        Partida partida,
        Guid atacanteId,
        Guid objetivoId,
        Jugador propietarioObjetivo)
    {
        var operacion =
            new OperacionAtaque();

        int seguridad = 0;

        while (propietarioObjetivo.Unidades.Any(u => u.Id == objetivoId) &&
               seguridad++ < 30)
        {
            ResultadoAccion resultado =
                operacion.Ejecutar(
                    partida,
                    new SolicitudAtaque(
                        atacanteId,
                        objetivoId));

            Assert.That(resultado.Exito, Is.True, resultado.Mensaje);
        }

        Assert.That(seguridad, Is.LessThan(30));
    }

    private static void AtacarHastaEliminarEdificio(
        Partida partida,
        Guid atacanteId,
        Guid objetivoId,
        Jugador propietarioObjetivo)
    {
        var operacion =
            new OperacionAtaque();

        int seguridad = 0;

        while (propietarioObjetivo.Edificios.Any(e => e.Id == objetivoId) &&
               seguridad++ < 30)
        {
            ResultadoAccion resultado =
                operacion.Ejecutar(
                    partida,
                    new SolicitudAtaque(
                        atacanteId,
                        objetivoId));

            Assert.That(resultado.Exito, Is.True, resultado.Mensaje);
        }

        Assert.That(seguridad, Is.LessThan(30));
    }

    private static void AtacarHastaEliminarConServicio(
        EstadoPartidaService estado,
        Guid atacanteId,
        Guid objetivoId,
        Func<bool> sigueExistiendo)
    {
        int seguridad = 0;

        while (sigueExistiendo() &&
               seguridad++ < 30)
        {
            ResultadoAccion resultado =
                estado.Atacar(
                    Ataque(
                        atacanteId,
                        objetivoId));

            Assert.That(resultado.Exito, Is.True, resultado.Mensaje);
        }

        Assert.That(seguridad, Is.LessThan(30));
    }

    private static AtacarRequest Ataque(
        Guid atacanteId,
        Guid objetivoId)
    {
        return new AtacarRequest
        {
            AtacanteId = atacanteId.ToString("D"),
            ObjetivoId = objetivoId.ToString("D")
        };
    }
}
