using System;
using System.Threading;
using System.Threading.Tasks;
using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Api.Servicios;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.IA;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;
using ImperiosEnGuerra.Servicios.Concurrencia;

namespace ImperiosEnGuerra.Tests;

public class DefensaReactivaIaTests
{
    [Test]
    public void PlanificadorPaseo_AldeanoLibre_MueveSoloUnaCasillaOrtogonal()
    {
        Partida partida =
            CrearPartidaConAldeanos(
                out Aldeano ocupado,
                out Aldeano libre);

        DecisionMaquina decision =
            new PlanificadorDecisionMaquina()
                .PrepararPaseoAldeano(
                    partida,
                    partida.JugadorMaquina,
                    new[]
                    {
                        ocupado.Id
                    });

        Assert.That(
            decision.Tipo,
            Is.EqualTo(
                TipoDecisionMaquina.Pasear));

        Assert.That(
            decision.UnidadId,
            Is.EqualTo(
                libre.Id));

        int distancia =
            Math.Abs(
                libre.Coordenada.X -
                decision.Objetivo.X) +
            Math.Abs(
                libre.Coordenada.Y -
                decision.Objetivo.Y);

        Assert.That(
            distancia,
            Is.EqualTo(1),
            "El paseo ambiental debe ser corto, ortogonal y no alterar la economía.");
    }

    [Test]
    public async Task ImpactoHumano_CreaFrenteDefensivoExtra_SoloConUnidadAtacada()
    {
        Partida partida =
            CrearPartidaDefensa(
                out Guerrero frenteNormal,
                out Guerrero unidadAtacada,
                out Guerrero objetivoNormal,
                out Guerrero atacanteHumano);

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
                TimeSpan.FromMilliseconds(
                    120));

        using var ia =
            new ServicioJugadorMaquina(
                estado,
                acciones,
                TimeSpan.FromMilliseconds(
                    10));

        ProcesoConcurrente procesoNormal =
            ia.EjecutarPaso();

        Assert.That(
            procesoNormal,
            Is.Not.Null,
            "La IA debe iniciar su frente militar normal.");

        Assert.That(
            SpinWait.SpinUntil(
                () =>
                    ia.UnidadesAsignadas >= 1,
                TimeSpan.FromSeconds(1)),
            Is.True);

        int vidaHumanoAntes =
            atacanteHumano.VidaActual;

        ProcesoConcurrente ataqueHumano =
            acciones.IniciarAtaque(
                new AtacarRequest
                {
                    AtacanteId =
                        atacanteHumano.Id
                            .ToString("D"),

                    ObjetivoId =
                        unidadAtacada.Id
                            .ToString("D")
                });

        Assert.That(
            SpinWait.SpinUntil(
                () =>
                    ia.UnidadesAsignadas >= 2,
                TimeSpan.FromSeconds(2)),
            Is.True,
            "La unidad atacada debe poder abrir un único frente defensivo adicional.");

        Assert.That(
            SpinWait.SpinUntil(
                () =>
                    atacanteHumano.VidaActual <
                    vidaHumanoAntes,
                TimeSpan.FromSeconds(2)),
            Is.True,
            "La unidad atacada debe contraatacar al humano que la impactó.");

        Assert.That(
            acciones.TieneProcesoActivo(
                unidadAtacada.Id),
            Is.True,
            "El proceso defensivo debe pertenecer exactamente a la unidad atacada.");

        Assert.That(
            acciones.TieneProcesoActivo(
                frenteNormal.Id),
            Is.True,
            "El frente militar normal debe poder continuar en paralelo.");

        Assert.That(
            objetivoNormal.VidaActual,
            Is.LessThanOrEqualTo(
                objetivoNormal.VidaMaxima));

        acciones.CancelarTodos();

        await ataqueHumano.Finalizacion;
        await procesoNormal.Finalizacion;

        Assert.That(
            SpinWait.SpinUntil(
                () =>
                    gestor.ProcesosActivos == 0,
                TimeSpan.FromSeconds(2)),
            Is.True);
    }

    private static Partida CrearPartidaConAldeanos(
        out Aldeano ocupado,
        out Aldeano libre)
    {
        var mapa =
            new Mapa(8, 8);

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

        humano.AgregarEdificio(
            new CentroUrbano(
                new Coordenada(7, 7)));

        maquina.AgregarEdificio(
            new CentroUrbano(
                new Coordenada(0, 0)));

        mapa.ObtenerCasilla(7, 7).Ocupar();
        mapa.ObtenerCasilla(0, 0).Ocupar();

        ocupado =
            new Aldeano(
                new Coordenada(1, 1));

        libre =
            new Aldeano(
                new Coordenada(4, 4));

        maquina.AgregarUnidad(
            ocupado);

        maquina.AgregarUnidad(
            libre);

        return new Partida(
            humano,
            maquina);
    }

    private static Partida CrearPartidaDefensa(
        out Guerrero frenteNormal,
        out Guerrero unidadAtacada,
        out Guerrero objetivoNormal,
        out Guerrero atacanteHumano)
    {
        var mapa =
            new Mapa(10, 10);

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

        humano.AgregarEdificio(
            new CentroUrbano(
                new Coordenada(9, 9)));

        maquina.AgregarEdificio(
            new CentroUrbano(
                new Coordenada(0, 0)));

        mapa.ObtenerCasilla(9, 9).Ocupar();
        mapa.ObtenerCasilla(0, 0).Ocupar();

        frenteNormal =
            new Guerrero(
                new Coordenada(2, 1));

        unidadAtacada =
            new Guerrero(
                new Coordenada(6, 6));

        objetivoNormal =
            new Guerrero(
                new Coordenada(3, 1));

        atacanteHumano =
            new Guerrero(
                new Coordenada(6, 5));

        maquina.AgregarUnidad(
            frenteNormal);

        maquina.AgregarUnidad(
            unidadAtacada);

        humano.AgregarUnidad(
            objetivoNormal);

        humano.AgregarUnidad(
            atacanteHumano);

        return new Partida(
            humano,
            maquina);
    }
}
