using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Api.Servicios;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;
using ImperiosEnGuerra.Servicios.Concurrencia;

namespace ImperiosEnGuerra.Tests;

public class Etapa45IntegracionTests
{
    [Test]
    public async Task EconomiaGanadaPorRecoleccion_PermiteConstruirYEntrenar()
    {
        var mapa = new Mapa(10, 10);
        var humano = new Jugador(
            "Humano", TipoJugador.Humano, mapa, new RecursosJugador());
        var maquina = new Jugador(
            "Maquina", TipoJugador.Maquina, mapa, new RecursosJugador());

        // Esta prueba construye la partida manualmente para controlar el mapa,
        // así que aplicamos explícitamente el mismo balance inicial usado por
        // InicializadorPartida: 0 Oro, 20 Madera y 30 Comida.
        new ConfiguracionInicioPartida()
            .AplicarSaldoInicial(humano.Recursos);

        var aldeano = new Aldeano(new Coordenada(2, 1));
        humano.AgregarUnidad(aldeano);
        humano.AgregarEdificio(new CentroUrbano(new Coordenada(1, 1)));
        mapa.ObtenerCasilla(1, 1).Ocupar();

        mapa.ColocarRecurso(new Recurso(
            TipoRecurso.Oro, new Coordenada(4, 1), 40));
        mapa.ColocarRecurso(new Recurso(
            TipoRecurso.Madera, new Coordenada(4, 2), 40));
        mapa.ColocarRecurso(new Recurso(
            TipoRecurso.Comida, new Coordenada(4, 3), 30));

        var partida = new Partida(humano, maquina);
        var estado = new EstadoPartidaService();
        estado.EstablecerPartida(partida);

        using var gestor = new GestorProcesosConcurrentes();
        var servicio = new ServicioAccionesConcurrentes(
            estado, gestor, TimeSpan.Zero);

        foreach ((int x, int y) in new[] { (4, 1), (4, 2), (4, 3) })
        {
            ProcesoConcurrente recoleccion =
                servicio.IniciarRecoleccion(
                    new RecolectarRequest
                    {
                        AldeanoId = aldeano.Id.ToString("D"),
                        Objetivo = new CoordenadaRequest { X = x, Y = y }
                    });

            await recoleccion.Finalizacion;

            Assert.That(
                servicio.IntentarObtenerResultado(
                    recoleccion.Id,
                    out ResultadoProcesoConcurrente resultado),
                Is.True);
            Assert.That(
                resultado.Resultado?.Exito,
                Is.True,
                resultado.Resultado?.Mensaje);
        }

        ProcesoConcurrente construccion =
            servicio.IniciarConstruccion(
                new ConstruirRequest
                {
                    AldeanoId = aldeano.Id.ToString("D"),
                    TipoEdificio = "CentroUrbano",
                    Destino = new CoordenadaRequest { X = 7, Y = 7 }
                });

        await construccion.Finalizacion;

        Assert.That(
            servicio.IntentarObtenerResultado(
                construccion.Id,
                out ResultadoProcesoConcurrente resultadoConstruccion),
            Is.True);
        Assert.That(
            resultadoConstruccion.Resultado?.Exito,
            Is.True,
            resultadoConstruccion.Resultado?.Mensaje);

        ProcesoConcurrente entrenamiento =
            servicio.IniciarEntrenamiento(
                new EntrenarRequest
                {
                    EdificioOrigen = new CoordenadaRequest { X = 1, Y = 1 },
                    TipoUnidad = "Guerrero",
                    Destino = new CoordenadaRequest { X = 8, Y = 8 }
                });

        await entrenamiento.Finalizacion;

        Assert.That(
            servicio.IntentarObtenerResultado(
                entrenamiento.Id,
                out ResultadoProcesoConcurrente resultadoEntrenamiento),
            Is.True);
        Assert.That(
            resultadoEntrenamiento.Resultado?.Exito,
            Is.True,
            resultadoEntrenamiento.Resultado?.Mensaje);

        Assert.That(
            humano.Edificios.OfType<CentroUrbano>().Count(),
            Is.EqualTo(2));
        Assert.That(
            humano.Unidades.OfType<Guerrero>().Count(),
            Is.EqualTo(1));

        Assert.That(humano.Recursos.ObtenerCantidad(TipoRecurso.Oro), Is.GreaterThanOrEqualTo(0));
        Assert.That(humano.Recursos.ObtenerCantidad(TipoRecurso.Madera), Is.GreaterThanOrEqualTo(0));
        Assert.That(humano.Recursos.ObtenerCantidad(TipoRecurso.Comida), Is.GreaterThanOrEqualTo(0));
    }

    [Test]
    public async Task CancelarConstruccion_ReembolsaCostoYLiberaReserva()
    {
        var mapa = new Mapa(6, 6);
        var humano = new Jugador(
            "Humano", TipoJugador.Humano, mapa, new RecursosJugador());
        var maquina = new Jugador(
            "Maquina", TipoJugador.Maquina, mapa, new RecursosJugador());

        humano.Recursos.Agregar(TipoRecurso.Oro, 100);
        humano.Recursos.Agregar(TipoRecurso.Madera, 100);

        var aldeano = new Aldeano(new Coordenada(1, 1));
        humano.AgregarUnidad(aldeano);

        var partida = new Partida(humano, maquina);
        var estado = new EstadoPartidaService();
        estado.EstablecerPartida(partida);

        using var gestor = new GestorProcesosConcurrentes();
        var servicio = new ServicioAccionesConcurrentes(
            estado, gestor, TimeSpan.FromSeconds(10));

        int oro = humano.Recursos.ObtenerCantidad(TipoRecurso.Oro);
        int madera = humano.Recursos.ObtenerCantidad(TipoRecurso.Madera);

        ProcesoConcurrente proceso =
            servicio.IniciarConstruccion(
                new ConstruirRequest
                {
                    AldeanoId = aldeano.Id.ToString("D"),
                    TipoEdificio = "CentroUrbano",
                    Destino = new CoordenadaRequest { X = 3, Y = 3 }
                });

        servicio.Cancelar(proceso.Id);
        await proceso.Finalizacion;

        Assert.That(
            humano.Recursos.ObtenerCantidad(TipoRecurso.Oro),
            Is.EqualTo(oro));
        Assert.That(
            humano.Recursos.ObtenerCantidad(TipoRecurso.Madera),
            Is.EqualTo(madera));
        Assert.That(
            mapa.ObtenerCasilla(3, 3).EstaOcupada,
            Is.False);
        Assert.That(
            humano.ObrasConstruccion,
            Is.Empty);
    }

    [Test]
    public void ServicioSinRecursos_RechazaConstruirYEntrenarConCostoVisible()
    {
        var mapa = new Mapa(5, 5);
        var humano = new Jugador(
            "Humano", TipoJugador.Humano, mapa, new RecursosJugador());
        var maquina = new Jugador(
            "Maquina", TipoJugador.Maquina, mapa, new RecursosJugador());
        var aldeano = new Aldeano(new Coordenada(0, 0));

        humano.AgregarUnidad(aldeano);
        humano.AgregarEdificio(new CentroUrbano(new Coordenada(1, 1)));
        mapa.ObtenerCasilla(1, 1).Ocupar();

        var estado = new EstadoPartidaService();
        estado.EstablecerPartida(new Partida(humano, maquina));

        ResultadoAccion construir =
            estado.Construir(
                new ConstruirRequest
                {
                    AldeanoId = aldeano.Id.ToString("D"),
                    TipoEdificio = "CentroUrbano",
                    Destino = new CoordenadaRequest { X = 3, Y = 3 }
                });

        ResultadoAccion entrenar =
            estado.Entrenar(
                new EntrenarRequest
                {
                    EdificioOrigen = new CoordenadaRequest { X = 1, Y = 1 },
                    TipoUnidad = "Guerrero",
                    Destino = new CoordenadaRequest { X = 2, Y = 2 }
                });

        Assert.That(construir.Exito, Is.False);
        Assert.That(entrenar.Exito, Is.False);
        Assert.That(construir.Mensaje, Does.Contain("Costo"));
        Assert.That(entrenar.Mensaje, Does.Contain("Costo"));
    }
}
