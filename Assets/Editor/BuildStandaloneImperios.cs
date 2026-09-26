using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildStandaloneImperios
{
    private const string MenuBase =
        "Imperios en Guerra/Build/";

    [MenuItem(
        MenuBase + "Build plataforma actual")]
    public static void BuildPlataformaActual()
    {
        BuildTarget target =
            EditorUserBuildSettings
                .activeBuildTarget;

        if (!EsStandaloneSoportado(
                target))
        {
            throw new BuildFailedException(
                $"La plataforma activa '{target}' no es un Standalone de escritorio soportado.");
        }

        EjecutarBuild(
            target);
    }

    [MenuItem(
        MenuBase + "Build Windows x64")]
    public static void BuildWindows()
    {
        EjecutarBuild(
            BuildTarget.StandaloneWindows64);
    }

    [MenuItem(
        MenuBase + "Build Linux x64")]
    public static void BuildLinux()
    {
        EjecutarBuild(
            BuildTarget.StandaloneLinux64);
    }

    private static void EjecutarBuild(
        BuildTarget target)
    {
        string[] escenas =
            EditorBuildSettings.scenes
                .Where(
                    escena =>
                        escena.enabled)
                .Select(
                    escena =>
                        escena.path)
                .ToArray();

        if (escenas.Length == 0)
        {
            throw new BuildFailedException(
                "No hay escenas habilitadas en EditorBuildSettings.");
        }

        string raizProyecto =
            Directory.GetParent(
                    Application.dataPath)
                ?.FullName
            ?? throw new BuildFailedException(
                "No fue posible localizar la raíz del proyecto.");

        string salida =
            ObtenerRutaSalida(
                raizProyecto,
                target);

        string directorio =
            Path.GetDirectoryName(
                salida)
            ?? raizProyecto;

        Directory.CreateDirectory(
            directorio);

        var opciones =
            new BuildPlayerOptions
            {
                scenes =
                    escenas,

                locationPathName =
                    salida,

                target =
                    target,

                options =
                    BuildOptions.None
            };

        Debug.Log(
            $"BUILD_INICIO: {target} -> {salida}");

        BuildReport reporte =
            BuildPipeline.BuildPlayer(
                opciones);

        if (reporte.summary.result !=
            BuildResult.Succeeded)
        {
            throw new BuildFailedException(
                $"El build terminó con estado {reporte.summary.result}. " +
                $"Errores: {reporte.summary.totalErrors}. " +
                $"Advertencias: {reporte.summary.totalWarnings}.");
        }

        Debug.Log(
            $"BUILD_OK: {target} | " +
            $"{reporte.summary.totalSize} bytes | " +
            $"{reporte.summary.totalTime}.");
    }

    private static string ObtenerRutaSalida(
        string raizProyecto,
        BuildTarget target)
    {
        return target switch
        {
            BuildTarget.StandaloneWindows64 =>
                Path.Combine(
                    raizProyecto,
                    "Builds",
                    "Windows",
                    "ImperiosEnGuerra.exe"),

            BuildTarget.StandaloneLinux64 =>
                Path.Combine(
                    raizProyecto,
                    "Builds",
                    "Linux",
                    "ImperiosEnGuerra.x86_64"),

            BuildTarget.StandaloneOSX =>
                Path.Combine(
                    raizProyecto,
                    "Builds",
                    "macOS",
                    "ImperiosEnGuerra.app"),

            _ =>
                throw new BuildFailedException(
                    $"La plataforma '{target}' no está soportada por esta herramienta.")
        };
    }

    private static bool EsStandaloneSoportado(
        BuildTarget target)
    {
        return target ==
                   BuildTarget.StandaloneWindows64 ||
               target ==
                   BuildTarget.StandaloneLinux64 ||
               target ==
                   BuildTarget.StandaloneOSX;
    }
}
