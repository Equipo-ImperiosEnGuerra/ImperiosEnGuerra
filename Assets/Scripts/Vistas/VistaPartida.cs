using System.Collections.Generic;
using ImperiosEnGuerra.Controladores.Red.Contratos;
using UnityEngine;

namespace ImperiosEnGuerra.Vistas
{
    /// <summary>Representa los datos recibidos de la API sin modificar el estado del juego.</summary>
    public class VistaPartida : MonoBehaviour
    {
        [SerializeField] private Camera camara;
        [SerializeField, Min(0.1f)] private float espacioCasilla = 2f;
        [SerializeField, Min(0.01f)] private float escalaRecursos = 0.75f;
        [SerializeField, Min(0.01f)] private float escalaEdificios = 0.58f;
        [SerializeField, Min(0.01f)] private float escalaUnidades = 0.65f;
        [SerializeField] private Sprite suelo;
        [SerializeField] private Sprite oro;
        [SerializeField] private Sprite madera;
        [SerializeField] private Sprite comida;
        [SerializeField] private Sprite centroHumano;
        [SerializeField] private Sprite centroMaquina;
        [SerializeField] private Sprite aldeanoHumano;
        [SerializeField] private Sprite guerreroHumano;
        [SerializeField] private Sprite lanceroHumano;
        [SerializeField] private Sprite arqueroHumano;
        [SerializeField] private Sprite monjeHumano;
        [SerializeField] private Sprite aldeanoMaquina;
        [SerializeField] private Sprite guerreroMaquina;
        [SerializeField] private Sprite lanceroMaquina;
        [SerializeField] private Sprite arqueroMaquina;
        [SerializeField] private Sprite monjeMaquina;

        private GameObject contenidoGenerado;
        private int anchoVisual;
        private int altoVisual;

        [SerializeField, Min(0.1f)]
        private float velocidadMovimientoVisual = 2.5f;

        private sealed class MovimientoVisualPendiente
        {
            public EntidadSeleccionableVista Entidad;
            public Vector3 Destino;
        }

        private readonly Dictionary<string, MovimientoVisualPendiente>
            movimientosVisuales =
                new Dictionary<string, MovimientoVisualPendiente>();

        public event System.Action AntesDeLimpiarContenido;

        public void Renderizar(EstadoPartidaDto estado)
        {
            Limpiar();
            if (estado == null || estado.mapa == null)
            {
                Debug.LogError("No se puede representar una partida sin estado o mapa.", this);
                return;
            }

            if (estado.mapa.ancho <= 0 || estado.mapa.alto <= 0)
            {
                Debug.LogError("El mapa recibido no tiene dimensiones visualizables.", this);
                return;
            }

            anchoVisual = estado.mapa.ancho;
            altoVisual = estado.mapa.alto;
            contenidoGenerado = new GameObject("ContenidoGenerado");
            contenidoGenerado.transform.SetParent(transform, false);
            Transform mapa = CrearContenedor("Mapa");
            Transform recursos = CrearContenedor("Recursos");
            Transform edificios = CrearContenedor("Edificios");
            Transform unidades = CrearContenedor("Unidades");

            RenderizarMapa(estado.mapa, mapa);
            RenderizarRecursos(estado.mapa.recursos, recursos);
            RenderizarJugador(estado.jugadorHumano, true, edificios, unidades);
            RenderizarJugador(estado.jugadorMaquina, false, edificios, unidades);
            AjustarCamara(estado.mapa);
        }

        /// <summary>
        /// Aplica un snapshot sin destruir la escena generada. Esto conserva
        /// la selección y permite que varias órdenes concurrentes actualicen
        /// unidades/obras sin interrumpir al jugador.
        /// </summary>
        public void Sincronizar(EstadoPartidaDto estado)
        {
            if (estado == null || estado.mapa == null)
            {
                Debug.LogError(
                    "No se puede sincronizar una partida sin estado o mapa.",
                    this);
                return;
            }

            if (contenidoGenerado == null ||
                anchoVisual != estado.mapa.ancho ||
                altoVisual != estado.mapa.alto)
            {
                Renderizar(estado);
                return;
            }

            Transform recursos =
                contenidoGenerado.transform.Find("Recursos");
            Transform edificios =
                contenidoGenerado.transform.Find("Edificios");
            Transform unidades =
                contenidoGenerado.transform.Find("Unidades");

            if (recursos == null ||
                edificios == null ||
                unidades == null)
            {
                Renderizar(estado);
                return;
            }

            SincronizarRecursos(
                estado.mapa.recursos);

            LimpiarObrasVisuales(
                edificios);

            SincronizarJugador(
                estado.jugadorHumano,
                true,
                edificios,
                unidades);

            SincronizarJugador(
                estado.jugadorMaquina,
                false,
                edificios,
                unidades);
        }

        private void SincronizarRecursos(
            RecursoEstadoDto[] recursos)
        {
            EntidadSeleccionableVista[] entidades =
                GetComponentsInChildren<EntidadSeleccionableVista>(
                    true);

            foreach (EntidadSeleccionableVista entidad in entidades)
            {
                if (entidad == null ||
                    entidad.Categoria != CategoriaEntidadVisual.Recurso)
                {
                    continue;
                }

                RecursoEstadoDto recurso =
                    BuscarRecurso(
                        recursos,
                        entidad.TipoLogico,
                        entidad.X,
                        entidad.Y);

                bool visible =
                    recurso != null &&
                    recurso.cantidadRestante > 0;

                if (entidad.gameObject.activeSelf != visible)
                {
                    entidad.gameObject.SetActive(
                        visible);
                }
            }
        }

        private static RecursoEstadoDto BuscarRecurso(
            RecursoEstadoDto[] recursos,
            string tipo,
            int x,
            int y)
        {
            if (recursos == null)
                return null;

            foreach (RecursoEstadoDto recurso in recursos)
            {
                if (recurso != null &&
                    recurso.coordenada != null &&
                    recurso.tipo == tipo &&
                    recurso.coordenada.x == x &&
                    recurso.coordenada.y == y)
                {
                    return recurso;
                }
            }

            return null;
        }

        private void SincronizarJugador(
            JugadorEstadoDto jugador,
            bool humano,
            Transform edificios,
            Transform unidades)
        {
            if (jugador == null)
                return;

            string propietario =
                humano
                    ? "Humano"
                    : "Maquina";

            SincronizarEdificios(
                jugador.edificios,
                humano,
                propietario,
                edificios);

            SincronizarObras(
                jugador.obrasConstruccion,
                humano,
                propietario,
                edificios);

            SincronizarUnidades(
                jugador.unidades,
                humano,
                propietario,
                unidades);
        }

        private void SincronizarEdificios(
            EdificioEstadoDto[] datos,
            bool humano,
            string propietario,
            Transform contenedor)
        {
            if (datos == null)
                return;

            OcultarEdificiosAusentes(datos, propietario);

            foreach (EdificioEstadoDto edificio in datos)
            {
                if (edificio == null ||
                    edificio.coordenada == null ||
                    edificio.tipo != "CentroUrbano")
                {
                    continue;
                }

                EntidadSeleccionableVista existente =
                    BuscarEntidad(
                        CategoriaEntidadVisual.Edificio,
                        edificio.id,
                        edificio.tipo,
                        propietario,
                        edificio.coordenada.x,
                        edificio.coordenada.y);

                if (existente != null)
                    continue;

                GameObject objeto =
                    CrearSprite(
                        $"Edificio_{propietario}_{edificio.tipo}_{edificio.coordenada.x}_{edificio.coordenada.y}",
                        humano
                            ? centroHumano
                            : centroMaquina,
                        edificio.coordenada.x,
                        edificio.coordenada.y,
                        20,
                        contenedor,
                        Vector3.one * escalaEdificios);

                ConfigurarSeleccionable(
                    objeto,
                    CategoriaEntidadVisual.Edificio,
                    edificio.tipo,
                    propietario,
                    edificio.coordenada,
                    edificio.id);
            }
        }

        private void SincronizarObras(
            ObraConstruccionEstadoDto[] obras,
            bool humano,
            string propietario,
            Transform contenedor)
        {
            if (obras == null)
                return;

            foreach (ObraConstruccionEstadoDto obra in obras)
            {
                if (obra == null ||
                    obra.coordenada == null ||
                    obra.tipo != "CentroUrbano")
                {
                    continue;
                }

                Sprite sprite =
                    humano
                        ? centroHumano
                        : centroMaquina;

                float factor =
                    Mathf.Lerp(
                        0.35f,
                        0.85f,
                        Mathf.Clamp01(
                            obra.progreso / 100f));

                GameObject objeto =
                    CrearSprite(
                        $"Obra_{propietario}_{obra.tipo}_{obra.coordenada.x}_{obra.coordenada.y}",
                        sprite,
                        obra.coordenada.x,
                        obra.coordenada.y,
                        18,
                        contenedor,
                        Vector3.one *
                        escalaEdificios *
                        factor);

                if (objeto == null)
                    continue;

                SpriteRenderer renderer =
                    objeto.GetComponent<SpriteRenderer>();

                if (renderer != null)
                {
                    Color color =
                        renderer.color;

                    color.a = 0.55f;
                    renderer.color = color;
                }
            }
        }

        private void SincronizarUnidades(
            UnidadEstadoDto[] datos,
            bool humano,
            string propietario,
            Transform contenedor)
        {
            if (datos == null)
                return;

            OcultarUnidadesAusentes(datos, propietario);

            foreach (UnidadEstadoDto unidad in datos)
            {
                if (unidad == null ||
                    unidad.coordenada == null ||
                    string.IsNullOrWhiteSpace(
                        unidad.id))
                {
                    continue;
                }

                EntidadSeleccionableVista existente =
                    BuscarEntidad(
                        CategoriaEntidadVisual.Unidad,
                        unidad.id,
                        unidad.tipo,
                        propietario,
                        unidad.coordenada.x,
                        unidad.coordenada.y,
                        ignorarCoordenada: true);

                if (existente != null)
                {
                    existente.ActualizarDatosLogicos(
                        unidad.coordenada.x,
                        unidad.coordenada.y,
                        unidad.estado,
                        unidad.ordenActiva);

                    movimientosVisuales[unidad.id] =
                        new MovimientoVisualPendiente
                        {
                            Entidad = existente,
                            Destino = PosicionVisual(
                                unidad.coordenada.x,
                                unidad.coordenada.y)
                        };

                    continue;
                }

                Sprite sprite =
                    ObtenerSpriteUnidad(
                        unidad.tipo,
                        humano);

                if (sprite == null)
                    continue;

                GameObject objeto =
                    CrearSprite(
                        $"Unidad_{propietario}_{unidad.tipo}_{unidad.coordenada.x}_{unidad.coordenada.y}",
                        sprite,
                        unidad.coordenada.x,
                        unidad.coordenada.y,
                        30,
                        contenedor,
                        Vector3.one * escalaUnidades);

                ConfigurarSeleccionable(
                    objeto,
                    CategoriaEntidadVisual.Unidad,
                    unidad.tipo,
                    propietario,
                    unidad.coordenada,
                    unidad.id,
                    unidad.estado,
                    unidad.ordenActiva);
            }
        }

        private void OcultarEdificiosAusentes(
            EdificioEstadoDto[] datos,
            string propietario)
        {
            EntidadSeleccionableVista[] entidades =
                GetComponentsInChildren<EntidadSeleccionableVista>(true);

            foreach (EntidadSeleccionableVista entidad in entidades)
            {
                if (entidad == null ||
                    entidad.Categoria != CategoriaEntidadVisual.Edificio ||
                    entidad.Propietario != propietario)
                    continue;

                bool existe = false;
                foreach (EdificioEstadoDto edificio in datos)
                {
                    if (edificio != null &&
                        edificio.id == entidad.IdLogico)
                    {
                        existe = true;
                        break;
                    }
                }

                if (!existe)
                    entidad.gameObject.SetActive(false);
            }
        }

        private void OcultarUnidadesAusentes(
            UnidadEstadoDto[] datos,
            string propietario)
        {
            EntidadSeleccionableVista[] entidades =
                GetComponentsInChildren<EntidadSeleccionableVista>(true);

            foreach (EntidadSeleccionableVista entidad in entidades)
            {
                if (entidad == null ||
                    entidad.Categoria != CategoriaEntidadVisual.Unidad ||
                    entidad.Propietario != propietario)
                    continue;

                bool existe = false;
                foreach (UnidadEstadoDto unidad in datos)
                {
                    if (unidad != null &&
                        unidad.id == entidad.IdLogico)
                    {
                        existe = true;
                        break;
                    }
                }

                if (!existe)
                {
                    movimientosVisuales.Remove(entidad.IdLogico);
                    entidad.gameObject.SetActive(false);
                }
            }
        }

        private EntidadSeleccionableVista BuscarEntidad(
            CategoriaEntidadVisual categoria,
            string id,
            string tipo,
            string propietario,
            int x,
            int y,
            bool ignorarCoordenada = false)
        {
            EntidadSeleccionableVista[] entidades =
                GetComponentsInChildren<EntidadSeleccionableVista>(
                    true);

            foreach (EntidadSeleccionableVista entidad in entidades)
            {
                if (entidad == null ||
                    entidad.Categoria != categoria ||
                    entidad.TipoLogico != tipo ||
                    entidad.Propietario != propietario)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(id) &&
                    entidad.IdLogico != id)
                {
                    continue;
                }

                if (!ignorarCoordenada &&
                    (entidad.X != x ||
                     entidad.Y != y))
                {
                    continue;
                }

                return entidad;
            }

            return null;
        }

        private Sprite ObtenerSpriteUnidad(
            string tipo,
            bool humano)
        {
            switch (tipo)
            {
                case "Aldeano":
                    return humano
                        ? aldeanoHumano
                        : aldeanoMaquina;
                case "Guerrero":
                    return humano
                        ? guerreroHumano
                        : guerreroMaquina;
                case "Lancero":
                    return humano
                        ? lanceroHumano
                        : lanceroMaquina;
                case "Arquero":
                    return humano
                        ? arqueroHumano
                        : arqueroMaquina;
                case "Monje":
                    return humano
                        ? monjeHumano
                        : monjeMaquina;
                default:
                    Debug.LogWarning(
                        $"Tipo de unidad desconocido: {tipo}",
                        this);
                    return null;
            }
        }

        private void LimpiarObrasVisuales(
            Transform edificios)
        {
            if (edificios == null)
                return;

            for (int i = edificios.childCount - 1;
                 i >= 0;
                 i--)
            {
                GameObject objeto =
                    edificios.GetChild(i).gameObject;

                if (objeto == null ||
                    !objeto.name.StartsWith("Obra_"))
                {
                    continue;
                }

                objeto.SetActive(false);

                if (Application.isPlaying)
                    Destroy(objeto);
                else
                    DestroyImmediate(objeto);
            }
        }

        private void Limpiar()
        {
            AntesDeLimpiarContenido?.Invoke();
            movimientosVisuales.Clear();
            anchoVisual = 0;
            altoVisual = 0;
            if (contenidoGenerado == null)
            {
                return;
            }

            // Destroy se completa al final del frame: ocultar antes evita superposición.
            contenidoGenerado.SetActive(false);
            if (Application.isPlaying)
            {
                Destroy(contenidoGenerado);
            }
            else
            {
                DestroyImmediate(contenidoGenerado);
            }
            contenidoGenerado = null;
        }

        private Transform CrearContenedor(string nombre)
        {
            var contenedor = new GameObject(nombre);
            contenedor.transform.SetParent(contenidoGenerado.transform, false);
            return contenedor.transform;
        }

        private void RenderizarMapa(MapaEstadoDto mapa, Transform contenedor)
        {
            if (suelo == null)
            {
                Debug.LogWarning("No está configurado el sprite de suelo.", this);
                return;
            }

            // Cubrir la casilla completa mantiene el terreno continuo al variar la separación.
            Vector3 escalaSuelo = new Vector3(
                espacioCasilla * suelo.pixelsPerUnit / suelo.rect.width,
                espacioCasilla * suelo.pixelsPerUnit / suelo.rect.height, 1f);
            for (int x = 0; x < mapa.ancho; x++)
            {
                for (int y = 0; y < mapa.alto; y++)
                {
                    CrearSprite($"Suelo_{x}_{y}", suelo, x, y, 0, contenedor, escalaSuelo);
                }
            }
        }

        private void RenderizarRecursos(RecursoEstadoDto[] recursos, Transform contenedor)
        {
            if (recursos == null)
            {
                return;
            }

            foreach (RecursoEstadoDto recurso in recursos)
            {
                if (recurso == null || recurso.coordenada == null)
                {
                    Debug.LogWarning("Recurso sin datos o coordenada; se omite su representación.", this);
                    continue;
                }

                Sprite sprite;
                switch (recurso.tipo)
                {
                    case "Oro": sprite = oro; break;
                    case "Madera": sprite = madera; break;
                    case "Comida": sprite = comida; break;
                    default:
                        Debug.LogWarning($"Tipo de recurso desconocido: {recurso.tipo}", this);
                        continue;
                }

                GameObject objeto = CrearSprite($"Recurso_{recurso.tipo}_{recurso.coordenada.x}_{recurso.coordenada.y}",
                    sprite, recurso.coordenada.x, recurso.coordenada.y, 10, contenedor,
                    Vector3.one * escalaRecursos);
                ConfigurarSeleccionable(objeto, CategoriaEntidadVisual.Recurso,
                    recurso.tipo, string.Empty, recurso.coordenada);
            }
        }

        private void RenderizarJugador(
            JugadorEstadoDto jugador, bool humano, Transform edificios, Transform unidades)
        {
            if (jugador == null)
            {
                Debug.LogWarning("Jugador sin datos; se omite su representación.", this);
                return;
            }

            string propietario = humano ? "Humano" : "Maquina";
            if (jugador.edificios != null)
            {
                foreach (EdificioEstadoDto edificio in jugador.edificios)
                {
                    if (edificio == null || edificio.coordenada == null)
                    {
                        Debug.LogWarning("Edificio sin datos o coordenada; se omite su representación.", this);
                        continue;
                    }
                    if (edificio.tipo != "CentroUrbano")
                    {
                        Debug.LogWarning($"Tipo de edificio desconocido: {edificio.tipo}", this);
                        continue;
                    }

                    GameObject objeto = CrearSprite($"Edificio_{propietario}_{edificio.tipo}_{edificio.coordenada.x}_{edificio.coordenada.y}",
                        humano ? centroHumano : centroMaquina,
                        edificio.coordenada.x, edificio.coordenada.y, 20, edificios,
                        Vector3.one * escalaEdificios);
                    ConfigurarSeleccionable(
                        objeto,
                        CategoriaEntidadVisual.Edificio,
                        edificio.tipo,
                        propietario,
                        edificio.coordenada,
                        edificio.id);
                }
            }

            if (jugador.obrasConstruccion != null)
            {
                foreach (ObraConstruccionEstadoDto obra in jugador.obrasConstruccion)
                {
                    if (obra == null ||
                        obra.coordenada == null ||
                        obra.tipo != "CentroUrbano")
                    {
                        continue;
                    }

                    Sprite sprite =
                        humano
                            ? centroHumano
                            : centroMaquina;

                    float factor =
                        Mathf.Lerp(
                            0.35f,
                            0.85f,
                            Mathf.Clamp01(
                                obra.progreso / 100f));

                    GameObject objeto =
                        CrearSprite(
                            $"Obra_{propietario}_{obra.tipo}_{obra.coordenada.x}_{obra.coordenada.y}",
                            sprite,
                            obra.coordenada.x,
                            obra.coordenada.y,
                            18,
                            edificios,
                            Vector3.one *
                            escalaEdificios *
                            factor);

                    if (objeto != null)
                    {
                        SpriteRenderer renderer =
                            objeto.GetComponent<SpriteRenderer>();

                        if (renderer != null)
                        {
                            Color color =
                                renderer.color;

                            color.a = 0.55f;
                            renderer.color = color;
                        }
                    }
                }
            }

            if (jugador.unidades == null)
            {
                return;
            }

            foreach (UnidadEstadoDto unidad in jugador.unidades)
            {
                if (unidad == null || unidad.coordenada == null)
                {
                    Debug.LogWarning("Unidad sin datos o coordenada; se omite su representación.", this);
                    continue;
                }

                Sprite sprite;
                switch (unidad.tipo)
                {
                    case "Aldeano": sprite = humano ? aldeanoHumano : aldeanoMaquina; break;
                    case "Guerrero": sprite = humano ? guerreroHumano : guerreroMaquina; break;
                    case "Lancero": sprite = humano ? lanceroHumano : lanceroMaquina; break;
                    case "Arquero": sprite = humano ? arqueroHumano : arqueroMaquina; break;
                    case "Monje": sprite = humano ? monjeHumano : monjeMaquina; break;
                    default:
                        Debug.LogWarning($"Tipo de unidad desconocido: {unidad.tipo}", this);
                        continue;
                }

                GameObject objeto = CrearSprite($"Unidad_{propietario}_{unidad.tipo}_{unidad.coordenada.x}_{unidad.coordenada.y}",
                    sprite, unidad.coordenada.x, unidad.coordenada.y, 30, unidades,
                    Vector3.one * escalaUnidades);
                ConfigurarSeleccionable(
                    objeto,
                    CategoriaEntidadVisual.Unidad,
                    unidad.tipo,
                    propietario,
                    unidad.coordenada,
                    unidad.id,
                    unidad.estado,
                    unidad.ordenActiva);
            }
        }

        private Vector3 PosicionVisual(float x, float y)
        {
            return new Vector3(x * espacioCasilla, y * espacioCasilla, 0f);
        }

        public bool ActualizarMovimientoUnidad(
            string unidadId,
            int x,
            int y,
            string estadoLogico,
            string ordenActiva)
        {
            if (string.IsNullOrWhiteSpace(unidadId))
            {
                return false;
            }

            EntidadSeleccionableVista[] entidades =
                GetComponentsInChildren<EntidadSeleccionableVista>(true);

            EntidadSeleccionableVista encontrada = null;

            foreach (EntidadSeleccionableVista entidad in entidades)
            {
                if (entidad != null &&
                    entidad.Categoria == CategoriaEntidadVisual.Unidad &&
                    entidad.IdLogico == unidadId)
                {
                    encontrada = entidad;
                    break;
                }
            }

            if (encontrada == null)
            {
                return false;
            }

            encontrada.ActualizarDatosLogicos(
                x,
                y,
                estadoLogico,
                ordenActiva);

            movimientosVisuales[unidadId] =
                new MovimientoVisualPendiente
                {
                    Entidad = encontrada,
                    Destino = PosicionVisual(x, y)
                };

            return true;
        }

        private void Update()
        {
            if (movimientosVisuales.Count == 0)
            {
                return;
            }

            var completados =
                new List<string>();

            foreach (KeyValuePair<string, MovimientoVisualPendiente> par
                     in movimientosVisuales)
            {
                MovimientoVisualPendiente movimiento = par.Value;

                if (movimiento == null ||
                    movimiento.Entidad == null ||
                    !movimiento.Entidad.isActiveAndEnabled)
                {
                    completados.Add(par.Key);
                    continue;
                }

                Transform transformUnidad =
                    movimiento.Entidad.transform;

                float paso =
                    velocidadMovimientoVisual *
                    Time.unscaledDeltaTime;

                transformUnidad.position =
                    Vector3.MoveTowards(
                        transformUnidad.position,
                        movimiento.Destino,
                        paso);

                if ((transformUnidad.position - movimiento.Destino)
                    .sqrMagnitude <= 0.0001f)
                {
                    transformUnidad.position =
                        movimiento.Destino;

                    completados.Add(par.Key);
                }
            }

            foreach (string unidadId in completados)
            {
                movimientosVisuales.Remove(unidadId);
            }
        }

        public bool TryObtenerCoordenadaLogica(Vector3 posicionMundo, out int x, out int y)
        {
            x = 0;
            y = 0;
            if (anchoVisual <= 0 || altoVisual <= 0 || espacioCasilla <= 0f)
            {
                return false;
            }

            float columna = posicionMundo.x / espacioCasilla;
            float fila = posicionMundo.y / espacioCasilla;
            // Casillas centradas en enteros: borde inferior incluido, superior excluido.
            // Esta comparación también rechaza NaN e infinitos sin convertirlos a int.
            if (!(columna >= -0.5f && columna < anchoVisual - 0.5f &&
                  fila >= -0.5f && fila < altoVisual - 0.5f))
            {
                return false;
            }

            x = Mathf.FloorToInt(columna + 0.5f);
            y = Mathf.FloorToInt(fila + 0.5f);
            return true;
        }

        private void ConfigurarSeleccionable(
            GameObject objeto,
            CategoriaEntidadVisual categoria,
            string tipo,
            string propietario,
            CoordenadaEstadoDto coordenada,
            string idLogico = "",
            string estadoLogico = "",
            string ordenActiva = "")
        {
            if (objeto == null)
            {
                return;
            }

            var entidad = objeto.AddComponent<EntidadSeleccionableVista>();

            entidad.Configurar(
                categoria,
                idLogico,
                tipo,
                propietario,
                coordenada.x,
                coordenada.y,
                estadoLogico,
                ordenActiva);

            var collider = objeto.AddComponent<BoxCollider2D>();

            collider.size = entidad.Renderer.sprite.bounds.size;
            collider.offset = entidad.Renderer.sprite.bounds.center;
        }

        private GameObject CrearSprite(
            string nombre, Sprite sprite, int x, int y, int orden, Transform contenedor, Vector3 escala)
        {
            if (sprite == null)
            {
                Debug.LogWarning($"Sprite no configurado para {nombre}; se omite.", this);
                return null;
            }

            var objeto = new GameObject(nombre);
            objeto.transform.SetParent(contenedor, false);
            objeto.transform.position = PosicionVisual(x, y);
            objeto.transform.localScale = escala;
            var renderer = objeto.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = orden;
            return objeto;
        }

        private void AjustarCamara(MapaEstadoDto mapa)
        {
            if (camara == null)
            {
                Debug.LogWarning("No está configurada la cámara de VistaPartida.", this);
                return;
            }

            camara.orthographic = true;
            camara.transform.position = PosicionVisual((mapa.ancho - 1) / 2f, (mapa.alto - 1) / 2f)
                + new Vector3(0f, 0f, -10f);
            camara.transform.rotation = Quaternion.identity;
            float aspecto = Mathf.Max(camara.aspect, 0.01f);
            camara.orthographicSize = Mathf.Max(
                mapa.alto * espacioCasilla / 2f,
                mapa.ancho * espacioCasilla / (2f * aspecto)) + espacioCasilla;
        }
    }
}
