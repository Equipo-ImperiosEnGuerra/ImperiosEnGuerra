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

        private void Limpiar()
        {
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

                CrearSprite($"Recurso_{recurso.tipo}_{recurso.coordenada.x}_{recurso.coordenada.y}",
                    sprite, recurso.coordenada.x, recurso.coordenada.y, 10, contenedor,
                    Vector3.one * escalaRecursos);
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

                    CrearSprite($"Edificio_{propietario}_{edificio.tipo}_{edificio.coordenada.x}_{edificio.coordenada.y}",
                        humano ? centroHumano : centroMaquina,
                        edificio.coordenada.x, edificio.coordenada.y, 20, edificios,
                        Vector3.one * escalaEdificios);
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

                CrearSprite($"Unidad_{propietario}_{unidad.tipo}_{unidad.coordenada.x}_{unidad.coordenada.y}",
                    sprite, unidad.coordenada.x, unidad.coordenada.y, 30, unidades,
                    Vector3.one * escalaUnidades);
            }
        }

        private Vector3 PosicionVisual(float x, float y)
        {
            return new Vector3(x * espacioCasilla, y * espacioCasilla, 0f);
        }

        private void CrearSprite(
            string nombre, Sprite sprite, int x, int y, int orden, Transform contenedor, Vector3 escala)
        {
            if (sprite == null)
            {
                Debug.LogWarning($"Sprite no configurado para {nombre}; se omite.", this);
                return;
            }

            var objeto = new GameObject(nombre);
            objeto.transform.SetParent(contenedor, false);
            objeto.transform.position = PosicionVisual(x, y);
            objeto.transform.localScale = escala;
            var renderer = objeto.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = orden;
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
