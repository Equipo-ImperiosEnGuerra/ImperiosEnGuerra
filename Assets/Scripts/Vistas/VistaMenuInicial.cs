using System;
using UnityEngine;
using UnityEngine.UI;

namespace ImperiosEnGuerra.Vistas
{
    /// <summary>
    /// Vista del menú inicial. Solo construye/presenta UI y publica eventos.
    /// No inicia partidas ni contiene reglas de gameplay.
    /// </summary>
    public sealed class VistaMenuInicial : MonoBehaviour
    {
        private Canvas canvas;
        private GameObject panelPrincipal;
        private GameObject panelInstrucciones;
        private Text estado;
        private Button jugar;

        public event Action JugarSolicitado;
        public event Action InstruccionesSolicitadas;
        public event Action VolverSolicitado;
        public event Action SalirSolicitado;

        private void Awake()
        {
            Construir();
            MostrarPrincipal();
        }

        public void MostrarPrincipal()
        {
            if (panelPrincipal != null)
                panelPrincipal.SetActive(true);

            if (panelInstrucciones != null)
                panelInstrucciones.SetActive(false);

            gameObject.SetActive(true);
        }

        public void MostrarInstrucciones()
        {
            if (panelPrincipal != null)
                panelPrincipal.SetActive(false);

            if (panelInstrucciones != null)
                panelInstrucciones.SetActive(true);

            gameObject.SetActive(true);
        }

        public void Ocultar()
        {
            gameObject.SetActive(false);
        }

        public void EstablecerCargando(
            bool cargando)
        {
            if (jugar != null)
                jugar.interactable = !cargando;
        }

        public void MostrarEstado(
            string mensaje,
            bool error = false)
        {
            if (estado == null)
                return;

            estado.text =
                mensaje ?? string.Empty;

            estado.color =
                error
                    ? new Color(1f, 0.55f, 0.55f)
                    : new Color(0.82f, 0.9f, 1f);
        }

        private void Construir()
        {
            if (canvas != null)
                return;

            canvas =
                gameObject.AddComponent<Canvas>();

            canvas.renderMode =
                RenderMode.ScreenSpaceOverlay;

            canvas.sortingOrder = 500;

            CanvasScaler escalador =
                gameObject.AddComponent<CanvasScaler>();

            escalador.uiScaleMode =
                CanvasScaler.ScaleMode.ScaleWithScreenSize;

            escalador.referenceResolution =
                new Vector2(1280f, 720f);

            escalador.screenMatchMode =
                CanvasScaler.ScreenMatchMode.Expand;

            gameObject.AddComponent<GraphicRaycaster>();

            Image fondo =
                CrearImagen(
                    transform,
                    "Fondo",
                    Vector2.zero,
                    Vector2.one,
                    Vector2.zero,
                    Vector2.zero,
                    new Color(0.025f, 0.035f, 0.055f, 0.985f));

            fondo.raycastTarget = true;

            panelPrincipal =
                CrearPanelPrincipal(
                    fondo.transform);

            panelInstrucciones =
                CrearPanelInstrucciones(
                    fondo.transform);

            panelInstrucciones.SetActive(false);
        }

        private GameObject CrearPanelPrincipal(
            Transform padre)
        {
            GameObject panel =
                CrearContenedorCentrado(
                    padre,
                    "Principal",
                    new Vector2(650f, 520f));

            CrearTexto(
                panel.transform,
                "Titulo",
                "IMPERIOS EN GUERRA",
                48,
                new Vector2(0f, 180f),
                new Vector2(600f, 74f),
                TextAnchor.MiddleCenter);

            CrearTexto(
                panel.transform,
                "Subtitulo",
                "Construye, reúne recursos y conquista el campo de batalla",
                22,
                new Vector2(0f, 125f),
                new Vector2(560f, 45f),
                TextAnchor.MiddleCenter);

            jugar =
                CrearBoton(
                    panel.transform,
                    "Jugar",
                    "JUGAR",
                    new Vector2(0f, 35f));

            Button instrucciones =
                CrearBoton(
                    panel.transform,
                    "Instrucciones",
                    "INSTRUCCIONES",
                    new Vector2(0f, -35f));

            Button salir =
                CrearBoton(
                    panel.transform,
                    "Salir",
                    "SALIR",
                    new Vector2(0f, -105f));

            estado =
                CrearTexto(
                    panel.transform,
                    "Estado",
                    "",
                    17,
                    new Vector2(0f, -180f),
                    new Vector2(560f, 64f),
                    TextAnchor.MiddleCenter);

            jugar.onClick.AddListener(
                () => JugarSolicitado?.Invoke());

            instrucciones.onClick.AddListener(
                () => InstruccionesSolicitadas?.Invoke());

            salir.onClick.AddListener(
                () => SalirSolicitado?.Invoke());

            return panel;
        }

        private GameObject CrearPanelInstrucciones(
            Transform padre)
        {
            GameObject panel =
                CrearContenedorCentrado(
                    padre,
                    "Instrucciones",
                    new Vector2(820f, 650f));

            CrearTexto(
                panel.transform,
                "Titulo",
                "CÓMO JUGAR",
                38,
                new Vector2(0f, 265f),
                new Vector2(740f, 62f),
                TextAnchor.MiddleCenter);

            const string instrucciones =
                "OBJETIVO\n" +
                "Defiende tu imperio de tres facciones enemigas: Morada, Verde y Amarilla. Ganas al eliminar los Centros Urbanos y las unidades militares de las tres IAs.\n\n" +
                "CONQUISTA\n" +
                "Cuando eliminas por completo una facción enemiga, tu imperio establece automáticamente un nuevo Centro Urbano en la zona conquistada.\n\n" +
                "ECONOMÍA\n" +
                "Las cuatro facciones compiten por los mismos nodos de Oro, Madera y Comida. Cuando un nodo se agota, después de un tiempo reaparece otro del mismo tipo en una casilla libre del mapa.\n\n" +
                "ÓRDENES\n" +
                "Selecciona una unidad o edificio propio y elige una acción. Si el objetivo está lejos, la unidad se acercará automáticamente. Puedes cancelar una orden activa desde el panel de la unidad.\n\n" +
                "COMBATE Y APOYO\n" +
                "Guerreros, Lanceros y Arqueros atacan enemigos cercanos. Los Monjes curan aliados heridos y se acercan si es necesario. Las unidades con poca vida se resaltan en rojo.\n\n" +
                "ENTRENAMIENTO\n" +
                "El Centro Urbano muestra qué unidad está entrenando, su progreso y cuántas órdenes quedan en la cola.";

            Text cuerpo =
                CrearTexto(
                    panel.transform,
                    "Cuerpo",
                    instrucciones,
                    18,
                    new Vector2(0f, 18f),
                    new Vector2(740f, 470f),
                    TextAnchor.UpperLeft);

            cuerpo.horizontalOverflow =
                HorizontalWrapMode.Wrap;

            cuerpo.verticalOverflow =
                VerticalWrapMode.Truncate;

            Button volver =
                CrearBoton(
                    panel.transform,
                    "Volver",
                    "VOLVER",
                    new Vector2(0f, -265f));

            volver.onClick.AddListener(
                () => VolverSolicitado?.Invoke());

            return panel;
        }

        private static GameObject CrearContenedorCentrado(
            Transform padre,
            string nombre,
            Vector2 tamano)
        {
            GameObject objeto =
                new GameObject(
                    nombre,
                    typeof(RectTransform),
                    typeof(Image));

            objeto.transform.SetParent(
                padre,
                false);

            RectTransform rect =
                objeto.GetComponent<RectTransform>();

            rect.anchorMin =
                new Vector2(0.5f, 0.5f);

            rect.anchorMax =
                rect.anchorMin;

            rect.pivot =
                new Vector2(0.5f, 0.5f);

            rect.anchoredPosition =
                Vector2.zero;

            rect.sizeDelta =
                tamano;

            Image imagen =
                objeto.GetComponent<Image>();

            imagen.color =
                new Color(
                    0.055f,
                    0.08f,
                    0.12f,
                    0.97f);

            imagen.raycastTarget = true;

            return objeto;
        }

        private static Image CrearImagen(
            Transform padre,
            string nombre,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax,
            Color color)
        {
            GameObject objeto =
                new GameObject(
                    nombre,
                    typeof(RectTransform),
                    typeof(Image));

            objeto.transform.SetParent(
                padre,
                false);

            RectTransform rect =
                objeto.GetComponent<RectTransform>();

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            Image imagen =
                objeto.GetComponent<Image>();

            imagen.color = color;

            return imagen;
        }

        private static Button CrearBoton(
            Transform padre,
            string nombre,
            string etiqueta,
            Vector2 posicion)
        {
            GameObject objeto =
                new GameObject(
                    nombre,
                    typeof(RectTransform),
                    typeof(Image),
                    typeof(Button));

            objeto.transform.SetParent(
                padre,
                false);

            RectTransform rect =
                objeto.GetComponent<RectTransform>();

            rect.anchorMin =
                new Vector2(0.5f, 0.5f);

            rect.anchorMax =
                rect.anchorMin;

            rect.pivot =
                new Vector2(0.5f, 0.5f);

            rect.anchoredPosition =
                posicion;

            rect.sizeDelta =
                new Vector2(300f, 54f);

            Image imagen =
                objeto.GetComponent<Image>();

            imagen.color =
                new Color(
                    0.16f,
                    0.28f,
                    0.4f,
                    1f);

            Button boton =
                objeto.GetComponent<Button>();

            boton.targetGraphic = imagen;

            CrearTexto(
                objeto.transform,
                "Texto",
                etiqueta,
                22,
                Vector2.zero,
                new Vector2(290f, 48f),
                TextAnchor.MiddleCenter);

            return boton;
        }

        private static Text CrearTexto(
            Transform padre,
            string nombre,
            string contenido,
            int tamano,
            Vector2 posicion,
            Vector2 dimensiones,
            TextAnchor alineacion)
        {
            GameObject objeto =
                new GameObject(
                    nombre,
                    typeof(RectTransform),
                    typeof(Text));

            objeto.transform.SetParent(
                padre,
                false);

            RectTransform rect =
                objeto.GetComponent<RectTransform>();

            rect.anchorMin =
                new Vector2(0.5f, 0.5f);

            rect.anchorMax =
                rect.anchorMin;

            rect.pivot =
                new Vector2(0.5f, 0.5f);

            rect.anchoredPosition =
                posicion;

            rect.sizeDelta =
                dimensiones;

            Text texto =
                objeto.GetComponent<Text>();

            texto.font =
                Resources.GetBuiltinResource<Font>(
                    "LegacyRuntime.ttf");

            texto.fontSize = tamano;
            texto.color = Color.white;
            texto.text = contenido;
            texto.alignment = alineacion;
            texto.raycastTarget = false;
            texto.resizeTextForBestFit = true;
            texto.resizeTextMinSize = 12;
            texto.resizeTextMaxSize = tamano;

            return texto;
        }
    }
}
