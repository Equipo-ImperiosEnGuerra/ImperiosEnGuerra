using System.Linq;
using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Api.Servicios;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;
using ImperiosEnGuerra.Servicios.Concurrencia;

namespace ImperiosEnGuerra.Tests;

public class ConstruccionConcurrenteTests
{
    [Test]
    public async Task ConstruccionConcurrente_CreaEdificioDesdeWorker()
    {
        Partida partida = CrearPartida(out Aldeano aldeano, out _);

        var estado = new EstadoPartidaService();
        estado.EstablecerPartida(partida);

        using var gestor = new GestorProcesosConcurrentes();

        var servicio = new ServicioAccionesConcurrentes(
            estado,
            gestor,
            TimeSpan.Zero);

        ProcesoConcurrente proceso =
            servicio.IniciarConstruccion(
                CrearRequest(aldeano, 3, 3));

        await proceso.Finalizacion;

        Assert.That(
            servicio.IntentarObtenerResultado(
                out ResultadoProcesoConcurrente resultado),
            Is.True);

        Assert.That(
            resultado.Estado,
            Is.EqualTo(EstadoProcesoConcurrente.Completado));

        Assert.That(resultado.Resultado, Is.Not.Null);
        Assert.That(resultado.Resultado.Exito, Is.True);

        Assert.That(
            partida.JugadorHumano.Edificios.Count,
            Is.EqualTo(1));

        Assert.That(
            partida.JugadorHumano.Mapa
                .ObtenerCasilla(3, 3)
                .EstaOcupada,
            Is.True);
    }

    [Test]
    public async Task CancelarConstruccion_AntesDeAplicar_NoOcupaCasilla()
    {
        Partida partida = CrearPartida(out Aldeano aldeano, out _);

        var estado = new EstadoPartidaService();
        estado.EstablecerPartida(partida);

        using var gestor = new GestorProcesosConcurrentes();

        var servicio = new ServicioAccionesConcurrentes(
            estado,
            gestor,
            TimeSpan.FromSeconds(10));

        ProcesoConcurrente proceso =
            servicio.IniciarConstruccion(
                CrearRequest(aldeano, 3, 3));

        Assert.That(
            servicio.Cancelar(proceso.Id),
            Is.True);

        await proceso.Finalizacion;

        Assert.That(
            servicio.IntentarObtenerResultado(
                out ResultadoProcesoConcurrente resultado),
            Is.True);

        Assert.That(
            resultado.Estado,
            Is.EqualTo(EstadoProcesoConcurrente.Cancelado));

        Assert.That(
            partida.JugadorHumano.Edificios,
            Is.Empty);

        Assert.That(
            partida.JugadorHumano.Mapa
                .ObtenerCasilla(3, 3)
                .EstaOcupada,
            Is.False);
    }

    [Test]
    public async Task DosConstrucciones_MismaCasilla_SoloUnaSeAplica()
    {
        Partida partida = CrearPartida(
            out Aldeano primero,
            out Aldeano segundo);

        var estado = new EstadoPartidaService();
        estado.EstablecerPartida(partida);

        using var gestor = new GestorProcesosConcurrentes();

        var servicio = new ServicioAccionesConcurrentes(
            estado,
            gestor,
            TimeSpan.FromMilliseconds(100));

        ProcesoConcurrente procesoA =
            servicio.IniciarConstruccion(
                CrearRequest(primero, 4, 4));

        ProcesoConcurrente procesoB =
            servicio.IniciarConstruccion(
                CrearRequest(segundo, 4, 4));

        Assert.That(
            gestor.ProcesosActivos,
            Is.EqualTo(2));

        await Task.WhenAll(
            procesoA.Finalizacion,
            procesoB.Finalizacion);

        var resultados =
            new List<ResultadoProcesoConcurrente>();

        while (servicio.IntentarObtenerResultado(
            out ResultadoProcesoConcurrente resultado))
        {
            resultados.Add(resultado);
        }

        Assert.That(resultados.Count, Is.EqualTo(2));

        Assert.That(
            resultados.Count(
                r => r.Resultado != null &&
                     r.Resultado.Exito),
            Is.EqualTo(1));

        Assert.That(
            resultados.Count(
                r => r.Resultado != null &&
                     !r.Resultado.Exito),
            Is.EqualTo(1));

        Assert.That(
            partida.JugadorHumano.Edificios.Count,
            Is.EqualTo(1));

        Assert.That(
            partida.JugadorHumano.Mapa
                .ObtenerCasilla(4, 4)
                .EstaOcupada,
            Is.True);
    }

    [Test]
    public async Task ConflictoConstruccion_Repetido_NoDuplicaEdificios()
    {
        const int repeticiones = 10;

        for (int intento = 0;
             intento < repeticiones;
             intento++)
        {
            Partida partida = CrearPartida(
                out Aldeano primero,
                out Aldeano segundo);

            var estado = new EstadoPartidaService();
            estado.EstablecerPartida(partida);

            using var gestor =
                new GestorProcesosConcurrentes();

            var servicio =
                new ServicioAccionesConcurrentes(
                    estado,
                    gestor,
                    TimeSpan.FromMilliseconds(5));

            ProcesoConcurrente procesoA =
                servicio.IniciarConstruccion(
                    CrearRequest(primero, 4, 4));

            ProcesoConcurrente procesoB =
                servicio.IniciarConstruccion(
                    CrearRequest(segundo, 4, 4));

            await Task.WhenAll(
                procesoA.Finalizacion,
                procesoB.Finalizacion);

            var resultados =
                new List<ResultadoProcesoConcurrente>();

            while (servicio.IntentarObtenerResultado(
                out ResultadoProcesoConcurrente resultado))
            {
                resultados.Add(resultado);
            }

            Assert.That(
                resultados.Count(
                    r => r.Resultado != null &&
                         r.Resultado.Exito),
                Is.EqualTo(1),
                $"Intento concurrente {intento + 1}");

            Assert.That(
                partida.JugadorHumano.Edificios.Count,
                Is.EqualTo(1),
                $"Intento concurrente {intento + 1}");
        }
    }

    private static Partida CrearPartida(
        out Aldeano primero,
        out Aldeano segundo)
    {
        var mapa = new Mapa(6, 6);

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

        humano.Recursos.Agregar(TipoRecurso.Oro, 500);
        humano.Recursos.Agregar(TipoRecurso.Madera, 500);
        humano.Recursos.Agregar(TipoRecurso.Comida, 500);

        primero = new Aldeano(
            new Coordenada(1, 1));

        segundo = new Aldeano(
            new Coordenada(1, 2));

        humano.AgregarUnidad(primero);
        humano.AgregarUnidad(segundo);

        return new Partida(humano, maquina);
    }

    private static ConstruirRequest CrearRequest(
        Aldeano aldeano,
        int x,
        int y)
    {
        return new ConstruirRequest
        {
            AldeanoId = aldeano.Id.ToString("D"),
            TipoEdificio = "CentroUrbano",
            Destino = new CoordenadaRequest
            {
                X = x,
                Y = y
            }
        };
    }
}
