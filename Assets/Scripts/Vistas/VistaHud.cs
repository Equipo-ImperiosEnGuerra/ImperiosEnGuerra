using System;
using UnityEngine;
using UnityEngine.UI;

namespace ImperiosEnGuerra.Vistas
{
    /// <summary>Muestra datos y opciones de interfaz; no consulta la API ni ejecuta acciones.</summary>
    public class VistaHud : MonoBehaviour
    {
        [SerializeField] private Text recursos;
        [SerializeField] private Text seleccion;
        [SerializeField] private Text mensaje;
        [SerializeField] private Button mover;
        [SerializeField] private Button recolectar;
        [SerializeField] private Button construir;
        [SerializeField] private Button entrenar;
        [SerializeField] private Button atacar;
        [SerializeField] private GameObject selectorEntrenamiento;
        [SerializeField] private Button entrenarAldeano;
        [SerializeField] private Button entrenarGuerrero;
        [SerializeField] private Button entrenarLancero;
        [SerializeField] private Button entrenarArquero;
        [SerializeField] private Button entrenarMonje;

        public event Action<string> AccionSolicitada;
        public event Action<string> TipoUnidadSolicitado;

        private void OnEnable()
        {
            if (mover != null) mover.onClick.AddListener(SolicitarMover);
            if (recolectar != null) recolectar.onClick.AddListener(SolicitarRecolectar);
            if (construir != null) construir.onClick.AddListener(SolicitarConstruir);
            if (entrenar != null) entrenar.onClick.AddListener(SolicitarEntrenar);
            if (atacar != null) atacar.onClick.AddListener(SolicitarAtacar);

            if (entrenarAldeano != null)
                entrenarAldeano.onClick.AddListener(SolicitarEntrenarAldeano);
            if (entrenarGuerrero != null)
                entrenarGuerrero.onClick.AddListener(SolicitarEntrenarGuerrero);

            if (entrenarLancero != null)
                entrenarLancero.onClick.AddListener(SolicitarEntrenarLancero);

            if (entrenarArquero != null)
                entrenarArquero.onClick.AddListener(SolicitarEntrenarArquero);

            if (entrenarMonje != null)
                entrenarMonje.onClick.AddListener(SolicitarEntrenarMonje);
        }

        private void OnDisable()
        {
            if (mover != null) mover.onClick.RemoveListener(SolicitarMover);
            if (recolectar != null) recolectar.onClick.RemoveListener(SolicitarRecolectar);
            if (construir != null) construir.onClick.RemoveListener(SolicitarConstruir);
            if (entrenar != null) entrenar.onClick.RemoveListener(SolicitarEntrenar);
            if (atacar != null) atacar.onClick.RemoveListener(SolicitarAtacar);

            if (entrenarAldeano != null)
                entrenarAldeano.onClick.RemoveListener(SolicitarEntrenarAldeano);

            if (entrenarGuerrero != null)
                entrenarGuerrero.onClick.RemoveListener(SolicitarEntrenarGuerrero);

            if (entrenarLancero != null)
                entrenarLancero.onClick.RemoveListener(SolicitarEntrenarLancero);

            if (entrenarArquero != null)
                entrenarArquero.onClick.RemoveListener(SolicitarEntrenarArquero);

            if (entrenarMonje != null)
                entrenarMonje.onClick.RemoveListener(SolicitarEntrenarMonje);
        }

        private void SolicitarMover() => AccionSolicitada?.Invoke("Mover");
        private void SolicitarRecolectar() => AccionSolicitada?.Invoke("Recolectar");
        private void SolicitarConstruir() => AccionSolicitada?.Invoke("Construir");
        private void SolicitarEntrenar() => AccionSolicitada?.Invoke("Entrenar");
        private void SolicitarAtacar() => AccionSolicitada?.Invoke("Atacar");

        private void SolicitarEntrenarAldeano() =>
            TipoUnidadSolicitado?.Invoke("Aldeano");
        private void SolicitarEntrenarGuerrero() =>
            TipoUnidadSolicitado?.Invoke("Guerrero");
        private void SolicitarEntrenarLancero() =>
            TipoUnidadSolicitado?.Invoke("Lancero");
        private void SolicitarEntrenarArquero() =>
            TipoUnidadSolicitado?.Invoke("Arquero");
        private void SolicitarEntrenarMonje() =>
            TipoUnidadSolicitado?.Invoke("Monje");
        public void MostrarRecursos(int oro, int madera, int comida)
        {
            if (recursos != null)
                recursos.text = $"Oro: {oro} | Madera: {madera} | Comida: {comida}";
        }

        public void MostrarSeleccion(EntidadSeleccionableVista entidad)
        {
            if (seleccion == null)
                return;

            if (entidad == null)
            {
                seleccion.text = "Sin selección";
                return;
            }

            string texto =
                $"{entidad.TipoLogico}\n" +
                $"Propietario: {entidad.Propietario}\n" +
                $"Coordenada: ({entidad.X},{entidad.Y})";

            if (entidad.Categoria == CategoriaEntidadVisual.Unidad)
            {
                string estado =
                    string.IsNullOrWhiteSpace(entidad.EstadoLogico)
                        ? "Desconocido"
                        : entidad.EstadoLogico;

                string orden =
                    string.IsNullOrWhiteSpace(entidad.OrdenActiva)
                        ? "Ninguna"
                        : entidad.OrdenActiva;

                texto +=
                    $"\nEstado: {estado}" +
                    $"\nOrden: {orden}";
            }

            if (entidad.Propietario == "Maquina")
                texto += " — Enemigo";

            seleccion.text = texto;
        }

        public void MostrarSelectorEntrenamiento(bool mostrar)
        {
            if (selectorEntrenamiento != null)
                selectorEntrenamiento.SetActive(mostrar);
        }

        public void MostrarOpciones(bool puedeMover, bool puedeRecolectar, bool puedeConstruir,
            bool puedeEntrenar, bool puedeAtacar)
        {
            if (mover != null) mover.gameObject.SetActive(puedeMover);
            if (recolectar != null) recolectar.gameObject.SetActive(puedeRecolectar);
            if (construir != null) construir.gameObject.SetActive(puedeConstruir);
            if (entrenar != null) entrenar.gameObject.SetActive(puedeEntrenar);
            if (atacar != null) atacar.gameObject.SetActive(puedeAtacar);
        }

        public void MostrarMensaje(string texto, bool error = false)
        {
            if (mensaje == null) return;
            mensaje.text = texto;
            mensaje.color = error ? new Color(1f, 0.55f, 0.55f) : Color.white;
        }
    }
}
