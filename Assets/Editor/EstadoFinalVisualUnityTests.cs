#if UNITY_EDITOR
using System.Reflection;
using ImperiosEnGuerra.Controladores;
using ImperiosEnGuerra.Controladores.Red;
using ImperiosEnGuerra.Controladores.Red.Contratos;
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
    public void FaccionMorada_MantieneVisibleElTinteDeVidaCritica()
    {
        MethodInfo metodoColor =
            typeof(VistaPartida)
                .GetMethod(
                    "ObtenerColorFaccion",
                    BindingFlags.Static |
                    BindingFlags.NonPublic);

        Assert.That(
            metodoColor,
            Is.Not.Null);

        Color morado =
            (Color)metodoColor.Invoke(
                null,
                new object[]
                {
                    new JugadorEstadoDto
                    {
                        tipo = "Maquina",
                        faccion = "Morada"
                    },
                    false
                });

        var objeto =
            new GameObject(
                "UnidadMoradaCritica",
                typeof(SpriteRenderer),
                typeof(EntidadSeleccionableVista));

        try
        {
            SpriteRenderer renderer =
                objeto.GetComponent<SpriteRenderer>();

            renderer.color =
                morado;

            EntidadSeleccionableVista entidad =
                objeto.GetComponent<EntidadSeleccionableVista>();

            entidad.Configurar(
                CategoriaEntidadVisual.Unidad,
                "morada-id",
                "Guerrero",
                "Maquina_Morada",
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
                Is.GreaterThan(renderer.color.b));

            Assert.That(
                renderer.color.r,
                Is.GreaterThan(renderer.color.g));
        }
        finally
        {
            Object.DestroyImmediate(
                objeto);
        }
    }

    [Test]
    public void VistaPartida_UsaEscalasVisualesAmpliadas()
    {
        var objeto =
            new GameObject(
                "VistaEscalaPrueba");

        try
        {
            VistaPartida vista =
                objeto.AddComponent<VistaPartida>();

            float recursos =
                (float)typeof(VistaPartida)
                    .GetField(
                        "escalaRecursos",
                        BindingFlags.Instance |
                        BindingFlags.NonPublic)
                    .GetValue(vista);

            float edificios =
                (float)typeof(VistaPartida)
                    .GetField(
                        "escalaEdificios",
                        BindingFlags.Instance |
                        BindingFlags.NonPublic)
                    .GetValue(vista);

            float unidades =
                (float)typeof(VistaPartida)
                    .GetField(
                        "escalaUnidades",
                        BindingFlags.Instance |
                        BindingFlags.NonPublic)
                    .GetValue(vista);

            Assert.That(
                recursos,
                Is.EqualTo(0.88f));

            Assert.That(
                edificios,
                Is.EqualTo(0.68f));

            Assert.That(
                unidades,
                Is.EqualTo(0.80f));
        }
        finally
        {
            Object.DestroyImmediate(
                objeto);
        }
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
