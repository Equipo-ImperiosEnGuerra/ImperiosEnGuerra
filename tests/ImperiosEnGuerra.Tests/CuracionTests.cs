using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Api.Servicios;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;
using ImperiosEnGuerra.Servicios.Concurrencia;

namespace ImperiosEnGuerra.Tests;

public class CuracionTests
{
    private Partida partida;
    private Monje monje;
    private Guerrero aliado;
    private Guerrero enemigo;
    private OperacionCuracion operacion;

    [SetUp]
    public void Preparar()
    {
        var mapa =
            new Mapa(8, 8);

        partida =
            new Partida(
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

        monje =
            new Monje(
                new Coordenada(1, 1));

        aliado =
            new Guerrero(
                new Coordenada(2, 1));

        enemigo =
            new Guerrero(
                new Coordenada(2, 2));

        partida.JugadorHumano.AgregarUnidad(
            monje);

        partida.JugadorHumano.AgregarUnidad(
            aliado);

        partida.JugadorMaquina.AgregarUnidad(
            enemigo);

        operacion =
            new OperacionCuracion();
    }

    [Test]
    public void Monje_NoTieneAtaqueOfensivo()
    {
        Assert.That(
            monje.DanioAtaque,
            Is.Zero);

        Assert.That(
            monje.AlcanceAtaque,
            Is.Zero);

        ResultadoAccion resultado =
            new OperacionAtaque()
                .Ejecutar(
                    partida,
                    new SolicitudAtaque(
                        monje.Id,
                        enemigo.Id));

        Assert.That(
            resultado.Exito,
            Is.False);
    }

    [Test]
    public void Curar_AliadoDanado_RecuperaVidaSinSuperarMaximo()
    {
        aliado.RecibirDanio(40);

        ResultadoAccion primero =
            operacion.Ejecutar(
                partida,
                new SolicitudCuracion(
                    monje.Id,
                    aliado.Id));

        Assert.That(
            primero.Exito,
            Is.True,
            primero.Mensaje);

        Assert.That(
            aliado.VidaActual,
            Is.EqualTo(95));

        operacion.Ejecutar(
            partida,
            new SolicitudCuracion(
                monje.Id,
                aliado.Id));

        operacion.Ejecutar(
            partida,
            new SolicitudCuracion(
                monje.Id,
                aliado.Id));

        Assert.That(
            aliado.VidaActual,
            Is.EqualTo(aliado.VidaMaxima));
    }

    [Test]
    public void Curar_Enemigo_SeRechaza()
    {
        enemigo.RecibirDanio(30);

        ResultadoAccion resultado =
            operacion.Ejecutar(
                partida,
                new SolicitudCuracion(
                    monje.Id,
                    enemigo.Id));

        Assert.That(
            resultado.Exito,
            Is.False);

        Assert.That(
            resultado.Mensaje,
            Does.Contain("aliada"));
    }

    [Test]
    public void Curar_UnidadConVidaCompleta_SeRechaza()
    {
        ResultadoAccion resultado =
            operacion.Ejecutar(
                partida,
                new SolicitudCuracion(
                    monje.Id,
                    aliado.Id));

        Assert.That(
            resultado.Exito,
            Is.False);

        Assert.That(
            resultado.Mensaje,
            Does.Contain("vida completa"));
    }

    [Test]
    public void Curar_FueraDeAlcance_SeRechaza()
    {
        var lejano =
            new Arquero(
                new Coordenada(6, 6));

        partida.JugadorHumano.AgregarUnidad(
            lejano);

        lejano.RecibirDanio(20);

        ResultadoAccion resultado =
            operacion.Ejecutar(
                partida,
                new SolicitudCuracion(
                    monje.Id,
                    lejano.Id));

        Assert.That(
            resultado.Exito,
            Is.False);

        Assert.That(
            resultado.Mensaje,
            Does.Contain("fuera de alcance"));
    }

    [Test]
    public async Task CuracionConcurrente_RepiteHastaVidaCompleta()
    {
        aliado.RecibirDanio(30);

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
                TimeSpan.Zero);

        var proceso =
            acciones.IniciarCuracion(
                new CurarRequest
                {
                    CuradorId =
                        monje.Id.ToString("D"),
                    ObjetivoId =
                        aliado.Id.ToString("D")
                });

        DateTime limite =
            DateTime.UtcNow.AddSeconds(2);

        ResultadoProcesoConcurrente resultado = null;

        while (DateTime.UtcNow < limite)
        {
            if (acciones.IntentarObtenerResultado(
                    proceso.Id,
                    out ResultadoProcesoConcurrente publicado))
            {
                resultado = publicado;
                break;
            }

            await Task.Delay(5);
        }

        Assert.That(
            resultado,
            Is.Not.Null);

        Assert.That(
            resultado!.Resultado?.Exito,
            Is.True,
            resultado.Resultado?.Mensaje);

        Assert.That(
            aliado.VidaActual,
            Is.EqualTo(aliado.VidaMaxima));

        Assert.That(
            monje.OrdenActiva,
            Is.Null);

        Assert.That(
            monje.Disponible,
            Is.True);
    }
}
