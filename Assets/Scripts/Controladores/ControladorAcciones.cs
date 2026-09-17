using ImperiosEnGuerra.Vistas;
using UnityEngine;

namespace ImperiosEnGuerra.Controladores
{
    /// <summary>Coordina opciones de interfaz. Preparar una intención no autoriza ni ejecuta gameplay.</summary>
    public class ControladorAcciones : MonoBehaviour
    {
        [SerializeField] private ControladorSeleccion controladorSeleccion;
        [SerializeField] private VistaHud vistaHud;

        private void OnEnable()
        {
            if (controladorSeleccion != null)
                controladorSeleccion.SeleccionCambio += ActualizarSeleccion;
            if (vistaHud != null)
                vistaHud.AccionSolicitada += PrepararAccion;
            ActualizarSeleccion(controladorSeleccion == null ? null : controladorSeleccion.SeleccionActual);
        }

        private void OnDisable()
        {
            if (controladorSeleccion != null)
                controladorSeleccion.SeleccionCambio -= ActualizarSeleccion;
            if (vistaHud != null)
                vistaHud.AccionSolicitada -= PrepararAccion;
        }

        private void ActualizarSeleccion(EntidadSeleccionableVista entidad)
        {
            if (vistaHud == null) return;
            vistaHud.MostrarSeleccion(entidad);
            vistaHud.MostrarOpciones(
                PermiteOpcion(entidad, "Mover"), PermiteOpcion(entidad, "Recolectar"),
                PermiteOpcion(entidad, "Construir"), PermiteOpcion(entidad, "Entrenar"),
                PermiteOpcion(entidad, "Atacar"));
            vistaHud.MostrarMensaje("");
        }

        private static bool PermiteOpcion(EntidadSeleccionableVista entidad, string accion)
        {
            if (entidad == null || !entidad.isActiveAndEnabled || entidad.Propietario != "Humano")
                return false;

            if (entidad.Categoria == CategoriaEntidadVisual.Edificio)
                return accion == "Entrenar";
            if (entidad.Categoria != CategoriaEntidadVisual.Unidad)
                return false;

            if (accion == "Mover") return true;
            if (accion == "Recolectar" || accion == "Construir")
                return entidad.TipoLogico == "Aldeano";
            return accion == "Atacar" &&
                (entidad.TipoLogico == "Guerrero" || entidad.TipoLogico == "Lancero" ||
                 entidad.TipoLogico == "Arquero" || entidad.TipoLogico == "Monje");
        }

        private void PrepararAccion(string accion)
        {
            if (vistaHud == null) return;
            var entidad = controladorSeleccion == null ? null : controladorSeleccion.SeleccionActual;
            if (!PermiteOpcion(entidad, accion))
            {
                vistaHud.MostrarMensaje("Selecciona una entidad humana apropiada para esta opción.", true);
                return;
            }

            vistaHud.MostrarMensaje($"Intención {accion} preparada. Ejecución pendiente de una fase posterior.");
        }
    }
}
