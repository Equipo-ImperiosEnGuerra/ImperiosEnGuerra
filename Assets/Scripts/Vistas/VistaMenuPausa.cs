using System;
using UnityEngine;
using UnityEngine.UI;

namespace ImperiosEnGuerra.Vistas
{
    /// <summary>
    /// Vista del menú de pausa. No contiene reglas de gameplay.
    /// </summary>
    public sealed class VistaMenuPausa : MonoBehaviour
    {
        private Canvas canvas;
        private GameObject fondo;

        public event Action ReanudarSolicitado;
        public event Action SalirSolicitado;

        private void Awake()
        {
            Construir();
            Ocultar();
        }

        public void Mostrar()
        {
            if (fondo != null)
            {
                fondo.SetActive(
                    true);
            }

            gameObject.SetActive(
                true);
        }

        public void Ocultar()
        {
            if (fondo != null)
            {
                fondo.SetActive(
                    false);
            }
        }

        private void Construir()
        {
            if (canvas != null)
                return;

            canvas =
                gameObject.AddComponent<Canvas>();

            canvas.renderMode =
                RenderMode.ScreenSpaceOverlay;

            canvas.sortingOrder =
                750;

            CanvasScaler escalador =
                gameObject.AddComponent<CanvasScaler>();

            escalador.uiScaleMode =
                CanvasScaler.ScaleMode.ScaleWithScreenSize;

            escalador.referenceResolution =
                new Vector2(
                    1280f,
                    720f);

            escalador.screenMatchMode =
                CanvasScaler.ScreenMatchMode.Expand;

            gameObject.AddComponent<GraphicRaycaster>();

            fondo =
                CrearFondo(
                    transform);

            GameObject panel =
                CrearPanel(
                    fondo.transform);

            CrearTexto(
                panel.transform,
                "Titulo",
                "PAUSA",
                38,
                new Vector2(
                    0f,
                    88f),
                new Vector2(
                    360f,
                    60f));

            CrearTexto(
                panel.transform,
                "Subtitulo",
                "La partida está detenida",
                18,
                new Vector2(
                    0f,
                    45f),
                new Vector2(
                    360f,
                    34f));

            Button reanudar =
                CrearBoton(
                    panel.transform,
                    "Reanudar",
                    "REANUDAR",
                    new Vector2(
                        0f,
                        -20f));

            Button salir =
                CrearBoton(
                    panel.transform,
                    "Salir",
                    "SALIR DEL JUEGO",
                    new Vector2(
                        0f,
                        -88f));

            reanudar.onClick.AddListener(
                () =>
                    ReanudarSolicitado?
                        .Invoke());

            salir.onClick.AddListener(
                () =>
                    SalirSolicitado?
                        .Invoke());
        }

        private static GameObject CrearFondo(
            Transform padre)
        {
            GameObject objeto =
                new GameObject(
                    "FondoPausa",
                    typeof(RectTransform),
                    typeof(Image));

            objeto.transform.SetParent(
                padre,
                false);

            RectTransform rect =
                objeto.GetComponent<RectTransform>();

            rect.anchorMin =
                Vector2.zero;

            rect.anchorMax =
                Vector2.one;

            rect.offsetMin =
                Vector2.zero;

            rect.offsetMax =
                Vector2.zero;

            Image imagen =
                objeto.GetComponent<Image>();

            imagen.color =
                new Color(
                    0.01f,
                    0.015f,
                    0.025f,
                    0.72f);

            imagen.raycastTarget =
                true;

            return objeto;
        }

        private static GameObject CrearPanel(
            Transform padre)
        {
            GameObject objeto =
                new GameObject(
                    "Panel",
                    typeof(RectTransform),
                    typeof(Image));

            objeto.transform.SetParent(
                padre,
                false);

            RectTransform rect =
                objeto.GetComponent<RectTransform>();

            rect.anchorMin =
                new Vector2(
                    0.5f,
                    0.5f);

            rect.anchorMax =
                rect.anchorMin;

            rect.pivot =
                rect.anchorMin;

            rect.anchoredPosition =
                Vector2.zero;

            rect.sizeDelta =
                new Vector2(
                    430f,
                    310f);

            Image imagen =
                objeto.GetComponent<Image>();

            imagen.color =
                new Color(
                    0.055f,
                    0.08f,
                    0.12f,
                    0.98f);

            return objeto;
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
                new Vector2(
                    0.5f,
                    0.5f);

            rect.anchorMax =
                rect.anchorMin;

            rect.pivot =
                rect.anchorMin;

            rect.anchoredPosition =
                posicion;

            rect.sizeDelta =
                new Vector2(
                    300f,
                    54f);

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

            boton.targetGraphic =
                imagen;

            CrearTexto(
                objeto.transform,
                "Texto",
                etiqueta,
                21,
                Vector2.zero,
                new Vector2(
                    290f,
                    48f));

            return boton;
        }

        private static Text CrearTexto(
            Transform padre,
            string nombre,
            string contenido,
            int tamano,
            Vector2 posicion,
            Vector2 dimensiones)
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
                new Vector2(
                    0.5f,
                    0.5f);

            rect.anchorMax =
                rect.anchorMin;

            rect.pivot =
                rect.anchorMin;

            rect.anchoredPosition =
                posicion;

            rect.sizeDelta =
                dimensiones;

            Text texto =
                objeto.GetComponent<Text>();

            texto.font =
                Resources.GetBuiltinResource<Font>(
                    "LegacyRuntime.ttf");

            texto.fontSize =
                tamano;

            texto.color =
                Color.white;

            texto.text =
                contenido;

            texto.alignment =
                TextAnchor.MiddleCenter;

            texto.raycastTarget =
                false;

            return texto;
        }
    }
}
