#if UNITY_EDITOR
using ImperiosEnGuerra.Vistas;
using NUnit.Framework;
using UnityEngine;

public class MenuInicialUnityTests
{
    [Test]
    public void VistaMenuInicial_CreaOpcionesPrincipales()
    {
        var objeto =
            new GameObject(
                "MenuPrueba");

        try
        {
            objeto.AddComponent<VistaMenuInicial>();

            Assert.That(
                objeto.GetComponent<Canvas>(),
                Is.Not.Null);

            Assert.That(
                objeto.transform.Find(
                    "Fondo/Principal/Jugar"),
                Is.Not.Null);

            Assert.That(
                objeto.transform.Find(
                    "Fondo/Principal/Instrucciones"),
                Is.Not.Null);

            Assert.That(
                objeto.transform.Find(
                    "Fondo/Principal/Salir"),
                Is.Not.Null);
        }
        finally
        {
            Object.DestroyImmediate(
                objeto);
        }
    }

    [Test]
    public void VistaMenuInicial_PuedeMostrarInstruccionesYVolver()
    {
        var objeto =
            new GameObject(
                "MenuPrueba");

        try
        {
            VistaMenuInicial vista =
                objeto.AddComponent<VistaMenuInicial>();

            vista.MostrarInstrucciones();

            Assert.That(
                objeto.transform
                    .Find("Fondo/Instrucciones")
                    .gameObject.activeSelf,
                Is.True);

            vista.MostrarPrincipal();

            Assert.That(
                objeto.transform
                    .Find("Fondo/Principal")
                    .gameObject.activeSelf,
                Is.True);
        }
        finally
        {
            Object.DestroyImmediate(
                objeto);
        }
    }
}
#endif
