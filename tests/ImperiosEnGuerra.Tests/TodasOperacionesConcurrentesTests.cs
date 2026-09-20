using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Api.Servicios;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;
using ImperiosEnGuerra.Servicios.Concurrencia;

namespace ImperiosEnGuerra.Tests;

public class TodasOperacionesConcurrentesTests
{
    [Test]
    public async Task TodasLasOperaciones_PuedenEjecutarseConcurrentemente()
    {
        Partida partida = CrearPartida(
            out Aldeano aldeanoMovimiento,
            out Aldeano aldeanoRecoleccion,
            out Aldeano aldeanoConstruccion,
            out Guerrero guerrero,
            out Lancero enemigo);


        var estado = new EstadoPartidaService();
        estado.EstablecerPartida(partida);


        using var gestor = new GestorProcesosConcurrentes();


        var servicio = new ServicioAccionesConcurrentes(
            estado,
            gestor,
            TimeSpan.FromMilliseconds(100));


        ProcesoConcurrente movimiento =
            servicio.IniciarMovimiento(
                new MoverUnidadRequest
                {
                    UnidadId = aldeanoMovimiento.Id.ToString(),
                    Destino = new CoordenadaRequest
                    {
                        X = 3,
                        Y = 3
                    }
                });


        ProcesoConcurrente recoleccion =
            servicio.IniciarRecoleccion(
                new RecolectarRequest
                {
                    AldeanoId = aldeanoRecoleccion.Id.ToString(),
                    Objetivo = new CoordenadaRequest
                    {
                        X = 2,
                        Y = 2
                    }
                });


        ProcesoConcurrente construccion =
            servicio.IniciarConstruccion(
                new ConstruirRequest
                {
                    AldeanoId = aldeanoConstruccion.Id.ToString(),
                    TipoEdificio = "CentroUrbano",
                    Destino = new CoordenadaRequest
                    {
                        X = 3,
                        Y = 2
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
                        X = 4,
                        Y = 1
                    }
                });


        ProcesoConcurrente ataque =
            servicio.IniciarAtaque(
                new AtacarRequest
                {
                    AtacanteId = guerrero.Id.ToString(),
                    ObjetivoId = enemigo.Id.ToString()
                });


        Assert.That(
            gestor.ProcesosActivos,
            Is.EqualTo(5));


        await Task.WhenAll(
            movimiento.Finalizacion,
            recoleccion.Finalizacion,
            construccion.Finalizacion,
            entrenamiento.Finalizacion,
            ataque.Finalizacion);


        AssertResultado(
            servicio,
            movimiento.Id,
            "MOVER");


        AssertResultado(
            servicio,
            recoleccion.Id,
            "RECOLECTAR");


        AssertResultado(
            servicio,
            construccion.Id,
            "CONSTRUIR");


        AssertResultado(
            servicio,
            entrenamiento.Id,
            "ENTRENAR");


        AssertResultado(
            servicio,
            ataque.Id,
            "ATACAR");
    }


    private static void AssertResultado(
        ServicioAccionesConcurrentes servicio,
        Guid id,
        string nombre)
    {
        Assert.That(
            servicio.IntentarObtenerResultado(
                id,
                out ResultadoProcesoConcurrente resultado),
            Is.True);

        Assert.That(
            resultado.Nombre,
            Is.EqualTo(nombre));

        Assert.That(
            resultado.Resultado,
            Is.Not.Null);

        Assert.That(
            resultado.Resultado.Exito,
            Is.True);
    }


    private static Partida CrearPartida(
        out Aldeano aldeanoMovimiento,
        out Aldeano aldeanoRecoleccion,
        out Aldeano aldeanoConstruccion,
        out Guerrero guerrero,
        out Lancero enemigo)
    {
        var mapa = new Mapa(6,6);


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


        aldeanoMovimiento =
            new Aldeano(new Coordenada(0,0));

        aldeanoRecoleccion =
            new Aldeano(new Coordenada(0,1));

        aldeanoConstruccion =
            new Aldeano(new Coordenada(0,2));


        guerrero =
            new Guerrero(new Coordenada(1,2));


        enemigo =
            new Lancero(new Coordenada(4,4));


        humano.AgregarUnidad(aldeanoMovimiento);
        humano.AgregarUnidad(aldeanoRecoleccion);
        humano.AgregarUnidad(aldeanoConstruccion);
        humano.AgregarUnidad(guerrero);


        maquina.AgregarUnidad(enemigo);


        humano.AgregarEdificio(
            new CentroUrbano(
                new Coordenada(1,1)));


        mapa.ObtenerCasilla(1,1).Ocupar();


        mapa.ColocarRecurso(
            new Recurso(
                TipoRecurso.Oro,
                new Coordenada(2,2)));


        return new Partida(
            humano,
            maquina);
    }
}
