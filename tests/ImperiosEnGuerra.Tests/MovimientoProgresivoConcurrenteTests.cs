using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Api.Servicios;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;
using ImperiosEnGuerra.Servicios.Concurrencia;

namespace ImperiosEnGuerra.Tests;

public class MovimientoProgresivoConcurrenteTests
{
    [Test]
    public async Task MovimientoConcurrente_AvanzaPorPasosYMantieneEstadoMoviendo()
    {
        Partida partida =
            CrearPartida(
                out Aldeano unidad);

        var estado =
            new EstadoPartidaService();

        estado.EstablecerPartida(
            partida);

        using var gestor =
            new GestorProcesosConcurrentes();

        var servicio =
            new ServicioAccionesConcurrentes(
                estado,
                gestor,
                TimeSpan.FromMilliseconds(120));

        ProcesoConcurrente proceso =
            servicio.IniciarMovimiento(
                CrearRequest(
                    unidad,
                    3,
                    1));

        await EsperarHasta(
            () =>
                unidad.Estado ==
                EstadoUnidad.Moviendo,
            1500);

        Assert.That(
            unidad.Coordenada.X,
            Is.EqualTo(1));

        await EsperarHasta(
            () =>
                unidad.Coordenada.X >= 2,
            2000);

        Assert.That(
            unidad.Estado,
            Is.EqualTo(
                EstadoUnidad.Moviendo));

        Assert.That(
            unidad.OrdenActiva,
            Is.EqualTo(
                TipoAccionJuego.Mover));

        await proceso.Finalizacion;

        Assert.That(
            servicio.IntentarObtenerResultado(
                proceso.Id,
                out ResultadoProcesoConcurrente resultado),
            Is.True);

        Assert.That(
            resultado.Estado,
            Is.EqualTo(
                EstadoProcesoConcurrente.Completado));

        Assert.That(
            resultado.Resultado.Exito,
            Is.True);

        Assert.That(
            unidad.Coordenada.X,
            Is.EqualTo(3));

        Assert.That(
            unidad.Coordenada.Y,
            Is.EqualTo(1));

        Assert.That(
            unidad.Estado,
            Is.EqualTo(
                EstadoUnidad.Idle));

        Assert.That(
            unidad.OrdenActiva,
            Is.Null);
    }

    [Test]
    public async Task CancelarDespuesDelPrimerPaso_DetieneMovimientoYVuelveAIdle()
    {
        Partida partida =
            CrearPartida(
                out Aldeano unidad);

        var estado =
            new EstadoPartidaService();

        estado.EstablecerPartida(
            partida);

        using var gestor =
            new GestorProcesosConcurrentes();

        var servicio =
            new ServicioAccionesConcurrentes(
                estado,
                gestor,
                TimeSpan.FromMilliseconds(150));

        ProcesoConcurrente proceso =
            servicio.IniciarMovimiento(
                CrearRequest(
                    unidad,
                    4,
                    1));

        await EsperarHasta(
            () =>
                unidad.Coordenada.X >= 2,
            2000);

        int xAlCancelar =
            unidad.Coordenada.X;

        Assert.That(
            servicio.Cancelar(
                proceso.Id),
            Is.True);

        await proceso.Finalizacion;

        Assert.That(
            servicio.IntentarObtenerResultado(
                proceso.Id,
                out ResultadoProcesoConcurrente resultado),
            Is.True);

        Assert.That(
            resultado.Estado,
            Is.EqualTo(
                EstadoProcesoConcurrente.Cancelado));

        Assert.That(
            unidad.Coordenada.X,
            Is.EqualTo(xAlCancelar));

        Assert.That(
            unidad.Coordenada.X,
            Is.LessThan(4));

        Assert.That(
            unidad.Estado,
            Is.EqualTo(
                EstadoUnidad.Idle));

        Assert.That(
            unidad.OrdenActiva,
            Is.Null);
    }

    private static async Task EsperarHasta(
        Func<bool> condicion,
        int tiempoMaximoMs)
    {
        int transcurrido = 0;

        while (transcurrido < tiempoMaximoMs)
        {
            if (condicion())
                return;

            await Task.Delay(10);
            transcurrido += 10;
        }

        Assert.Fail(
            "La condición esperada no ocurrió dentro del tiempo máximo.");
    }

    private static Partida CrearPartida(
        out Aldeano unidad)
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
                "Máquina",
                TipoJugador.Maquina,
                mapa,
                new RecursosJugador());

        unidad =
            new Aldeano(
                new Coordenada(1, 1));

        humano.AgregarUnidad(
            unidad);

        return new Partida(
            humano,
            maquina);
    }

    private static MoverUnidadRequest CrearRequest(
        Unidad unidad,
        int x,
        int y)
    {
        return new MoverUnidadRequest
        {
            UnidadId =
                unidad.Id.ToString("D"),

            Destino =
                new CoordenadaRequest
                {
                    X = x,
                    Y = y
                }
        };
    }
}
