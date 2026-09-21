using UnityEngine;

namespace ImperiosEnGuerra.Vistas
{
    public enum CategoriaEntidadVisual
    {
        Unidad,
        Edificio,
        Recurso
    }

    /// <summary>Metadatos y resaltado de una representación, sin reglas del juego.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class EntidadSeleccionableVista : MonoBehaviour
    {
        public CategoriaEntidadVisual Categoria { get; private set; }
        public string IdLogico { get; private set; }
        public string TipoLogico { get; private set; }
        public string Propietario { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public string EstadoLogico { get; private set; }
        public string OrdenActiva { get; private set; }

        public SpriteRenderer Renderer { get; private set; }

        private Color colorOriginal;
        private bool seleccionada;

        public void Configurar(
            CategoriaEntidadVisual categoria,
            string tipo,
            string propietario,
            int x,
            int y)
        {
            Configurar(
                categoria,
                string.Empty,
                tipo,
                propietario,
                x,
                y);
        }

        public void Configurar(
            CategoriaEntidadVisual categoria,
            string idLogico,
            string tipo,
            string propietario,
            int x,
            int y,
            string estadoLogico = "",
            string ordenActiva = "")
        {
            OcultarSeleccion();

            Categoria = categoria;
            IdLogico = idLogico ?? string.Empty;
            TipoLogico = tipo;
            Propietario = propietario;
            X = x;
            Y = y;

            EstadoLogico = estadoLogico ?? string.Empty;
            OrdenActiva = ordenActiva ?? string.Empty;

            Renderer = GetComponent<SpriteRenderer>();
            colorOriginal = Renderer.color;
        }

        public void ActualizarDatosLogicos(
            int x,
            int y,
            string estadoLogico,
            string ordenActiva)
        {
            X = x;
            Y = y;
            EstadoLogico = estadoLogico ?? string.Empty;
            OrdenActiva = ordenActiva ?? string.Empty;
        }

        public void MostrarSeleccion()
        {
            if (Renderer == null || seleccionada)
            {
                return;
            }

            colorOriginal = Renderer.color;
            // Atenuar verde produce un tinte visible conservando los canales rojo y azul.
            Renderer.color = colorOriginal * new Color(1f, 0.45f, 1f, 1f);
            seleccionada = true;
        }

        public void OcultarSeleccion()
        {
            if (seleccionada && Renderer != null)
            {
                Renderer.color = colorOriginal;
            }
            seleccionada = false;
        }

        private void OnDisable()
        {
            OcultarSeleccion();
        }
    }
}
