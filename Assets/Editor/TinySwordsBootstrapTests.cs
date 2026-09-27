#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;
using NUnit.Framework;

/// <summary>
/// Comprueba que la instalación automática reconozca rutas válidas y rechace carpetas incorrectas.
/// </summary>
public class TinySwordsBootstrapTests
{
    private string temporal;

    //Cada prueba trabaja sobre una carpeta temporal independiente.
    [SetUp]
    public void Preparar()
    {
        temporal =
            Path.Combine(
                Path.GetTempPath(),
                "ImperiosEnGuerra_TinySwords_" +
                Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(
            temporal);
    }

    [TearDown]
    public void Limpiar()
    {
        if (Directory.Exists(
                temporal))
        {
            Directory.Delete(
                temporal,
                true);
        }
    }

    [Test]
    public void ResolverCarpetaPaquete_AceptaRaizDirecta()
    {
        CrearEstructuraValida(
            temporal);

        string resultado =
            TinySwordsBootstrap
                .ResolverCarpetaPaquete(
                    temporal);

        Assert.That(
            resultado,
            Is.EqualTo(temporal));
    }

    [Test]
    public void ResolverCarpetaPaquete_AceptaCarpetaPadre()
    {
        string paquete =
            Path.Combine(
                temporal,
                "Tiny Swords (Free Pack)");

        Directory.CreateDirectory(
            paquete);

        CrearEstructuraValida(
            paquete);

        string resultado =
            TinySwordsBootstrap
                .ResolverCarpetaPaquete(
                    temporal);

        Assert.That(
            resultado,
            Is.EqualTo(paquete));
    }

    [Test]
    public void ResolverCarpetaPaquete_RechazaCarpetaInvalida()
    {
        Assert.Throws<InvalidOperationException>(
            () =>
                TinySwordsBootstrap
                    .ResolverCarpetaPaquete(
                        temporal));
    }

    private static void CrearEstructuraValida(
        string raiz)
    {
        Directory.CreateDirectory(
            Path.Combine(
                raiz,
                "Buildings"));

        Directory.CreateDirectory(
            Path.Combine(
                raiz,
                "Terrain"));

        Directory.CreateDirectory(
            Path.Combine(
                raiz,
                "Units"));
    }
}
#endif
