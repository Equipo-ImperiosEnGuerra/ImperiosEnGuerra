using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Api.Servicios;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;
using ImperiosEnGuerra.Servicios.Concurrencia;

namespace ImperiosEnGuerra.Tests;

public class AtaqueConcurrenteTests
{
    [Test]
    public async Task AtaqueConcurrente_EjecutaEnWorkerYPublicaResultado()
    {
        Partida partida = CrearPartida(
            out Guerrero atacante,
            out Lancero objetivo);

        var estado = new EstadoPartidaService();
        estado.EstablecerPartida(partida);

        using var gestor = new GestorProcesosConcurrentes();

        var servicio = new ServicioAccionesConcurrentes(
            estado,
            gestor,
            TimeSpan.Zero);

        ProcesoConcurrente proceso =
            servicio.IniciarAtaque(
                new AtacarRequest
                {
                    AtacanteId = atacante.Id.ToString(),
                    ObjetivoId = objetivo.Id.ToString()
                });

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
        Assert.That(resultado.Resultado.Mensaje,
            Does.Contain("pendiente"));
    }


    [Test]
    public async Task CancelarAtaque_AntesDeAplicar_NoModificaModelo()
    {
        Partida partida = CrearPartida(
            out Guerrero atacante,
            out Lancero objetivo);

        var estado = new EstadoPartidaService();
        estado.EstablecerPartida(partida);

        using var gestor = new GestorProcesosConcurrentes();

        var servicio = new ServicioAccionesConcurrentes(
            estado,
            gestor,
            TimeSpan.FromSeconds(10));


        ProcesoConcurrente proceso =
            servicio.IniciarAtaque(
                new AtacarRequest
                {
                    AtacanteId = atacante.Id.ToString(),
                    ObjetivoId = objetivo.Id.ToString()
                });


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


        Assert.That(objetivo.Coordenada.X, Is.EqualTo(4));
        Assert.That(objetivo.Coordenada.Y, Is.EqualTo(4));
    }


    private static Partida CrearPartida(
        out Guerrero atacante,
        out Lancero objetivo)
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


        atacante =
            new Guerrero(
                new Coordenada(1, 1));


        objetivo =
            new Lancero(
                new Coordenada(4, 4));


        humano.AgregarUnidad(atacante);
        maquina.AgregarUnidad(objetivo);


        return new Partida(
            humano,
            maquina);
    }
}