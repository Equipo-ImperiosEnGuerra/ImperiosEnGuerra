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
        private VistaPartida vistaSuscrita;

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
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
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
                LimpiarSeleccion();
                return;
            }

            // VistaPartida representa las entidades en el plano mundial z=0.
            Vector3 mundo = camara.ScreenToWorldPoint(
                new Vector3(pantalla.x, pantalla.y, -camara.transform.position.z));
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
                    // A igual orden: primero creado en su contenedor, según el orden del DTO.
                    // Cada categoría tiene un sortingOrder distinto y un único contenedor.
                    candidata = entidad;
                }
            }

            Seleccionar(candidata);
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
