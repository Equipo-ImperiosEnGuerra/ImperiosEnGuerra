#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

/// <summary>
/// Herramienta exclusiva del Editor de Unity para configurar
/// automáticamente los sprites de Tiny Swords utilizados
/// por Imperios en Guerra.
///
/// No contiene lógica del juego.
/// Solo prepara recursos gráficos de la Vista.
/// </summary>
public static class TinySwordsSpriteConfigurator
{
    private const string RutaUnidades =
        "Assets/Art/TinySwords/Units";

    private const string RutaRecursos =
        "Assets/Art/TinySwords/Terrain/Resources";

    private const string RutaEdificios =
        "Assets/Art/TinySwords/Buildings";

    private const float PixelsPorUnidad = 64f;

    // Necesario para archivos grandes como Lancer_Idle (3840 px).
    private const int MaximoTamanoTextura = 4096;

    // =========================================================
    // MENÚ
    // =========================================================

    [MenuItem(
        "Tools/Imperios en Guerra/Tiny Swords/Configurar unidades"
    )]
    public static void ConfigurarUnidades()
    {
        ProcesarUnidades();
    }

    [MenuItem(
        "Tools/Imperios en Guerra/Tiny Swords/Configurar recursos"
    )]
    public static void ConfigurarRecursos()
    {
        ProcesarRecursos();
    }

    [MenuItem(
        "Tools/Imperios en Guerra/Tiny Swords/Configurar edificios"
    )]
    public static void ConfigurarEdificios()
    {
        ProcesarEdificios();
    }

    [MenuItem(
        "Tools/Imperios en Guerra/Tiny Swords/Configurar todo"
    )]
    public static void ConfigurarTodo()
    {
        Debug.Log(
            "[Tiny Swords] Iniciando configuración completa."
        );

        ProcesarUnidades();
        ProcesarRecursos();
        ProcesarEdificios();

        Debug.Log(
            "[Tiny Swords] Configuración completa finalizada."
        );
    }

    // =========================================================
    // UNIDADES
    // =========================================================

    private static void ProcesarUnidades()
    {
        string[] guids = AssetDatabase.FindAssets(
            "t:Texture2D",
            new[] { RutaUnidades }
        );

        int configurados = 0;
        int omitidos = 0;

        try
        {
            for (int i = 0; i < guids.Length; i++)
            {
                string ruta =
                    AssetDatabase.GUIDToAssetPath(guids[i]);

                if (!EsPng(ruta))
                {
                    continue;
                }

                EditorUtility.DisplayProgressBar(
                    "Configurando unidades Tiny Swords",
                    ruta,
                    CalcularProgreso(i, guids.Length)
                );

                TextureImporter importer =
                    ObtenerImporter(ruta);

                if (importer == null)
                {
                    omitidos++;
                    continue;
                }

                importer.GetSourceTextureWidthAndHeight(
                    out int ancho,
                    out int alto
                );

                // Arrow.png es un sprite individual de 64x64.
                if (ancho == 64 && alto == 64)
                {
                    ConfigurarSpriteIndividual(
                        importer,
                        ruta
                    );

                    configurados++;
                    continue;
                }

                int tamanoCelda =
                    ObtenerTamanoCeldaUnidad(alto);

                if (tamanoCelda == 0 ||
                    ancho % tamanoCelda != 0)
                {
                    Debug.LogWarning(
                        "[Tiny Swords] Unidad con formato " +
                        "no reconocido: " +
                        ruta +
                        " (" +
                        ancho +
                        "x" +
                        alto +
                        ")"
                    );

                    omitidos++;
                    continue;
                }

                int cantidadFrames =
                    ancho / tamanoCelda;

                ConfigurarSpriteMultiple(
                    importer,
                    ruta,
                    tamanoCelda,
                    tamanoCelda,
                    cantidadFrames
                );

                Debug.Log(
                    "[Tiny Swords] Unidad configurada: " +
                    ruta +
                    " | " +
                    cantidadFrames +
                    " frames de " +
                    tamanoCelda +
                    "x" +
                    tamanoCelda
                );

                configurados++;
            }
        }
        finally
        {
            FinalizarProcesamiento();
        }

        Debug.Log(
            "[Tiny Swords] Unidades terminadas. " +
            "Configurados: " +
            configurados +
            ". Omitidos: " +
            omitidos +
            "."
        );
    }

    private static int ObtenerTamanoCeldaUnidad(
        int alto)
    {
        // Warrior, Archer, Monk y Pawn.
        if (alto == 192)
        {
            return 192;
        }

        // Lancer.
        if (alto == 320)
        {
            return 320;
        }

        return 0;
    }

    // =========================================================
    // RECURSOS
    // =========================================================

    private static void ProcesarRecursos()
    {
        string[] guids = AssetDatabase.FindAssets(
            "t:Texture2D",
            new[] { RutaRecursos }
        );

        int configurados = 0;
        int omitidos = 0;

        try
        {
            for (int i = 0; i < guids.Length; i++)
            {
                string ruta =
                    AssetDatabase.GUIDToAssetPath(guids[i]);

                if (!EsPng(ruta))
                {
                    continue;
                }

                EditorUtility.DisplayProgressBar(
                    "Configurando recursos Tiny Swords",
                    ruta,
                    CalcularProgreso(i, guids.Length)
                );

                TextureImporter importer =
                    ObtenerImporter(ruta);

                if (importer == null)
                {
                    omitidos++;
                    continue;
                }

                importer.GetSourceTextureWidthAndHeight(
                    out int ancho,
                    out int alto
                );

                if (ConfigurarOro(
                        importer,
                        ruta,
                        ancho,
                        alto))
                {
                    configurados++;
                    continue;
                }

                if (ConfigurarOveja(
                        importer,
                        ruta,
                        ancho,
                        alto))
                {
                    configurados++;
                    continue;
                }

                if (ConfigurarArbolOTocon(
                        importer,
                        ruta,
                        ancho,
                        alto))
                {
                    configurados++;
                    continue;
                }

                Debug.LogWarning(
                    "[Tiny Swords] Recurso con formato " +
                    "no reconocido: " +
                    ruta +
                    " (" +
                    ancho +
                    "x" +
                    alto +
                    ")"
                );

                omitidos++;
            }
        }
        finally
        {
            FinalizarProcesamiento();
        }

        Debug.Log(
            "[Tiny Swords] Recursos terminados. " +
            "Configurados: " +
            configurados +
            ". Omitidos: " +
            omitidos +
            "."
        );
    }

    // =========================================================
    // ORO
    // =========================================================

    private static bool ConfigurarOro(
        TextureImporter importer,
        string ruta,
        int ancho,
        int alto)
    {
        if (!ruta.Contains(
                "/Gold/Gold Stones/",
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string nombre =
            Path.GetFileNameWithoutExtension(ruta);

        // Gold Stone 1..6: 128x128.
        if (!nombre.EndsWith(
                "_Highlight",
                StringComparison.OrdinalIgnoreCase) &&
            ancho == 128 &&
            alto == 128)
        {
            ConfigurarSpriteIndividual(
                importer,
                ruta
            );

            Debug.Log(
                "[Tiny Swords] Oro estático configurado: " +
                ruta
            );

            return true;
        }

        // Highlight: 768x128 = 6 frames.
        if (nombre.EndsWith(
                "_Highlight",
                StringComparison.OrdinalIgnoreCase) &&
            ancho == 768 &&
            alto == 128)
        {
            ConfigurarSpriteMultiple(
                importer,
                ruta,
                128,
                128,
                6
            );

            Debug.Log(
                "[Tiny Swords] Highlight de oro configurado: " +
                ruta +
                " | 6 frames de 128x128"
            );

            return true;
        }

        return false;
    }

    // =========================================================
    // OVEJAS
    // =========================================================

    private static bool ConfigurarOveja(
        TextureImporter importer,
        string ruta,
        int ancho,
        int alto)
    {
        if (!ruta.Contains(
                "/Meat/Sheep/",
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string nombre =
            Path.GetFileNameWithoutExtension(ruta);

        if (alto != 128)
        {
            return false;
        }

        if (nombre.Equals(
                "Sheep_Grass",
                StringComparison.OrdinalIgnoreCase) &&
            ancho == 1536)
        {
            ConfigurarSpriteMultiple(
                importer,
                ruta,
                128,
                128,
                12
            );

            Debug.Log(
                "[Tiny Swords] Sheep_Grass configurado: " +
                "12 frames de 128x128"
            );

            return true;
        }

        if (nombre.Equals(
                "Sheep_Idle",
                StringComparison.OrdinalIgnoreCase) &&
            ancho == 768)
        {
            ConfigurarSpriteMultiple(
                importer,
                ruta,
                128,
                128,
                6
            );

            Debug.Log(
                "[Tiny Swords] Sheep_Idle configurado: " +
                "6 frames de 128x128"
            );

            return true;
        }

        if (nombre.Equals(
                "Sheep_Move",
                StringComparison.OrdinalIgnoreCase) &&
            ancho == 512)
        {
            ConfigurarSpriteMultiple(
                importer,
                ruta,
                128,
                128,
                4
            );

            Debug.Log(
                "[Tiny Swords] Sheep_Move configurado: " +
                "4 frames de 128x128"
            );

            return true;
        }

        return false;
    }

    // =========================================================
    // ÁRBOLES Y TOCONES
    // =========================================================

    private static bool ConfigurarArbolOTocon(
        TextureImporter importer,
        string ruta,
        int ancho,
        int alto)
    {
        if (!ruta.Contains(
                "/Wood/Trees/",
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string nombre =
            Path.GetFileNameWithoutExtension(ruta);

        // Stump 1..4: sprite individual 192x256.
        if (nombre.StartsWith(
                "Stump ",
                StringComparison.OrdinalIgnoreCase) &&
            ancho == 192 &&
            alto == 256)
        {
            ConfigurarSpriteIndividual(
                importer,
                ruta
            );

            Debug.Log(
                "[Tiny Swords] Tocón configurado: " +
                ruta
            );

            return true;
        }

        // Tree1 y Tree2: 8 frames de 192x256.
        if ((nombre.Equals(
                 "Tree1",
                 StringComparison.OrdinalIgnoreCase) ||
             nombre.Equals(
                 "Tree2",
                 StringComparison.OrdinalIgnoreCase)) &&
            ancho == 1536 &&
            alto == 256)
        {
            ConfigurarSpriteMultiple(
                importer,
                ruta,
                192,
                256,
                8
            );

            Debug.Log(
                "[Tiny Swords] Árbol configurado: " +
                ruta +
                " | 8 frames de 192x256"
            );

            return true;
        }

        // Tree3 y Tree4: 8 frames de 192x192.
        if ((nombre.Equals(
                 "Tree3",
                 StringComparison.OrdinalIgnoreCase) ||
             nombre.Equals(
                 "Tree4",
                 StringComparison.OrdinalIgnoreCase)) &&
            ancho == 1536 &&
            alto == 192)
        {
            ConfigurarSpriteMultiple(
                importer,
                ruta,
                192,
                192,
                8
            );

            Debug.Log(
                "[Tiny Swords] Árbol configurado: " +
                ruta +
                " | 8 frames de 192x192"
            );

            return true;
        }

        return false;
    }

    // =========================================================
    // EDIFICIOS
    // =========================================================

    private static void ProcesarEdificios()
    {
        string[] guids = AssetDatabase.FindAssets(
            "t:Texture2D",
            new[] { RutaEdificios }
        );

        int configurados = 0;
        int omitidos = 0;

        try
        {
            for (int i = 0; i < guids.Length; i++)
            {
                string ruta =
                    AssetDatabase.GUIDToAssetPath(guids[i]);

                if (!EsPng(ruta))
                {
                    continue;
                }

                EditorUtility.DisplayProgressBar(
                    "Configurando edificios Tiny Swords",
                    ruta,
                    CalcularProgreso(i, guids.Length)
                );

                TextureImporter importer =
                    ObtenerImporter(ruta);

                if (importer == null)
                {
                    omitidos++;
                    continue;
                }

                importer.GetSourceTextureWidthAndHeight(
                    out int ancho,
                    out int alto
                );

                if (!EsEdificioConocido(
                        ancho,
                        alto))
                {
                    Debug.LogWarning(
                        "[Tiny Swords] Edificio con dimensiones " +
                        "no reconocidas: " +
                        ruta +
                        " (" +
                        ancho +
                        "x" +
                        alto +
                        ")"
                    );

                    omitidos++;
                    continue;
                }

                ConfigurarSpriteIndividual(
                    importer,
                    ruta
                );

                Debug.Log(
                    "[Tiny Swords] Edificio configurado: " +
                    ruta +
                    " (" +
                    ancho +
                    "x" +
                    alto +
                    ")"
                );

                configurados++;
            }
        }
        finally
        {
            FinalizarProcesamiento();
        }

        Debug.Log(
            "[Tiny Swords] Edificios terminados. " +
            "Configurados: " +
            configurados +
            ". Omitidos: " +
            omitidos +
            "."
        );
    }

    private static bool EsEdificioConocido(
        int ancho,
        int alto)
    {
        // Archery / Barracks
        if (ancho == 192 && alto == 256)
        {
            return true;
        }

        // Castle
        if (ancho == 320 && alto == 256)
        {
            return true;
        }

        // House1 / House2 / House3
        if (ancho == 128 && alto == 192)
        {
            return true;
        }

        // Monastery
        if (ancho == 192 && alto == 320)
        {
            return true;
        }

        // Tower
        if (ancho == 128 && alto == 256)
        {
            return true;
        }

        return false;
    }

    // =========================================================
    // CONFIGURACIÓN COMÚN
    // =========================================================

    private static void ConfigurarSpriteIndividual(
        TextureImporter importer,
        string ruta)
    {
        ConfigurarImportacionComun(
            importer,
            SpriteImportMode.Single
        );

        importer.SaveAndReimport();

        Debug.Log(
            "[Tiny Swords] Sprite individual configurado: " +
            ruta
        );
    }

    private static void ConfigurarSpriteMultiple(
        TextureImporter importer,
        string ruta,
        int anchoCelda,
        int altoCelda,
        int cantidadFrames)
    {
        ConfigurarImportacionComun(
            importer,
            SpriteImportMode.Multiple
        );

        importer.SaveAndReimport();

        importer =
            AssetImporter.GetAtPath(ruta)
            as TextureImporter;

        if (importer == null)
        {
            Debug.LogError(
                "[Tiny Swords] No se pudo recuperar " +
                "el importer después de reimportar: " +
                ruta
            );

            return;
        }

        ConfigurarCortes(
            importer,
            ruta,
            anchoCelda,
            altoCelda,
            cantidadFrames
        );
    }

    private static void ConfigurarImportacionComun(
        TextureImporter importer,
        SpriteImportMode modo)
    {
        TextureImporterSettings settings =
            new TextureImporterSettings();

        importer.ReadTextureSettings(settings);

        settings.spriteMeshType =
            SpriteMeshType.FullRect;

        settings.spriteGenerateFallbackPhysicsShape =
            false;

        settings.spritePixelsPerUnit =
            PixelsPorUnidad;

        importer.SetTextureSettings(settings);

        importer.textureType =
            TextureImporterType.Sprite;

        importer.spriteImportMode =
            modo;

        importer.spritePixelsPerUnit =
            PixelsPorUnidad;

        importer.wrapMode =
            TextureWrapMode.Clamp;

        importer.filterMode =
            FilterMode.Point;

        importer.textureCompression =
            TextureImporterCompression.Uncompressed;

        importer.mipmapEnabled =
            false;

        importer.alphaIsTransparency =
            true;

        importer.anisoLevel =
            0;

        importer.npotScale =
            TextureImporterNPOTScale.None;

        importer.maxTextureSize =
            MaximoTamanoTextura;
    }

    // =========================================================
    // CORTE DE SPRITES
    // =========================================================

    private static void ConfigurarCortes(
        TextureImporter importer,
        string ruta,
        int anchoCelda,
        int altoCelda,
        int cantidadFrames)
    {
        SpriteDataProviderFactories factory =
            new SpriteDataProviderFactories();

        factory.Init();

        ISpriteEditorDataProvider dataProvider =
            factory.GetSpriteEditorDataProviderFromObject(
                importer
            );

        if (dataProvider == null)
        {
            Debug.LogError(
                "[Tiny Swords] No se pudo obtener " +
                "el proveedor de sprites para: " +
                ruta
            );

            return;
        }

        dataProvider.InitSpriteEditorDataProvider();

        SpriteRect[] anteriores =
            dataProvider.GetSpriteRects();

        Dictionary<string, GUID> idsExistentes =
            anteriores
                .GroupBy(sprite => sprite.name)
                .ToDictionary(
                    grupo => grupo.Key,
                    grupo => grupo.First().spriteID
                );

        string nombreBase =
            Path.GetFileNameWithoutExtension(ruta);

        SpriteRect[] nuevos =
            new SpriteRect[cantidadFrames];

        for (int i = 0;
             i < cantidadFrames;
             i++)
        {
            string nombre =
                nombreBase + "_" + i;

            GUID spriteId;

            if (!idsExistentes.TryGetValue(
                    nombre,
                    out spriteId))
            {
                spriteId =
                    GUID.Generate();
            }

            nuevos[i] =
                new SpriteRect
                {
                    name = nombre,

                    spriteID = spriteId,

                    rect = new Rect(
                        i * anchoCelda,
                        0,
                        anchoCelda,
                        altoCelda
                    ),

                    alignment =
                        SpriteAlignment.Center,

                    pivot =
                        new Vector2(
                            0.5f,
                            0.5f
                        )
                };
        }

        dataProvider.SetSpriteRects(nuevos);

        ISpriteNameFileIdDataProvider nameProvider =
            dataProvider.GetDataProvider<
                ISpriteNameFileIdDataProvider
            >();

        if (nameProvider != null)
        {
            List<SpriteNameFileIdPair> pares =
                nuevos
                    .Select(
                        sprite =>
                            new SpriteNameFileIdPair(
                                sprite.name,
                                sprite.spriteID
                            )
                    )
                    .ToList();

            nameProvider.SetNameFileIdPairs(
                pares
            );
        }

        dataProvider.Apply();

        importer.SaveAndReimport();
    }

    // =========================================================
    // UTILIDADES
    // =========================================================

    private static TextureImporter ObtenerImporter(
        string ruta)
    {
        TextureImporter importer =
            AssetImporter.GetAtPath(ruta)
            as TextureImporter;

        if (importer == null)
        {
            Debug.LogWarning(
                "[Tiny Swords] No se pudo obtener " +
                "TextureImporter de: " +
                ruta
            );
        }

        return importer;
    }

    private static bool EsPng(
        string ruta)
    {
        return ruta.EndsWith(
            ".png",
            StringComparison.OrdinalIgnoreCase
        );
    }

    private static float CalcularProgreso(
        int indice,
        int total)
    {
        if (total <= 0)
        {
            return 1f;
        }

        return (float)indice / total;
    }

    private static void FinalizarProcesamiento()
    {
        EditorUtility.ClearProgressBar();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}

#endif