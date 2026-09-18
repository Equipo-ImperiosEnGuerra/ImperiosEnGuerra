#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Reflection;
using ImperiosEnGuerra.Controladores;
using ImperiosEnGuerra.Controladores.Red;
using ImperiosEnGuerra.Controladores.Red.Contratos;
using ImperiosEnGuerra.Vistas;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class MovimientoUnityTests
{
    private GameObject raiz;
    private ControladorSeleccion seleccion;
    private ControladorAcciones acciones;
    private ControladorConexionApi conexion;
    private VistaPartida vista;
    private EntidadSeleccionableVista unidad;
    private Text mensaje;
    private const string IdModelo = "11111111-1111-1111-1111-111111111111";

    [SetUp]
    public void Preparar()
    {
        raiz = new GameObject("Prueba movimiento");
        raiz.SetActive(false);
        vista = raiz.AddComponent<VistaPartida>();
        seleccion = raiz.AddComponent<ControladorSeleccion>();
        acciones = raiz.AddComponent<ControladorAcciones>();
        conexion = raiz.AddComponent<ControladorConexionApi>();
        var hud = raiz.AddComponent<VistaHud>();
        var texto = new GameObject("Mensaje", typeof(RectTransform), typeof(Text));
        texto.transform.SetParent(raiz.transform);
        mensaje = texto.GetComponent<Text>();
        Campo(hud, "mensaje", mensaje);
        Campo(seleccion, "vistaPartida", vista);
        Campo(acciones, "controladorSeleccion", seleccion);
        Campo(acciones, "vistaHud", hud);
        Campo(acciones, "conexionApi", conexion);
        Campo(conexion, "vistaHud", hud);
        var objeto = new GameObject("Unidad", typeof(SpriteRenderer), typeof(EntidadSeleccionableVista));
        objeto.transform.SetParent(raiz.transform);
        unidad = objeto.GetComponent<EntidadSeleccionableVista>();
        unidad.Configurar(CategoriaEntidadVisual.Unidad, IdModelo, "Aldeano", "Humano", 1, 1);
        raiz.SetActive(true);
        // Estos MonoBehaviour no usan ExecuteAlways: EditMode no ejecuta su ciclo de vida.
        Invocar(seleccion, "OnEnable");
        Invocar(acciones, "OnEnable");
        Invocar(seleccion, "Seleccionar", unidad);
    }

    [TearDown]
    public void Limpiar()
    {
        Invocar(acciones, "OnDisable");
        Invocar(seleccion, "OnDisable");
        Object.DestroyImmediate(raiz);
    }

    [TestCase(CategoriaEntidadVisual.Edificio, "Humano", IdModelo)]
    [TestCase(CategoriaEntidadVisual.Recurso, "Humano", IdModelo)]
    [TestCase(CategoriaEntidadVisual.Unidad, "Maquina", IdModelo)]
    [TestCase(CategoriaEntidadVisual.Unidad, "Humano", "")]
    public void SeleccionInapropiada_NoPreparaMovimiento(CategoriaEntidadVisual categoria, string propietario, string id)
    {
        unidad.Configurar(categoria, id, "Aldeano", propietario, 1, 1);
        Invocar(acciones, "PrepararAccion", "Mover");
        Assert.That(seleccion.CapturandoDestino, Is.False);
        Assert.That(conexion.MovimientoEnCurso, Is.False);
    }

    [Test]
    public void Preparar_ConservaIdSinEnviarNiMover()
    {
        Vector3 posicion = unidad.transform.position;
        Invocar(acciones, "PrepararAccion", "Mover");
        Assert.That(seleccion.CapturandoDestino, Is.True);
        Assert.That(LeerCampo(acciones, "unidadIdPendiente"), Is.EqualTo(IdModelo));
        Assert.That(mensaje.text, Is.EqualTo("Selecciona una casilla destino."));
        Assert.That(conexion.MovimientoEnCurso, Is.False);
        Assert.That(unidad.transform.position, Is.EqualTo(posicion));
    }

    [Test]
    public void CancelacionUsadaPorEscape_LimpiaIntencionSinEnviarNiDeseleccionar()
    {
        Invocar(acciones, "PrepararAccion", "Mover");
        Invocar(seleccion, "CancelarCapturaDestino");
        ComprobarCancelacion();
        Assert.That(seleccion.SeleccionActual, Is.SameAs(unidad));
    }

    [Test]
    public void CambioSeleccion_CancelaSinEnviar()
    {
        Invocar(acciones, "PrepararAccion", "Mover");
        seleccion.LimpiarSeleccion();
        ComprobarCancelacion();
    }

    [Test]
    public void UnidadDesaparece_CancelaSinEnviar()
    {
        Invocar(acciones, "PrepararAccion", "Mover");
        Object.DestroyImmediate(unidad.gameObject);
        Invocar(acciones, "Update");
        ComprobarCancelacion();
    }

    [Test]
    public void Dto_UsaIdSeleccionadoYNombresDelContrato()
    {
        Invocar(acciones, "PrepararAccion", "Mover");
        var dto = new MoverUnidadDto
        {
            unidadId = (string)LeerCampo(acciones, "unidadIdPendiente"),
            destino = new CoordenadaDto(4, 5)
        };
        Assert.That(JsonUtility.ToJson(dto),
            Is.EqualTo("{\"unidadId\":\"" + IdModelo + "\",\"destino\":{\"x\":4,\"y\":5}}"));
    }

    [Test]
    public void DtoConcurrente_LeeInicioYResultadoDelWorker()
    {
        string inicioJson =
            "{\"procesoId\":\"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa\",\"nombre\":\"MOVER\",\"estado\":\"iniciado\"}";

        ProcesoIniciadoDto inicio =
            JsonUtility.FromJson<ProcesoIniciadoDto>(inicioJson);

        Assert.That(
            inicio.procesoId,
            Is.EqualTo("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
        Assert.That(inicio.nombre, Is.EqualTo("MOVER"));
        Assert.That(inicio.estado, Is.EqualTo("iniciado"));

        string resultadoJson =
            "{\"procesoId\":\"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa\",\"nombre\":\"MOVER\",\"estado\":\"Completado\",\"hiloTrabajoId\":7,\"exito\":true,\"mensaje\":\"Movimiento realizado.\",\"errorTecnico\":null}";

        ResultadoProcesoDto resultado =
            JsonUtility.FromJson<ResultadoProcesoDto>(
                resultadoJson);

        Assert.That(resultado.estado, Is.EqualTo("Completado"));
        Assert.That(resultado.hiloTrabajoId, Is.EqualTo(7));
        Assert.That(resultado.exito, Is.True);
        Assert.That(
            resultado.mensaje,
            Is.EqualTo("Movimiento realizado."));
    }

    [TestCase(8f, 10f, 4, 5)]
    [TestCase(0f, 0f, 0, 0)]
    public void CasillaVacia_SeConvierteSinEntidad(float mundoX, float mundoY, int esperadoX, int esperadoY)
    {
        Campo(vista, "anchoVisual", 10);
        Campo(vista, "altoVisual", 10);
        Assert.That(vista.TryObtenerCoordenadaLogica(new Vector3(mundoX, mundoY), out int x, out int y), Is.True);
        Assert.That(x, Is.EqualTo(esperadoX));
        Assert.That(y, Is.EqualTo(esperadoY));
    }

    private void ComprobarCancelacion()
    {
        Assert.That(seleccion.CapturandoDestino, Is.False);
        Assert.That(LeerCampo(acciones, "unidadIdPendiente"), Is.Null);
        Assert.That(conexion.MovimientoEnCurso, Is.False);
        Assert.That(mensaje.text, Is.EqualTo("Movimiento cancelado."));
    }

    private static object LeerCampo(object objeto, string nombre) => objeto.GetType()
        .GetField(nombre, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(objeto);

    private static void Campo(object objeto, string nombre, object valor) => objeto.GetType()
        .GetField(nombre, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(objeto, valor);

    private static void Invocar(object objeto, string nombre, params object[] argumentos) => objeto.GetType()
        .GetMethod(nombre, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(objeto, argumentos);
}
#endif
