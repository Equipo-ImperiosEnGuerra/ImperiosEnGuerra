#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Reflection;
using ImperiosEnGuerra.Controladores;
using ImperiosEnGuerra.Controladores.Red;
using ImperiosEnGuerra.Controladores.Red.Contratos;
using ImperiosEnGuerra.Vistas;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class RecoleccionUnityTests
{
    private GameObject raiz;
    private ControladorSeleccion seleccion;
    private ControladorAcciones acciones;
    private ControladorConexionApi conexion;
    private VistaPartida vista;
    private EntidadSeleccionableVista aldeano;
    private Text mensaje;

    private const string IdAldeano =
        "22222222-2222-2222-2222-222222222222";

    [SetUp]
    public void Preparar()
    {
        raiz = new GameObject("Prueba recoleccion");
        raiz.SetActive(false);

        vista = raiz.AddComponent<VistaPartida>();
        seleccion = raiz.AddComponent<ControladorSeleccion>();
        acciones = raiz.AddComponent<ControladorAcciones>();
        conexion = raiz.AddComponent<ControladorConexionApi>();

        var hud = raiz.AddComponent<VistaHud>();

        var texto = new GameObject(
            "Mensaje",
            typeof(RectTransform),
            typeof(Text));

        texto.transform.SetParent(raiz.transform);
        mensaje = texto.GetComponent<Text>();

        Campo(hud, "mensaje", mensaje);
        Campo(seleccion, "vistaPartida", vista);
        Campo(acciones, "controladorSeleccion", seleccion);
        Campo(acciones, "vistaHud", hud);
        Campo(acciones, "conexionApi", conexion);
        Campo(conexion, "vistaHud", hud);

        var objeto = new GameObject(
            "Aldeano",
            typeof(SpriteRenderer),
            typeof(EntidadSeleccionableVista));

        objeto.transform.SetParent(raiz.transform);

        aldeano =
            objeto.GetComponent<EntidadSeleccionableVista>();

        aldeano.Configurar(
            CategoriaEntidadVisual.Unidad,
            IdAldeano,
            "Aldeano",
            "Humano",
            1,
            1);

        raiz.SetActive(true);

        Invocar(seleccion, "OnEnable");
        Invocar(acciones, "OnEnable");
        Invocar(seleccion, "Seleccionar", aldeano);
    }

    [TearDown]
    public void Limpiar()
    {
        Invocar(acciones, "OnDisable");
        Invocar(seleccion, "OnDisable");

        Object.DestroyImmediate(raiz);
    }

    [Test]
    public void AldeanoHumano_PreparaRecoleccion()
    {
        Invocar(
            acciones,
            "PrepararAccion",
            "Recolectar");

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
            Is.EqualTo("Recolectar"));

        Assert.That(
            mensaje.text,
            Is.EqualTo("Selecciona un recurso."));

        Assert.That(
            conexion.RecoleccionEnCurso,
            Is.False);
    }

    [Test]
    public void UnidadNoAldeano_NoPreparaRecoleccion()
    {
        aldeano.Configurar(
            CategoriaEntidadVisual.Unidad,
            IdAldeano,
            "Guerrero",
            "Humano",
            1,
            1);

        Invocar(
            acciones,
            "PrepararAccion",
            "Recolectar");

        Assert.That(
            seleccion.CapturandoDestino,
            Is.False);

        Assert.That(
            LeerCampo(
                acciones,
                "unidadIdPendiente"),
            Is.Null);

        Assert.That(
            conexion.RecoleccionEnCurso,
            Is.False);
    }

    [Test]
    public void AldeanoMaquina_NoPreparaRecoleccion()
    {
        aldeano.Configurar(
            CategoriaEntidadVisual.Unidad,
            IdAldeano,
            "Aldeano",
            "Maquina",
            1,
            1);

        Invocar(
            acciones,
            "PrepararAccion",
            "Recolectar");

        Assert.That(
            seleccion.CapturandoDestino,
            Is.False);

        Assert.That(
            conexion.RecoleccionEnCurso,
            Is.False);
    }

    [Test]
    public void AldeanoSinIdentidad_NoPreparaRecoleccion()
    {
        aldeano.Configurar(
            CategoriaEntidadVisual.Unidad,
            "",
            "Aldeano",
            "Humano",
            1,
            1);

        Invocar(
            acciones,
            "PrepararAccion",
            "Recolectar");

        Assert.That(
            seleccion.CapturandoDestino,
            Is.False);

        Assert.That(
            mensaje.text,
            Does.Contain("identidad"));
    }

    [Test]
    public void CancelarRecoleccion_LimpiaIntencion()
    {
        Invocar(
            acciones,
            "PrepararAccion",
            "Recolectar");

        Invocar(
            seleccion,
            "CancelarCapturaDestino");

        Assert.That(
            seleccion.CapturandoDestino,
            Is.False);

        Assert.That(
            LeerCampo(
                acciones,
                "unidadIdPendiente"),
            Is.Null);

        Assert.That(
            LeerCampo(
                acciones,
                "accionPendiente"),
            Is.Null);

        Assert.That(
            mensaje.text,
            Is.EqualTo("Recolección cancelada."));

        Assert.That(
            conexion.RecoleccionEnCurso,
            Is.False);
    }

    [Test]
    public void DtoRecoleccion_UsaIdYCoordenada()
    {
        var dto = new RecolectarDto
        {
            aldeanoId = IdAldeano,
            objetivo = new CoordenadaDto(4, 5)
        };

        string json =
            JsonUtility.ToJson(dto);

        Assert.That(
            json,
            Is.EqualTo(
                "{\"aldeanoId\":\"" +
                IdAldeano +
                "\",\"objetivo\":{\"x\":4,\"y\":5}}"));
    }

    [Test]
    public void DtoConcurrente_RecoleccionConservaContrato()
    {
        var inicio = new ProcesoIniciadoDto
        {
            procesoId =
                "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
            nombre = "RECOLECTAR",
            estado = "iniciado"
        };

        string json = JsonUtility.ToJson(inicio);

        Assert.That(
            json,
            Does.Contain("\"nombre\":\"RECOLECTAR\""));

        var resultado = new ResultadoProcesoDto
        {
            procesoId = inicio.procesoId,
            nombre = "RECOLECTAR",
            estado = "Completado",
            hiloTrabajoId = 9,
            exito = true,
            mensaje = "Recolección de Oro preparada."
        };

        string resultadoJson =
            JsonUtility.ToJson(resultado);

        Assert.That(
            resultadoJson,
            Does.Contain("\"estado\":\"Completado\""));

        Assert.That(
            resultadoJson,
            Does.Contain("\"hiloTrabajoId\":9"));
    }

    [Test]
    public void MovimientoEnCurso_NoBloqueaPrepararRecoleccion()
    {
        CampoAutomatico(
            conexion,
            "MovimientoEnCurso",
            true);

        Invocar(
            acciones,
            "PrepararAccion",
            "Recolectar");

        Assert.That(
            seleccion.CapturandoDestino,
            Is.True);

        Assert.That(
            LeerCampo(
                acciones,
                "accionPendiente"),
            Is.EqualTo("Recolectar"));
    }

    [Test]
    public void CambioSeleccion_CancelaRecoleccion()
    {
        Invocar(
            acciones,
            "PrepararAccion",
            "Recolectar");

        seleccion.LimpiarSeleccion();

        Assert.That(
            seleccion.CapturandoDestino,
            Is.False);

        Assert.That(
            LeerCampo(
                acciones,
                "accionPendiente"),
            Is.Null);

        Assert.That(
            conexion.RecoleccionEnCurso,
            Is.False);
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

    private static void CampoAutomatico(
        object objeto,
        string nombre,
        object valor)
    {
        objeto.GetType()
            .GetField(
                $"<{nombre}>k__BackingField",
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