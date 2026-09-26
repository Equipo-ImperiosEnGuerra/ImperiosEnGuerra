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
                    new Vector2(920f, 700f));

            CrearTexto(
                panel.transform,
                "Titulo",
                "CÓMO JUGAR",
                38,
                new Vector2(0f, 300f),
                new Vector2(840f, 62f),
                TextAnchor.MiddleCenter);

            const string instrucciones =
                "1. EMPIEZA TU ECONOMÍA\n" +
                "Comienzas con un Centro Urbano y Aldeanos. En la parte superior ves tu Oro, Madera y Comida. Haz clic en un Aldeano, pulsa RECOLECTAR y después haz clic sobre un recurso del mapa. El Aldeano caminará hasta él, recogerá recursos y los llevará a tu Centro Urbano.\n\n" +
                "2. SELECCIONAR Y MOVER\n" +
                "Haz clic sobre una unidad o edificio de tu bando. Sus datos y acciones aparecen abajo a la izquierda. Para mover una unidad, pulsa MOVER y luego haz clic en la casilla de destino. Si cambias de idea, puedes seleccionar otra unidad o usar CANCELAR cuando exista una orden activa.\n\n" +
                "3. CONSTRUIR Y ENTRENAR\n" +
                "Selecciona un Aldeano y pulsa CONSTRUIR para levantar un nuevo Centro Urbano en una casilla válida. Para crear tropas, selecciona un Centro Urbano, pulsa ENTRENAR y elige Aldeano, Guerrero, Lancero, Arquero o Monje. El juego muestra el progreso y la cola de entrenamiento.\n\n" +
                "4. COMBATIR Y CURAR\n" +
                "Selecciona un Guerrero, Lancero o Arquero, pulsa ATACAR y elige una unidad o edificio enemigo. Si está lejos, tu tropa se acercará automáticamente. El Monje usa CURAR: selecciona un aliado herido y el Monje se acercará hasta estar a rango. Las unidades con poca vida se resaltan en rojo.\n\n" +
                "5. NO DEJES ALDEANOS SIN TAREA\n" +
                "Cuando un Aldeano termine de recolectar o construir, aparecerá un aviso breve arriba para recordarte que está disponible para una nueva tarea.\n\n" +
                "6. TU ENEMIGO\n" +
                "Las facciones Morada, Verde y Amarilla compiten por los mismos recursos y atacan únicamente al jugador humano. Primero desarrollan su economía y después lanzan ofensivas escalonadas. Los recursos agotados reaparecen más tarde en otra casilla libre.\n\n" +
                "7. CONQUISTA Y VICTORIA\n" +
                "Una facción cae cuando pierde todos sus Centros Urbanos y sus unidades militares. Al eliminar una IA, obtienes automáticamente un nuevo Centro Urbano en la zona conquistada. Ganas al eliminar las tres facciones. Pierdes si te quedas sin Centros Urbanos y sin unidades militares.";

            Text cuerpo =
                CrearTexto(
                    panel.transform,
                    "Cuerpo",
                    instrucciones,
                    17,
                    new Vector2(0f, 18f),
                    new Vector2(840f, 545f),
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
                    new Vector2(0f, -310f));

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
