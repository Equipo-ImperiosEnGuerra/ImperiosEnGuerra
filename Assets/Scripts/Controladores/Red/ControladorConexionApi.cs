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
    public bool ConstruccionEnCurso { get; private set; }
    public bool EntrenamientoEnCurso { get; private set; }
    public bool AtaqueEnCurso { get; private set; }

public bool AccionEnCurso =>
    MovimientoEnCurso ||
    RecoleccionEnCurso ||
    ConstruccionEnCurso ||
    EntrenamientoEnCurso ||
    AtaqueEnCurso;

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

        public void Construir(
        string aldeanoId,
        string tipoEdificio,
        int x,
        int y)
    {
        if (!isActiveAndEnabled || AccionEnCurso)
        {
            MostrarError(
                "La conexión no está disponible o hay una acción en curso.");
            return;
        }

        StartCoroutine(
            EnviarConstruccion(
                new ConstruirDto
                {
                    aldeanoId = aldeanoId,
                    tipoEdificio = tipoEdificio,
                    destino = new CoordenadaDto(x, y)
                }));
    }

        public void Entrenar(
            int edificioX,
            int edificioY,
            string tipoUnidad,
            int destinoX,
            int destinoY)
        {
            if (!isActiveAndEnabled || AccionEnCurso)
            {
                MostrarError(
                    "La conexión no está disponible o hay una acción en curso.");
                return;
            }

            StartCoroutine(
                EnviarEntrenamiento(
                    new EntrenarDto
                    {
                        edificioOrigen =
                            new CoordenadaDto(
                                edificioX,
                                edificioY),

                        tipoUnidad = tipoUnidad,

                        destino =
                            new CoordenadaDto(
                                destinoX,
                                destinoY)
                    }));
        }

        public void Atacar(
            string atacanteId,
            string objetivoId)
        {
            if (!isActiveAndEnabled || AccionEnCurso)
            {
                MostrarError(
                    "La conexión no está disponible o hay una acción en curso.");
                return;
            }

            StartCoroutine(
                EnviarAtaque(
                    new AtaqueDto
                    {
                        atacanteId = atacanteId,
                        objetivoId = objetivoId
                    }));
        }

        private void OnDisable()
        {
            StopAllCoroutines();

            MovimientoEnCurso = false;
            RecoleccionEnCurso = false;
            ConstruccionEnCurso = false;
            EntrenamientoEnCurso = false;
            AtaqueEnCurso = false;
        }

        private IEnumerator EnviarMovimiento(MoverUnidadDto movimiento)
        {
            MovimientoEnCurso = true;

            try
            {
                using var request = new UnityWebRequest(
                    $"{urlBaseApi}/api/partida/mover-concurrente",
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

                if (request.result != UnityWebRequest.Result.Success)
                {
                    MostrarError(
                        $"No se pudo iniciar el movimiento concurrente. HTTP {request.responseCode}: {request.error}");

                    yield break;
                }

                ProcesoIniciadoDto proceso =
                    LeerProcesoIniciado(request.downloadHandler.text);

                if (proceso == null ||
                    string.IsNullOrWhiteSpace(proceso.procesoId))
                {
                    MostrarError(
                        "La API no devolvió un identificador válido para el movimiento concurrente.");

                    yield break;
                }

                if (vistaHud != null)
                {
                    vistaHud.MostrarMensaje(
                        "Movimiento concurrente en curso...");
                }

                yield return EsperarResultadoMovimiento(proceso.procesoId);
            }
            finally
            {
                MovimientoEnCurso = false;
            }
        }

        private IEnumerator EsperarResultadoMovimiento(string procesoId)
        {
            const float intervaloConsulta = 0.1f;
            const float tiempoMaximo = 15f;
            float tiempoTranscurrido = 0f;

            while (tiempoTranscurrido < tiempoMaximo)
            {
                using UnityWebRequest request =
                    UnityWebRequest.Get(
                        $"{urlBaseApi}/api/procesos/resultado");

                request.timeout = 5;

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    MostrarError(
                        $"No se pudo consultar el movimiento concurrente. HTTP {request.responseCode}: {request.error}");

                    yield break;
                }

                if (request.responseCode == 204 ||
                    string.IsNullOrWhiteSpace(
                        request.downloadHandler.text))
                {
                    yield return new WaitForSecondsRealtime(
                        intervaloConsulta);

                    tiempoTranscurrido += intervaloConsulta;
                    continue;
                }

                ResultadoProcesoDto resultado =
                    LeerResultadoProceso(
                        request.downloadHandler.text);

                if (resultado == null)
                {
                    MostrarError(
                        "La API devolvió un resultado concurrente inválido.");

                    yield break;
                }

                if (resultado.procesoId != procesoId)
                {
                    MostrarError(
                        "Se recibió el resultado de un proceso distinto al movimiento esperado.");

                    yield break;
                }

                if (resultado.estado == "Cancelado")
                {
                    if (vistaHud != null)
                    {
                        vistaHud.MostrarMensaje(
                            "Movimiento cancelado.");
                    }

                    yield break;
                }

                if (resultado.estado == "Fallido")
                {
                    MostrarError(
                        string.IsNullOrWhiteSpace(
                            resultado.errorTecnico)
                            ? "El worker de movimiento finalizó con error."
                            : resultado.errorTecnico);

                    yield break;
                }

                if (resultado.estado != "Completado")
                {
                    MostrarError(
                        $"Estado concurrente no reconocido: {resultado.estado}");

                    yield break;
                }

                if (!resultado.exito)
                {
                    MostrarError(
                        string.IsNullOrWhiteSpace(
                            resultado.mensaje)
                            ? "El movimiento fue rechazado por el Modelo."
                            : resultado.mensaje);

                    yield break;
                }

                Debug.Log(
                    $"Movimiento ejecutado por worker " +
                    $"{resultado.hiloTrabajoId}.");

                yield return ObtenerPartidaActiva(
                    string.IsNullOrWhiteSpace(
                        resultado.mensaje)
                        ? "Movimiento realizado."
                        : resultado.mensaje,
                    "Movimiento completado, pero no se pudo actualizar la vista. ");

                yield break;
            }

            MostrarError(
                "El movimiento concurrente excedió el tiempo máximo de espera.");
        }

        private static ProcesoIniciadoDto LeerProcesoIniciado(
            string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                return JsonUtility.FromJson<ProcesoIniciadoDto>(json);
            }
            catch (System.ArgumentException ex)
            {
                Debug.LogWarning(
                    $"La respuesta de inicio concurrente no es JSON válido: {ex.Message}");

                return null;
            }
        }

        private static ResultadoProcesoDto LeerResultadoProceso(
            string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                return JsonUtility.FromJson<ResultadoProcesoDto>(json);
            }
            catch (System.ArgumentException ex)
            {
                Debug.LogWarning(
                    $"La respuesta concurrente no es JSON válido: {ex.Message}");

                return null;
            }
        }

        private IEnumerator EnviarRecoleccion(RecolectarDto recoleccion)
        {
            RecoleccionEnCurso = true;

            try
            {
                using var request = new UnityWebRequest(
                    $"{urlBaseApi}/api/partida/recolectar-concurrente",
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

                if (request.result != UnityWebRequest.Result.Success)
                {
                    MostrarError(
                        $"No se pudo iniciar la recolección concurrente. HTTP {request.responseCode}: {request.error}");
                    yield break;
                }

                ProcesoIniciadoDto proceso =
                    LeerProcesoIniciado(request.downloadHandler.text);

                if (proceso == null ||
                    string.IsNullOrWhiteSpace(proceso.procesoId))
                {
                    MostrarError(
                        "La API no devolvió un identificador válido para la recolección concurrente.");
                    yield break;
                }

                if (vistaHud != null)
                    vistaHud.MostrarMensaje(
                        "Recolección concurrente en curso...");

                yield return EsperarResultadoRecoleccion(
                    proceso.procesoId);
            }
            finally
            {
                RecoleccionEnCurso = false;
            }
        }

        private IEnumerator EsperarResultadoRecoleccion(
            string procesoId)
        {
            const float intervaloConsulta = 0.1f;
            const float tiempoMaximo = 15f;
            float tiempoTranscurrido = 0f;

            while (tiempoTranscurrido < tiempoMaximo)
            {
                using UnityWebRequest request =
                    UnityWebRequest.Get(
                        $"{urlBaseApi}/api/procesos/resultado");

                request.timeout = 5;

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    MostrarError(
                        $"No se pudo consultar la recolección concurrente. HTTP {request.responseCode}: {request.error}");
                    yield break;
                }

                if (request.responseCode == 204 ||
                    string.IsNullOrWhiteSpace(
                        request.downloadHandler.text))
                {
                    yield return new WaitForSecondsRealtime(
                        intervaloConsulta);

                    tiempoTranscurrido += intervaloConsulta;
                    continue;
                }

                ResultadoProcesoDto resultado =
                    LeerResultadoProceso(
                        request.downloadHandler.text);

                if (resultado == null)
                {
                    MostrarError(
                        "La API devolvió un resultado concurrente inválido.");
                    yield break;
                }

                if (resultado.procesoId != procesoId)
                {
                    MostrarError(
                        "Se recibió el resultado de un proceso distinto a la recolección esperada.");
                    yield break;
                }

                if (resultado.estado == "Cancelado")
                {
                    if (vistaHud != null)
                        vistaHud.MostrarMensaje(
                            "Recolección cancelada.");
                    yield break;
                }

                if (resultado.estado == "Fallido")
                {
                    MostrarError(
                        string.IsNullOrWhiteSpace(
                            resultado.errorTecnico)
                            ? "El worker de recolección finalizó con error."
                            : resultado.errorTecnico);
                    yield break;
                }

                if (resultado.estado != "Completado")
                {
                    MostrarError(
                        $"Estado concurrente no reconocido: {resultado.estado}");
                    yield break;
                }

                if (!resultado.exito)
                {
                    MostrarError(
                        string.IsNullOrWhiteSpace(
                            resultado.mensaje)
                            ? "La recolección fue rechazada por el Modelo."
                            : resultado.mensaje);
                    yield break;
                }

                Debug.Log(
                    $"Recolección ejecutada por worker " +
                    $"{resultado.hiloTrabajoId}.");

                yield return ObtenerPartidaActiva(
                    string.IsNullOrWhiteSpace(
                        resultado.mensaje)
                        ? "Recolección preparada."
                        : resultado.mensaje,
                    "Recolección completada, pero no se pudo actualizar la vista. ");

                yield break;
            }

            MostrarError(
                "La recolección concurrente excedió el tiempo máximo de espera.");
        }

        private IEnumerator EnviarConstruccion(
            ConstruirDto construccion)
        {
            ConstruccionEnCurso = true;

            try
            {
                using var request =
                    new UnityWebRequest(
                        $"{urlBaseApi}/api/partida/construir",
                        UnityWebRequest.kHttpVerbPOST);

                request.uploadHandler =
                    new UploadHandlerRaw(
                        Encoding.UTF8.GetBytes(
                            JsonUtility.ToJson(construccion)));

                request.downloadHandler =
                    new DownloadHandlerBuffer();

                request.SetRequestHeader(
                    "Content-Type",
                    "application/json");

                request.timeout = 15;

                yield return request.SendWebRequest();

                ResultadoAccionDto resultado =
                    LeerResultado(
                        request.downloadHandler.text);

                if (request.result !=
                    UnityWebRequest.Result.Success)
                {
                    MostrarError(
                        MensajeError(
                            resultado,
                            $"No se pudo realizar la construcción. HTTP {request.responseCode}: {request.error}"));

                    yield break;
                }

                if (resultado == null ||
                    !resultado.exito)
                {
                    MostrarError(
                        MensajeError(
                            resultado,
                            "La API no confirmó la construcción."));

                    yield break;
                }

                yield return ObtenerPartidaActiva(
                    string.IsNullOrWhiteSpace(
                        resultado.mensaje)
                        ? "Construcción realizada."
                        : resultado.mensaje,
                    "Construcción aceptada, pero no se pudo actualizar la vista. ");
            }
            finally
            {
                ConstruccionEnCurso = false;
            }
        }
        

            private IEnumerator EnviarEntrenamiento(
            EntrenarDto entrenamiento)
        {
            EntrenamientoEnCurso = true;

            try
            {
                using var request =
                    new UnityWebRequest(
                        $"{urlBaseApi}/api/partida/entrenar",
                        UnityWebRequest.kHttpVerbPOST);

                request.uploadHandler =
                    new UploadHandlerRaw(
                        Encoding.UTF8.GetBytes(
                            JsonUtility.ToJson(entrenamiento)));

                request.downloadHandler =
                    new DownloadHandlerBuffer();

                request.SetRequestHeader(
                    "Content-Type",
                    "application/json");

                request.timeout = 15;

                yield return request.SendWebRequest();

                ResultadoAccionDto resultado =
                    LeerResultado(
                        request.downloadHandler.text);

                if (request.result !=
                    UnityWebRequest.Result.Success)
                {
                    MostrarError(
                        MensajeError(
                            resultado,
                            $"No se pudo realizar el entrenamiento. HTTP {request.responseCode}: {request.error}"));

                    yield break;
                }

                if (resultado == null ||
                    !resultado.exito)
                {
                    MostrarError(
                        MensajeError(
                            resultado,
                            "La API no confirmó el entrenamiento."));

                    yield break;
                }

                yield return ObtenerPartidaActiva(
                    string.IsNullOrWhiteSpace(
                        resultado.mensaje)
                        ? "Entrenamiento realizado."
                        : resultado.mensaje,
                    "Entrenamiento aceptado, pero no se pudo actualizar la vista. ");
            }
            finally
            {
                EntrenamientoEnCurso = false;
            }
        }

        private IEnumerator EnviarAtaque(
            AtaqueDto ataque)
        {
            AtaqueEnCurso = true;

            try
            {
                using var request =
                    new UnityWebRequest(
                        $"{urlBaseApi}/api/partida/atacar",
                        UnityWebRequest.kHttpVerbPOST);

                request.uploadHandler =
                    new UploadHandlerRaw(
                        Encoding.UTF8.GetBytes(
                            JsonUtility.ToJson(ataque)));

                request.downloadHandler =
                    new DownloadHandlerBuffer();

                request.SetRequestHeader(
                    "Content-Type",
                    "application/json");

                request.timeout = 15;

                yield return request.SendWebRequest();

                ResultadoAccionDto resultado =
                    LeerResultado(
                        request.downloadHandler.text);

                if (request.result !=
                    UnityWebRequest.Result.Success)
                {
                    MostrarError(
                        MensajeError(
                            resultado,
                            $"No se pudo preparar el ataque. HTTP {request.responseCode}: {request.error}"));

                    yield break;
                }

                if (resultado == null ||
                    !resultado.exito)
                {
                    MostrarError(
                        MensajeError(
                            resultado,
                            "La API no confirmó el ataque."));

                    yield break;
                }

                yield return ObtenerPartidaActiva(
                    string.IsNullOrWhiteSpace(
                        resultado.mensaje)
                        ? "Ataque preparado."
                        : resultado.mensaje,
                    "Ataque aceptado, pero no se pudo actualizar la vista. ");
            }
            finally
            {
                AtaqueEnCurso = false;
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