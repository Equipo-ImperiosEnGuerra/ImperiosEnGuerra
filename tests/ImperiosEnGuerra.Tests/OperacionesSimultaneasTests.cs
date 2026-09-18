using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Api.Servicios;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;
using ImperiosEnGuerra.Servicios.Concurrencia;

namespace ImperiosEnGuerra.Tests;

public class OperacionesSimultaneasTests
{
    [Test]
    public async Task MovimientoYEntrenamiento_PuedenEjecutarseSimultaneamente()
    {
        Partida partida = CrearPartida(out Aldeano aldeano);

        var estado = new EstadoPartidaService();
        estado.EstablecerPartida(partida);

        using var gestor = new GestorProcesosConcurrentes();

        var servicio = new ServicioAccionesConcurrentes(
            estado,
            gestor,
            TimeSpan.FromMilliseconds(150));

        ProcesoConcurrente movimiento =
            servicio.IniciarMovimiento(
                new MoverUnidadRequest
                {
                    UnidadId = aldeano.Id.ToString("D"),
                    Destino = new CoordenadaRequest
                    {
                        X = 2,
                        Y = 1
                    }
                });

        ProcesoConcurrente entrenamiento =
            servicio.IniciarEntrenamiento(
                new EntrenarRequest
                {
                    EdificioOrigen = new CoordenadaRequest
                    {
                        X = 1,
                        Y = 1
                    },
                    TipoUnidad = "Guerrero",
                    Destino = new CoordenadaRequest
                    {
                        X = 3,
                        Y = 3
                    }
                });

        Assert.That(
            gestor.ProcesosActivos,
            Is.EqualTo(2));

        await Task.WhenAll(
            movimiento.Finalizacion,
            entrenamiento.Finalizacion);

        Assert.That(
            servicio.IntentarObtenerResultado(
                movimiento.Id,
                out ResultadoProcesoConcurrente resultadoMovimiento),
            Is.True);

        Assert.That(
            servicio.IntentarObtenerResultado(
                entrenamiento.Id,
                out ResultadoProcesoConcurrente resultadoEntrenamiento),
            Is.True);

        Assert.That(resultadoMovimiento.Nombre, Is.EqualTo("MOVER"));
        Assert.That(resultadoMovimiento.Resultado.Exito, Is.True);

        Assert.That(
            resultadoEntrenamiento.Nombre,
            Is.EqualTo("ENTRENAR"));

        Assert.That(
            resultadoEntrenamiento.Resultado.Exito,
            Is.True);

        Assert.That(aldeano.Coordenada.X, Is.EqualTo(2));
        Assert.That(aldeano.Coordenada.Y, Is.EqualTo(1));

        Assert.That(
            partida.JugadorHumano.Unidades.Count,
            Is.EqualTo(2));

        Assert.That(
            partida.JugadorHumano.Unidades[1],
            Is.TypeOf<Guerrero>());
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
            new Coordenada(0, 0));

        humano.AgregarUnidad(aldeano);

        humano.AgregarEdificio(
            new CentroUrbano(
                new Coordenada(1, 1)));

        maquina.AgregarEdificio(
            new CentroUrbano(
                new Coordenada(5, 5)));

        mapa.ObtenerCasilla(1, 1).Ocupar();
        mapa.ObtenerCasilla(5, 5).Ocupar();

        return new Partida(humano, maquina);
    }
}
