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
        [SerializeField] private Button cancelar;
        [SerializeField] private GameObject selectorEntrenamiento;
        [SerializeField] private Button entrenarAldeano;
        [SerializeField] private Button entrenarGuerrero;
        [SerializeField] private Button entrenarLancero;
        [SerializeField] private Button entrenarArquero;
        [SerializeField] private Button entrenarMonje;

        private GameObject pantallaResultadoFinal;
        private Text tituloResultadoFinal;
        private Text detalleResultadoFinal;

        public event Action<string> AccionSolicitada;
        public event Action<string> TipoUnidadSolicitado;
        public event Action VolverMenuSolicitado;
        public event Action SalirSolicitado;

        private void Awake()
        {
            CrearBotonCancelarSiFalta();
            AplicarLayoutCompacto();
            ConfigurarRaycastsNoBloqueantes();
            CrearPantallaResultadoFinal();
        }

        private void CrearBotonCancelarSiFalta()
        {
            if (cancelar != null)
                return;

            RectTransform panel =
                transform.Find("PanelContextual")
                    as RectTransform;

            if (panel == null ||
                atacar == null)
            {
                return;
            }

            GameObject objeto =
                Instantiate(
                    atacar.gameObject,
                    panel,
                    false);

            objeto.name = "Cancelar";

            cancelar =
                objeto.GetComponent<Button>();

            if (cancelar != null)
            {
                cancelar.onClick.RemoveAllListeners();
            }

            Text etiqueta =
                objeto.GetComponentInChildren<Text>(
                    true);

            if (etiqueta != null)
            {
                etiqueta.text = "Cancelar";
            }

            objeto.SetActive(false);
        }

        private void ConfigurarRaycastsNoBloqueantes()
        {
            // Los fondos y textos informativos del HUD son decorativos.
            // No deben impedir seleccionar unidades/casillas que queden
            // visualmente detrás del panel. Los Button conservan su Image
            // con raycastTarget=true y siguen siendo interactivos.
            Transform panel =
                transform.Find(
                    "PanelContextual");

            if (panel != null)
            {
                Image fondoPanel =
                    panel.GetComponent<Image>();

                if (fondoPanel != null)
                {
                    fondoPanel.raycastTarget =
                        false;
                }

                DesactivarRaycastGrafico(
                    panel.Find("Seleccion"));

                DesactivarRaycastGrafico(
                    panel.Find("Mensaje"));

                Transform selector =
                    panel.Find(
                        "SelectorEntrenamiento");

                if (selector != null)
                {
                    Image fondoSelector =
                        selector.GetComponent<Image>();

                    if (fondoSelector != null)
                    {
                        fondoSelector.raycastTarget =
                            false;
                    }
                }
            }

            Transform barraRecursos =
                transform.Find(
                    "BarraRecursos");

            if (barraRecursos != null)
            {
                Image fondoBarra =
                    barraRecursos.GetComponent<Image>();

                if (fondoBarra != null)
                {
                    fondoBarra.raycastTarget =
                        false;
                }

                DesactivarRaycastGrafico(
                    barraRecursos.Find(
                        "Recursos"));
            }
        }

        private static void DesactivarRaycastGrafico(
            Transform objetivo)
        {
            if (objetivo == null)
                return;

            Graphic grafico =
                objetivo.GetComponent<Graphic>();

            if (grafico != null)
            {
                grafico.raycastTarget =
                    false;
            }
        }

        private void AplicarLayoutCompacto()
        {
            RectTransform panel =
                transform.Find("PanelContextual")
                    as RectTransform;

            if (panel == null)
                return;

            ConfigurarRect(
                panel,
                Vector2.zero,
                Vector2.zero,
                Vector2.zero,
                new Vector2(12f, 12f),
                new Vector2(382f, 182f));

            ConfigurarRectHijo(
                panel,
                "Seleccion",
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(12f, -64f),
                new Vector2(-12f, -8f));

            ConfigurarRectHijo(
                panel,
                "Mensaje",
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(12f, -106f),
                new Vector2(-12f, -68f));

            string[] acciones =
            {
                "Mover",
                "Recolectar",
                "Construir",
                "Entrenar",
                "Atacar",
                "Cancelar"
            };

            for (int i = 0; i < acciones.Length; i++)
            {
                float x =
                    12f + i * 58f;

                RectTransform boton =
                    panel.Find(
                        acciones[i])
                    as RectTransform;

                if (boton == null)
                    continue;

                ConfigurarRect(
                    boton,
                    Vector2.zero,
                    Vector2.zero,
                    Vector2.zero,
                    new Vector2(x, 12f),
                    new Vector2(x + 54f, 48f));

                Text etiqueta =
                    boton.GetComponentInChildren<Text>(
                        true);

                if (etiqueta != null)
                {
                    etiqueta.fontSize = 12;
                    etiqueta.resizeTextMinSize = 8;
                    etiqueta.resizeTextMaxSize = 12;
                }
            }

            RectTransform selector =
                panel.Find(
                    "SelectorEntrenamiento")
                as RectTransform;

            if (selector != null)
            {
                ConfigurarRect(
                    selector,
                    Vector2.zero,
                    Vector2.zero,
                    Vector2.zero,
                    new Vector2(12f, 52f),
                    new Vector2(300f, 98f));

                string[] tipos =
                {
                    "Aldeano",
                    "Guerrero",
                    "Lancero",
                    "Arquero",
                    "Monje"
                };

                for (int i = 0; i < tipos.Length; i++)
                {
                    RectTransform botonTipo =
                        selector.Find(
                            tipos[i])
                        as RectTransform;

                    if (botonTipo == null)
                        continue;

                    float x =
                        i * 56f;

                    ConfigurarRect(
                        botonTipo,
                        Vector2.zero,
                        Vector2.zero,
                        Vector2.zero,
                        new Vector2(x, 2f),
                        new Vector2(x + 54f, 42f));

                    Text etiqueta =
                        botonTipo.GetComponentInChildren<Text>(
                            true);

                    if (etiqueta != null)
                    {
                        etiqueta.fontSize = 11;
                        etiqueta.resizeTextMinSize = 8;
                        etiqueta.resizeTextMaxSize = 11;
                    }
                }
            }
        }

        private static void ConfigurarRectHijo(
            RectTransform padre,
            string nombre,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivote,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            RectTransform rect =
                padre.Find(nombre)
                    as RectTransform;

            if (rect == null)
                return;

            ConfigurarRect(
                rect,
                anchorMin,
                anchorMax,
                pivote,
                offsetMin,
                offsetMax);
        }

        private static void ConfigurarRect(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivote,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivote;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private void OnEnable()
        {
            if (mover != null) mover.onClick.AddListener(SolicitarMover);
            if (recolectar != null) recolectar.onClick.AddListener(SolicitarRecolectar);
            if (construir != null) construir.onClick.AddListener(SolicitarConstruir);
            if (entrenar != null) entrenar.onClick.AddListener(SolicitarEntrenar);
            if (atacar != null) atacar.onClick.AddListener(SolicitarAtacar);
            if (cancelar != null) cancelar.onClick.AddListener(SolicitarCancelar);

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
            if (cancelar != null) cancelar.onClick.RemoveListener(SolicitarCancelar);

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
        private void SolicitarCancelar() => AccionSolicitada?.Invoke("Cancelar");

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

            string nombre =
                entidad.TipoLogico == "CentroUrbano"
                    ? "Centro Urbano"
                    : entidad.TipoLogico;

            string bando =
                entidad.Propietario != null &&
                entidad.Propietario.StartsWith(
                    "Maquina")
                    ? DescribirFaccionEnemiga(
                        entidad.Propietario)
                    : "Tu bando";

            string texto =
                $"{nombre}\n" +
                $"Bando: {bando}\n" +
                $"Posición: ({entidad.X},{entidad.Y})";

            if (entidad.VidaMaxima > 0)
            {
                texto +=
                    $"\nVida: {entidad.VidaActual}/{entidad.VidaMaxima}";
            }

            if (entidad.Categoria == CategoriaEntidadVisual.Unidad)
            {
                string estado =
                    TraducirEstado(
                        entidad.EstadoLogico);

                string orden =
                    TraducirOrden(
                        entidad.OrdenActiva);

                texto +=
                    $"\nEstado: {estado}";

                if (!string.IsNullOrWhiteSpace(
                        orden))
                {
                    texto +=
                        $"\nOrden: {orden}";
                }

                if (entidad.Danio > 0)
                {
                    texto +=
                        $"\nDaño: {entidad.Danio}" +
                        $" | Alcance: {entidad.Alcance}";
                }
            }

            seleccion.text = texto;
        }

        private static string DescribirFaccionEnemiga(
            string propietario)
        {
            if (string.IsNullOrWhiteSpace(
                    propietario))
            {
                return "Enemigo";
            }

            int separador =
                propietario.IndexOf('_');

            if (separador < 0 ||
                separador >=
                    propietario.Length - 1)
            {
                return "Enemigo";
            }

            return $"Enemigo {propietario.Substring(separador + 1)}";
        }

        private static string TraducirEstado(
            string estado)
        {
            switch (estado)
            {
                case "Idle":
                    return "En espera";
                case "Moviendo":
                    return "En movimiento";
                case "Recolectando":
                    return "Recolectando";
                case "Construyendo":
                    return "Construyendo";
                case "Atacando":
                    return "En combate";
                case "Curando":
                    return "Curando";
                default:
                    return string.IsNullOrWhiteSpace(
                            estado)
                        ? "En espera"
                        : estado;
            }
        }

        private static string TraducirOrden(
            string orden)
        {
            switch (orden)
            {
                case "Mover":
                    return "Mover";
                case "Recolectar":
                    return "Recolectar";
                case "Construir":
                    return "Construir";
                case "Atacar":
                    return "Atacar";
                case "Curar":
                    return "Curar";
                default:
                    return string.IsNullOrWhiteSpace(
                            orden)
                        ? string.Empty
                        : orden;
            }
        }

        public void ConfigurarCostosEntrenamiento(
            string aldeano,
            string guerrero,
            string lancero,
            string arquero,
            string monje)
        {
            ConfigurarEtiquetaEntrenamiento(
                entrenarAldeano,
                "Aldeano",
                aldeano);

            ConfigurarEtiquetaEntrenamiento(
                entrenarGuerrero,
                "Guerrero",
                guerrero);

            ConfigurarEtiquetaEntrenamiento(
                entrenarLancero,
                "Lancero",
                lancero);

            ConfigurarEtiquetaEntrenamiento(
                entrenarArquero,
                "Arquero",
                arquero);

            ConfigurarEtiquetaEntrenamiento(
                entrenarMonje,
                "Monje",
                monje);
        }

        private static void ConfigurarEtiquetaEntrenamiento(
            Button boton,
            string tipo,
            string costo)
        {
            if (boton == null)
                return;

            Text etiqueta =
                boton.GetComponentInChildren<Text>(
                    true);

            if (etiqueta == null)
                return;

            etiqueta.text =
                string.IsNullOrWhiteSpace(costo)
                    ? tipo
                    : $"{tipo}\n{costo}";

            etiqueta.fontSize = 10;
            etiqueta.resizeTextMinSize = 7;
            etiqueta.resizeTextMaxSize = 10;
        }

        public void MostrarSelectorEntrenamiento(bool mostrar)
        {
            if (selectorEntrenamiento != null)
                selectorEntrenamiento.SetActive(mostrar);
        }

        public void MostrarOpciones(bool puedeMover, bool puedeRecolectar, bool puedeConstruir,
            bool puedeEntrenar, bool puedeAtacar, bool mostrarComoCurar = false,
            bool puedeCancelar = false)
        {
            if (mover != null) mover.gameObject.SetActive(puedeMover);
            if (recolectar != null) recolectar.gameObject.SetActive(puedeRecolectar);
            if (construir != null) construir.gameObject.SetActive(puedeConstruir);
            if (entrenar != null) entrenar.gameObject.SetActive(puedeEntrenar);
            if (cancelar != null) cancelar.gameObject.SetActive(puedeCancelar);
            if (atacar != null)
            {
                atacar.gameObject.SetActive(puedeAtacar);

                Text etiqueta =
                    atacar.GetComponentInChildren<Text>(
                        true);

                if (etiqueta != null)
                {
                    etiqueta.text =
                        mostrarComoCurar
                            ? "Curar"
                            : "Atacar";
                }
            }
        }

        public void MostrarResultadoFinal(
            string ganador,
            string ganadorNombre,
            string motivo)
        {
            MostrarOpciones(
                false,
                false,
                false,
                false,
                false);

            MostrarSelectorEntrenamiento(
                false);

            bool victoriaHumana =
                ganador == "Humano";

            string titulo =
                victoriaHumana
                    ? "VICTORIA"
                    : "DERROTA";

            string detalle;

            if (victoriaHumana)
            {
                detalle =
                    "Has derrotado a las tres facciones enemigas.\n" +
                    "Ya no conservan Centros Urbanos ni unidades militares.";
            }
            else if (!string.IsNullOrWhiteSpace(
                         ganador))
            {
                detalle =
                    "Tu imperio ha caído.\n" +
                    "Ya no conservas Centros Urbanos ni unidades militares.";
            }
            else
            {
                detalle =
                    string.IsNullOrWhiteSpace(
                        motivo)
                        ? "La batalla ha terminado."
                        : motivo;
            }

            MostrarMensaje(
                detalle,
                !victoriaHumana);

            CrearPantallaResultadoFinal();

            if (tituloResultadoFinal != null)
                tituloResultadoFinal.text = titulo;

            if (detalleResultadoFinal != null)
            {
                detalleResultadoFinal.text =
                    detalle;
            }

            if (pantallaResultadoFinal != null)
            {
                pantallaResultadoFinal.SetActive(true);
                pantallaResultadoFinal.transform.SetAsLastSibling();
            }
        }

        public void OcultarResultadoFinal()
        {
            if (pantallaResultadoFinal != null)
                pantallaResultadoFinal.SetActive(false);
        }

        private void CrearPantallaResultadoFinal()
        {
            if (pantallaResultadoFinal != null)
                return;

            Transform existente =
                transform.Find(
                    "PantallaResultadoFinal");

            if (existente != null)
            {
                pantallaResultadoFinal =
                    existente.gameObject;
            }
            else
            {
                pantallaResultadoFinal =
                    new GameObject(
                        "PantallaResultadoFinal",
                        typeof(RectTransform),
                        typeof(Image));

                pantallaResultadoFinal.transform.SetParent(
                    transform,
                    false);
            }

            RectTransform rect =
                pantallaResultadoFinal
                    .GetComponent<RectTransform>();

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot =
                new Vector2(
                    0.5f,
                    0.5f);

            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image fondo =
                pantallaResultadoFinal
                    .GetComponent<Image>();

            fondo.color =
                new Color(
                    0.02f,
                    0.025f,
                    0.04f,
                    0.97f);

            fondo.raycastTarget = true;

            tituloResultadoFinal =
                CrearTextoResultado(
                    pantallaResultadoFinal.transform,
                    "Titulo",
                    "DERROTA",
                    54,
                    new Vector2(0f, 90f),
                    new Vector2(700f, 90f));

            tituloResultadoFinal.alignment =
                TextAnchor.MiddleCenter;

            detalleResultadoFinal =
                CrearTextoResultado(
                    pantallaResultadoFinal.transform,
                    "Detalle",
                    "",
                    22,
                    new Vector2(0f, 0f),
                    new Vector2(760f, 120f));

            detalleResultadoFinal.alignment =
                TextAnchor.MiddleCenter;

            Button volverMenu =
                CrearBotonResultado(
                    pantallaResultadoFinal.transform,
                    "VolverMenu",
                    "VOLVER AL MENÚ",
                    new Vector2(-115f, -125f));

            Button salir =
                CrearBotonResultado(
                    pantallaResultadoFinal.transform,
                    "Salir",
                    "SALIR",
                    new Vector2(115f, -125f));

            volverMenu.onClick.RemoveAllListeners();
            volverMenu.onClick.AddListener(
                () => VolverMenuSolicitado?.Invoke());

            salir.onClick.RemoveAllListeners();
            salir.onClick.AddListener(
                () => SalirSolicitado?.Invoke());

            pantallaResultadoFinal.SetActive(false);
        }

        private static Text CrearTextoResultado(
            Transform padre,
            string nombre,
            string contenido,
            int tamano,
            Vector2 posicion,
            Vector2 dimensiones)
        {
            Transform existente =
                padre.Find(nombre);

            GameObject objeto =
                existente == null
                    ? new GameObject(
                        nombre,
                        typeof(RectTransform),
                        typeof(Text))
                    : existente.gameObject;

            if (existente == null)
            {
                objeto.transform.SetParent(
                    padre,
                    false);
            }

            RectTransform rect =
                objeto.GetComponent<RectTransform>();

            rect.anchorMin =
                new Vector2(
                    0.5f,
                    0.5f);

            rect.anchorMax =
                rect.anchorMin;

            rect.pivot =
                new Vector2(
                    0.5f,
                    0.5f);

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
            texto.raycastTarget = false;
            texto.resizeTextForBestFit = true;
            texto.resizeTextMinSize = 16;
            texto.resizeTextMaxSize = tamano;

            return texto;
        }

        private static Button CrearBotonResultado(
            Transform padre,
            string nombre,
            string textoBoton,
            Vector2 posicion)
        {
            Transform existente =
                padre.Find(nombre);

            GameObject objeto =
                existente == null
                    ? new GameObject(
                        nombre,
                        typeof(RectTransform),
                        typeof(Image),
                        typeof(Button))
                    : existente.gameObject;

            if (existente == null)
            {
                objeto.transform.SetParent(
                    padre,
                    false);
            }

            RectTransform rect =
                objeto.GetComponent<RectTransform>();

            rect.anchorMin =
                new Vector2(
                    0.5f,
                    0.5f);

            rect.anchorMax =
                rect.anchorMin;

            rect.pivot =
                new Vector2(
                    0.5f,
                    0.5f);

            rect.anchoredPosition =
                posicion;

            rect.sizeDelta =
                new Vector2(
                    210f,
                    54f);

            Image imagen =
                objeto.GetComponent<Image>();

            imagen.color =
                new Color(
                    0.24f,
                    0.30f,
                    0.38f,
                    1f);

            Button boton =
                objeto.GetComponent<Button>();

            boton.targetGraphic = imagen;

            Text etiqueta =
                CrearTextoResultado(
                    objeto.transform,
                    "Texto",
                    textoBoton,
                    19,
                    Vector2.zero,
                    new Vector2(
                        200f,
                        48f));

            etiqueta.alignment =
                TextAnchor.MiddleCenter;

            return boton;
        }

        public void MostrarMensaje(string texto, bool error = false)
        {
            if (mensaje == null) return;
            mensaje.text = texto;
            mensaje.color = error ? new Color(1f, 0.55f, 0.55f) : Color.white;
        }
    }
}
