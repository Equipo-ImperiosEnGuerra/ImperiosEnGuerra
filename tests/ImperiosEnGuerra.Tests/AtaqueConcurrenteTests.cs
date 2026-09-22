using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Api.Servicios;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;
using ImperiosEnGuerra.Servicios.Concurrencia;

namespace ImperiosEnGuerra.Tests;

public class AtaqueConcurrenteTests
{
    [Test]
    public async Task AtaqueConcurrente_AplicaUnImpactoYLiberaOrden()
    {
        Partida partida = CrearPartida(out Guerrero atacante, out Lancero objetivo);

        var estado = new EstadoPartidaService();
        estado.EstablecerPartida(partida);

        using var gestor = new GestorProcesosConcurrentes();
        var servicio = new ServicioAccionesConcurrentes(estado, gestor, TimeSpan.Zero);

        ProcesoConcurrente proceso = servicio.IniciarAtaque(
            new AtacarRequest
            {
                AtacanteId = atacante.Id.ToString(),
                ObjetivoId = objetivo.Id.ToString()
            });

        await proceso.Finalizacion;

        Assert.That(
            servicio.IntentarObtenerResultado(proceso.Id, out ResultadoProcesoConcurrente resultado),
            Is.True);

        Assert.That(resultado.Estado, Is.EqualTo(EstadoProcesoConcurrente.Completado));
        Assert.That(resultado.Resultado?.Exito, Is.True, resultado.Resultado?.Mensaje);
        Assert.That(objetivo.VidaActual, Is.EqualTo(70));
        Assert.That(partida.JugadorMaquina.Unidades, Does.Contain(objetivo));
        Assert.That(atacante.Disponible, Is.True);
    }

    [Test]
    public async Task AtaqueConcurrente_FueraDeAlcance_SeAproximaYAplicaImpacto()
    {
        var mapa = new Mapa(8, 8);

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

        var atacante =
            new Guerrero(
                new Coordenada(1, 1));

        var centro =
            new CentroUrbano(
                new Coordenada(5, 1));

        humano.AgregarUnidad(atacante);
        maquina.AgregarEdificio(centro);
        mapa.ObtenerCasilla(5, 1).Ocupar();

        var partida =
            new Partida(
                humano,
                maquina);

        var estado =
            new EstadoPartidaService();

        estado.EstablecerPartida(partida);

        using var gestor =
            new GestorProcesosConcurrentes();

        var servicio =
            new ServicioAccionesConcurrentes(
                estado,
                gestor,
                TimeSpan.Zero);

        ProcesoConcurrente proceso =
            servicio.IniciarAtaque(
                new AtacarRequest
                {
                    AtacanteId =
                        atacante.Id.ToString("D"),
                    ObjetivoId =
                        centro.Id.ToString("D")
                });

        await proceso.Finalizacion;

        Assert.That(
            servicio.IntentarObtenerResultado(
                proceso.Id,
                out ResultadoProcesoConcurrente resultado),
            Is.True);

        Assert.That(
            resultado.Resultado?.Exito,
            Is.True,
            resultado.Resultado?.Mensaje);

        Assert.That(
            centro.VidaActual,
            Is.EqualTo(270));

        Assert.That(
            Math.Max(
                Math.Abs(atacante.Coordenada.X - centro.Coordenada.X),
                Math.Abs(atacante.Coordenada.Y - centro.Coordenada.Y)),
            Is.EqualTo(1));

        Assert.That(
            atacante.Disponible,
            Is.True);
    }

    [Test]
    public async Task CancelarAtaque_AntesDeAplicar_NoReduceVida()
    {
        Partida partida = CrearPartida(out Guerrero atacante, out Lancero objetivo);

        var estado = new EstadoPartidaService();
        estado.EstablecerPartida(partida);

        using var gestor = new GestorProcesosConcurrentes();
        var servicio = new ServicioAccionesConcurrentes(
            estado,
            gestor,
            TimeSpan.FromSeconds(10));

        ProcesoConcurrente proceso = servicio.IniciarAtaque(
            new AtacarRequest
            {
                AtacanteId = atacante.Id.ToString(),
                ObjetivoId = objetivo.Id.ToString()
            });

        Assert.That(servicio.Cancelar(proceso.Id), Is.True);

        await proceso.Finalizacion;

        Assert.That(
            servicio.IntentarObtenerResultado(proceso.Id, out ResultadoProcesoConcurrente resultado),
            Is.True);

        Assert.That(resultado.Estado, Is.EqualTo(EstadoProcesoConcurrente.Cancelado));
        Assert.That(objetivo.VidaActual, Is.EqualTo(objetivo.VidaMaxima));
        Assert.That(atacante.Disponible, Is.True);
    }

    private static Partida CrearPartida(out Guerrero atacante, out Lancero objetivo)
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

        atacante = new Guerrero(new Coordenada(1, 1));
        objetivo = new Lancero(new Coordenada(2, 1));

        humano.AgregarUnidad(atacante);
        maquina.AgregarUnidad(objetivo);

        return new Partida(humano, maquina);
    }
}
