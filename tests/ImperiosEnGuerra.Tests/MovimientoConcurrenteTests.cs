using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Api.Servicios;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;
using ImperiosEnGuerra.Servicios.Concurrencia;

namespace ImperiosEnGuerra.Tests;

public class MovimientoConcurrenteTests
{
    [Test]
    public async Task MovimientoConcurrente_EjecutaEnWorkerYPublicaResultado()
    {
        Partida partida = CrearPartida(out Unidad unidad);
        var estado = new EstadoPartidaService();
        estado.EstablecerPartida(partida);

        using var gestor = new GestorProcesosConcurrentes();

        var servicio = new ServicioAccionesConcurrentes(
            estado,
            gestor,
            TimeSpan.Zero);

        ProcesoConcurrente proceso = servicio.IniciarMovimiento(
            CrearRequest(unidad, 4, 5));

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
        Assert.That(unidad.Coordenada.X, Is.EqualTo(4));
        Assert.That(unidad.Coordenada.Y, Is.EqualTo(5));
    }

    [Test]
    public async Task CancelarMovimiento_AntesDeAplicar_NoModificaModelo()
    {
        Partida partida = CrearPartida(out Unidad unidad);
        Coordenada origen = unidad.Coordenada;

        var estado = new EstadoPartidaService();
        estado.EstablecerPartida(partida);

        using var gestor = new GestorProcesosConcurrentes();

        var servicio = new ServicioAccionesConcurrentes(
            estado,
            gestor,
            TimeSpan.FromSeconds(10));

        ProcesoConcurrente proceso = servicio.IniciarMovimiento(
            CrearRequest(unidad, 4, 5));

        Assert.That(servicio.Cancelar(proceso.Id), Is.True);

        await proceso.Finalizacion;

        Assert.That(
            servicio.IntentarObtenerResultado(
                out ResultadoProcesoConcurrente resultado),
            Is.True);

        Assert.That(
            resultado.Estado,
            Is.EqualTo(EstadoProcesoConcurrente.Cancelado));

        Assert.That(unidad.Coordenada, Is.SameAs(origen));
    }

    [Test]
    public async Task SolicitudInvalida_SeCompletaConResultadoLogicoRechazado()
    {
        Partida partida = CrearPartida(out Unidad unidad);

        var estado = new EstadoPartidaService();
        estado.EstablecerPartida(partida);

        using var gestor = new GestorProcesosConcurrentes();

        var servicio = new ServicioAccionesConcurrentes(
            estado,
            gestor,
            TimeSpan.Zero);

        MoverUnidadRequest request = CrearRequest(unidad, 4, 5);
        request.UnidadId = "id-invalido";

        ProcesoConcurrente proceso =
            servicio.IniciarMovimiento(request);

        await proceso.Finalizacion;

        Assert.That(
            servicio.IntentarObtenerResultado(
                out ResultadoProcesoConcurrente resultado),
            Is.True);

        Assert.That(
            resultado.Estado,
            Is.EqualTo(EstadoProcesoConcurrente.Completado));

        Assert.That(resultado.Resultado, Is.Not.Null);
        Assert.That(resultado.Resultado.Exito, Is.False);
        Assert.That(resultado.Resultado.Mensaje, Does.Contain("Guid"));
    }

    private static Partida CrearPartida(
        out Unidad unidad)
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

        unidad = new Aldeano(new Coordenada(1, 1));
        humano.AgregarUnidad(unidad);

        return new Partida(humano, maquina);
    }

    private static MoverUnidadRequest CrearRequest(
        Unidad unidad,
        int x,
        int y)
    {
        return new MoverUnidadRequest
        {
            UnidadId = unidad.Id.ToString("D"),
            Destino = new CoordenadaRequest
            {
                X = x,
                Y = y
            }
        };
    }
}
