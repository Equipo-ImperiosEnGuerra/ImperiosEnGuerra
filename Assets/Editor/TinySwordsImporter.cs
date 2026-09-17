#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public static class TinySwordsImporter
{
    internal const string RutaTilemap =
        "Assets/Art/TinySwords/Terrain/Tileset/Tilemap_color1.png";

    private const string NombreSuelo = "Tilemap_color1_9";

    [MenuItem("Tools/Imperios en Guerra/Configurar Tiny Swords")]
    public static void Configurar()
    {
        try
        {
            ConfigurarTilemap();
            Debug.Log("Tiny Swords configurado correctamente.");
        }
        catch (Exception ex)
        {
            Debug.LogError("No se pudo configurar Tiny Swords: " + ex.Message);
        }
    }

    internal static void ConfigurarAutomaticamente()
    {
        if (AssetDatabase.LoadAssetAtPath<Texture2D>(RutaTilemap) == null)
        {
            return;
        }

        if (TieneSpriteSuelo())
        {
            return;
        }

        try
        {
            ConfigurarTilemap();
            Debug.Log("Tiny Swords se configuró automáticamente.");
        }
        catch (Exception ex)
        {
            Debug.LogError("No se pudo configurar Tiny Swords automáticamente: " + ex.Message);
        }
    }

    private static void ConfigurarTilemap()
    {
        TextureImporter importer = AssetImporter.GetAtPath(RutaTilemap) as TextureImporter;
        if (importer == null)
        {
            throw new InvalidOperationException(
                "No se encontró Tilemap_color1.png. Instale Tiny Swords antes de configurar.");
        }

        bool requiereReimportacion = false;

        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            requiereReimportacion = true;
        }

        if (importer.spriteImportMode != SpriteImportMode.Multiple)
        {
            importer.spriteImportMode = SpriteImportMode.Multiple;
            requiereReimportacion = true;
        }

        if (Math.Abs(importer.spritePixelsPerUnit - 64f) > 0.01f)
        {
            importer.spritePixelsPerUnit = 64f;
            requiereReimportacion = true;
        }

        if (importer.mipmapEnabled)
        {
            importer.mipmapEnabled = false;
            requiereReimportacion = true;
        }

        if (!importer.alphaIsTransparency)
        {
            importer.alphaIsTransparency = true;
            requiereReimportacion = true;
        }

        if (requiereReimportacion)
        {
            importer.SaveAndReimport();
        }

        Texture2D textura = AssetDatabase.LoadAssetAtPath<Texture2D>(RutaTilemap);
        if (textura == null || textura.width < 128 || textura.height < 320)
        {
            throw new InvalidOperationException(
                "Tilemap_color1.png no tiene las dimensiones esperadas para Tiny Swords.");
        }

        if (TieneSpriteSuelo())
        {
            return;
        }

        var factory = new SpriteDataProviderFactories();
        factory.Init();

        ISpriteEditorDataProvider dataProvider =
            factory.GetSpriteEditorDataProviderFromObject(importer);

        if (dataProvider == null)
        {
            throw new InvalidOperationException(
                "Unity no pudo obtener el proveedor de datos del Sprite Editor.");
        }

        dataProvider.InitSpriteEditorDataProvider();

        var spriteRects = dataProvider.GetSpriteRects().ToList();
        spriteRects.RemoveAll(sprite => sprite.name == NombreSuelo);

        var spriteSuelo = new SpriteRect
        {
            name = NombreSuelo,
            rect = new Rect(64f, 256f, 64f, 64f),
            alignment = SpriteAlignment.Center,
            pivot = new Vector2(0.5f, 0.5f),
            spriteID = GUID.Generate()
        };

        spriteRects.Add(spriteSuelo);
        dataProvider.SetSpriteRects(spriteRects.ToArray());

        ISpriteNameFileIdDataProvider nombres =
            dataProvider.GetDataProvider<ISpriteNameFileIdDataProvider>();

        if (nombres == null)
        {
            throw new InvalidOperationException(
                "Unity no pudo actualizar los identificadores de los sub-sprites.");
        }

        var pares = nombres.GetNameFileIdPairs().ToList();
        pares.RemoveAll(par => par.name == NombreSuelo);
        pares.Add(new SpriteNameFileIdPair(spriteSuelo.name, spriteSuelo.spriteID));
        nombres.SetNameFileIdPairs(pares);

        dataProvider.Apply();
        importer.SaveAndReimport();

        if (!TieneSpriteSuelo())
        {
            throw new InvalidOperationException(
                "No se pudo crear el sub-sprite Tilemap_color1_9.");
        }
    }

    private static bool TieneSpriteSuelo()
    {
        return AssetDatabase.LoadAllAssetsAtPath(RutaTilemap)
            .OfType<Sprite>()
            .Any(sprite => sprite.name == NombreSuelo);
    }
}

public sealed class TinySwordsAssetPostprocessor : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        if (!importedAssets.Contains(TinySwordsImporter.RutaTilemap))
        {
            return;
        }

        EditorApplication.delayCall -= TinySwordsImporter.ConfigurarAutomaticamente;
        EditorApplication.delayCall += TinySwordsImporter.ConfigurarAutomaticamente;
    }
}
#endif
