using ImperiosEnGuerra.Vistas;
using ImperiosEnGuerra.Controladores.Red;
using UnityEngine;

namespace ImperiosEnGuerra.Controladores
{
    /// <summary>
    /// Coordina opciones de interfaz.
    /// Preparar una intención no autoriza ni ejecuta gameplay.
    /// </summary>
    public class ControladorAcciones : MonoBehaviour
    {
        [SerializeField] private ControladorSeleccion controladorSeleccion;
        [SerializeField] private VistaHud vistaHud;
        [SerializeField] private ControladorConexionApi conexionApi;

        private string unidadIdPendiente;
        private EntidadSeleccionableVista unidadPendiente;
        private string accionPendiente;
        private EntidadSeleccionableVista edificioPendiente;
        private string tipoUnidadPendiente;

        private bool EsperandoObjetivo =>
            !string.IsNullOrEmpty(accionPendiente);

        private void OnEnable()
        {
            if (controladorSeleccion != null)
            {
                controladorSeleccion.SeleccionCambio += ActualizarSeleccion;
                controladorSeleccion.DestinoSeleccionado += EnviarObjetivo;
                controladorSeleccion.ObjetivoEntidadSeleccionado += EnviarObjetivoAtaque;
                controladorSeleccion.CapturaCancelada += CancelarCaptura;
            }

            if (vistaHud != null)
            {
                vistaHud.AccionSolicitada += PrepararAccion;
                vistaHud.TipoUnidadSolicitado += SeleccionarTipoUnidad;
            }

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
                controladorSeleccion.ObjetivoEntidadSeleccionado -= EnviarObjetivoAtaque;
                controladorSeleccion.CapturaCancelada -= CancelarCaptura;
            }

            if (vistaHud != null)
            {
                vistaHud.AccionSolicitada -= PrepararAccion;
                vistaHud.TipoUnidadSolicitado -= SeleccionarTipoUnidad;
            }
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
            if (controladorSeleccion == null ||
                !controladorSeleccion.isActiveAndEnabled)
            {
                return false;
            }

            if (accionPendiente == "Entrenar")
            {
                return edificioPendiente != null &&
                    edificioPendiente.isActiveAndEnabled &&
                    controladorSeleccion.SeleccionActual == edificioPendiente &&
                    PermiteOpcion(edificioPendiente, accionPendiente);
            }

            return unidadPendiente != null &&
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
            edificioPendiente = null;
            tipoUnidadPendiente = null;

            if (controladorSeleccion != null)
            {
                controladorSeleccion.FinalizarCapturaDestino();
                controladorSeleccion.FinalizarCapturaObjetivoEntidad();
            }

            if (vistaHud != null)
                vistaHud.MostrarSelectorEntrenamiento(false);
        }

        private void CancelarCaptura()
        {
            string accionCancelada = accionPendiente;

            LimpiarCaptura();

            if (vistaHud != null)
            {
                vistaHud.MostrarMensaje(
                    ObtenerMensajeCancelacion(accionCancelada));
            }
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
            string tipoUnidad = tipoUnidadPendiente;

            int edificioX =
                edificioPendiente != null
                    ? edificioPendiente.X
                    : 0;

            int edificioY =
                edificioPendiente != null
                    ? edificioPendiente.Y
                    : 0;

            LimpiarCaptura();

            if (conexionApi == null ||
                !conexionApi.isActiveAndEnabled)
            {
                if (vistaHud != null)
                {
                    vistaHud.MostrarMensaje(
                        "La conexión con la API no está disponible.",
                        true);
                }

                return;
            }

            if (accion == "Entrenar")
            {
                conexionApi.Entrenar(
                    edificioX,
                    edificioY,
                    tipoUnidad,
                    x,
                    y);

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

            if (accion == "Construir")
            {
                conexionApi.Construir(
                    id,
                    "CentroUrbano",
                    x,
                    y);

                return;
            }

            if (vistaHud != null)
            {
                vistaHud.MostrarMensaje(
                    "La acción preparada no reconoce un objetivo válido.",
                    true);
            }
        }

        private void EnviarObjetivoAtaque(EntidadSeleccionableVista objetivo)
        {
            if (accionPendiente != "Atacar")
                return;

            if (!ConservaSeleccion())
            {
                CancelarCaptura();
                return;
            }

            if (!EsObjetivoAtaqueValido(objetivo))
            {
                if (vistaHud != null)
                {
                    vistaHud.MostrarMensaje(
                        "Selecciona una unidad enemiga válida como objetivo.",
                        true);
                }

                return;
            }

            string atacanteId = unidadIdPendiente;
            string objetivoId = objetivo.IdLogico;

            LimpiarCaptura();

            if (conexionApi == null ||
                !conexionApi.isActiveAndEnabled)
            {
                if (vistaHud != null)
                {
                    vistaHud.MostrarMensaje(
                        "La conexión con la API no está disponible.",
                        true);
                }

                return;
            }

            conexionApi.Atacar(
                atacanteId,
                objetivoId);
        }

        private static bool EsObjetivoAtaqueValido(
            EntidadSeleccionableVista objetivo)
        {
            return objetivo != null &&
                objetivo.isActiveAndEnabled &&
                objetivo.Categoria == CategoriaEntidadVisual.Unidad &&
                objetivo.Propietario == "Maquina" &&
                !string.IsNullOrWhiteSpace(objetivo.IdLogico);
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
            {
                return accion == "Entrenar" &&
                    entidad.TipoLogico == "CentroUrbano";
            }

            if (entidad.Categoria != CategoriaEntidadVisual.Unidad)
                return false;

            if (accion == "Mover")
                return true;

            if (accion == "Recolectar" ||
                accion == "Construir")
            {
                return entidad.TipoLogico == "Aldeano";
            }

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

            if (accion == "Entrenar")
            {
                if (conexionApi == null ||
                    !conexionApi.isActiveAndEnabled ||
                    conexionApi.AccionEnCurso)
                {
                    vistaHud.MostrarMensaje(
                        "La conexión no está disponible o hay una acción en curso.",
                        true);

                    return;
                }

                edificioPendiente = entidad;
                accionPendiente = accion;

                vistaHud.MostrarSelectorEntrenamiento(true);

                vistaHud.MostrarMensaje(
                    "Selecciona el tipo de unidad a entrenar.");

                return;
            }

            if (accion == "Atacar")
            {
                if (string.IsNullOrWhiteSpace(entidad.IdLogico))
                {
                    vistaHud.MostrarMensaje(
                        "La unidad atacante no tiene identidad disponible.",
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

                controladorSeleccion.IniciarCapturaObjetivoEntidad();

                vistaHud.MostrarMensaje(
                    "Selecciona una unidad enemiga como objetivo.");

                return;
            }

            if (accion == "Mover" ||
                accion == "Recolectar" ||
                accion == "Construir")
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

                if (accion == "Mover")
                {
                    vistaHud.MostrarMensaje(
                        "Selecciona una casilla destino.");
                }
                else if (accion == "Recolectar")
                {
                    vistaHud.MostrarMensaje(
                        "Selecciona un recurso.");
                }
                else
                {
                    vistaHud.MostrarMensaje(
                        "Selecciona una casilla para construir el Centro Urbano.");
                }

                return;
            }

            vistaHud.MostrarMensaje(
                $"Intención {accion} preparada. Ejecución pendiente de una fase posterior.");
        }

        private void SeleccionarTipoUnidad(string tipoUnidad)
        {
            if (accionPendiente != "Entrenar" ||
                edificioPendiente == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(tipoUnidad))
                return;

            tipoUnidadPendiente = tipoUnidad;

            vistaHud.MostrarSelectorEntrenamiento(false);

            controladorSeleccion.IniciarCapturaDestino();

            vistaHud.MostrarMensaje(
                $"Selecciona una casilla para crear {tipoUnidad}.");
        }

        private static string ObtenerMensajeCancelacion(string accion)
        {
            if (accion == "Recolectar")
                return "Recolección cancelada.";

            if (accion == "Construir")
                return "Construcción cancelada.";

            if (accion == "Entrenar")
                return "Entrenamiento cancelado.";

            if (accion == "Atacar")
                return "Ataque cancelado.";

            return "Movimiento cancelado.";
        }
    }
}