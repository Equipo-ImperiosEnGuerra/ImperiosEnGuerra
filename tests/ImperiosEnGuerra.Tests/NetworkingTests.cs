using System.Text.Json;
using ImperiosEnGuerra.Api.Servicios;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;
using ImperiosEnGuerra.Servicios.Concurrencia;

namespace ImperiosEnGuerra.Tests;

public class NetworkingTests
{
    [Test]
    public async Task MensajeMover_DespachaWorkerYModificaModelo()
    {
        using EntornoRed entorno =
            CrearEntorno();

        string json =
            CrearMensaje(
                "MOVER",
                new
                {
                    unidadId =
                        entorno.GuerreroHumano.Id
                            .ToString("D"),
                    destino =
                        new
                        {
                            x = 2,
                            y = 1
                        }
                });

        var despacho =
            entorno.Despachador.Procesar(
                json);

        Assert.That(
            despacho.Exito,
            Is.True,
            despacho.Mensaje);

        ResultadoProcesoConcurrente resultado =
            await EsperarResultado(
                entorno.Acciones,
                despacho.ProcesoId);

        Assert.That(
            resultado.Resultado?.Exito,
            Is.True,
            resultado.Resultado?.Mensaje);

        Assert.That(
            entorno.GuerreroHumano.Coordenada.X,
            Is.EqualTo(2));
    }

    [Test]
    public async Task MensajeRecolectar_UsaMismasReglasConcurrentes()
    {
        using EntornoRed entorno =
            CrearEntorno();

        string json =
            CrearMensaje(
                "RECOLECTAR",
                new
                {
                    aldeanoId =
                        entorno.AldeanoHumano.Id
                            .ToString("D"),
                    objetivo =
                        new
                        {
                            x = 3,
                            y = 3
                        }
                });

        var despacho =
            entorno.Despachador.Procesar(
                json);

        Assert.That(
            despacho.Exito,
            Is.True,
            despacho.Mensaje);

        ResultadoProcesoConcurrente resultado =
            await EsperarResultado(
                entorno.Acciones,
                despacho.ProcesoId);

        Assert.That(
            resultado.Resultado?.Exito,
            Is.True,
            resultado.Resultado?.Mensaje);

        Assert.That(
            entorno.Partida.JugadorHumano.Recursos
                .ObtenerCantidad(TipoRecurso.Oro),
            Is.GreaterThan(100));
    }

    [Test]
    public async Task MensajeConstruir_DespachaConstruccion()
    {
        using EntornoRed entorno =
            CrearEntorno();

        string json =
            CrearMensaje(
                "CONSTRUIR",
                new
                {
                    aldeanoId =
                        entorno.AldeanoHumano.Id
                            .ToString("D"),
                    tipoEdificio =
                        "CentroUrbano",
                    destino =
                        new
                        {
                            x = 4,
                            y = 1
                        }
                });

        var despacho =
            entorno.Despachador.Procesar(
                json);

        Assert.That(
            despacho.Exito,
            Is.True,
            despacho.Mensaje);

        ResultadoProcesoConcurrente resultado =
            await EsperarResultado(
                entorno.Acciones,
                despacho.ProcesoId);

        Assert.That(
            resultado.Resultado?.Exito,
            Is.True,
            resultado.Resultado?.Mensaje);

        Assert.That(
            entorno.Partida.JugadorHumano.Edificios.Count,
            Is.EqualTo(2));
    }

    [Test]
    public async Task MensajeEntrenar_DespachaEntrenamiento()
    {
        using EntornoRed entorno =
            CrearEntorno();

        int iniciales =
            entorno.Partida.JugadorHumano.Unidades.Count;

        string json =
            CrearMensaje(
                "ENTRENAR",
                new
                {
                    edificioOrigen =
                        new
                        {
                            x = 0,
                            y = 0
                        },
                    tipoUnidad =
                        "Arquero"
                });

        var despacho =
            entorno.Despachador.Procesar(
                json);

        Assert.That(
            despacho.Exito,
            Is.True,
            despacho.Mensaje);

        ResultadoProcesoConcurrente resultado =
            await EsperarResultado(
                entorno.Acciones,
                despacho.ProcesoId);

        Assert.That(
            resultado.Resultado?.Exito,
            Is.True,
            resultado.Resultado?.Mensaje);

        Assert.That(
            entorno.Partida.JugadorHumano.Unidades.Count,
            Is.EqualTo(iniciales + 1));
    }

    [Test]
    public async Task MensajeAtacar_DespachaIntencionDeAtaque()
    {
        using EntornoRed entorno =
            CrearEntorno();

        string json =
            CrearMensaje(
                "ATACAR",
                new
                {
                    atacanteId =
                        entorno.GuerreroHumano.Id
                            .ToString("D"),
                    objetivoId =
                        entorno.GuerreroMaquina.Id
                            .ToString("D")
                });

        var despacho =
            entorno.Despachador.Procesar(
                json);

        Assert.That(
            despacho.Exito,
            Is.True,
            despacho.Mensaje);

        ResultadoProcesoConcurrente resultado =
            await EsperarResultado(
                entorno.Acciones,
                despacho.ProcesoId);

        Assert.That(
            resultado.Resultado?.Exito,
            Is.True,
            resultado.Resultado?.Mensaje);

        Assert.That(
            resultado.Resultado?.Mensaje,
            Does.Contain("pendiente"));
    }

    [TestCase("{esto-no-es-json")]
    [TestCase("{\"tipo\":\"DESCONOCIDO\",\"datos\":{}}")]
    public void MensajeInvalido_SeRechazaSinLanzarExcepcion(
        string json)
    {
        using EntornoRed entorno =
            CrearEntorno();

        var resultado =
            entorno.Despachador.Procesar(
                json);

        Assert.That(
            resultado.Exito,
            Is.False);

        Assert.That(
            resultado.ProcesoId,
            Is.Null);
    }

    private static string CrearMensaje(
        string tipo,
        object datos)
    {
        return JsonSerializer.Serialize(
            new
            {
                tipo,
                emisorId = "instancia-prueba",
                mensajeId = Guid.NewGuid().ToString("D"),
                datos
            },
            new JsonSerializerOptions(
                JsonSerializerDefaults.Web));
    }

    private static async Task<ResultadoProcesoConcurrente>
        EsperarResultado(
            ServicioAccionesConcurrentes acciones,
            Guid? procesoId)
    {
        Assert.That(
            procesoId,
            Is.Not.Null);

        DateTime limite =
            DateTime.UtcNow
                .AddSeconds(3);

        while (DateTime.UtcNow < limite)
        {
            if (acciones.IntentarObtenerResultado(
                    procesoId!.Value,
                    out ResultadoProcesoConcurrente resultado))
            {
                return resultado;
            }

            await Task.Delay(5);
        }

        Assert.Fail(
            "El proceso de red no publicó resultado a tiempo.");

        throw new InvalidOperationException();
    }

    private static EntornoRed CrearEntorno()
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

        humano.Recursos.Agregar(
            TipoRecurso.Oro,
            100);

        humano.Recursos.Agregar(
            TipoRecurso.Madera,
            100);

        humano.Recursos.Agregar(
            TipoRecurso.Comida,
            100);

        var centroHumano =
            new CentroUrbano(
                new Coordenada(0, 0));

        var centroMaquina =
            new CentroUrbano(
                new Coordenada(7, 7));

        humano.AgregarEdificio(
            centroHumano);

        maquina.AgregarEdificio(
            centroMaquina);

        mapa.ObtenerCasilla(0, 0).Ocupar();
        mapa.ObtenerCasilla(7, 7).Ocupar();

        var aldeano =
            new Aldeano(
                new Coordenada(1, 2));

        var guerreroHumano =
            new Guerrero(
                new Coordenada(1, 1));

        var guerreroMaquina =
            new Guerrero(
                new Coordenada(6, 6));

        humano.AgregarUnidad(
            aldeano);

        humano.AgregarUnidad(
            guerreroHumano);

        maquina.AgregarUnidad(
            guerreroMaquina);

        mapa.ColocarRecurso(
            new Recurso(
                TipoRecurso.Oro,
                new Coordenada(3, 3),
                10));

        var partida =
            new Partida(
                humano,
                maquina);

        var estado =
            new EstadoPartidaService();

        estado.EstablecerPartida(
            partida);

        var gestor =
            new GestorProcesosConcurrentes();

        var acciones =
            new ServicioAccionesConcurrentes(
                estado,
                gestor,
                TimeSpan.Zero);

        return new EntornoRed(
            partida,
            acciones,
            new DespachadorMensajesRed(
                acciones),
            gestor,
            aldeano,
            guerreroHumano,
            guerreroMaquina);
    }

    private sealed class EntornoRed : IDisposable
    {
        public Partida Partida { get; }
        public ServicioAccionesConcurrentes Acciones { get; }
        public DespachadorMensajesRed Despachador { get; }
        public Aldeano AldeanoHumano { get; }
        public Guerrero GuerreroHumano { get; }
        public Guerrero GuerreroMaquina { get; }

        private readonly GestorProcesosConcurrentes gestor;

        public EntornoRed(
            Partida partida,
            ServicioAccionesConcurrentes acciones,
            DespachadorMensajesRed despachador,
            GestorProcesosConcurrentes gestor,
            Aldeano aldeanoHumano,
            Guerrero guerreroHumano,
            Guerrero guerreroMaquina)
        {
            Partida = partida;
            Acciones = acciones;
            Despachador = despachador;
            this.gestor = gestor;
            AldeanoHumano = aldeanoHumano;
            GuerreroHumano = guerreroHumano;
            GuerreroMaquina = guerreroMaquina;
        }

        public void Dispose()
        {
            gestor.Dispose();
        }
    }
}
