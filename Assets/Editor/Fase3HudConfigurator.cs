#if UNITY_EDITOR
using System.Linq;
using ImperiosEnGuerra.Controladores;
using ImperiosEnGuerra.Controladores.Red;
using ImperiosEnGuerra.Vistas;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Prepara únicamente los objetos de interfaz y sus referencias en el Editor.</summary>
public static class Fase3HudConfigurator
{
    private const string RutaEscena = "Assets/Scenes/SampleScene.unity";

    [MenuItem("Tools/Imperios en Guerra/Fase 3/Configurar HUD y acciones")]
    public static void ConfigurarHudYAcciones()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogError("Configure el HUD fuera del modo Play.");
            return;
        }

        Scene escena = SceneManager.GetSceneByPath(RutaEscena);
        if (!escena.IsValid() || !escena.isLoaded)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }
            escena = EditorSceneManager.OpenScene(RutaEscena);
        }

        var conexion = BuscarComponente<ControladorConexionApi>(escena);
        var seleccion = BuscarComponente<ControladorSeleccion>(escena);
        if (conexion == null || seleccion == null)
        {
            Debug.LogError("SampleScene necesita ControladorConexionApi y ControladorSeleccion. " +
                "Ejecute primero Fase 2/Configurar vista inicial.");
            return;
        }

        Configurar(escena, conexion, seleccion);
        EditorSceneManager.MarkSceneDirty(escena);
        if (!EditorSceneManager.SaveScene(escena))
        {
            Debug.LogError("No se pudo guardar SampleScene después de configurar el HUD.");
            return;
        }
        Debug.Log("HUD y acciones configurados y SampleScene guardada. Inicie la API y ejecute Play.");
    }

    public static void Configurar(Scene escena, ControladorConexionApi conexion, ControladorSeleccion seleccion)
    {
        GameObject raiz = Buscar(escena, "HudPartida");
        if (raiz == null)
        {
            raiz = new GameObject("HudPartida", typeof(RectTransform));
            SceneManager.MoveGameObjectToScene(raiz, escena);
            Undo.RegisterCreatedObjectUndo(raiz, "Crear HUD");
        }
        // El propio HudPartida es el Canvas raíz de toda su UI.
        RectTransform rectRaiz = Componente<RectTransform>(raiz);
        if (rectRaiz.parent != null)
        {
            Undo.SetTransformParent(rectRaiz, null, "Colocar HUD en la raíz de la escena");
        }
        // Unity controla el RectTransform del Canvas Overlay raíz.
        raiz.SetActive(true);
        Canvas canvas = Componente<Canvas>(raiz);
        canvas.enabled = true;
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler escalador = Componente<CanvasScaler>(raiz);
        escalador.enabled = true;
        escalador.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escalador.referenceResolution = new Vector2(1280, 720);
        // Conserva al menos el espacio lógico de referencia en ambas dimensiones.
        escalador.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        Canvas.ForceUpdateCanvases();
        Componente<GraphicRaycaster>(raiz).enabled = true;
        VistaHud hud = Componente<VistaHud>(raiz);
        hud.enabled = true;

        RectTransform barra = Rect(raiz.transform, "BarraRecursos", new Vector2(0, 1),
            new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(16, -64), new Vector2(-16, -16));
        Fondo(barra);
        Text recursos = Texto(barra, "Recursos", "Oro: 0 | Madera: 0 | Comida: 0",
            new Vector2(12, 12), new Vector2(-12, -12));
        Rect(barra, "Recursos", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            new Vector2(12, 12), new Vector2(-12, -12));
        recursos.alignment = TextAnchor.MiddleCenter;

        RectTransform panel = Rect(raiz.transform, "PanelContextual", Vector2.zero,
            Vector2.zero, Vector2.zero, new Vector2(16, 16), new Vector2(396, 226));
        Fondo(panel);
        Text descripcion = Texto(panel, "Seleccion", "Sin selección",
            new Vector2(16, -77), new Vector2(-16, -12));
        Text mensaje = Texto(panel, "Mensaje", "", new Vector2(16, -125), new Vector2(-16, -85));
        mensaje.fontSize = 17;
        mensaje.resizeTextMaxSize = 17;

        var serializado = new SerializedObject(hud);
        serializado.FindProperty("recursos").objectReferenceValue = recursos;
        serializado.FindProperty("seleccion").objectReferenceValue = descripcion;
        serializado.FindProperty("mensaje").objectReferenceValue = mensaje;
        string[] acciones = { "Mover", "Recolectar", "Construir", "Entrenar", "Atacar" };
        for (int i = 0; i < acciones.Length; i++)
        {
            string accion = acciones[i];
            // Cinco botones de 64x42: margen 16 y separación 7 dentro de los 380 px.
            float x = 16 + i * 71;
            RectTransform rect = Rect(panel, accion, Vector2.zero, Vector2.zero,
                Vector2.zero, new Vector2(x, 16), new Vector2(x + 64, 58));
            Image imagen = Componente<Image>(rect.gameObject);
            imagen.enabled = true;
            imagen.color = new Color(0.18f, 0.28f, 0.38f, 1);
            Button boton = Componente<Button>(rect.gameObject);
            boton.enabled = true;
            boton.targetGraphic = imagen;
            Text etiqueta = Texto(rect, "Texto", accion, new Vector2(3, -38), new Vector2(-3, -4));
            etiqueta.fontSize = 14;
            etiqueta.resizeTextMinSize = 10;
            etiqueta.resizeTextMaxSize = 14;
            etiqueta.alignment = TextAnchor.MiddleCenter;
            serializado.FindProperty(char.ToLowerInvariant(accion[0]) + accion.Substring(1))
                .objectReferenceValue = boton;
            rect.gameObject.SetActive(false);
        }
        serializado.ApplyModifiedProperties();

        var controladorAcciones = BuscarComponente<ControladorAcciones>(escena);
        GameObject objetoAcciones = controladorAcciones == null
            ? Buscar(escena, "ControladorAcciones") : controladorAcciones.gameObject;
        if (objetoAcciones == null)
        {
            objetoAcciones = new GameObject("ControladorAcciones");
            SceneManager.MoveGameObjectToScene(objetoAcciones, escena);
            Undo.RegisterCreatedObjectUndo(objetoAcciones, "Crear controlador de acciones");
        }
        objetoAcciones.SetActive(true);
        controladorAcciones = Componente<ControladorAcciones>(objetoAcciones);
        controladorAcciones.enabled = true;
        var accionesSerializadas = new SerializedObject(controladorAcciones);
        accionesSerializadas.FindProperty("controladorSeleccion").objectReferenceValue = seleccion;
        accionesSerializadas.FindProperty("vistaHud").objectReferenceValue = hud;
        accionesSerializadas.FindProperty("conexionApi").objectReferenceValue = conexion;
        accionesSerializadas.ApplyModifiedProperties();
        var conexionSerializada = new SerializedObject(conexion);
        conexionSerializada.FindProperty("vistaHud").objectReferenceValue = hud;
        conexionSerializada.ApplyModifiedProperties();

        GameObject eventos = escena.GetRootGameObjects()
            .SelectMany(objeto => objeto.GetComponentsInChildren<EventSystem>(true))
            .Select(sistema => sistema.gameObject).FirstOrDefault();
        if (eventos == null)
        {
            eventos = Buscar(escena, "EventSystem");
            if (eventos == null)
            {
                eventos = new GameObject("EventSystem");
                SceneManager.MoveGameObjectToScene(eventos, escena);
                Undo.RegisterCreatedObjectUndo(eventos, "Crear EventSystem");
            }
        }
        eventos.SetActive(true);
        Componente<EventSystem>(eventos).enabled = true;
        foreach (StandaloneInputModule antiguo in eventos.GetComponents<StandaloneInputModule>())
        {
            Undo.DestroyObjectImmediate(antiguo);
        }
        var modulo = Componente<InputSystemUIInputModule>(eventos);
        modulo.enabled = true;
        if (modulo.actionsAsset == null) modulo.AssignDefaultActions();
        EditorUtility.SetDirty(hud);
        EditorUtility.SetDirty(canvas);
        EditorUtility.SetDirty(escalador);
    }

    private static GameObject Buscar(Scene escena, string nombre)
    {
        return escena.GetRootGameObjects()
            .SelectMany(raiz => raiz.GetComponentsInChildren<Transform>(true))
            .Select(elemento => elemento.gameObject).FirstOrDefault(objeto => objeto.name == nombre);
    }

    private static T Componente<T>(GameObject objeto) where T : Component
    {
        // Unity puede devolver un objeto que compara igual a null en el Editor.
        // ?? no utiliza esa comparación y puede omitir la creación del componente.
        T componente = objeto.GetComponent<T>();
        if (componente == null)
        {
            componente = Undo.AddComponent<T>(objeto);
        }
        return componente;
    }

    private static T BuscarComponente<T>(Scene escena) where T : Component
    {
        return escena.GetRootGameObjects()
            .SelectMany(raiz => raiz.GetComponentsInChildren<T>(true)).FirstOrDefault();
    }

    private static RectTransform Rect(Transform padre, string nombre, Vector2 minimo, Vector2 maximo,
        Vector2 pivote, Vector2 offsetMinimo, Vector2 offsetMaximo)
    {
        Transform existente = padre.Find(nombre);
        GameObject objeto = existente == null ? new GameObject(nombre, typeof(RectTransform)) : existente.gameObject;
        if (existente == null)
        {
            objeto.transform.SetParent(padre, false);
            Undo.RegisterCreatedObjectUndo(objeto, "Crear elemento HUD");
        }
        objeto.SetActive(true);
        var rect = Componente<RectTransform>(objeto);
        Undo.RecordObject(rect, "Configurar HUD");
        rect.anchorMin = minimo;
        rect.anchorMax = maximo;
        rect.pivot = pivote;
        // Reiniciar también la representación posición/tamaño de los objetos reutilizados.
        // Los offsets finales determinan ambos valores sin depender del estado anterior.
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.offsetMin = offsetMinimo;
        rect.offsetMax = offsetMaximo;
        rect.anchoredPosition3D = new Vector3(rect.anchoredPosition.x, rect.anchoredPosition.y, 0);
        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one;
        return rect;
    }

    private static void Fondo(RectTransform rect)
    {
        Image imagen = Componente<Image>(rect.gameObject);
        imagen.enabled = true;
        imagen.color = new Color(0.035f, 0.055f, 0.08f, 0.94f);
        imagen.raycastTarget = true;
        EditorUtility.SetDirty(imagen);
    }

    private static Text Texto(Transform padre, string nombre, string contenido, Vector2 offsetMinimo, Vector2 offsetMaximo)
    {
        RectTransform rect = Rect(padre, nombre, new Vector2(0, 1), new Vector2(1, 1),
            new Vector2(0.5f, 1), offsetMinimo, offsetMaximo);
        Text texto = Componente<Text>(rect.gameObject);
        texto.enabled = true;
        texto.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        texto.fontSize = 20;
        texto.color = Color.white;
        texto.alignment = TextAnchor.UpperLeft;
        texto.horizontalOverflow = HorizontalWrapMode.Wrap;
        texto.verticalOverflow = VerticalWrapMode.Truncate;
        texto.resizeTextForBestFit = true;
        texto.resizeTextMinSize = 14;
        texto.resizeTextMaxSize = 20;
        texto.supportRichText = false;
        texto.raycastTarget = false;
        texto.text = contenido;
        EditorUtility.SetDirty(texto);
        return texto;
    }
}
#endif
