#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Comprueba al abrir el proyecto si faltan los gráficos locales de Tiny Swords.
/// Si faltan, guía al usuario para seleccionar su copia del paquete y realiza
/// la instalación/configuración necesaria sin exponer herramientas de desarrollo
/// en el menú final del Editor.
/// </summary>
[InitializeOnLoad]
public static class TinySwordsBootstrap
{
    private const string ClaveSesion =
        "ImperiosEnGuerra.TinySwords.PreguntaInstalacionMostrada";

    private const string CarpetaPaquete =
        "Tiny Swords (Free Pack)";

    private const string DestinoRelativo =
        "Assets/Art/TinySwords";

    private static readonly string[] CarpetasNecesarias =
    {
        "Buildings/Blue Buildings",
        "Buildings/Red Buildings",
        "Terrain/Tileset",
        "Terrain/Resources/Gold/Gold Stones",
        "Terrain/Resources/Meat/Sheep",
        "Terrain/Resources/Wood/Trees",
        "Units/Blue Units",
        "Units/Red Units"
    };

    private static readonly string[] ArchivosRequeridos =
    {
        "Buildings/Blue Buildings/Castle.png",
        "Buildings/Red Buildings/Castle.png",
        "Terrain/Tileset/Tilemap_color1.png",
        "Terrain/Resources/Gold/Gold Stones/Gold Stone 1.png",
        "Terrain/Resources/Meat/Sheep/Sheep_Idle.png",
        "Terrain/Resources/Wood/Trees/Tree1.png",
        "Units/Blue Units/Warrior/Warrior_Idle.png",
        "Units/Blue Units/Lancer/Lancer_Idle.png",
        "Units/Blue Units/Archer/Archer_Idle.png",
        "Units/Blue Units/Monk/Idle.png",
        "Units/Blue Units/Pawn/Pawn_Idle.png",
        "Units/Red Units/Warrior/Warrior_Idle.png",
        "Units/Red Units/Lancer/Lancer_Idle.png",
        "Units/Red Units/Archer/Archer_Idle.png",
        "Units/Red Units/Monk/Idle.png",
        "Units/Red Units/Pawn/Pawn_Idle.png"
    };

    static TinySwordsBootstrap()
    {
        if (Application.isBatchMode)
        {
            return;
        }

        EditorApplication.delayCall +=
            VerificarInstalacionAlAbrir;
    }

    private static void VerificarInstalacionAlAbrir()
    {
        EditorApplication.delayCall -=
            VerificarInstalacionAlAbrir;

        if (EditorApplication.isPlayingOrWillChangePlaymode ||
            AssetsInstalados())
        {
            return;
        }

        if (SessionState.GetBool(
                ClaveSesion,
                false))
        {
            return;
        }

        SessionState.SetBool(
            ClaveSesion,
            true);

        bool instalar =
            EditorUtility.DisplayDialog(
                "Imperios en Guerra — Tiny Swords",
                "Faltan los gráficos locales de Tiny Swords.\n\n" +
                "Por licencia no se guardan en el repositorio. " +
                "Seleccione su carpeta 'Tiny Swords (Free Pack)' y Unity " +
                "copiará y configurará automáticamente solo los archivos " +
                "que necesita el proyecto.",
                "Seleccionar carpeta",
                "Ahora no");

        if (!instalar)
        {
            Debug.LogWarning(
                "Tiny Swords no está instalado. " +
                "El proyecto volverá a ofrecer la instalación al abrir Unity nuevamente.");

            return;
        }

        string seleccion =
            EditorUtility.OpenFolderPanel(
                "Seleccione Tiny Swords (Free Pack)",
                Environment.GetFolderPath(
                    Environment.SpecialFolder.UserProfile),
                string.Empty);

        if (string.IsNullOrWhiteSpace(
                seleccion))
        {
            return;
        }

        try
        {
            InstalarDesde(
                seleccion);

            EditorUtility.DisplayDialog(
                "Imperios en Guerra — Tiny Swords",
                "Tiny Swords quedó instalado y configurado correctamente.",
                "Aceptar");
        }
        catch (Exception ex)
        {
            SessionState.SetBool(
                ClaveSesion,
                false);

            Debug.LogError(
                "No se pudo instalar Tiny Swords automáticamente: " +
                ex.Message);

            EditorUtility.DisplayDialog(
                "No se pudo instalar Tiny Swords",
                ex.Message +
                "\n\nPuede volver a abrir el proyecto e intentarlo otra vez.",
                "Aceptar");
        }
    }

    internal static bool AssetsInstalados()
    {
        string destino =
            ObtenerDestinoAbsoluto();

        return ArchivosRequeridos.All(
            archivo =>
                File.Exists(
                    CombinarRutaRelativa(
                        destino,
                        archivo)));
    }

    internal static void InstalarDesde(
        string rutaSeleccionada)
    {
        string paquete =
            ResolverCarpetaPaquete(
                rutaSeleccionada);

        string destino =
            ObtenerDestinoAbsoluto();

        Directory.CreateDirectory(
            destino);

        foreach (string carpeta
                 in CarpetasNecesarias)
        {
            CopiarCarpetaGrafica(
                CombinarRutaRelativa(
                    paquete,
                    carpeta),
                CombinarRutaRelativa(
                    destino,
                    carpeta));
        }

        if (!AssetsInstalados())
        {
            throw new InvalidOperationException(
                "La copia terminó, pero faltan uno o más archivos requeridos de Tiny Swords.");
        }

        AssetDatabase.Refresh(
            ImportAssetOptions.ForceSynchronousImport);

        // Configura unidades, recursos y edificios; el importador del tileset
        // crea además el sub-sprite de suelo usado por la Vista.
        TinySwordsSpriteConfigurator.ConfigurarTodo();
        TinySwordsImporter.Configurar();

        bool sueloConfigurado =
            AssetDatabase
                .LoadAllAssetsAtPath(
                    TinySwordsImporter.RutaTilemap)
                .OfType<Sprite>()
                .Any(
                    sprite =>
                        sprite.name ==
                        "Tilemap_color1_9");

        if (!sueloConfigurado)
        {
            throw new InvalidOperationException(
                "Tiny Swords se copió, pero Unity no pudo configurar el sub-sprite de suelo requerido.");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            "Tiny Swords instalado y configurado automáticamente para Imperios en Guerra.");
    }

    internal static string ResolverCarpetaPaquete(
        string rutaSeleccionada)
    {
        if (string.IsNullOrWhiteSpace(
                rutaSeleccionada) ||
            !Directory.Exists(
                rutaSeleccionada))
        {
            throw new DirectoryNotFoundException(
                "La carpeta seleccionada no existe.");
        }

        string posibleSubcarpeta =
            Path.Combine(
                rutaSeleccionada,
                CarpetaPaquete);

        if (EsRaizValida(
                posibleSubcarpeta))
        {
            return posibleSubcarpeta;
        }

        if (EsRaizValida(
                rutaSeleccionada))
        {
            return rutaSeleccionada;
        }

        throw new InvalidOperationException(
            "La carpeta seleccionada no corresponde a Tiny Swords (Free Pack). " +
            "Debe contener las carpetas Buildings, Terrain y Units, " +
            "o contener directamente una subcarpeta llamada '" +
            CarpetaPaquete +
            "'.");
    }

    private static bool EsRaizValida(
        string ruta)
    {
        return Directory.Exists(
                   Path.Combine(
                       ruta,
                       "Buildings")) &&
               Directory.Exists(
                   Path.Combine(
                       ruta,
                       "Terrain")) &&
               Directory.Exists(
                   Path.Combine(
                       ruta,
                       "Units"));
    }

    private static void CopiarCarpetaGrafica(
        string origen,
        string destino)
    {
        if (!Directory.Exists(
                origen))
        {
            throw new DirectoryNotFoundException(
                "Falta la carpeta requerida: " +
                origen);
        }

        Directory.CreateDirectory(
            destino);

        IEnumerable<string> archivos =
            Directory.EnumerateFiles(
                    origen,
                    "*.*",
                    SearchOption.AllDirectories)
                .Where(
                    EsArchivoGrafico);

        foreach (string archivo
                 in archivos)
        {
            string relativo =
                archivo
                    .Substring(
                        origen.Length)
                    .TrimStart(
                        Path.DirectorySeparatorChar,
                        Path.AltDirectorySeparatorChar);

            string destinoArchivo =
                Path.Combine(
                    destino,
                    relativo);

            string directorioDestino =
                Path.GetDirectoryName(
                    destinoArchivo);

            if (!string.IsNullOrWhiteSpace(
                    directorioDestino))
            {
                Directory.CreateDirectory(
                    directorioDestino);
            }

            File.Copy(
                archivo,
                destinoArchivo,
                true);
        }
    }

    private static bool EsArchivoGrafico(
        string ruta)
    {
        string extension =
            Path.GetExtension(
                    ruta)
                .ToLowerInvariant();

        return extension == ".png" ||
               extension == ".jpg" ||
               extension == ".jpeg";
    }

    private static string ObtenerDestinoAbsoluto()
    {
        string raiz =
            Directory.GetParent(
                    Application.dataPath)
                ?.FullName
            ?? throw new InvalidOperationException(
                "No fue posible localizar la raíz del proyecto Unity.");

        return CombinarRutaRelativa(
            raiz,
            DestinoRelativo);
    }

    private static string CombinarRutaRelativa(
        string basePath,
        string relativa)
    {
        string[] partes =
            relativa
                .Split(
                    new[]
                    {
                        '/',
                        '\\'
                    },
                    StringSplitOptions.RemoveEmptyEntries);

        string resultado =
            basePath;

        foreach (string parte
                 in partes)
        {
            resultado =
                Path.Combine(
                    resultado,
                    parte);
        }

        return resultado;
    }
}
#endif
