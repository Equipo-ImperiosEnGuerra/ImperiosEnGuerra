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
        private string accionPendiente;

        private bool EsperandoObjetivo =>
            !string.IsNullOrEmpty(unidadIdPendiente) &&
            !string.IsNullOrEmpty(accionPendiente);

        private void OnEnable()
        {
            if (controladorSeleccion != null)
            {
                controladorSeleccion.SeleccionCambio += ActualizarSeleccion;
                controladorSeleccion.DestinoSeleccionado += EnviarObjetivo;
                controladorSeleccion.CapturaCancelada += CancelarCaptura;
            }

            if (vistaHud != null)
                vistaHud.AccionSolicitada += PrepararAccion;

            ActualizarSeleccion(
                controladorSeleccion == null
                    ? null
                    : controladorSeleccion.SeleccionActual);
        }

        private void OnDisable()
        {
            LimpiarCaptura();

            if (controladorSeleccion != null)
            {
                controladorSeleccion.SeleccionCambio -= ActualizarSeleccion;
                controladorSeleccion.DestinoSeleccionado -= EnviarObjetivo;
                controladorSeleccion.CapturaCancelada -= CancelarCaptura;
            }

            if (vistaHud != null)
                vistaHud.AccionSolicitada -= PrepararAccion;
        }

        private void ActualizarSeleccion(EntidadSeleccionableVista entidad)
        {
            bool cancelar = EsperandoObjetivo;
            string accionCancelada = accionPendiente;

            if (cancelar)
                LimpiarCaptura();

            if (vistaHud == null)
                return;

            vistaHud.MostrarSeleccion(entidad);

            vistaHud.MostrarOpciones(
                PermiteOpcion(entidad, "Mover"),
                PermiteOpcion(entidad, "Recolectar"),
                PermiteOpcion(entidad, "Construir"),
                PermiteOpcion(entidad, "Entrenar"),
                PermiteOpcion(entidad, "Atacar"));

            vistaHud.MostrarMensaje(
                cancelar
                    ? ObtenerMensajeCancelacion(accionCancelada)
                    : "");
        }

        private void Update()
        {
            if (EsperandoObjetivo && !ConservaSeleccion())
                CancelarCaptura();
        }

        private bool ConservaSeleccion()
        {
            return controladorSeleccion != null &&
                controladorSeleccion.isActiveAndEnabled &&
                unidadPendiente != null &&
                unidadPendiente.isActiveAndEnabled &&
                controladorSeleccion.SeleccionActual == unidadPendiente &&
                unidadPendiente.IdLogico == unidadIdPendiente &&
                PermiteOpcion(unidadPendiente, accionPendiente);
        }

        private void LimpiarCaptura()
        {
            unidadIdPendiente = null;
            unidadPendiente = null;
            accionPendiente = null;

            if (controladorSeleccion != null)
                controladorSeleccion.FinalizarCapturaDestino();
        }

        private void CancelarCaptura()
        {
            string accionCancelada = accionPendiente;

            LimpiarCaptura();

            if (vistaHud != null)
                vistaHud.MostrarMensaje(
                    ObtenerMensajeCancelacion(accionCancelada));
        }

        private void EnviarObjetivo(int x, int y)
        {
            if (!EsperandoObjetivo)
                return;

            if (!ConservaSeleccion())
            {
                CancelarCaptura();
                return;
            }

            string id = unidadIdPendiente;
            string accion = accionPendiente;

            LimpiarCaptura();

            if (conexionApi == null || !conexionApi.isActiveAndEnabled)
            {
                if (vistaHud != null)
                {
                    vistaHud.MostrarMensaje(
                        "La conexión con la API no está disponible.",
                        true);
                }

                return;
            }

            if (accion == "Mover")
            {
                conexionApi.MoverUnidad(id, x, y);
                return;
            }

            if (accion == "Recolectar")
            {
                conexionApi.IniciarRecoleccion(id, x, y);
                return;
            }

            if (vistaHud != null)
            {
                vistaHud.MostrarMensaje(
                    "La acción preparada no reconoce un objetivo válido.",
                    true);
            }
        }

        private static bool PermiteOpcion(
            EntidadSeleccionableVista entidad,
            string accion)
        {
            if (entidad == null ||
                !entidad.isActiveAndEnabled ||
                entidad.Propietario != "Humano")
            {
                return false;
            }

            if (entidad.Categoria == CategoriaEntidadVisual.Edificio)
                return accion == "Entrenar";

            if (entidad.Categoria != CategoriaEntidadVisual.Unidad)
                return false;

            if (accion == "Mover")
                return true;

            if (accion == "Recolectar" || accion == "Construir")
                return entidad.TipoLogico == "Aldeano";

            return accion == "Atacar" &&
                (entidad.TipoLogico == "Guerrero" ||
                 entidad.TipoLogico == "Lancero" ||
                 entidad.TipoLogico == "Arquero" ||
                 entidad.TipoLogico == "Monje");
        }

        private void PrepararAccion(string accion)
        {
            if (vistaHud == null)
                return;

            LimpiarCaptura();

            var entidad =
                controladorSeleccion == null
                    ? null
                    : controladorSeleccion.SeleccionActual;

            if (!PermiteOpcion(entidad, accion))
            {
                vistaHud.MostrarMensaje(
                    "Selecciona una entidad humana apropiada para esta opción.",
                    true);

                return;
            }

            if (accion == "Mover" || accion == "Recolectar")
            {
                if (string.IsNullOrWhiteSpace(entidad.IdLogico))
                {
                    vistaHud.MostrarMensaje(
                        "La unidad seleccionada no tiene identidad disponible.",
                        true);

                    return;
                }

                if (conexionApi == null ||
                    !conexionApi.isActiveAndEnabled ||
                    conexionApi.AccionEnCurso)
                {
                    vistaHud.MostrarMensaje(
                        "La conexión no está disponible o hay una acción en curso.",
                        true);

                    return;
                }

                unidadPendiente = entidad;
                unidadIdPendiente = entidad.IdLogico;
                accionPendiente = accion;

                controladorSeleccion.IniciarCapturaDestino();

                vistaHud.MostrarMensaje(
                    accion == "Mover"
                        ? "Selecciona una casilla destino."
                        : "Selecciona un recurso.");

                return;
            }

            vistaHud.MostrarMensaje(
                $"Intención {accion} preparada. Ejecución pendiente de una fase posterior.");
        }

        private static string ObtenerMensajeCancelacion(string accion)
        {
            return accion == "Recolectar"
                ? "Recolección cancelada."
                : "Movimiento cancelado.";
        }
    }
}