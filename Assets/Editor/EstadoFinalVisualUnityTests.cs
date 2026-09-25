#if UNITY_EDITOR
using System.Reflection;
using ImperiosEnGuerra.Controladores;
using ImperiosEnGuerra.Controladores.Red;
using ImperiosEnGuerra.Vistas;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class EstadoFinalVisualUnityTests
{
    [Test]
    public void VidaCritica_TinteRojo_PersisteSinSeleccion()
    {
        var objeto =
            new GameObject(
                "EntidadPrueba",
                typeof(SpriteRenderer),
                typeof(EntidadSeleccionableVista));

        try
        {
            SpriteRenderer renderer =
                objeto.GetComponent<SpriteRenderer>();

            renderer.color =
                Color.white;

            EntidadSeleccionableVista entidad =
                objeto.GetComponent<EntidadSeleccionableVista>();

            entidad.Configurar(
                CategoriaEntidadVisual.Unidad,
                "id",
                "Guerrero",
                "Humano",
                1,
                1,
                "Idle",
                "",
                20,
                120,
                30,
                1);

            Assert.That(
                renderer.color.r,
                Is.GreaterThan(renderer.color.g));

            Assert.That(
                renderer.color.r,
                Is.GreaterThan(renderer.color.b));
        }
        finally
        {
            Object.DestroyImmediate(
                objeto);
        }
    }

    [Test]
    public void Atacando_AplicaResaltadoVisual()
    {
        var objeto =
            new GameObject(
                "EntidadAtaquePrueba",
                typeof(SpriteRenderer),
                typeof(EntidadSeleccionableVista));

        try
        {
            SpriteRenderer renderer =
                objeto.GetComponent<SpriteRenderer>();

            renderer.color =
                Color.white;

            EntidadSeleccionableVista entidad =
                objeto.GetComponent<EntidadSeleccionableVista>();

            entidad.Configurar(
                CategoriaEntidadVisual.Unidad,
                "id-ataque",
                "Guerrero",
                "Humano",
                1,
                1,
                "Atacando",
                "Atacar",
                100,
                120,
                30,
                1);

            Assert.That(
                renderer.color,
                Is.Not.EqualTo(Color.white));
        }
        finally
        {
            Object.DestroyImmediate(
                objeto);
        }
    }

    [Test]
    public void ResultadoFinal_NoMuestraReglasTecnicas()
    {
        var objeto =
            new GameObject(
                "HudPrueba",
                typeof(RectTransform));

        try
        {
            VistaHud hud =
                objeto.AddComponent<VistaHud>();

            hud.MostrarResultadoFinal(
                "Maquina",
                "CPU",
                "Regla AND cumplida: mensaje técnico de prueba.");

            Text detalle =
                objeto.transform
                    .Find(
                        "PantallaResultadoFinal/Detalle")
                    .GetComponent<Text>();

            Assert.That(
                detalle.text,
                Does.Not.Contain("AND"));

            Assert.That(
                detalle.text,
                Does.Not.Contain("regla"));

            Assert.That(
                detalle.text,
                Does.Contain("Tu imperio ha caído"));
        }
        finally
        {
            Object.DestroyImmediate(
                objeto);
        }
    }

    [Test]
    public void ErrorTecnico_SeTraduceAntesDeLlegarAlJugador()
    {
        MethodInfo metodo =
            typeof(ControladorConexionApi)
                .GetMethod(
                    "TraducirMensajeParaJugador",
                    BindingFlags.Static |
                    BindingFlags.NonPublic);

        Assert.That(
            metodo,
            Is.Not.Null);

        string mensaje =
            (string)metodo.Invoke(
                null,
                new object[]
                {
                    "El worker concurrente falló con HTTP 500."
                });

        Assert.That(
            mensaje,
            Does.Not.Contain("worker"));

        Assert.That(
            mensaje,
            Does.Not.Contain("HTTP"));

        Assert.That(
            mensaje,
            Does.Not.Contain("concurrente"));

        Assert.That(
            mensaje,
            Does.Contain("Inténtalo de nuevo"));
    }

    [Test]
    public void BloqueoFinal_ControladorSeleccion_QuedaInactivoParaClicks()
    {
        var objeto =
            new GameObject(
                "SeleccionPrueba",
                typeof(ControladorSeleccion));

        try
        {
            ControladorSeleccion controlador =
                objeto.GetComponent<ControladorSeleccion>();

            controlador.BloquearInteraccion();

            Assert.That(
                controlador.InteraccionBloqueada,
                Is.True);

            controlador.DesbloquearInteraccion();

            Assert.That(
                controlador.InteraccionBloqueada,
                Is.False);
        }
        finally
        {
            Object.DestroyImmediate(
                objeto);
        }
    }
}
#endif
