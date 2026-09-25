using System.Collections;
using System.Text;
using ImperiosEnGuerra.Controladores;
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

        [SerializeField]
        private ControladorSeleccion controladorSeleccion;

        private EconomiaEstadoDto economiaActual;
        private bool iniciandoPartidaDesdeMenu;
        private bool ultimoInicioPartidaExitoso;
        private bool ultimoEstadoPartidaValido;
        private bool sesionVisualActiva;
        private Coroutine sincronizacionPeriodica;

        private const float IntervaloSincronizacionEstado = 0.25f;
        private const float IntervaloConsultaProceso = 0.25f;

        public event System.Action PartidaIniciadaDesdeMenu;
        public event System.Action<string> InicioPartidaFallido;

    public bool MovimientoEnCurso { get; private set; }
    public bool RecoleccionEnCurso { get; private set; }
    public bool ConstruccionEnCurso { get; private set; }
    public bool EntrenamientoEnCurso { get; private set; }
    public bool AtaqueEnCurso { get; private set; }
    public bool CuracionEnCurso { get; private set; }
    public bool PartidaFinalizada { get; private set; }
    public bool ApiDisponible { get; private set; }

    public string MensajeAccionNoDisponible
    {
        get
        {
            if (PartidaFinalizada)
            {
                return "La partida ya finalizó.";
            }

            if (!ApiDisponible)
            {
                return "La conexión con la API no está disponible.";
            }

            return "La acción no está disponible en este momento.";
        }
    }

    private int movimientosActivos;
    private int recoleccionesActivas;
    private int construccionesActivas;
    private int entrenamientosActivos;
    private int ataquesActivos;
    private int curacionesActivas;

public bool AccionEnCurso =>
    MovimientoEnCurso ||
    RecoleccionEnCurso ||
    ConstruccionEnCurso ||
    EntrenamientoEnCurso ||
    AtaqueEnCurso ||
    CuracionEnCurso;

public bool PuedeIniciarMovimiento =>
    isActiveAndEnabled &&
    ApiDisponible &&
    !PartidaFinalizada;

public bool PuedeIniciarRecoleccion =>
    isActiveAndEnabled &&
    ApiDisponible &&
    !PartidaFinalizada;

public bool PuedeIniciarConstruccion =>
    isActiveAndEnabled &&
    ApiDisponible &&
    !PartidaFinalizada;

public bool PuedeIniciarEntrenamiento =>
    isActiveAndEnabled &&
    ApiDisponible &&
    !PartidaFinalizada;

public bool PuedeIniciarAtaque =>
    isActiveAndEnabled &&
    ApiDisponible &&
    !PartidaFinalizada;

public bool PuedeIniciarCuracion =>
    isActiveAndEnabled &&
    ApiDisponible &&
    !PartidaFinalizada;

public bool PuedeCancelarAccion =>
    isActiveAndEnabled &&
    ApiDisponible &&
    !PartidaFinalizada;

        public void MoverUnidad(string unidadId, int x, int y)
        {
            if (!PuedeIniciarMovimiento)
            {
                MostrarError(MensajeAccionNoDisponible);
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
                MostrarError(MensajeAccionNoDisponible);
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
                    MensajeAccionNoDisponible);
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
                    MensajeAccionNoDisponible);
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

        public void Curar(
            string curadorId,
            string objetivoId)
        {
            if (!PuedeIniciarCuracion)
            {
                MostrarError(
                    MensajeAccionNoDisponible);
                return;
            }

            StartCoroutine(
                EnviarCuracion(
                    new CuracionDto
                    {
                        curadorId = curadorId,
                        objetivoId = objetivoId
                    }));
        }

        public void CancelarAccionUnidad(
            string unidadId)
        {
            if (!PuedeCancelarAccion ||
                string.IsNullOrWhiteSpace(
                    unidadId))
            {
                MostrarError(
                    MensajeAccionNoDisponible);
                return;
            }

            StartCoroutine(
                EnviarCancelacionAccionUnidad(
                    unidadId));
        }

        private IEnumerator EnviarCancelacionAccionUnidad(
            string unidadId)
        {
            using var request =
                new UnityWebRequest(
                    $"{urlBaseApi}/api/partida/unidades/{unidadId}/cancelar-accion",
                    UnityWebRequest.kHttpVerbPOST);

            request.downloadHandler =
                new DownloadHandlerBuffer();

            request.timeout = 5;

            yield return request.SendWebRequest();

            if (request.result !=
                UnityWebRequest.Result.Success)
            {
                MostrarError(
                    MensajeError(
                        LeerResultado(
                            request.downloadHandler.text),
                        $"No se pudo cancelar la acción. HTTP {request.responseCode}: {request.error}"));

                yield break;
            }

            if (vistaHud != null)
            {
                vistaHud.MostrarMensaje(
                    "Acción cancelada.");
            }

            yield return new WaitForSecondsRealtime(
                0.05f);

            yield return ObtenerPartidaActiva(
                "",
                "",
                false,
                true);
        }

        public void PrepararRegresoAlMenu()
        {
            // El menú puede mostrarse sin recargar la escena. Así Play Mode
            // continúa activo y una nueva partida puede iniciarse después.
            sesionVisualActiva = false;
            sincronizacionPeriodica = null;
            StopAllCoroutines();

            iniciandoPartidaDesdeMenu = false;
            ultimoInicioPartidaExitoso = false;
            ultimoEstadoPartidaValido = false;
            economiaActual = null;

            movimientosActivos = 0;
            recoleccionesActivas = 0;
            construccionesActivas = 0;
            entrenamientosActivos = 0;
            ataquesActivos = 0;
            curacionesActivas = 0;

            MovimientoEnCurso = false;
            RecoleccionEnCurso = false;
            ConstruccionEnCurso = false;
            EntrenamientoEnCurso = false;
            AtaqueEnCurso = false;
            CuracionEnCurso = false;

            PartidaFinalizada = false;
            ApiDisponible = false;

            controladorSeleccion?.BloquearInteraccion();
            vistaHud?.OcultarResultadoFinal();
            vistaHud?.MostrarMensaje("");

            if (isActiveAndEnabled)
            {
                StartCoroutine(
                    PausarSesionRemota());
            }
        }

        private void OnDisable()
        {
            sesionVisualActiva = false;
            sincronizacionPeriodica = null;
            StopAllCoroutines();

            movimientosActivos = 0;
            recoleccionesActivas = 0;
            construccionesActivas = 0;
            entrenamientosActivos = 0;
            ataquesActivos = 0;
            curacionesActivas = 0;

            MovimientoEnCurso = false;
            RecoleccionEnCurso = false;
            ConstruccionEnCurso = false;
            EntrenamientoEnCurso = false;
            AtaqueEnCurso = false;
            CuracionEnCurso = false;
            ApiDisponible = false;
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
            const float intervaloConsulta = IntervaloConsultaProceso;
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
            const float intervaloConsulta = IntervaloConsultaProceso;
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
            const float intervaloConsulta = IntervaloConsultaProceso;
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
                        $"{urlBaseApi}/api/partida/atacar-concurrente",
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

                if (request.result !=
                    UnityWebRequest.Result.Success)
                {
                    MostrarError(
                        $"No se pudo iniciar el ataque concurrente. HTTP {request.responseCode}: {request.error}");

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
                        "La API no devolvió un identificador válido para el ataque concurrente.");

                    yield break;
                }

                yield return EsperarResultadoAtaque(
                    proceso.procesoId);
            }
            finally
            {
                ataquesActivos =
                    Mathf.Max(0, ataquesActivos - 1);

                AtaqueEnCurso =
                    ataquesActivos > 0;
            }
        }

        private IEnumerator EsperarResultadoAtaque(
            string procesoId)
        {
            const float intervaloConsulta = IntervaloConsultaProceso;

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
                        $"Consulta temporal de ataque fallida. Se reintentará: " +
                        $"HTTP {request.responseCode}: {request.error}",
                        this);

                    yield return new WaitForSecondsRealtime(
                        0.5f);

                    continue;
                }

                if (request.responseCode == 204 ||
                    string.IsNullOrWhiteSpace(
                        request.downloadHandler.text))
                {
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
                        "La API devolvió un resultado concurrente inválido para el ataque.");

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.procesoId != procesoId)
                {
                    MostrarError(
                        "Se recibió el resultado de un proceso distinto al ataque esperado.");

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.estado == "Cancelado")
                {
                    Debug.Log(
                        "Ataque cancelado.");

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.estado == "Fallido")
                {
                    MostrarError(
                        string.IsNullOrWhiteSpace(
                            resultado.errorTecnico)
                            ? "El worker de ataque finalizó con error."
                            : resultado.errorTecnico);

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.estado != "Completado")
                {
                    MostrarError(
                        $"Estado concurrente de ataque no reconocido: {resultado.estado}");

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (!resultado.exito)
                {
                    MostrarError(
                        string.IsNullOrWhiteSpace(
                            resultado.mensaje)
                            ? "El ataque fue rechazado por el Modelo."
                            : resultado.mensaje);

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                yield return ObtenerPartidaActiva(
                    string.IsNullOrWhiteSpace(
                        resultado.mensaje)
                        ? "Ataque realizado."
                        : resultado.mensaje,
                    "Ataque completado, pero no se pudo actualizar la vista. ",
                    false);

                yield break;
            }
        }

        private IEnumerator EnviarCuracion(
            CuracionDto curacion)
        {
            curacionesActivas++;
            CuracionEnCurso = curacionesActivas > 0;

            try
            {
                using var request =
                    new UnityWebRequest(
                        $"{urlBaseApi}/api/partida/curar-concurrente",
                        UnityWebRequest.kHttpVerbPOST);

                request.uploadHandler =
                    new UploadHandlerRaw(
                        Encoding.UTF8.GetBytes(
                            JsonUtility.ToJson(curacion)));

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
                        $"No se pudo iniciar la curación concurrente. HTTP {request.responseCode}: {request.error}");

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
                        "La API no devolvió un identificador válido para la curación concurrente.");

                    yield break;
                }

                yield return EsperarResultadoCuracion(
                    proceso.procesoId);
            }
            finally
            {
                curacionesActivas =
                    Mathf.Max(0, curacionesActivas - 1);

                CuracionEnCurso =
                    curacionesActivas > 0;
            }
        }

        private IEnumerator EsperarResultadoCuracion(
            string procesoId)
        {
            const float intervaloConsulta = IntervaloConsultaProceso;

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
                        $"Consulta temporal de curación fallida. Se reintentará: " +
                        $"HTTP {request.responseCode}: {request.error}",
                        this);

                    yield return new WaitForSecondsRealtime(
                        0.5f);

                    continue;
                }

                if (request.responseCode == 204 ||
                    string.IsNullOrWhiteSpace(
                        request.downloadHandler.text))
                {
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
                        "La API devolvió un resultado concurrente inválido para la curación.");

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.procesoId != procesoId)
                {
                    MostrarError(
                        "Se recibió el resultado de un proceso distinto a la curación esperada.");

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.estado == "Cancelado")
                {
                    Debug.Log(
                        "Curación cancelada.");

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.estado == "Fallido")
                {
                    MostrarError(
                        string.IsNullOrWhiteSpace(
                            resultado.errorTecnico)
                            ? "El worker de curación finalizó con error."
                            : resultado.errorTecnico);

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.estado != "Completado")
                {
                    MostrarError(
                        $"Estado concurrente de curación no reconocido: {resultado.estado}");

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (!resultado.exito)
                {
                    MostrarError(
                        string.IsNullOrWhiteSpace(
                            resultado.mensaje)
                            ? "La curación fue rechazada por el Modelo."
                            : resultado.mensaje);

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                yield return ObtenerPartidaActiva(
                    string.IsNullOrWhiteSpace(
                        resultado.mensaje)
                        ? "Curación realizada."
                        : resultado.mensaje,
                    "Curación completada, pero no se pudo actualizar la vista. ",
                    false);

                yield break;
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
            return DescribirCosto(
                ObtenerCostoUnidad(
                    tipoUnidad));
        }

        public string DescribirCostoUnidadCompacto(
            string tipoUnidad)
        {
            CostoEstadoDto costo =
                ObtenerCostoUnidad(
                    tipoUnidad);

            if (costo == null)
                return string.Empty;

            var partes =
                new System.Collections.Generic.List<string>();

            if (costo.oro > 0)
                partes.Add($"O{costo.oro}");

            if (costo.madera > 0)
                partes.Add($"M{costo.madera}");

            if (costo.comida > 0)
                partes.Add($"C{costo.comida}");

            return partes.Count == 0
                ? "Gratis"
                : string.Join(" ", partes);
        }

        private CostoEstadoDto ObtenerCostoUnidad(
            string tipoUnidad)
        {
            if (economiaActual == null ||
                string.IsNullOrWhiteSpace(tipoUnidad))
            {
                return null;
            }

            switch (tipoUnidad)
            {
                case "Aldeano":
                    return economiaActual.aldeano;
                case "Guerrero":
                    return economiaActual.guerrero;
                case "Lancero":
                    return economiaActual.lancero;
                case "Arquero":
                    return economiaActual.arquero;
                case "Monje":
                    return economiaActual.monje;
                default:
                    return null;
            }
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
            ApiDisponible = false;
            PartidaFinalizada = false;

            if (controladorSeleccion == null)
            {
                controladorSeleccion =
                    FindFirstObjectByType<ControladorSeleccion>();
            }

            controladorSeleccion?.BloquearInteraccion();
        }

        public void IniciarPartidaDesdeMenu()
        {
            if (iniciandoPartidaDesdeMenu)
                return;

            StartCoroutine(
                ComprobarConexion());
        }

        private IEnumerator ComprobarConexion()
        {
            iniciandoPartidaDesdeMenu = true;
            ApiDisponible = false;
            ultimoInicioPartidaExitoso = false;
            ultimoEstadoPartidaValido = false;

            string url =
                $"{urlBaseApi}/api/estado";

            using UnityWebRequest request =
                UnityWebRequest.Get(url);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                string mensaje =
                    $"No se pudo conectar con la API: {request.error}";

                Debug.LogError(
                    mensaje);

                iniciandoPartidaDesdeMenu = false;
                InicioPartidaFallido?.Invoke(
                    mensaje);

                yield break;
            }

            ApiDisponible = true;

            Debug.Log(
                $"API conectada correctamente: " +
                $"{request.downloadHandler.text}");

            yield return IniciarPartidaPrueba();

            if (!ultimoInicioPartidaExitoso)
            {
                iniciandoPartidaDesdeMenu = false;
                InicioPartidaFallido?.Invoke(
                    "La API respondió, pero no fue posible crear la partida.");

                yield break;
            }

            yield return ObtenerPartidaActiva(
                "",
                "",
                false);

            iniciandoPartidaDesdeMenu = false;

            if (!ultimoEstadoPartidaValido)
            {
                InicioPartidaFallido?.Invoke(
                    "La partida fue creada, pero no fue posible cargar su estado inicial.");

                yield break;
            }

            sesionVisualActiva = true;

            if (sincronizacionPeriodica == null)
            {
                sincronizacionPeriodica =
                    StartCoroutine(
                        SincronizarPartidaPeriodicamente());
            }

            PartidaIniciadaDesdeMenu?.Invoke();
        }

        private IEnumerator SincronizarPartidaPeriodicamente()
        {
            var espera =
                new WaitForSecondsRealtime(
                    IntervaloSincronizacionEstado);

            while (sesionVisualActiva &&
                   isActiveAndEnabled &&
                   !PartidaFinalizada)
            {
                yield return espera;

                if (!sesionVisualActiva ||
                    !isActiveAndEnabled ||
                    PartidaFinalizada)
                {
                    break;
                }

                yield return ObtenerPartidaActiva(
                    "",
                    "",
                    false,
                    true);
            }

            sincronizacionPeriodica = null;
        }

        private IEnumerator PausarSesionRemota()
        {
            using var request =
                new UnityWebRequest(
                    $"{urlBaseApi}/api/sesion/pausar",
                    UnityWebRequest.kHttpVerbPOST);

            request.downloadHandler =
                new DownloadHandlerBuffer();

            request.timeout = 3;

            yield return request.SendWebRequest();

            if (request.result !=
                UnityWebRequest.Result.Success)
            {
                Debug.LogWarning(
                    $"No se pudo notificar la pausa de sesión a la API: {request.error}",
                    this);
            }
        }

        private IEnumerator IniciarPartidaPrueba()
        {
            ultimoInicioPartidaExitoso = false;

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

            PartidaFinalizada = false;
            ApiDisponible = true;
            ultimoInicioPartidaExitoso = true;

            if (controladorSeleccion == null)
            {
                controladorSeleccion =
                    FindFirstObjectByType<ControladorSeleccion>();
            }

            controladorSeleccion?.DesbloquearInteraccion();
            vistaHud?.OcultarResultadoFinal();

            Debug.Log(
                $"Partida iniciada correctamente: " +
                $"{request.downloadHandler.text}");
        }

        private IEnumerator ObtenerPartidaActiva(
            string mensajeExito = "Partida recibida correctamente.",
            string contextoError = "",
            bool mostrarMensaje = true,
            bool silencioso = false)
        {
            ultimoEstadoPartidaValido = false;

            string url =
                $"{urlBaseApi}/api/partida";

            using UnityWebRequest request =
                UnityWebRequest.Get(url);

            request.timeout = 15;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                if (!silencioso)
                {
                    MostrarError(
                        contextoError +
                        MensajeError(
                            LeerResultado(request.downloadHandler.text),
                            $"No se pudo obtener la partida activa. HTTP {request.responseCode}: {request.error}"));
                }

                yield break;
            }

            if (!silencioso)
            {
                Debug.Log(
                    "Partida activa obtenida correctamente.");
            }

            if (vistaPartida == null)
            {
                if (!silencioso)
                {
                    MostrarError(
                        contextoError +
                        "VistaPartida no está configurada en ControladorAPI.");
                }

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
                if (!silencioso)
                {
                    MostrarError(
                        contextoError +
                        $"La respuesta de la partida no es JSON válido: {ex.Message}");
                }

                yield break;
            }

            if (estadoPartida?.mapa == null ||
                estadoPartida.mapa.ancho <= 0 ||
                estadoPartida.mapa.alto <= 0 ||
                estadoPartida.jugadorHumano == null ||
                estadoPartida.jugadorMaquina == null)
            {
                if (!silencioso)
                {
                    MostrarError(
                        contextoError +
                        "La API devolvió un estado de partida incompleto.");
                }

                yield break;
            }

            PartidaFinalizada =
                estadoPartida.estado ==
                "finalizada";

            if (PartidaFinalizada)
            {
                sesionVisualActiva = false;
            }

            vistaPartida.Sincronizar(estadoPartida);
            ultimoEstadoPartidaValido = true;

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

                    if (PartidaFinalizada)
                    {
                        if (controladorSeleccion == null)
                        {
                            controladorSeleccion =
                                FindFirstObjectByType<ControladorSeleccion>();
                        }

                        controladorSeleccion?.BloquearInteraccion();

                        vistaHud.MostrarResultadoFinal(
                            estadoPartida.ganador,
                            estadoPartida.ganadorNombre,
                            estadoPartida.motivoFinalizacion);
                    }
                    else if (mostrarMensaje)
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

                // Dos nodos por tipo alrededor de cada mitad del mapa.
                // Se dejan corredores y varias casillas adyacentes libres para
                // evitar que un recurso quede encerrado por el Centro Urbano
                // u otros recursos físicos.
                recursosHumano = new[]
                {
                    new RecursoInicialDto(
                        "Oro",
                        3,
                        1),

                    new RecursoInicialDto(
                        "Oro",
                        4,
                        3),

                    new RecursoInicialDto(
                        "Madera",
                        1,
                        4),

                    new RecursoInicialDto(
                        "Madera",
                        3,
                        5),

                    new RecursoInicialDto(
                        "Comida",
                        4,
                        1),

                    new RecursoInicialDto(
                        "Comida",
                        1,
                        5),

                    new RecursoInicialDto(
                        "Comida",
                        2,
                        7)
                },

                recursosMaquina = new[]
                {
                    new RecursoInicialDto(
                        "Oro",
                        6,
                        8),

                    new RecursoInicialDto(
                        "Oro",
                        5,
                        6),

                    new RecursoInicialDto(
                        "Madera",
                        8,
                        5),

                    new RecursoInicialDto(
                        "Madera",
                        6,
                        4),

                    new RecursoInicialDto(
                        "Comida",
                        5,
                        8),

                    new RecursoInicialDto(
                        "Comida",
                        8,
                        4),

                    new RecursoInicialDto(
                        "Comida",
                        7,
                        2)
                }
            };
        }
    }
}