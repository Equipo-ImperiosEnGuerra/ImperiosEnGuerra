#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImperiosEnGuerra.Controladores.Red;
using ImperiosEnGuerra.Controladores;
using ImperiosEnGuerra.Controladores.Red.Contratos;
using ImperiosEnGuerra.Vistas;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Fase2VistaConfigurator
{
    private const string RutaEscena = "Assets/Scenes/SampleScene.unity";
    private const string RutaArte = "Assets/Art/TinySwords/";
    private static GameObject contenidoPrueba;

    [MenuItem("Tools/Imperios en Guerra/Fase 2/Probar unidades visuales")]
    public static void ProbarUnidadesVisuales()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogError("La prueba visual solo puede ejecutarse fuera de Play.");
            return;
        }

        Scene escena = SceneManager.GetSceneByPath(RutaEscena);
        if (!escena.IsValid() || !escena.isLoaded)
        {
            Debug.LogError("Abra SampleScene y ejecute Configurar vista inicial antes de la prueba visual.");
            return;
        }

        GameObject objeto = BuscarObjeto(escena, "VistaPartida");
        VistaPartida vista = objeto == null ? null : objeto.GetComponent<VistaPartida>();
        if (vista == null)
        {
            Debug.LogError("No se encontró VistaPartida en SampleScene. Ejecute Configurar vista inicial.");
            return;
        }

        var dtoPrueba = new EstadoPartidaDto
        {
            estado = "prueba visual del Editor",
            mapa = new MapaEstadoDto
            {
                ancho = 10,
                alto = 10,
                recursos = Array.Empty<RecursoEstadoDto>()
            },
            jugadorHumano = CrearJugadorPrueba("Humano", 1),
            jugadorMaquina = CrearJugadorPrueba("Maquina", 8)
        };

        LimpiarPruebaVisual();
        vista.Renderizar(dtoPrueba);
        Transform generado = vista.transform.Find("ContenidoGenerado");
        if (generado != null)
        {
            contenidoPrueba = generado.gameObject;
            // La representación de prueba no debe persistir al guardar la escena.
            foreach (Transform elemento in contenidoPrueba.GetComponentsInChildren<Transform>(true))
            {
                elemento.gameObject.hideFlags |= HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            }
        }

        EditorApplication.playModeStateChanged -= AlCambiarModoPrueba;
        EditorApplication.playModeStateChanged += AlCambiarModoPrueba;
        AssemblyReloadEvents.beforeAssemblyReload -= LimpiarPruebaVisual;
        AssemblyReloadEvents.beforeAssemblyReload += LimpiarPruebaVisual;
        SceneView.RepaintAll();
        Debug.Log("Prueba visual temporal: cinco unidades humanas en y=1 y cinco de máquina en y=8. No se consultó la API.");
    }

    private static JugadorEstadoDto CrearJugadorPrueba(string tipo, int y)
    {
        string[] tipos = { "Aldeano", "Guerrero", "Lancero", "Arquero", "Monje" };
        return new JugadorEstadoDto
        {
            nombre = tipo,
            tipo = tipo,
            recursos = new RecursosJugadorEstadoDto(),
            edificios = Array.Empty<EdificioEstadoDto>(),
            unidades = tipos.Select((unidad, indice) => new UnidadEstadoDto
            {
                tipo = unidad,
                coordenada = new CoordenadaEstadoDto { x = indice + 1, y = y },
                disponible = true
            }).ToArray()
        };
    }

    private static void AlCambiarModoPrueba(PlayModeStateChange estado)
    {
        if (estado == PlayModeStateChange.ExitingEditMode)
        {
            LimpiarPruebaVisual();
        }
    }

    private static void LimpiarPruebaVisual()
    {
        if (contenidoPrueba != null)
        {
            UnityEngine.Object.DestroyImmediate(contenidoPrueba);
            contenidoPrueba = null;
        }
    }

    [MenuItem("Tools/Imperios en Guerra/Fase 2/Configurar vista inicial")]
    public static void Configurar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogError("Configure la vista fuera del modo Play.");
            return;
        }

        Dictionary<string, Sprite> sprites;
        try
        {
            // Validar todos los gráficos antes de cambiar la escena.
            sprites = CargarSprites();
        }
        catch (InvalidOperationException ex)
        {
            Debug.LogError(ex.Message);
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

        GameObject controladorObjeto = BuscarObjeto(escena, "ControladorAPI");
        ControladorConexionApi controlador = controladorObjeto == null
            ? null : controladorObjeto.GetComponent<ControladorConexionApi>();
        GameObject camaraObjeto = BuscarObjeto(escena, "Main Camera");
        Camera camara = camaraObjeto == null ? null : camaraObjeto.GetComponent<Camera>();
        if (controlador == null || camara == null)
        {
            Debug.LogError("SampleScene necesita ControladorAPI con ControladorConexionApi y Main Camera con Camera.");
            return;
        }

        GameObject vistaObjeto = BuscarObjeto(escena, "VistaPartida");
        if (vistaObjeto == null)
        {
            vistaObjeto = new GameObject("VistaPartida");
            SceneManager.MoveGameObjectToScene(vistaObjeto, escena);
            Undo.RegisterCreatedObjectUndo(vistaObjeto, "Crear VistaPartida");
        }

        VistaPartida vista = vistaObjeto.GetComponent<VistaPartida>();
        if (vista == null)
        {
            vista = Undo.AddComponent<VistaPartida>(vistaObjeto);
        }

        var vistaSerializada = new SerializedObject(vista);
        vistaSerializada.FindProperty("camara").objectReferenceValue = camara;
        vistaSerializada.FindProperty("espacioCasilla").floatValue = 2f;
        vistaSerializada.FindProperty("escalaRecursos").floatValue = 0.75f;
        vistaSerializada.FindProperty("escalaEdificios").floatValue = 0.58f;
        vistaSerializada.FindProperty("escalaUnidades").floatValue = 0.65f;
        foreach (var sprite in sprites)
        {
            vistaSerializada.FindProperty(sprite.Key).objectReferenceValue = sprite.Value;
        }
        vistaSerializada.ApplyModifiedProperties();

        var controladorSerializado = new SerializedObject(controlador);
        controladorSerializado.FindProperty("vistaPartida").objectReferenceValue = vista;
        controladorSerializado.FindProperty("urlBaseApi").stringValue = "http://localhost:5086";
        controladorSerializado.ApplyModifiedProperties();

        GameObject seleccionObjeto = BuscarObjeto(escena, "ControladorSeleccion");
        if (seleccionObjeto == null)
        {
            seleccionObjeto = new GameObject("ControladorSeleccion");
            SceneManager.MoveGameObjectToScene(seleccionObjeto, escena);
            Undo.RegisterCreatedObjectUndo(seleccionObjeto, "Crear ControladorSeleccion");
        }
        ControladorSeleccion seleccion = seleccionObjeto.GetComponent<ControladorSeleccion>();
        if (seleccion == null)
        {
            seleccion = Undo.AddComponent<ControladorSeleccion>(seleccionObjeto);
        }
        var seleccionSerializada = new SerializedObject(seleccion);
        seleccionSerializada.FindProperty("camara").objectReferenceValue = camara;
        seleccionSerializada.FindProperty("vistaPartida").objectReferenceValue = vista;
        seleccionSerializada.ApplyModifiedProperties();

        Fase3HudConfigurator.Configurar(escena, controlador, seleccion);

        EditorSceneManager.MarkSceneDirty(escena);
        if (!EditorSceneManager.SaveScene(escena))
        {
            Debug.LogError("No se pudo guardar SampleScene después de configurar la vista.");
            return;
        }
        Debug.Log("Vista inicial configurada. Inicie la API en http://localhost:5086 y ejecute Play.");
    }

    private static GameObject BuscarObjeto(Scene escena, string nombre)
    {
        return escena.GetRootGameObjects()
            .SelectMany(raiz => raiz.GetComponentsInChildren<Transform>(true))
            .Select(elemento => elemento.gameObject)
            .FirstOrDefault(objeto => objeto.name == nombre);
    }

    private static Dictionary<string, Sprite> CargarSprites()
    {
        var sprites = new Dictionary<string, Sprite>
        {
            // Baldosa central de césped: rectángulo (64, 256, 64, 64) en el atlas local.
            { "suelo", CargarMultiple("Terrain/Tileset/Tilemap_color1.png", "Tilemap_color1_9") },
            { "oro", CargarSingle("Terrain/Resources/Gold/Gold Stones/Gold Stone 1.png") },
            { "madera", CargarMultiple("Terrain/Resources/Wood/Trees/Tree1.png") },
            { "comida", CargarMultiple("Terrain/Resources/Meat/Sheep/Sheep_Idle.png") },
            { "centroHumano", CargarSingle("Buildings/Blue Buildings/Castle.png") },
            { "centroMaquina", CargarSingle("Buildings/Red Buildings/Castle.png") }
        };

        CargarUnidades(sprites, "Blue Units", "Humano");
        CargarUnidades(sprites, "Red Units", "Maquina");
        return sprites;
    }

    private static void CargarUnidades(
        Dictionary<string, Sprite> sprites, string color, string jugador)
    {
        string ruta = "Units/" + color + "/";
        sprites.Add("aldeano" + jugador, CargarMultiple(ruta + "Pawn/Pawn_Idle.png"));
        sprites.Add("guerrero" + jugador, CargarMultiple(ruta + "Warrior/Warrior_Idle.png"));
        sprites.Add("lancero" + jugador, CargarMultiple(ruta + "Lancer/Lancer_Idle.png"));
        sprites.Add("arquero" + jugador, CargarMultiple(ruta + "Archer/Archer_Idle.png"));
        sprites.Add("monje" + jugador, CargarMultiple(ruta + "Monk/Idle.png"));
    }

    private static Sprite CargarSingle(string rutaRelativa)
    {
        string ruta = ValidarArchivo(rutaRelativa);
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
        if (sprite == null)
        {
            throw new InvalidOperationException("No se encontró el Sprite Single importado: " + ruta);
        }
        return sprite;
    }

    private static Sprite CargarMultiple(string rutaRelativa, string nombre = null)
    {
        string ruta = ValidarArchivo(rutaRelativa);
        // Orden de lectura del atlas: fila superior y luego columna izquierda.
        Sprite sprite = AssetDatabase.LoadAllAssetsAtPath(ruta)
            .OfType<Sprite>()
            .OrderByDescending(elemento => elemento.rect.y)
            .ThenBy(elemento => elemento.rect.x)
            .ThenBy(elemento => elemento.name, StringComparer.Ordinal)
            .FirstOrDefault(elemento => nombre == null || elemento.name == nombre);
        if (sprite == null)
        {
            throw new InvalidOperationException(
                "No se encontró el sub-sprite importado " + (nombre ?? "(primer frame)") + ": " + ruta);
        }
        return sprite;
    }

    private static string ValidarArchivo(string rutaRelativa)
    {
        string ruta = RutaArte + rutaRelativa;
        if (!File.Exists(ruta))
        {
            throw new InvalidOperationException(
                "Faltan los gráficos locales de Tiny Swords. " +
                "Ejecute scripts/instalar_tinyswords.ps1 antes de configurar la vista. Falta: " + ruta);
        }
        return ruta;
    }
}
#endif
