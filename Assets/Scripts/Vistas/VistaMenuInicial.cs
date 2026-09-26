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
                    new Color(0.018f, 0.03f, 0.052f, 0.992f));

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
                    new Vector2(700f, 560f));

            CrearBarraDecorativa(
                panel.transform,
                "LineaSuperior",
                new Vector2(0f, 242f),
                new Vector2(590f, 4f),
                new Color(0.85f, 0.65f, 0.22f, 1f));

            CrearTexto(
                panel.transform,
                "Titulo",
                "IMPERIOS EN GUERRA",
                50,
                new Vector2(0f, 188f),
                new Vector2(620f, 78f),
                TextAnchor.MiddleCenter);

            Text subtitulo =
                CrearTexto(
                    panel.transform,
                    "Subtitulo",
                    "Construye · reúne recursos · conquista",
                    21,
                    new Vector2(0f, 132f),
                    new Vector2(590f, 40f),
                    TextAnchor.MiddleCenter);

            subtitulo.color =
                new Color(0.78f, 0.84f, 0.9f);

            Text desafio =
                CrearTexto(
                    panel.transform,
                    "Desafio",
                    "1 HUMANO  •  3 IMPERIOS",
                    16,
                    new Vector2(0f, 92f),
                    new Vector2(420f, 32f),
                    TextAnchor.MiddleCenter);

            desafio.color =
                new Color(0.95f, 0.78f, 0.34f);

            CrearBarraDecorativa(
                panel.transform,
                "FaccionMorada",
                new Vector2(-48f, 64f),
                new Vector2(84f, 5f),
                new Color(0.58f, 0.32f, 0.75f, 1f));

            CrearBarraDecorativa(
                panel.transform,
                "FaccionVerde",
                new Vector2(48f, 64f),
                new Vector2(84f, 5f),
                new Color(0.31f, 0.66f, 0.39f, 1f));

            CrearBarraDecorativa(
                panel.transform,
                "FaccionAmarilla",
                new Vector2(144f, 64f),
                new Vector2(84f, 5f),
                new Color(0.9f, 0.72f, 0.22f, 1f));

            jugar =
                CrearBoton(
                    panel.transform,
                    "Jugar",
                    "JUGAR",
                    new Vector2(0f, 12f));

            Button instrucciones =
                CrearBoton(
                    panel.transform,
                    "Instrucciones",
                    "INSTRUCCIONES",
                    new Vector2(0f, -62f));

            Button salir =
                CrearBoton(
                    panel.transform,
                    "Salir",
                    "SALIR",
                    new Vector2(0f, -136f));

            CrearTexto(
                panel.transform,
                "Recursos",
                "ORO   •   MADERA   •   COMIDA",
                15,
                new Vector2(0f, -190f),
                new Vector2(500f, 28f),
                TextAnchor.MiddleCenter)
                .color =
                    new Color(0.72f, 0.78f, 0.84f);

            estado =
                CrearTexto(
                    panel.transform,
                    "Estado",
                    "",
                    16,
                    new Vector2(0f, -230f),
                    new Vector2(590f, 48f),
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
                "1. ECONOMÍA\n" +
                "Empiezas con Centro Urbano y Aldeanos. Selecciona un Aldeano, pulsa RECOLECTAR y haz clic en Oro, Madera o Comida. Caminará, cargará y depositará el recurso en tu Centro Urbano.\n\n" +
                "2. SELECCIONAR Y MOVER\n" +
                "Haz clic en una unidad propia. Pulsa MOVER y elige una casilla. Una unidad solo mantiene una orden activa; puedes cancelar una orden y dar otra.\n\n" +
                "3. CONSTRUIR Y ENTRENAR\n" +
                "Con un Aldeano pulsa CONSTRUIR para levantar un Centro Urbano. Con un Centro Urbano pulsa ENTRENAR para crear Aldeanos, Guerreros, Lanceros, Arqueros o Monjes. Costos, progreso y cola se muestran en pantalla.\n\n" +
                "4. COMBATIR Y CURAR\n" +
                "Guerrero, Lancero y Arquero pueden atacar: pulsa ATACAR y elige un enemigo. El Monje puede curar aliados heridos. Las entidades con vida crítica se ven rojas.\n\n" +
                "5. VARIAS TAREAS A LA VEZ\n" +
                "Distintas unidades pueden moverse, recolectar, construir, entrenar o combatir al mismo tiempo. Si un Aldeano queda sin tarea, recibirás un aviso.\n\n" +
                "6. PAUSA\n" +
                "Pulsa ESC durante la partida para abrir el menú de pausa. Desde allí puedes reanudar o salir.\n\n" +
                "7. TRES FACCIONES ENEMIGAS\n" +
                "Morada, Verde y Amarilla desarrollan economía y lanzan ofensivas contra ti. Cada recurso agotado reaparece más tarde en una casilla libre.\n\n" +
                "8. CONQUISTA Y VICTORIA\n" +
                "Una facción cae al quedarse sin Centros Urbanos y sin unidades militares. Al eliminar una IA recibes un nuevo Centro Urbano en la zona conquistada. Ganas al eliminar las tres facciones; pierdes bajo la misma condición.";

            Text cuerpo =
                CrearTexto(
                    panel.transform,
                    "Cuerpo",
                    instrucciones,
                    15,
                    new Vector2(0f, 10f),
                    new Vector2(840f, 548f),
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
                    0.045f,
                    0.075f,
                    0.115f,
                    0.985f);

            imagen.raycastTarget = true;

            Outline borde =
                objeto.AddComponent<Outline>();

            borde.effectColor =
                new Color(
                    0.48f,
                    0.58f,
                    0.68f,
                    0.42f);

            borde.effectDistance =
                new Vector2(2f, -2f);

            return objeto;
        }

        private static Image CrearBarraDecorativa(
            Transform padre,
            string nombre,
            Vector2 posicion,
            Vector2 tamano,
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

            rect.anchorMin =
                new Vector2(0.5f, 0.5f);

            rect.anchorMax =
                rect.anchorMin;

            rect.pivot =
                new Vector2(0.5f, 0.5f);

            rect.anchoredPosition =
                posicion;

            rect.sizeDelta =
                tamano;

            Image imagen =
                objeto.GetComponent<Image>();

            imagen.color =
                color;

            imagen.raycastTarget =
                false;

            return imagen;
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
                new Vector2(330f, 58f);

            Image imagen =
                objeto.GetComponent<Image>();

            imagen.color =
                new Color(
                    0.12f,
                    0.25f,
                    0.38f,
                    1f);

            Outline borde =
                objeto.AddComponent<Outline>();

            borde.effectColor =
                new Color(
                    0.86f,
                    0.68f,
                    0.3f,
                    0.48f);

            borde.effectDistance =
                new Vector2(1.5f, -1.5f);

            Button boton =
                objeto.GetComponent<Button>();

            boton.targetGraphic = imagen;

            ColorBlock colores =
                boton.colors;

            colores.normalColor =
                new Color(
                    0.12f,
                    0.25f,
                    0.38f,
                    1f);

            colores.highlightedColor =
                new Color(
                    0.2f,
                    0.38f,
                    0.52f,
                    1f);

            colores.pressedColor =
                new Color(
                    0.08f,
                    0.18f,
                    0.29f,
                    1f);

            colores.selectedColor =
                colores.highlightedColor;

            colores.disabledColor =
                new Color(
                    0.08f,
                    0.12f,
                    0.16f,
                    0.7f);

            boton.colors =
                colores;

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
