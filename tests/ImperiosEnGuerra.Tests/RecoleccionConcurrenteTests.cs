using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Api.Servicios;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;
using ImperiosEnGuerra.Servicios.Concurrencia;

namespace ImperiosEnGuerra.Tests;

public class RecoleccionConcurrenteTests
{
    [Test]
    public async Task RecoleccionConcurrente_PublicaResultadoDesdeWorker()
    {
        Partida partida = CrearPartida(out Aldeano aldeano);

        var estado = new EstadoPartidaService();
        estado.EstablecerPartida(partida);

        using var gestor = new GestorProcesosConcurrentes();

        var servicio = new ServicioAccionesConcurrentes(
            estado,
            gestor,
            TimeSpan.Zero);

        ProcesoConcurrente proceso =
            servicio.IniciarRecoleccion(
                CrearRequest(aldeano, 2, 2));

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
            resultado.Resultado.Mensaje,
            Does.Contain("Oro"));

        Assert.That(
            Math.Abs(
                aldeano.Coordenada.X - 2) +
            Math.Abs(
                aldeano.Coordenada.Y - 2),
            Is.EqualTo(1));

        Assert.That(
            aldeano.Estado,
            Is.EqualTo(
                EstadoUnidad.Idle));

        Assert.That(
            aldeano.OrdenActiva,
            Is.Null);
    }

    [Test]
    public async Task CancelarRecoleccion_AntesDeAplicar_DevuelveCancelado()
    {
        Partida partida = CrearPartida(out Aldeano aldeano);

        var estado = new EstadoPartidaService();
        estado.EstablecerPartida(partida);

        using var gestor = new GestorProcesosConcurrentes();

        var servicio = new ServicioAccionesConcurrentes(
            estado,
            gestor,
            TimeSpan.FromSeconds(10));

        ProcesoConcurrente proceso =
            servicio.IniciarRecoleccion(
                CrearRequest(aldeano, 2, 2));

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
    }

    [Test]
    public async Task RecoleccionInvalida_ConservaRechazoDelModelo()
    {
        Partida partida = CrearPartida(out Aldeano aldeano);

        var estado = new EstadoPartidaService();
        estado.EstablecerPartida(partida);

        using var gestor = new GestorProcesosConcurrentes();

        var servicio = new ServicioAccionesConcurrentes(
            estado,
            gestor,
            TimeSpan.Zero);

        ProcesoConcurrente proceso =
            servicio.IniciarRecoleccion(
                CrearRequest(aldeano, 5, 5));

        await proceso.Finalizacion;

        Assert.That(
            servicio.IntentarObtenerResultado(
                out ResultadoProcesoConcurrente resultado),
            Is.True);

        Assert.That(resultado.Resultado, Is.Not.Null);
        Assert.That(resultado.Resultado.Exito, Is.False);
        Assert.That(
            resultado.Resultado.Mensaje,
            Does.Contain("No existe un recurso"));
    }

    private static Partida CrearPartida(
        out Aldeano aldeano)
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

        aldeano = new Aldeano(
            new Coordenada(1, 1));

        humano.AgregarUnidad(aldeano);

        Assert.That(
            mapa.ColocarRecurso(
                new Recurso(
                    TipoRecurso.Oro,
                    new Coordenada(2, 2))),
            Is.True);

        return new Partida(humano, maquina);
    }

    private static RecolectarRequest CrearRequest(
        Aldeano aldeano,
        int x,
        int y)
    {
        return new RecolectarRequest
        {
            AldeanoId = aldeano.Id.ToString("D"),
            Objetivo = new CoordenadaRequest
            {
                X = x,
                Y = y
            }
        };
    }
}
