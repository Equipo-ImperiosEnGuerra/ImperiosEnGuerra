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
        public int VidaActual { get; private set; }
        public int VidaMaxima { get; private set; }
        public int Danio { get; private set; }
        public int Alcance { get; private set; }

        public SpriteRenderer Renderer { get; private set; }

        private const float UmbralVidaCritica = 0.25f;
        private const float VelocidadPulsoAtaque = 5f;
        private const float IntensidadMinimaAtaque = 0.28f;
        private const float IntensidadMaximaAtaque = 0.72f;

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
            string ordenActiva = "",
            int vidaActual = 0,
            int vidaMaxima = 0,
            int danio = 0,
            int alcance = 0)
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
            VidaActual = vidaActual;
            VidaMaxima = vidaMaxima;
            Danio = danio;
            Alcance = alcance;

            Renderer = GetComponent<SpriteRenderer>();
            colorOriginal = Renderer.color;

            AplicarColorVisual();
        }

        public void ActualizarDatosLogicos(
            int x,
            int y,
            string estadoLogico,
            string ordenActiva,
            int vidaActual = 0,
            int vidaMaxima = 0,
            int danio = 0,
            int alcance = 0)
        {
            X = x;
            Y = y;
            EstadoLogico = estadoLogico ?? string.Empty;
            OrdenActiva = ordenActiva ?? string.Empty;

            // Las actualizaciones incrementales históricas de movimiento no
            // incluyen stats de combate. En ese caso se conserva la vida.
            if (vidaMaxima > 0)
            {
                VidaActual = vidaActual;
                VidaMaxima = vidaMaxima;
                Danio = danio;
                Alcance = alcance;
            }

            AplicarColorVisual();
        }

        private void Update()
        {
            // Feedback puramente visual. El estado autoritativo sigue
            // llegando desde el Modelo mediante los snapshots de la API.
            if (Renderer != null &&
                EsAtacando())
            {
                AplicarColorVisual();
            }
        }

        public void MostrarSeleccion()
        {
            if (Renderer == null || seleccionada)
                return;

            seleccionada = true;
            AplicarColorVisual();
        }

        public void OcultarSeleccion()
        {
            seleccionada = false;
            AplicarColorVisual();
        }

        private void AplicarColorVisual()
        {
            if (Renderer == null)
                return;

            Color visual =
                colorOriginal;

            if (EsVidaCritica())
            {
                // Feedback puramente visual: no modifica daño, vida ni reglas.
                visual =
                    Color.Lerp(
                        colorOriginal,
                        new Color(1f, 0.1f, 0.1f, colorOriginal.a),
                        0.72f);
            }

            if (EsAtacando())
            {
                float pulso =
                    (Mathf.Sin(
                         Time.unscaledTime *
                         VelocidadPulsoAtaque) +
                     1f) *
                    0.5f;

                float intensidad =
                    Mathf.Lerp(
                        IntensidadMinimaAtaque,
                        IntensidadMaximaAtaque,
                        pulso);

                visual =
                    Color.Lerp(
                        visual,
                        new Color(
                            1f,
                            0.62f,
                            0.08f,
                            visual.a),
                        intensidad);
            }

            if (seleccionada)
            {
                visual *=
                    new Color(
                        1f,
                        0.58f,
                        1f,
                        1f);
            }

            Renderer.color = visual;
        }

        private bool EsAtacando()
        {
            return Categoria ==
                       CategoriaEntidadVisual.Unidad &&
                   (EstadoLogico == "Atacando" ||
                    OrdenActiva == "Atacar");
        }

        private bool EsVidaCritica()
        {
            return VidaMaxima > 0 &&
                   VidaActual > 0 &&
                   VidaActual <=
                       Mathf.CeilToInt(
                           VidaMaxima *
                           UmbralVidaCritica);
        }

        private void OnDisable()
        {
            seleccionada = false;

            if (Renderer != null)
                Renderer.color = colorOriginal;
        }
    }
}
