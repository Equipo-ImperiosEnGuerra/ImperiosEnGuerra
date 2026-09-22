using ImperiosEnGuerra.Vistas;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

namespace ImperiosEnGuerra.Controladores
{
    /// <summary>Coordina la selección local; no envía órdenes de gameplay.</summary>
    public class ControladorSeleccion : MonoBehaviour
    {
        [SerializeField] private Camera camara;
        [SerializeField] private VistaPartida vistaPartida;

        public EntidadSeleccionableVista SeleccionActual { get; private set; }
        public string IdUnidadSeleccionada =>
            SeleccionActual != null && SeleccionActual.Categoria == CategoriaEntidadVisual.Unidad
                ? SeleccionActual.IdLogico
                : string.Empty;
        public event System.Action<EntidadSeleccionableVista> SeleccionCambio;
        public bool CapturandoDestino { get; private set; }
        public bool CapturandoRecurso { get; private set; }
        public bool CapturandoObjetivoEntidad { get; private set; }
        public event System.Action<int, int> DestinoSeleccionado;
        public event System.Action<EntidadSeleccionableVista> ObjetivoEntidadSeleccionado;
        public event System.Action CapturaCancelada;
        private VistaPartida vistaSuscrita;

        public bool InteraccionBloqueada { get; private set; }

        public void BloquearInteraccion()
        {
            InteraccionBloqueada = true;
            CancelarCapturaDestino();
            LimpiarSeleccion();
        }

        public void DesbloquearInteraccion()
        {
            InteraccionBloqueada = false;
        }

        public void IniciarCapturaDestino()
        {
            CapturandoObjetivoEntidad = false;
            CapturandoRecurso = false;
            CapturandoDestino = true;
        }

        public void IniciarCapturaRecurso()
        {
            CapturandoObjetivoEntidad = false;
            CapturandoRecurso = true;
            CapturandoDestino = true;
        }

        public void FinalizarCapturaDestino()
        {
            CapturandoDestino = false;
            CapturandoRecurso = false;
        }

        public void IniciarCapturaObjetivoEntidad()
        {
            CapturandoDestino = false;
            CapturandoRecurso = false;
            CapturandoObjetivoEntidad = true;
        }

        public void FinalizarCapturaObjetivoEntidad() =>
            CapturandoObjetivoEntidad = false;

        private void CancelarCapturaDestino()
        {
            CapturandoDestino = false;
            CapturandoRecurso = false;
            CapturandoObjetivoEntidad = false;
            CapturaCancelada?.Invoke();
        }

        private void OnEnable()
        {
            vistaSuscrita = vistaPartida;
            if (vistaSuscrita != null)
            {
                vistaSuscrita.AntesDeLimpiarContenido += LimpiarSeleccion;
            }
            else
            {
                Debug.LogError("ControladorSeleccion necesita una VistaPartida configurada.", this);
            }
        }

        private void OnDisable()
        {
            if (vistaSuscrita != null)
            {
                vistaSuscrita.AntesDeLimpiarContenido -= LimpiarSeleccion;
            }
            vistaSuscrita = null;
            LimpiarSeleccion();
            FinalizarCapturaDestino();
            FinalizarCapturaObjetivoEntidad();
        }

        private void Update()
        {
            if (InteraccionBloqueada)
                return;

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (CapturandoDestino || CapturandoObjetivoEntidad)
                    CancelarCapturaDestino();
                else
                    LimpiarSeleccion();
                return;
            }

            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
            {
                return;
            }

            if (camara == null || vistaPartida == null)
            {
                Debug.LogError("Configure cámara y VistaPartida en ControladorSeleccion.", this);
                return;
            }

            Vector2 pantalla = Mouse.current.position.ReadValue();
            if (EventSystem.current != null)
            {
                var puntero = new PointerEventData(EventSystem.current) { position = pantalla };
                var resultados = new List<RaycastResult>();
                EventSystem.current.RaycastAll(puntero, resultados);
                if (resultados.Exists(resultado => resultado.module is GraphicRaycaster))
                {
                    return;
                }
            }
            if (!camara.pixelRect.Contains(pantalla))
            {
                if (!CapturandoDestino) LimpiarSeleccion();
                return;
            }

            // VistaPartida representa las entidades en el plano mundial z=0.
            Vector3 mundo = camara.ScreenToWorldPoint(
                new Vector3(pantalla.x, pantalla.y, -camara.transform.position.z));
            if (CapturandoDestino)
            {
                if (CapturandoRecurso)
                {
                    EntidadSeleccionableVista recurso =
                        ObtenerRecursoEn(mundo);

                    if (recurso != null)
                    {
                        DestinoSeleccionado?.Invoke(
                            recurso.X,
                            recurso.Y);
                    }

                    // En recolección solo una entidad Recurso válida puede
                    // convertirse en objetivo lógico.
                    return;
                }

                if (vistaPartida.TryObtenerCoordenadaLogica(mundo, out int x, out int y))
                    DestinoSeleccionado?.Invoke(x, y);
                // El clic de destino se consume incluso si queda fuera del mapa.
                return;
            }

            EntidadSeleccionableVista candidata = ObtenerEntidadEn(mundo);

            if (CapturandoObjetivoEntidad)
            {
                ObjetivoEntidadSeleccionado?.Invoke(candidata);
                return;
            }

            Seleccionar(candidata);
        }

        private EntidadSeleccionableVista ObtenerRecursoEn(
            Vector3 mundo)
        {
            Physics2D.SyncTransforms();

            EntidadSeleccionableVista candidata = null;
            float mejorDistancia = float.PositiveInfinity;

            foreach (Collider2D collider in Physics2D.OverlapPointAll(mundo))
            {
                var entidad =
                    collider.GetComponent<EntidadSeleccionableVista>();

                if (entidad == null ||
                    !entidad.isActiveAndEnabled ||
                    entidad.Categoria != CategoriaEntidadVisual.Recurso ||
                    !entidad.transform.IsChildOf(vistaPartida.transform) ||
                    entidad.Renderer == null ||
                    !entidad.Renderer.enabled)
                {
                    continue;
                }

                Vector2 centroVisual =
                    entidad.Renderer.bounds.center;

                float distancia =
                    ((Vector2)mundo - centroVisual).sqrMagnitude;

                if (candidata == null ||
                    distancia < mejorDistancia)
                {
                    candidata = entidad;
                    mejorDistancia = distancia;
                }
            }

            return candidata;
        }

        private EntidadSeleccionableVista ObtenerEntidadEn(Vector3 mundo)
        {
            Physics2D.SyncTransforms();
            EntidadSeleccionableVista candidata = null;

            foreach (Collider2D collider in Physics2D.OverlapPointAll(mundo))
            {
                var entidad = collider.GetComponent<EntidadSeleccionableVista>();
                if (entidad == null || !entidad.isActiveAndEnabled ||
                    !entidad.transform.IsChildOf(vistaPartida.transform) ||
                    entidad.Renderer == null || !entidad.Renderer.enabled)
                {
                    continue;
                }

                if (candidata == null ||
                    entidad.Renderer.sortingOrder > candidata.Renderer.sortingOrder ||
                    (entidad.Renderer.sortingOrder == candidata.Renderer.sortingOrder &&
                     entidad.transform.GetSiblingIndex() < candidata.transform.GetSiblingIndex()))
                {
                    candidata = entidad;
                }
            }

            return candidata;
        }

        private void Seleccionar(EntidadSeleccionableVista entidad)
        {
            if (SeleccionActual == entidad)
            {
                return;
            }

            LimpiarSeleccion();
            SeleccionActual = entidad;
            if (SeleccionActual != null)
            {
                SeleccionActual.MostrarSeleccion();
                string id = string.IsNullOrEmpty(entidad.IdLogico) ? string.Empty : $" id={entidad.IdLogico}";
                Debug.Log($"Seleccionado: {entidad.Categoria} {entidad.TipoLogico} {entidad.Propietario}{id} ({entidad.X},{entidad.Y})", entidad);
            }
            SeleccionCambio?.Invoke(SeleccionActual);
        }

        public void LimpiarSeleccion()
        {
            if (SeleccionActual != null)
            {
                SeleccionActual.OcultarSeleccion();
                Debug.Log("Selección limpiada", this);
            }
            SeleccionActual = null;
            SeleccionCambio?.Invoke(null);
        }
    }
}
