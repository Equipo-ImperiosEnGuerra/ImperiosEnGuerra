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

        private void Start()
        {
            StartCoroutine(ComprobarConexion());
        }

        private IEnumerator ComprobarConexion()
        {
            string url = $"{urlBaseApi}/api/estado";

            using UnityWebRequest request = UnityWebRequest.Get(url);

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
            IniciarPartidaDto partida = CrearPartidaPrueba();

            string json = JsonUtility.ToJson(partida);
            byte[] cuerpo = Encoding.UTF8.GetBytes(json);

            string url = $"{urlBaseApi}/api/partida/iniciar";

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

        private IEnumerator ObtenerPartidaActiva()
        {
            string url = $"{urlBaseApi}/api/partida";

            using UnityWebRequest request =
                UnityWebRequest.Get(url);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(
                    $"No se pudo obtener la partida activa. " +
                    $"HTTP {request.responseCode}: " +
                    $"{request.downloadHandler.text}");

                yield break;
            }

            Debug.Log(
                $"Partida activa obtenida correctamente: " +
                $"{request.downloadHandler.text}");

            if (vistaPartida == null)
            {
                Debug.LogError("VistaPartida no está configurada en ControladorAPI.");
                yield break;
            }

            string json = request.downloadHandler.text;
            EstadoPartidaDto estadoPartida;
            try
            {
                estadoPartida = JsonUtility.FromJson<EstadoPartidaDto>(json);
            }
            catch (System.ArgumentException ex)
            {
                Debug.LogError($"La respuesta de la partida no es JSON válido: {ex.Message}");
                yield break;
            }

            if (estadoPartida == null)
            {
                Debug.LogError("La API devolvió un estado de partida nulo.");
                yield break;
            }

            vistaPartida.Renderizar(estadoPartida);
            if (vistaHud != null)
            {
                var recursos = estadoPartida.jugadorHumano?.recursos;
                if (recursos != null)
                {
                    vistaHud.MostrarRecursos(recursos.oro, recursos.madera, recursos.comida);
                    vistaHud.MostrarMensaje("Partida recibida correctamente.");
                }
                else
                {
                    vistaHud.MostrarMensaje("La respuesta no contiene los recursos del jugador humano.", true);
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
