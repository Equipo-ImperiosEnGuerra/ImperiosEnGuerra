using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Api.Servicios;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;
using ImperiosEnGuerra.Servicios.Concurrencia;

namespace ImperiosEnGuerra.Tests;

public class CicloRecoleccionTests
{
    [Test]
    public async Task CicloCompleto_AgotaNodoYDepositaTodo()
    {
        Partida partida =
            CrearPartida(
                out Aldeano aldeano,
                cantidadNodo: 12,
                capacidad: 5);

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
            servicio.IniciarRecoleccion(
                Request(aldeano));

        await proceso.Finalizacion;

        Assert.That(
            servicio.IntentarObtenerResultado(
                proceso.Id,
                out ResultadoProcesoConcurrente resultado),
            Is.True);

        Assert.That(
            resultado.Resultado?.Exito,
            Is.True);

        Assert.That(
            partida.JugadorHumano.Recursos
                .ObtenerCantidad(TipoRecurso.Oro),
            Is.EqualTo(12));

        Assert.That(
            partida.JugadorHumano.Mapa
                .ObtenerRecursoEn(new Coordenada(4, 1))
                .CantidadRestante,
            Is.Zero);

        Assert.That(
            aldeano.CargaActual,
            Is.Zero);

        Assert.That(
            aldeano.Estado,
            Is.EqualTo(EstadoUnidad.Idle));
    }

    [Test]
    public async Task DosAldeanos_MismoNodo_NoDuplicanSaldo()
    {
        Partida partida =
            CrearPartidaDosAldeanos(
                out Aldeano a,
                out Aldeano b,
                19);

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

        ProcesoConcurrente primero =
            servicio.IniciarRecoleccion(Request(a));

        ProcesoConcurrente segundo =
            servicio.IniciarRecoleccion(Request(b));

        await Task.WhenAll(
            primero.Finalizacion,
            segundo.Finalizacion);

        Assert.That(
            partida.JugadorHumano.Recursos
                .ObtenerCantidad(TipoRecurso.Oro),
            Is.EqualTo(19));

        Assert.That(
            partida.JugadorHumano.Mapa
                .ObtenerRecursoEn(new Coordenada(4, 1))
                .CantidadRestante,
            Is.Zero);

        Assert.That(a.CargaActual + b.CargaActual, Is.Zero);
    }

    private static Partida CrearPartida(
        out Aldeano aldeano,
        int cantidadNodo,
        int capacidad)
    {
        var mapa = new Mapa(8, 6);
        var humano = new Jugador(
            "Humano", TipoJugador.Humano, mapa, new RecursosJugador());
        var maquina = new Jugador(
            "Maquina", TipoJugador.Maquina, mapa, new RecursosJugador());

        aldeano =
            new Aldeano(new Coordenada(2, 1), capacidad);

        humano.AgregarUnidad(aldeano);
        humano.AgregarEdificio(
            new CentroUrbano(new Coordenada(1, 1)));
        mapa.ObtenerCasilla(1, 1).Ocupar();
        mapa.ColocarRecurso(
            new Recurso(
                TipoRecurso.Oro,
                new Coordenada(4, 1),
                cantidadNodo));

        return new Partida(humano, maquina);
    }

    private static Partida CrearPartidaDosAldeanos(
        out Aldeano a,
        out Aldeano b,
        int cantidadNodo)
    {
        Partida partida =
            CrearPartida(
                out a,
                cantidadNodo,
                5);

        b = new Aldeano(
            new Coordenada(2, 2),
            5);

        partida.JugadorHumano.AgregarUnidad(b);
        return partida;
    }

    private static RecolectarRequest Request(
        Aldeano aldeano)
    {
        return new RecolectarRequest
        {
            AldeanoId = aldeano.Id.ToString("D"),
            Objetivo = new CoordenadaRequest
            {
                X = 4,
                Y = 1
            }
        };
    }
}
