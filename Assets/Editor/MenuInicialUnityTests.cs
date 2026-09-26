#if UNITY_EDITOR
using ImperiosEnGuerra.Vistas;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

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
    public void Instrucciones_UsanLenguajeDeJuego_NoTecnico()
    {
        var objeto =
            new GameObject(
                "MenuPrueba");

        try
        {
            objeto.AddComponent<VistaMenuInicial>();

            Text cuerpo =
                objeto.transform
                    .Find(
                        "Fondo/Instrucciones/Cuerpo")
                    .GetComponent<Text>();

            Assert.That(
                cuerpo.text,
                Does.Not.Contain("CONCURRENCIA"));

            Assert.That(
                cuerpo.text,
                Does.Not.Contain("Regla AND"));

            Assert.That(
                cuerpo.text,
                Does.Not.Contain("académico"));

            Assert.That(
                cuerpo.text,
                Does.Contain("Monjes"));

            Assert.That(
                cuerpo.text,
                Does.Contain("cancelar"));

            Assert.That(
                cuerpo.text,
                Does.Contain("tres facciones"));

            Assert.That(
                cuerpo.text,
                Does.Contain("Morada"));

            Assert.That(
                cuerpo.text,
                Does.Contain("reaparece"));

            Assert.That(
                cuerpo.text,
                Does.Contain("nuevo Centro Urbano"));
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
