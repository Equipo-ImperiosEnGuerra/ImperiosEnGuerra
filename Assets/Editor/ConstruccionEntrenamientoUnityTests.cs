#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Reflection;
using ImperiosEnGuerra.Controladores;
using ImperiosEnGuerra.Controladores.Red;
using ImperiosEnGuerra.Controladores.Red.Contratos;
using ImperiosEnGuerra.Vistas;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class ConstruccionEntrenamientoUnityTests
{
    private GameObject raiz;
    private ControladorSeleccion seleccion;
    private ControladorAcciones acciones;
    private ControladorConexionApi conexion;
    private VistaPartida vista;
    private VistaHud hud;

    private EntidadSeleccionableVista aldeano;
    private EntidadSeleccionableVista centroUrbano;

    private GameObject selectorEntrenamiento;
    private Text mensaje;

    private const string IdAldeano =
        "33333333-3333-3333-3333-333333333333";

    [SetUp]
    public void Preparar()
    {
        raiz = new GameObject(
            "Prueba construccion entrenamiento");

        raiz.SetActive(false);

        vista = raiz.AddComponent<VistaPartida>();
        seleccion = raiz.AddComponent<ControladorSeleccion>();
        acciones = raiz.AddComponent<ControladorAcciones>();
        conexion = raiz.AddComponent<ControladorConexionApi>();
        hud = raiz.AddComponent<VistaHud>();

        var objetoMensaje = new GameObject(
            "Mensaje",
            typeof(RectTransform),
            typeof(Text));

        objetoMensaje.transform.SetParent(raiz.transform);
        mensaje = objetoMensaje.GetComponent<Text>();

        selectorEntrenamiento =
            new GameObject("SelectorEntrenamiento");

        selectorEntrenamiento.transform.SetParent(
            raiz.transform);

        Campo(hud, "mensaje", mensaje);

        Campo(
            hud,
            "selectorEntrenamiento",
            selectorEntrenamiento);

        Campo(seleccion, "vistaPartida", vista);

        Campo(
            acciones,
            "controladorSeleccion",
            seleccion);

        Campo(
            acciones,
            "vistaHud",
            hud);

        Campo(
            acciones,
            "conexionApi",
            conexion);

        Campo(
            conexion,
            "vistaHud",
            hud);

        var objetoAldeano = new GameObject(
            "Aldeano",
            typeof(SpriteRenderer),
            typeof(EntidadSeleccionableVista));

        objetoAldeano.transform.SetParent(
            raiz.transform);

        aldeano =
            objetoAldeano.GetComponent<
                EntidadSeleccionableVista>();

        aldeano.Configurar(
            CategoriaEntidadVisual.Unidad,
            IdAldeano,
            "Aldeano",
            "Humano",
            1,
            1);

        var objetoCentro = new GameObject(
            "CentroUrbano",
            typeof(SpriteRenderer),
            typeof(EntidadSeleccionableVista));

        objetoCentro.transform.SetParent(
            raiz.transform);

        centroUrbano =
            objetoCentro.GetComponent<
                EntidadSeleccionableVista>();

        centroUrbano.Configurar(
            CategoriaEntidadVisual.Edificio,
            "",
            "CentroUrbano",
            "Humano",
            2,
            2);

        selectorEntrenamiento.SetActive(false);
        raiz.SetActive(true);

        Invocar(seleccion, "OnEnable");
        Invocar(acciones, "OnEnable");
    }

    [TearDown]
    public void Limpiar()
    {
        Invocar(acciones, "OnDisable");
        Invocar(seleccion, "OnDisable");

        Object.DestroyImmediate(raiz);
    }

    [Test]
    public void AldeanoHumano_PreparaConstruccion()
    {
        Invocar(
            seleccion,
            "Seleccionar",
            aldeano);

        Invocar(
            acciones,
            "PrepararAccion",
            "Construir");

        Assert.That(
            seleccion.CapturandoDestino,
            Is.True);

        Assert.That(
            LeerCampo(
                acciones,
                "unidadIdPendiente"),
            Is.EqualTo(IdAldeano));

        Assert.That(
            LeerCampo(
                acciones,
                "accionPendiente"),
            Is.EqualTo("Construir"));

        Assert.That(
            mensaje.text,
            Does.Contain("Centro Urbano"));
    }

    [Test]
    public void UnidadNoAldeano_NoPreparaConstruccion()
    {
        aldeano.Configurar(
            CategoriaEntidadVisual.Unidad,
            IdAldeano,
            "Guerrero",
            "Humano",
            1,
            1);

        Invocar(
            seleccion,
            "Seleccionar",
            aldeano);

        Invocar(
            acciones,
            "PrepararAccion",
            "Construir");

        Assert.That(
            seleccion.CapturandoDestino,
            Is.False);

        Assert.That(
            LeerCampo(
                acciones,
                "accionPendiente"),
            Is.Null);
    }

    [Test]
    public void CentroUrbanoHumano_MuestraSelectorEntrenamiento()
    {
        Invocar(
            seleccion,
            "Seleccionar",
            centroUrbano);

        Invocar(
            acciones,
            "PrepararAccion",
            "Entrenar");

        Assert.That(
            selectorEntrenamiento.activeSelf,
            Is.True);

        Assert.That(
            seleccion.CapturandoDestino,
            Is.False);

        Assert.That(
            LeerCampo(
                acciones,
                "accionPendiente"),
            Is.EqualTo("Entrenar"));

        Assert.That(
            mensaje.text,
            Does.Contain("tipo de unidad"));
    }

    [Test]
    public void SeleccionarArquero_PreparaDestino()
    {
        Invocar(
            seleccion,
            "Seleccionar",
            centroUrbano);

        Invocar(
            acciones,
            "PrepararAccion",
            "Entrenar");

        Invocar(
            acciones,
            "SeleccionarTipoUnidad",
            "Arquero");

        Assert.That(
            seleccion.CapturandoDestino,
            Is.True);

        Assert.That(
            selectorEntrenamiento.activeSelf,
            Is.False);

        Assert.That(
            LeerCampo(
                acciones,
                "tipoUnidadPendiente"),
            Is.EqualTo("Arquero"));

        Assert.That(
            mensaje.text,
            Is.EqualTo(
                "Selecciona una casilla de referencia para Arquero."));
    }

    [Test]
    public void CentroUrbanoMaquina_NoPreparaEntrenamiento()
    {
        centroUrbano.Configurar(
            CategoriaEntidadVisual.Edificio,
            "",
            "CentroUrbano",
            "Maquina",
            8,
            8);

        Invocar(
            seleccion,
            "Seleccionar",
            centroUrbano);

        Invocar(
            acciones,
            "PrepararAccion",
            "Entrenar");

        Assert.That(
            selectorEntrenamiento.activeSelf,
            Is.False);

        Assert.That(
            seleccion.CapturandoDestino,
            Is.False);

        Assert.That(
            LeerCampo(
                acciones,
                "accionPendiente"),
            Is.Null);
    }

    [Test]
    public void CancelarConstruccion_LimpiaIntencion()
    {
        Invocar(
            seleccion,
            "Seleccionar",
            aldeano);

        Invocar(
            acciones,
            "PrepararAccion",
            "Construir");

        Invocar(
            seleccion,
            "CancelarCapturaDestino");

        Assert.That(
            seleccion.CapturandoDestino,
            Is.False);

        Assert.That(
            LeerCampo(
                acciones,
                "accionPendiente"),
            Is.Null);

        Assert.That(
            mensaje.text,
            Is.EqualTo("Construcción cancelada."));
    }

    [Test]
    public void DtoConstruccion_UsaContratoEsperado()
    {
        var dto = new ConstruirDto
        {
            aldeanoId = IdAldeano,
            tipoEdificio = "CentroUrbano",
            destino = new CoordenadaDto(4, 5)
        };

        string json = JsonUtility.ToJson(dto);

        Assert.That(
            json,
            Is.EqualTo(
                "{\"aldeanoId\":\"" +
                IdAldeano +
                "\",\"tipoEdificio\":\"CentroUrbano\"," +
                "\"destino\":{\"x\":4,\"y\":5}}"));
    }

    [Test]
    public void DtoConcurrente_ConstruccionConservaContrato()
    {
        var inicio = new ProcesoIniciadoDto
        {
            procesoId =
                "cccccccc-cccc-cccc-cccc-cccccccccccc",
            nombre = "CONSTRUIR",
            estado = "iniciado"
        };

        string json = JsonUtility.ToJson(inicio);

        Assert.That(
            json,
            Does.Contain("\"nombre\":\"CONSTRUIR\""));

        var resultado = new ResultadoProcesoDto
        {
            procesoId = inicio.procesoId,
            nombre = "CONSTRUIR",
            estado = "Completado",
            hiloTrabajoId = 11,
            exito = true,
            mensaje = "Construcción realizada correctamente."
        };

        string resultadoJson =
            JsonUtility.ToJson(resultado);

        Assert.That(
            resultadoJson,
            Does.Contain("\"estado\":\"Completado\""));

        Assert.That(
            resultadoJson,
            Does.Contain("\"hiloTrabajoId\":11"));
    }

    [Test]
    public void DtoEntrenamiento_UsaContratoEsperado()
    {
        var dto = new EntrenarDto
        {
            edificioOrigen = new CoordenadaDto(2, 2),
            tipoUnidad = "Guerrero",
            destino = new CoordenadaDto(3, 2)
        };

        string json = JsonUtility.ToJson(dto);

        Assert.That(
            json,
            Is.EqualTo(
                "{\"edificioOrigen\":{\"x\":2,\"y\":2}," +
                "\"tipoUnidad\":\"Guerrero\"," +
                "\"destino\":{\"x\":3,\"y\":2}}"));
    }

    [Test]
    public void DtoConcurrente_EntrenamientoConservaContrato()
    {
        var inicio = new ProcesoIniciadoDto
        {
            procesoId =
                "dddddddd-dddd-dddd-dddd-dddddddddddd",
            nombre = "ENTRENAR",
            estado = "iniciado"
        };

        string json = JsonUtility.ToJson(inicio);

        Assert.That(
            json,
            Does.Contain("\"nombre\":\"ENTRENAR\""));

        var resultado = new ResultadoProcesoDto
        {
            procesoId = inicio.procesoId,
            nombre = "ENTRENAR",
            estado = "Completado",
            hiloTrabajoId = 13,
            exito = true,
            mensaje = "Unidad Guerrero entrenada correctamente."
        };

        string resultadoJson =
            JsonUtility.ToJson(resultado);

        Assert.That(
            resultadoJson,
            Does.Contain("\"estado\":\"Completado\""));

        Assert.That(
            resultadoJson,
            Does.Contain("\"hiloTrabajoId\":13"));
    }

    private static object LeerCampo(
        object objeto,
        string nombre)
    {
        return objeto.GetType()
            .GetField(
                nombre,
                BindingFlags.Instance |
                BindingFlags.NonPublic)
            .GetValue(objeto);
    }

    private static void Campo(
        object objeto,
        string nombre,
        object valor)
    {
        objeto.GetType()
            .GetField(
                nombre,
                BindingFlags.Instance |
                BindingFlags.NonPublic)
            .SetValue(objeto, valor);
    }

    private static void Invocar(
        object objeto,
        string nombre,
        params object[] argumentos)
    {
        objeto.GetType()
            .GetMethod(
                nombre,
                BindingFlags.Instance |
                BindingFlags.NonPublic)
            .Invoke(objeto, argumentos);
    }
}
#endif