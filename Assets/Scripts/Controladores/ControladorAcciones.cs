using ImperiosEnGuerra.Vistas;
using ImperiosEnGuerra.Controladores.Red;
using UnityEngine;

namespace ImperiosEnGuerra.Controladores
{
    /// <summary>Coordina opciones de interfaz. Preparar una intención no autoriza ni ejecuta gameplay.</summary>
    public class ControladorAcciones : MonoBehaviour
    {
        [SerializeField] private ControladorSeleccion controladorSeleccion;
        [SerializeField] private VistaHud vistaHud;
        [SerializeField] private ControladorConexionApi conexionApi;
        private string unidadIdPendiente;
        private EntidadSeleccionableVista unidadPendiente;

        private bool EsperandoDestino => !string.IsNullOrEmpty(unidadIdPendiente);

        private void OnEnable()
        {
            if (controladorSeleccion != null)
            {
                controladorSeleccion.SeleccionCambio += ActualizarSeleccion;
                controladorSeleccion.DestinoSeleccionado += EnviarMovimiento;
                controladorSeleccion.CapturaCancelada += CancelarMovimiento;
            }
            if (vistaHud != null)
                vistaHud.AccionSolicitada += PrepararAccion;
            ActualizarSeleccion(controladorSeleccion == null ? null : controladorSeleccion.SeleccionActual);
        }

        private void OnDisable()
        {
            LimpiarMovimiento();
            if (controladorSeleccion != null)
            {
                controladorSeleccion.SeleccionCambio -= ActualizarSeleccion;
                controladorSeleccion.DestinoSeleccionado -= EnviarMovimiento;
                controladorSeleccion.CapturaCancelada -= CancelarMovimiento;
            }
            if (vistaHud != null)
                vistaHud.AccionSolicitada -= PrepararAccion;
        }

        private void ActualizarSeleccion(EntidadSeleccionableVista entidad)
        {
            bool cancelar = EsperandoDestino;
            if (cancelar) LimpiarMovimiento();
            if (vistaHud == null) return;
            vistaHud.MostrarSeleccion(entidad);
            vistaHud.MostrarOpciones(
                PermiteOpcion(entidad, "Mover"), PermiteOpcion(entidad, "Recolectar"),
                PermiteOpcion(entidad, "Construir"), PermiteOpcion(entidad, "Entrenar"),
                PermiteOpcion(entidad, "Atacar"));
            vistaHud.MostrarMensaje(cancelar ? "Movimiento cancelado." : "");
        }

        private void Update()
        {
            if (EsperandoDestino && !ConservaSeleccion()) CancelarMovimiento();
        }

        private bool ConservaSeleccion()
        {
            return controladorSeleccion != null && controladorSeleccion.isActiveAndEnabled &&
                unidadPendiente != null && unidadPendiente.isActiveAndEnabled &&
                controladorSeleccion.SeleccionActual == unidadPendiente &&
                unidadPendiente.IdLogico == unidadIdPendiente && PermiteOpcion(unidadPendiente, "Mover");
        }

        private void LimpiarMovimiento()
        {
            unidadIdPendiente = null;
            unidadPendiente = null;
            if (controladorSeleccion != null) controladorSeleccion.FinalizarCapturaDestino();
        }

        private void CancelarMovimiento()
        {
            LimpiarMovimiento();
            if (vistaHud != null) vistaHud.MostrarMensaje("Movimiento cancelado.");
        }

        private void EnviarMovimiento(int x, int y)
        {
            if (!EsperandoDestino) return;
            if (!ConservaSeleccion())
            {
                CancelarMovimiento();
                return;
            }
            string id = unidadIdPendiente;
            LimpiarMovimiento();
            if (conexionApi == null || !conexionApi.isActiveAndEnabled)
            {
                if (vistaHud != null) vistaHud.MostrarMensaje("La conexión con la API no está disponible.", true);
                return;
            }
            conexionApi.MoverUnidad(id, x, y);
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
            LimpiarMovimiento();
            var entidad = controladorSeleccion == null ? null : controladorSeleccion.SeleccionActual;
            if (!PermiteOpcion(entidad, accion))
            {
                vistaHud.MostrarMensaje("Selecciona una entidad humana apropiada para esta opción.", true);
                return;
            }

            if (accion == "Mover")
            {
                if (string.IsNullOrWhiteSpace(entidad.IdLogico))
                {
                    vistaHud.MostrarMensaje("La unidad seleccionada no tiene identidad disponible.", true);
                    return;
                }
                if (conexionApi == null || !conexionApi.isActiveAndEnabled || conexionApi.MovimientoEnCurso)
                {
                    vistaHud.MostrarMensaje("La conexión no está disponible o hay un movimiento en curso.", true);
                    return;
                }
                unidadPendiente = entidad;
                unidadIdPendiente = entidad.IdLogico;
                controladorSeleccion.IniciarCapturaDestino();
                vistaHud.MostrarMensaje("Selecciona una casilla destino.");
                return;
            }
            vistaHud.MostrarMensaje($"Intención {accion} preparada. Ejecución pendiente de una fase posterior.");
        }
    }
}
