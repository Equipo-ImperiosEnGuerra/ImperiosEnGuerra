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

        private EconomiaEstadoDto economiaActual;

    public bool MovimientoEnCurso { get; private set; }
    public bool RecoleccionEnCurso { get; private set; }
    public bool ConstruccionEnCurso { get; private set; }
    public bool EntrenamientoEnCurso { get; private set; }
    public bool AtaqueEnCurso { get; private set; }

    private int movimientosActivos;
    private int recoleccionesActivas;
    private int construccionesActivas;
    private int entrenamientosActivos;
    private int ataquesActivos;

public bool AccionEnCurso =>
    MovimientoEnCurso ||
    RecoleccionEnCurso ||
    ConstruccionEnCurso ||
    EntrenamientoEnCurso ||
    AtaqueEnCurso;

public bool PuedeIniciarMovimiento =>
    isActiveAndEnabled;

public bool PuedeIniciarRecoleccion =>
    isActiveAndEnabled;

public bool PuedeIniciarConstruccion =>
    isActiveAndEnabled;

public bool PuedeIniciarEntrenamiento =>
    isActiveAndEnabled;

public bool PuedeIniciarAtaque =>
    isActiveAndEnabled;

        public void MoverUnidad(string unidadId, int x, int y)
        {
            if (!PuedeIniciarMovimiento)
            {
                MostrarError("La conexión con la API no está disponible.");
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
            if (!PuedeIniciarRecoleccion)
            {
                MostrarError("La conexión con la API no está disponible.");
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
        if (!PuedeIniciarConstruccion)
        {
            MostrarError(
                "La conexión con la API no está disponible.");
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
            if (!PuedeIniciarEntrenamiento)
            {
                MostrarError(
                    "La conexión con la API no está disponible.");
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
            if (!PuedeIniciarAtaque)
            {
                MostrarError(
                    "La conexión con la API no está disponible.");
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

            movimientosActivos = 0;
            recoleccionesActivas = 0;
            construccionesActivas = 0;
            entrenamientosActivos = 0;
            ataquesActivos = 0;

            MovimientoEnCurso = false;
            RecoleccionEnCurso = false;
            ConstruccionEnCurso = false;
            EntrenamientoEnCurso = false;
            AtaqueEnCurso = false;
        }

        private IEnumerator EnviarMovimiento(MoverUnidadDto movimiento)
        {
            movimientosActivos++;
            MovimientoEnCurso = movimientosActivos > 0;

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

                yield return EsperarResultadoMovimiento(
                    proceso.procesoId,
                    movimiento.unidadId);
            }
            finally
            {
                movimientosActivos =
                    Mathf.Max(0, movimientosActivos - 1);

                MovimientoEnCurso =
                    movimientosActivos > 0;
            }
        }

        private IEnumerator EsperarResultadoMovimiento(
            string procesoId,
            string unidadId)
        {
            const float intervaloConsulta = 0.1f;
            while (isActiveAndEnabled)
            {
                using UnityWebRequest request =
                    UnityWebRequest.Get(
                        $"{urlBaseApi}/api/procesos/{procesoId}/resultado");

                request.timeout = 5;

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning(
                        $"Consulta temporal de movimiento fallida. Se reintentará: " +
                        $"HTTP {request.responseCode}: {request.error}",
                        this);

                    yield return new WaitForSecondsRealtime(0.5f);
                    continue;
                }

                if (request.responseCode == 204 ||
                    string.IsNullOrWhiteSpace(
                        request.downloadHandler.text))
                {
                    yield return ActualizarMovimientoEnCurso(
                        unidadId);

                    yield return new WaitForSecondsRealtime(
                        intervaloConsulta);
                    continue;
                }

                ResultadoProcesoDto resultado =
                    LeerResultadoProceso(
                        request.downloadHandler.text);

                if (resultado == null)
                {
                    MostrarError(
                        "La API devolvió un resultado concurrente inválido.");

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.procesoId != procesoId)
                {
                    MostrarError(
                        "Se recibió el resultado de un proceso distinto al movimiento esperado.");

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.estado == "Cancelado")
                {
                    Debug.Log("Movimiento cancelado.");
                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.estado == "Fallido")
                {
                    MostrarError(
                        string.IsNullOrWhiteSpace(
                            resultado.errorTecnico)
                            ? "El worker de movimiento finalizó con error."
                            : resultado.errorTecnico);

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.estado != "Completado")
                {
                    MostrarError(
                        $"Estado concurrente no reconocido: {resultado.estado}");

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (!resultado.exito)
                {
                    MostrarError(
                        string.IsNullOrWhiteSpace(
                            resultado.mensaje)
                            ? "El movimiento fue rechazado por el Modelo."
                            : resultado.mensaje);

                    yield return SincronizarEstadoDespuesDeProceso();
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
                    "Movimiento completado, pero no se pudo actualizar la vista. ",
                    false);

                yield break;
            }

            Debug.Log(
                "Seguimiento concurrente detenido porque el controlador dejó de estar activo.");
        }

        private IEnumerator ActualizarMovimientoEnCurso(
            string unidadId)
        {
            if (vistaPartida == null ||
                string.IsNullOrWhiteSpace(unidadId))
            {
                yield break;
            }

            using UnityWebRequest request =
                UnityWebRequest.Get(
                    $"{urlBaseApi}/api/partida");

            request.timeout = 5;

            yield return request.SendWebRequest();

            if (request.result !=
                UnityWebRequest.Result.Success)
            {
                Debug.LogWarning(
                    $"No se pudo actualizar el snapshot de movimiento: {request.error}",
                    this);

                yield break;
            }

            EstadoPartidaDto estado;

            try
            {
                estado =
                    JsonUtility.FromJson<EstadoPartidaDto>(
                        request.downloadHandler.text);
            }
            catch (System.ArgumentException ex)
            {
                Debug.LogWarning(
                    $"Snapshot de movimiento inválido: {ex.Message}",
                    this);

                yield break;
            }

            UnidadEstadoDto unidad =
                BuscarUnidadHumana(
                    estado,
                    unidadId);

            if (unidad?.coordenada == null)
            {
                yield break;
            }

            vistaPartida.ActualizarMovimientoUnidad(
                unidad.id,
                unidad.coordenada.x,
                unidad.coordenada.y,
                unidad.estado,
                unidad.ordenActiva);
        }

        private static UnidadEstadoDto BuscarUnidadHumana(
            EstadoPartidaDto estado,
            string unidadId)
        {
            UnidadEstadoDto[] unidades =
                estado?.jugadorHumano?.unidades;

            if (unidades == null)
            {
                return null;
            }

            foreach (UnidadEstadoDto unidad in unidades)
            {
                if (unidad != null &&
                    unidad.id == unidadId)
                {
                    return unidad;
                }
            }

            return null;
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
            recoleccionesActivas++;
            RecoleccionEnCurso = recoleccionesActivas > 0;

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

                yield return EsperarResultadoRecoleccion(
                    proceso.procesoId,
                    recoleccion.aldeanoId);
            }
            finally
            {
                recoleccionesActivas =
                    Mathf.Max(0, recoleccionesActivas - 1);

                RecoleccionEnCurso =
                    recoleccionesActivas > 0;
            }
        }

        private IEnumerator EsperarResultadoRecoleccion(
            string procesoId,
            string unidadId)
        {
            const float intervaloConsulta = 0.1f;
            // La recolección orgánica incluye desplazamiento y varios ciclos
            // de carga, por lo que puede superar el límite anterior de 15 s.
            while (isActiveAndEnabled)
            {
                using UnityWebRequest request =
                    UnityWebRequest.Get(
                        $"{urlBaseApi}/api/procesos/{procesoId}/resultado");

                request.timeout = 5;

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning(
                        $"Consulta temporal de recolección fallida. Se reintentará: " +
                        $"HTTP {request.responseCode}: {request.error}",
                        this);

                    yield return new WaitForSecondsRealtime(0.5f);
                    continue;
                }

                if (request.responseCode == 204 ||
                    string.IsNullOrWhiteSpace(
                        request.downloadHandler.text))
                {
                    // La recolección también contiene una fase de movimiento.
                    // Consumimos snapshots intermedios para que Unity represente
                    // cada paso en vez de saltar a la posición final.
                    yield return ActualizarRecoleccionEnCurso(
                        unidadId);

                    yield return new WaitForSecondsRealtime(
                        intervaloConsulta);
                    continue;
                }

                ResultadoProcesoDto resultado =
                    LeerResultadoProceso(
                        request.downloadHandler.text);

                if (resultado == null)
                {
                    MostrarError(
                        "La API devolvió un resultado concurrente inválido.");
                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.procesoId != procesoId)
                {
                    MostrarError(
                        "Se recibió el resultado de un proceso distinto a la recolección esperada.");
                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.estado == "Cancelado")
                {
                    Debug.Log(
                        "Recolección cancelada.");
                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.estado == "Fallido")
                {
                    MostrarError(
                        string.IsNullOrWhiteSpace(
                            resultado.errorTecnico)
                            ? "El worker de recolección finalizó con error."
                            : resultado.errorTecnico);

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.estado != "Completado")
                {
                    MostrarError(
                        $"Estado concurrente no reconocido: {resultado.estado}");
                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (!resultado.exito)
                {
                    MostrarError(
                        string.IsNullOrWhiteSpace(
                            resultado.mensaje)
                            ? "La recolección fue rechazada por el Modelo."
                            : resultado.mensaje);
                    yield return SincronizarEstadoDespuesDeProceso();
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
                    "Recolección completada, pero no se pudo actualizar la vista. ",
                    false);

                yield break;
            }

            Debug.Log(
                "Seguimiento concurrente detenido porque el controlador dejó de estar activo.");
        }

        private IEnumerator ActualizarRecoleccionEnCurso(
            string unidadId)
        {
            if (vistaPartida == null ||
                string.IsNullOrWhiteSpace(unidadId))
            {
                yield break;
            }

            using UnityWebRequest request =
                UnityWebRequest.Get(
                    $"{urlBaseApi}/api/partida");

            request.timeout = 5;

            yield return request.SendWebRequest();

            if (request.result !=
                UnityWebRequest.Result.Success)
            {
                yield break;
            }

            EstadoPartidaDto estado;

            try
            {
                estado =
                    JsonUtility.FromJson<EstadoPartidaDto>(
                        request.downloadHandler.text);
            }
            catch (System.ArgumentException)
            {
                yield break;
            }

            UnidadEstadoDto unidad =
                BuscarUnidadHumana(
                    estado,
                    unidadId);

            if (unidad?.coordenada != null)
            {
                vistaPartida.ActualizarMovimientoUnidad(
                    unidad.id,
                    unidad.coordenada.x,
                    unidad.coordenada.y,
                    unidad.estado,
                    unidad.ordenActiva);
            }

            var recursos =
                estado?.jugadorHumano?.recursos;

            if (vistaHud != null &&
                recursos != null)
            {
                vistaHud.MostrarRecursos(
                    recursos.oro,
                    recursos.madera,
                    recursos.comida);

            }
        }

        private IEnumerator EnviarConstruccion(
            ConstruirDto construccion)
        {
            construccionesActivas++;
            ConstruccionEnCurso = construccionesActivas > 0;

            try
            {
                using var request =
                    new UnityWebRequest(
                        $"{urlBaseApi}/api/partida/construir-concurrente",
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

                if (request.result !=
                    UnityWebRequest.Result.Success)
                {
                    MostrarError(
                        $"No se pudo iniciar la construcción concurrente. HTTP {request.responseCode}: {request.error}");

                    yield break;
                }

                ProcesoIniciadoDto proceso =
                    LeerProcesoIniciado(
                        request.downloadHandler.text);

                if (proceso == null ||
                    string.IsNullOrWhiteSpace(
                        proceso.procesoId))
                {
                    MostrarError(
                        "La API no devolvió un identificador válido para la construcción concurrente.");

                    yield break;
                }

                yield return EsperarResultadoConstruccion(
                    proceso.procesoId);
            }
            finally
            {
                construccionesActivas =
                    Mathf.Max(0, construccionesActivas - 1);

                ConstruccionEnCurso =
                    construccionesActivas > 0;
            }
        }

        private IEnumerator EsperarResultadoConstruccion(
            string procesoId)
        {
            const float intervaloConsulta = 0.1f;
            while (isActiveAndEnabled)
            {
                using UnityWebRequest request =
                    UnityWebRequest.Get(
                        $"{urlBaseApi}/api/procesos/{procesoId}/resultado");

                request.timeout = 5;

                yield return request.SendWebRequest();

                if (request.result !=
                    UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning(
                        $"Consulta temporal de construcción fallida. Se reintentará: " +
                        $"HTTP {request.responseCode}: {request.error}",
                        this);

                    yield return new WaitForSecondsRealtime(0.5f);
                    continue;
                }

                if (request.responseCode == 204 ||
                    string.IsNullOrWhiteSpace(
                        request.downloadHandler.text))
                {
                    yield return ObtenerPartidaActiva(
                        "",
                        "",
                        false);

                    yield return new WaitForSecondsRealtime(
                        0.5f);
                    continue;
                }

                ResultadoProcesoDto resultado =
                    LeerResultadoProceso(
                        request.downloadHandler.text);

                if (resultado == null)
                {
                    MostrarError(
                        "La API devolvió un resultado concurrente inválido.");

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.procesoId != procesoId)
                {
                    MostrarError(
                        "Se recibió el resultado de un proceso distinto a la construcción esperada.");

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.estado == "Cancelado")
                {
                    Debug.Log("Construcción cancelada.");
                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.estado == "Fallido")
                {
                    MostrarError(
                        string.IsNullOrWhiteSpace(
                            resultado.errorTecnico)
                            ? "El worker de construcción finalizó con error."
                            : resultado.errorTecnico);

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.estado != "Completado")
                {
                    MostrarError(
                        $"Estado concurrente no reconocido: {resultado.estado}");

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (!resultado.exito)
                {
                    MostrarError(
                        string.IsNullOrWhiteSpace(
                            resultado.mensaje)
                            ? "La construcción fue rechazada por el Modelo."
                            : resultado.mensaje);

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                Debug.Log(
                    $"Construcción ejecutada por worker " +
                    $"{resultado.hiloTrabajoId}.");

                yield return ObtenerPartidaActiva(
                    string.IsNullOrWhiteSpace(
                        resultado.mensaje)
                        ? "Construcción realizada."
                        : resultado.mensaje,
                    "Construcción completada, pero no se pudo actualizar la vista. ",
                    false);

                yield break;
            }

            Debug.Log(
                "Seguimiento concurrente detenido porque el controlador dejó de estar activo.");
        }
        

        private IEnumerator EnviarEntrenamiento(
            EntrenarDto entrenamiento)
        {
            entrenamientosActivos++;
            EntrenamientoEnCurso = entrenamientosActivos > 0;

            try
            {
                using var request =
                    new UnityWebRequest(
                        $"{urlBaseApi}/api/partida/entrenar-concurrente",
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

                if (request.result !=
                    UnityWebRequest.Result.Success)
                {
                    MostrarError(
                        $"No se pudo iniciar el entrenamiento concurrente. HTTP {request.responseCode}: {request.error}");

                    yield break;
                }

                ProcesoIniciadoDto proceso =
                    LeerProcesoIniciado(
                        request.downloadHandler.text);

                if (proceso == null ||
                    string.IsNullOrWhiteSpace(
                        proceso.procesoId))
                {
                    MostrarError(
                        "La API no devolvió un identificador válido para el entrenamiento concurrente.");

                    yield break;
                }

                yield return EsperarResultadoEntrenamiento(
                    proceso.procesoId,
                    entrenamiento.edificioOrigen);
            }
            finally
            {
                entrenamientosActivos =
                    Mathf.Max(0, entrenamientosActivos - 1);

                EntrenamientoEnCurso =
                    entrenamientosActivos > 0;
            }
        }

        private IEnumerator EsperarResultadoEntrenamiento(
            string procesoId,
            CoordenadaDto edificioOrigen)
        {
            const float intervaloConsulta = 0.25f;
            while (isActiveAndEnabled)
            {
                using UnityWebRequest request =
                    UnityWebRequest.Get(
                        $"{urlBaseApi}/api/procesos/{procesoId}/resultado");

                request.timeout = 5;

                yield return request.SendWebRequest();

                if (request.result !=
                    UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning(
                        $"Consulta temporal de entrenamiento fallida. Se reintentará: " +
                        $"HTTP {request.responseCode}: {request.error}",
                        this);

                    yield return new WaitForSecondsRealtime(0.5f);
                    continue;
                }

                if (request.responseCode == 204 ||
                    string.IsNullOrWhiteSpace(
                        request.downloadHandler.text))
                {
                    yield return ActualizarEntrenamientoEnCurso(
                        edificioOrigen);

                    yield return new WaitForSecondsRealtime(
                        intervaloConsulta);
                    continue;
                }

                ResultadoProcesoDto resultado =
                    LeerResultadoProceso(
                        request.downloadHandler.text);

                if (resultado == null)
                {
                    MostrarError(
                        "La API devolvió un resultado concurrente inválido.");

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.procesoId != procesoId)
                {
                    MostrarError(
                        "Se recibió el resultado de un proceso distinto al entrenamiento esperado.");

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.estado == "Cancelado")
                {
                    Debug.Log("Entrenamiento cancelado.");
                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.estado == "Fallido")
                {
                    MostrarError(
                        string.IsNullOrWhiteSpace(
                            resultado.errorTecnico)
                            ? "El worker de entrenamiento finalizó con error."
                            : resultado.errorTecnico);

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.estado != "Completado")
                {
                    MostrarError(
                        $"Estado concurrente no reconocido: {resultado.estado}");

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (!resultado.exito)
                {
                    MostrarError(
                        string.IsNullOrWhiteSpace(
                            resultado.mensaje)
                            ? "El entrenamiento fue rechazado por el Modelo."
                            : resultado.mensaje);

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                Debug.Log(
                    $"Entrenamiento ejecutado por worker " +
                    $"{resultado.hiloTrabajoId}.");

                yield return ObtenerPartidaActiva(
                    string.IsNullOrWhiteSpace(
                        resultado.mensaje)
                        ? "Entrenamiento realizado."
                        : resultado.mensaje,
                    "Entrenamiento completado, pero no se pudo actualizar la vista. ",
                    false);

                yield break;
            }

            Debug.Log(
                "Seguimiento concurrente detenido porque el controlador dejó de estar activo.");
        }

        private IEnumerator ActualizarEntrenamientoEnCurso(
            CoordenadaDto edificioOrigen)
        {
            if (edificioOrigen == null)
                yield break;

            using UnityWebRequest request =
                UnityWebRequest.Get(
                    $"{urlBaseApi}/api/partida");

            request.timeout = 5;

            yield return request.SendWebRequest();

            if (request.result !=
                UnityWebRequest.Result.Success)
            {
                yield break;
            }

            EstadoPartidaDto estado;

            try
            {
                estado =
                    JsonUtility.FromJson<EstadoPartidaDto>(
                        request.downloadHandler.text);
            }
            catch (System.ArgumentException)
            {
                yield break;
            }

            var recursos =
                estado?.jugadorHumano?.recursos;

            if (vistaHud != null &&
                recursos != null)
            {
                vistaHud.MostrarRecursos(
                    recursos.oro,
                    recursos.madera,
                    recursos.comida);
            }
        }

        private IEnumerator EnviarAtaque(
            AtaqueDto ataque)
        {
            ataquesActivos++;
            AtaqueEnCurso = ataquesActivos > 0;

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
                    "Ataque aceptado, pero no se pudo actualizar la vista. ",
                    false);
            }
            finally
            {
                ataquesActivos =
                    Mathf.Max(0, ataquesActivos - 1);

                AtaqueEnCurso =
                    ataquesActivos > 0;
            }
        }

        private IEnumerator SincronizarEstadoDespuesDeProceso()
        {
            // Los workers limpian OrdenActiva/Estado en sus bloques finally.
            // Esta sincronización evita que Unity conserve una copia visual
            // antigua (por ejemplo Estado=Moviendo) después de un fallo,
            // cancelación o rechazo del Modelo.
            yield return ObtenerPartidaActiva(
                "",
                "",
                false);
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

        public string DescribirCostoConstruccion()
        {
            return DescribirCosto(
                economiaActual?.centroUrbano);
        }

        public string DescribirCostoUnidad(
            string tipoUnidad)
        {
            if (economiaActual == null ||
                string.IsNullOrWhiteSpace(tipoUnidad))
            {
                return string.Empty;
            }

            CostoEstadoDto costo = null;

            switch (tipoUnidad)
            {
                case "Aldeano":
                    costo = economiaActual.aldeano;
                    break;
                case "Guerrero":
                    costo = economiaActual.guerrero;
                    break;
                case "Lancero":
                    costo = economiaActual.lancero;
                    break;
                case "Arquero":
                    costo = economiaActual.arquero;
                    break;
                case "Monje":
                    costo = economiaActual.monje;
                    break;
            }

            return DescribirCosto(
                costo);
        }

        private static string DescribirCosto(
            CostoEstadoDto costo)
        {
            if (costo == null)
                return string.Empty;

            return $"Costo: Oro {costo.oro}, " +
                   $"Madera {costo.madera}, " +
                   $"Comida {costo.comida}.";
        }

        private void MostrarError(string mensaje)
        {
            bool tecnico =
                EsErrorTecnico(mensaje);

            if (tecnico)
            {
                Debug.LogError(
                    mensaje,
                    this);
            }
            else
            {
                Debug.LogWarning(
                    mensaje,
                    this);
            }

            if (vistaHud != null)
            {
                vistaHud.MostrarMensaje(
                    mensaje,
                    tecnico);
            }
        }

        private static bool EsErrorTecnico(
            string mensaje)
        {
            if (string.IsNullOrWhiteSpace(
                    mensaje))
            {
                return false;
            }

            return
                mensaje.Contains("HTTP") ||
                mensaje.Contains("API no") ||
                mensaje.Contains("No se pudo consultar") ||
                mensaje.Contains("No se pudo conectar") ||
                mensaje.Contains("JSON") ||
                mensaje.Contains("worker") ||
                mensaje.Contains("Estado concurrente no reconocido") ||
                mensaje.Contains("VistaPartida no está configurada");
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
            string contextoError = "",
            bool mostrarMensaje = true)
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

                economiaActual =
                    estadoPartida?.economia;
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

            vistaPartida.Sincronizar(estadoPartida);

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

                    if (mostrarMensaje)
                    {
                        vistaHud.MostrarMensaje(
                            mensajeExito);
                    }
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