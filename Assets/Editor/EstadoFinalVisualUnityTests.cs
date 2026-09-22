#if UNITY_EDITOR
using ImperiosEnGuerra.Controladores;
using ImperiosEnGuerra.Vistas;
using NUnit.Framework;
using UnityEngine;

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
