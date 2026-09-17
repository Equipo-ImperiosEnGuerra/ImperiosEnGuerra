using System.Collections;
using System.Text;
using ImperiosEnGuerra.Controladores.Red.Contratos;
using ImperiosEnGuerra.Vistas;
using UnityEngine;
using UnityEngine.Networking;

namespace ImperiosEnGuerra.Controladores.Red
{
    public class ControladorConexionApi : MonoBehaviour
    {
        [SerializeField]
        private string urlBaseApi = "http://localhost:5086";

        [SerializeField]
        private VistaPartida vistaPartida;

        [SerializeField]
        private VistaHud vistaHud;

        public bool MovimientoEnCurso { get; private set; }
        public bool RecoleccionEnCurso { get; private set; }
        public bool AccionEnCurso => MovimientoEnCurso || RecoleccionEnCurso;

        public void MoverUnidad(string unidadId, int x, int y)
        {
            if (!isActiveAndEnabled || AccionEnCurso)
            {
                MostrarError("La conexión no está disponible o hay una acción en curso.");
                return;
            }

            StartCoroutine(EnviarMovimiento(new MoverUnidadDto
            {
                unidadId = unidadId,
                destino = new CoordenadaDto(x, y)
            }));
        }

        public void IniciarRecoleccion(string aldeanoId, int x, int y)
        {
            if (!isActiveAndEnabled || AccionEnCurso)
            {
                MostrarError("La conexión no está disponible o hay una acción en curso.");
                return;
            }

            StartCoroutine(EnviarRecoleccion(new RecolectarDto
            {
                aldeanoId = aldeanoId,
                objetivo = new CoordenadaDto(x, y)
            }));
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            MovimientoEnCurso = false;
            RecoleccionEnCurso = false;
        }

        private IEnumerator EnviarMovimiento(MoverUnidadDto movimiento)
        {
            MovimientoEnCurso = true;

            try
            {
                using var request = new UnityWebRequest(
                    $"{urlBaseApi}/api/partida/mover",
                    UnityWebRequest.kHttpVerbPOST);

                request.uploadHandler = new UploadHandlerRaw(
                    Encoding.UTF8.GetBytes(
                        JsonUtility.ToJson(movimiento)));

                request.downloadHandler =
                    new DownloadHandlerBuffer();

                request.SetRequestHeader(
                    "Content-Type",
                    "application/json");

                request.timeout = 15;

                yield return request.SendWebRequest();

                ResultadoAccionDto resultado =
                    LeerResultado(request.downloadHandler.text);

                if (request.result != UnityWebRequest.Result.Success)
                {
                    MostrarError(
                        MensajeError(
                            resultado,
                            $"No se pudo confirmar el movimiento. HTTP {request.responseCode}: {request.error}"));

                    yield break;
                }

                if (resultado == null || !resultado.exito)
                {
                    MostrarError(
                        MensajeError(
                            resultado,
                            "La API no confirmó el movimiento."));

                    yield break;
                }

                yield return ObtenerPartidaActiva(
                    string.IsNullOrWhiteSpace(resultado.mensaje)
                        ? "Movimiento realizado."
                        : resultado.mensaje,
                    "Movimiento aceptado, pero no se pudo actualizar la vista. ");
            }
            finally
            {
                MovimientoEnCurso = false;
            }
        }

        private IEnumerator EnviarRecoleccion(RecolectarDto recoleccion)
        {
            RecoleccionEnCurso = true;

            try
            {
                using var request = new UnityWebRequest(
                    $"{urlBaseApi}/api/partida/recolectar",
                    UnityWebRequest.kHttpVerbPOST);

                request.uploadHandler = new UploadHandlerRaw(
                    Encoding.UTF8.GetBytes(
                        JsonUtility.ToJson(recoleccion)));

                request.downloadHandler =
                    new DownloadHandlerBuffer();

                request.SetRequestHeader(
                    "Content-Type",
                    "application/json");

                request.timeout = 15;

                yield return request.SendWebRequest();

                ResultadoAccionDto resultado =
                    LeerResultado(request.downloadHandler.text);

                if (request.result != UnityWebRequest.Result.Success)
                {
                    MostrarError(
                        MensajeError(
                            resultado,
                            $"No se pudo preparar la recolección. HTTP {request.responseCode}: {request.error}"));

                    yield break;
                }

                if (resultado == null || !resultado.exito)
                {
                    MostrarError(
                        MensajeError(
                            resultado,
                            "La API no confirmó la recolección."));

                    yield break;
                }

                string mensaje =
                    string.IsNullOrWhiteSpace(resultado.mensaje)
                        ? "Recolección preparada."
                        : resultado.mensaje;

                if (vistaHud != null)
                    vistaHud.MostrarMensaje(mensaje);

                Debug.Log(mensaje);
            }
            finally
            {
                RecoleccionEnCurso = false;
            }
        }

        private static ResultadoAccionDto LeerResultado(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                return JsonUtility.FromJson<ResultadoAccionDto>(json);
            }
            catch (System.ArgumentException ex)
            {
                Debug.LogWarning(
                    $"La respuesta de la API no es JSON válido: {ex.Message}");

                return null;
            }
        }

        private static string MensajeError(
            ResultadoAccionDto resultado,
            string alternativa)
        {
            if (!string.IsNullOrWhiteSpace(resultado?.mensaje))
                return resultado.mensaje;

            if (!string.IsNullOrWhiteSpace(resultado?.error))
                return resultado.error;

            return alternativa;
        }

        private void MostrarError(string mensaje)
        {
            Debug.LogError(mensaje, this);

            if (vistaHud != null)
                vistaHud.MostrarMensaje(mensaje, true);
        }

        private void Start()
        {
            StartCoroutine(ComprobarConexion());
        }

        private IEnumerator ComprobarConexion()
        {
            string url =
                $"{urlBaseApi}/api/estado";

            using UnityWebRequest request =
                UnityWebRequest.Get(url);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(
                    $"No se pudo conectar con la API: {request.error}");

                yield break;
            }

            Debug.Log(
                $"API conectada correctamente: " +
                $"{request.downloadHandler.text}");

            yield return IniciarPartidaPrueba();

            yield return ObtenerPartidaActiva();
        }

        private IEnumerator IniciarPartidaPrueba()
        {
            IniciarPartidaDto partida =
                CrearPartidaPrueba();

            string json =
                JsonUtility.ToJson(partida);

            byte[] cuerpo =
                Encoding.UTF8.GetBytes(json);

            string url =
                $"{urlBaseApi}/api/partida/iniciar";

            using UnityWebRequest request =
                new UnityWebRequest(
                    url,
                    UnityWebRequest.kHttpVerbPOST);

            request.uploadHandler =
                new UploadHandlerRaw(cuerpo);

            request.downloadHandler =
                new DownloadHandlerBuffer();

            request.SetRequestHeader(
                "Content-Type",
                "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(
                    $"No se pudo iniciar la partida. " +
                    $"HTTP {request.responseCode}: " +
                    $"{request.downloadHandler.text}");

                yield break;
            }

            Debug.Log(
                $"Partida iniciada correctamente: " +
                $"{request.downloadHandler.text}");
        }

        private IEnumerator ObtenerPartidaActiva(
            string mensajeExito = "Partida recibida correctamente.",
            string contextoError = "")
        {
            string url =
                $"{urlBaseApi}/api/partida";

            using UnityWebRequest request =
                UnityWebRequest.Get(url);

            request.timeout = 15;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                MostrarError(
                    contextoError +
                    MensajeError(
                        LeerResultado(request.downloadHandler.text),
                        $"No se pudo obtener la partida activa. HTTP {request.responseCode}: {request.error}"));

                yield break;
            }

            Debug.Log(
                $"Partida activa obtenida correctamente: " +
                $"{request.downloadHandler.text}");

            if (vistaPartida == null)
            {
                MostrarError(
                    contextoError +
                    "VistaPartida no está configurada en ControladorAPI.");

                yield break;
            }

            string json =
                request.downloadHandler.text;

            EstadoPartidaDto estadoPartida;

            try
            {
                estadoPartida =
                    JsonUtility.FromJson<EstadoPartidaDto>(json);
            }
            catch (System.ArgumentException ex)
            {
                MostrarError(
                    contextoError +
                    $"La respuesta de la partida no es JSON válido: {ex.Message}");

                yield break;
            }

            if (estadoPartida?.mapa == null ||
                estadoPartida.mapa.ancho <= 0 ||
                estadoPartida.mapa.alto <= 0 ||
                estadoPartida.jugadorHumano == null ||
                estadoPartida.jugadorMaquina == null)
            {
                MostrarError(
                    contextoError +
                    "La API devolvió un estado de partida incompleto.");

                yield break;
            }

            vistaPartida.Renderizar(estadoPartida);

            if (vistaHud != null)
            {
                var recursos =
                    estadoPartida.jugadorHumano?.recursos;

                if (recursos != null)
                {
                    vistaHud.MostrarRecursos(
                        recursos.oro,
                        recursos.madera,
                        recursos.comida);

                    vistaHud.MostrarMensaje(
                        mensajeExito);
                }
                else
                {
                    vistaHud.MostrarMensaje(
                        "La respuesta no contiene los recursos del jugador humano.",
                        true);
                }
            }
        }

        private IniciarPartidaDto CrearPartidaPrueba()
        {
            return new IniciarPartidaDto
            {
                nombreHumano = "Jugador",
                nombreMaquina = "CPU",

                anchoMapa = 10,
                altoMapa = 10,

                centroHumano =
                    new CoordenadaDto(1, 1),

                centroMaquina =
                    new CoordenadaDto(8, 8),

                recursosHumano = new[]
                {
                    new RecursoInicialDto(
                        "Oro",
                        1,
                        2),

                    new RecursoInicialDto(
                        "Madera",
                        2,
                        1),

                    new RecursoInicialDto(
                        "Comida",
                        2,
                        2)
                },

                recursosMaquina = new[]
                {
                    new RecursoInicialDto(
                        "Oro",
                        8,
                        7),

                    new RecursoInicialDto(
                        "Madera",
                        7,
                        8),

                    new RecursoInicialDto(
                        "Comida",
                        7,
                        7)
                }
            };
        }
    }
}