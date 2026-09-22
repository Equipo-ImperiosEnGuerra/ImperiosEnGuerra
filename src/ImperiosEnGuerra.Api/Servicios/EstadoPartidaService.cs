using System;
using System.IO;
using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Api.Mapeadores;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Unidades;
using ImperiosEnGuerra.Servicios;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Movimiento;
using ImperiosEnGuerra.Modelo.Recoleccion;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.IA;
using System.Linq; // para usar FirstOrDefault()

namespace ImperiosEnGuerra.Api.Servicios;

public sealed class EstadoPartidaService
{
    private readonly object sincronizacion = new();
    private readonly ServicioArchivos? servicioArchivos;
    private readonly ConfiguracionEconomia configuracionEconomia =
        new ConfiguracionEconomia();
    private Partida? partidaActiva;
    private bool finalizacionNotificada;

    public event Action? PartidaFinalizada;

    public EstadoPartidaService()
    {
    }

    public EstadoPartidaService(ServicioArchivos servicioArchivos)
    {
        this.servicioArchivos =
            servicioArchivos ?? throw new ArgumentNullException(nameof(servicioArchivos));
    }

    public ResultadoAccion MoverUnidad(MoverUnidadRequest? request)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
                return RegistrarResultado(
                    "MOVER",
                    ResultadoAccion.Fallido("No hay una partida activa."));

            if (partidaActiva.Finalizada)
                return RegistrarResultado(
                    "MOVER",
                    ResultadoAccion.Fallido("La partida ya finalizó."));

            if (request == null)
                return RegistrarResultado(
                    "MOVER",
                    ResultadoAccion.Fallido("La solicitud de movimiento es obligatoria."));

            if (!Guid.TryParse(request.UnidadId, out Guid unidadId))
                return RegistrarResultado(
                    "MOVER",
                    ResultadoAccion.Fallido("El ID de la unidad debe tener formato Guid válido."));

            if (request.Destino == null)
                return RegistrarResultado(
                    "MOVER",
                    ResultadoAccion.Fallido("El destino es obligatorio."));

            var solicitud = new SolicitudMovimiento(
                unidadId,
                PartidaRequestMapper.ConvertirCoordenada(request.Destino));

            return RegistrarResultado(
                "MOVER",
                new OperacionMovimiento().Ejecutar(partidaActiva, solicitud));
        }
    }

    public ResultadoPlanMovimiento PrepararMovimientoProgresivo(
        MoverUnidadRequest? request,
        bool permitirOrdenMovimientoActiva = false)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
                return ResultadoPlanMovimiento.Fallido(
                    "No hay una partida activa.");

            if (request == null)
                return ResultadoPlanMovimiento.Fallido(
                    "La solicitud de movimiento es obligatoria.");

            if (!Guid.TryParse(
                    request.UnidadId,
                    out Guid unidadId))
            {
                return ResultadoPlanMovimiento.Fallido(
                    "El ID de la unidad debe tener formato Guid válido.");
            }

            if (request.Destino == null)
                return ResultadoPlanMovimiento.Fallido(
                    "El destino es obligatorio.");

            var solicitud =
                new SolicitudMovimiento(
                    unidadId,
                    PartidaRequestMapper.ConvertirCoordenada(
                        request.Destino));

            return new PlanificadorMovimiento()
                .Preparar(
                    partidaActiva,
                    solicitud,
                    permitirOrdenMovimientoActiva);
        }
    }

    public bool IntentarIniciarOrdenUnidad(
        Guid unidadId,
        TipoAccionJuego tipo)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null ||
                partidaActiva.Finalizada)
                return false;

            Jugador? propietario =
                partidaActiva.BuscarJugadorPorUnidad(
                    unidadId);

            Unidad? unidad =
                propietario?.Unidades
                    .FirstOrDefault(
                        u => u.Id == unidadId);

            return unidad != null &&
                   unidad.IntentarIniciarOrden(tipo);
        }
    }

    public bool IntentarReemplazarOrdenUnidad(
        Guid unidadId,
        TipoAccionJuego tipo)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null ||
                partidaActiva.Finalizada)
                return false;

            Jugador? propietario =
                partidaActiva.BuscarJugadorPorUnidad(
                    unidadId);

            Unidad? unidad =
                propietario?.Unidades
                    .FirstOrDefault(
                        u => u.Id == unidadId);

            return unidad != null &&
                   unidad.IntentarReemplazarOrden(tipo);
        }
    }

    public void CompletarOrdenUnidad(
        Guid unidadId)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
                return;

            Jugador? propietario =
                partidaActiva.BuscarJugadorPorUnidad(
                    unidadId);

            Unidad? unidad =
                propietario?.Unidades
                    .FirstOrDefault(
                        u => u.Id == unidadId);

            unidad?.CompletarOrden();
        }
    }

    public ResultadoAccion AvanzarMovimiento(
        Guid unidadId,
        Coordenada siguiente)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
                return ResultadoAccion.Fallido(
                    "No hay una partida activa.");

            return new OperacionPasoMovimiento()
                .Ejecutar(
                    partidaActiva,
                    unidadId,
                    siguiente);
        }
    }

    public ResultadoAproximacionRecurso PrepararAproximacionRecurso(
        RecolectarRequest? request,
        bool permitirOrdenMovimientoActiva = false)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
            {
                return ResultadoAproximacionRecurso.Fallido(
                    "No hay una partida activa.");
            }

            if (request == null)
            {
                return ResultadoAproximacionRecurso.Fallido(
                    "La solicitud de recolección es obligatoria.");
            }

            if (!Guid.TryParse(
                    request.AldeanoId,
                    out Guid aldeanoId))
            {
                return ResultadoAproximacionRecurso.Fallido(
                    "El ID del Aldeano debe tener formato Guid válido.");
            }

            if (request.Objetivo == null)
            {
                return ResultadoAproximacionRecurso.Fallido(
                    "El objetivo de recolección es obligatorio.");
            }

            var solicitud =
                new SolicitudRecoleccion(
                    aldeanoId,
                    PartidaRequestMapper.ConvertirCoordenada(
                        request.Objetivo));

            return new PlanificadorAproximacionRecurso()
                .Preparar(
                    partidaActiva,
                    solicitud,
                    permitirOrdenMovimientoActiva);
        }
    }

    public ResultadoAproximacionDeposito PrepararAproximacionDeposito(
        Guid aldeanoId,
        bool permitirOrdenMovimientoActiva = false)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
            {
                return ResultadoAproximacionDeposito.Fallido(
                    "No hay una partida activa.");
            }

            return new PlanificadorAproximacionDeposito()
                .Preparar(
                    partidaActiva,
                    aldeanoId,
                    permitirOrdenMovimientoActiva);
        }
    }

    public ResultadoDepositoRecoleccion DepositarCarga(
        Guid aldeanoId,
        Coordenada centroUrbano)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
            {
                return ResultadoDepositoRecoleccion.Fallido(
                    "No hay una partida activa.");
            }

            return new OperacionDepositoRecoleccion()
                .Ejecutar(
                    partidaActiva,
                    aldeanoId,
                    centroUrbano);
        }
    }

    public bool RecursoExiste(
        Coordenada objetivo)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null ||
                objetivo == null)
            {
                return false;
            }

            return partidaActiva.JugadorHumano.Mapa
                .ObtenerRecursoEn(objetivo) != null;
        }
    }

    public bool RecursoExiste(
        Guid unidadId,
        Coordenada objetivo)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null ||
                objetivo == null)
            {
                return false;
            }

            Jugador? propietario =
                partidaActiva.BuscarJugadorPorUnidad(
                    unidadId);

            return propietario?.Mapa
                .ObtenerRecursoEn(objetivo) != null;
        }
    }

    public bool RecursoDisponible(
        Coordenada objetivo)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null ||
                objetivo == null)
            {
                return false;
            }

            Recurso? recurso =
                partidaActiva.JugadorHumano.Mapa
                    .ObtenerRecursoEn(objetivo);

            return recurso != null &&
                   !recurso.Agotado;
        }
    }

    public bool RecursoDisponible(
        Guid unidadId,
        Coordenada objetivo)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null ||
                objetivo == null)
            {
                return false;
            }

            Jugador? propietario =
                partidaActiva.BuscarJugadorPorUnidad(
                    unidadId);

            Recurso? recurso =
                propietario?.Mapa
                    .ObtenerRecursoEn(objetivo);

            return recurso != null &&
                   !recurso.Agotado;
        }
    }

    public DecisionMaquina PrepararDecisionMaquina(
        IReadOnlyCollection<Guid>? unidadesExcluidas = null,
        IReadOnlyCollection<Coordenada>? centrosExcluidos = null)
    {
        lock (sincronizacion)
        {
            return new PlanificadorDecisionMaquina()
                .Preparar(
                    partidaActiva,
                    unidadesExcluidas,
                    centrosExcluidos);
        }
    }

    public ResultadoPasoRecoleccion RecolectarPaso(
        Guid aldeanoId,
        Coordenada objetivo,
        int tasa)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
            {
                return ResultadoPasoRecoleccion.Fallido(
                    "No hay una partida activa.");
            }

            return new OperacionPasoRecoleccion()
                .Ejecutar(
                    partidaActiva,
                    aldeanoId,
                    objetivo,
                    tasa);
        }
    }

    public ResultadoAccion IniciarRecoleccion(RecolectarRequest? request)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
                return RegistrarResultado(
                    "RECOLECTAR",
                    ResultadoAccion.Fallido("No hay una partida activa."));

            if (partidaActiva.Finalizada)
                return RegistrarResultado(
                    "RECOLECTAR",
                    ResultadoAccion.Fallido("La partida ya finalizó."));

            if (request == null)
                return RegistrarResultado(
                    "RECOLECTAR",
                    ResultadoAccion.Fallido("La solicitud de recolección es obligatoria."));

            if (!Guid.TryParse(request.AldeanoId, out Guid aldeanoId))
                return RegistrarResultado(
                    "RECOLECTAR",
                    ResultadoAccion.Fallido("El ID del Aldeano debe tener formato Guid válido."));

            if (request.Objetivo == null)
                return RegistrarResultado(
                    "RECOLECTAR",
                    ResultadoAccion.Fallido("El objetivo de recolección es obligatorio."));

            var solicitud = new SolicitudRecoleccion(
                aldeanoId,
                PartidaRequestMapper.ConvertirCoordenada(request.Objetivo));

            return RegistrarResultado(
                "RECOLECTAR",
                new OperacionRecoleccion().Ejecutar(partidaActiva, solicitud));
        }
    }

    public ResultadoAccion ReservarCostoConstruccion(
        Guid aldeanoId,
        string tipoEdificio,
        out CostoRecursos costo)
    {
        lock (sincronizacion)
        {
            costo = null;

            if (partidaActiva == null)
            {
                return ResultadoAccion.Fallido(
                    "No hay una partida activa.");
            }

            if (partidaActiva.Finalizada)
            {
                return ResultadoAccion.Fallido(
                    "La partida ya finalizó.");
            }

            if (!configuracionEconomia
                .IntentarObtenerCostoEdificio(
                    tipoEdificio,
                    out costo))
            {
                return ResultadoAccion.Fallido(
                    "El tipo de edificio no tiene un costo configurado.");
            }

            Jugador? propietario =
                partidaActiva.BuscarJugadorPorUnidad(
                    aldeanoId);

            if (propietario == null)
            {
                return ResultadoAccion.Fallido(
                    "No existe una unidad propietaria para reservar el costo.");
            }

            if (!propietario.Recursos
                .IntentarGastar(costo))
            {
                return ResultadoAccion.Fallido(
                    $"Recursos insuficientes. Costo: {costo}.");
            }

            return ResultadoAccion.Exitoso(
                $"Costo reservado: {costo}.");
        }
    }

    public ResultadoAccion ReservarCostoEntrenamiento(
        string tipoUnidad,
        out CostoRecursos costo)
    {
        lock (sincronizacion)
        {
            costo = null;

            if (partidaActiva == null)
            {
                return ResultadoAccion.Fallido(
                    "No hay una partida activa.");
            }

            if (partidaActiva.Finalizada)
            {
                return ResultadoAccion.Fallido(
                    "La partida ya finalizó.");
            }

            if (!configuracionEconomia
                .IntentarObtenerCostoUnidad(
                    tipoUnidad,
                    out costo))
            {
                return ResultadoAccion.Fallido(
                    "El tipo de unidad no tiene un costo configurado.");
            }

            if (!partidaActiva.JugadorHumano.Recursos
                .IntentarGastar(costo))
            {
                return ResultadoAccion.Fallido(
                    $"Recursos insuficientes. Costo: {costo}.");
            }

            return ResultadoAccion.Exitoso(
                $"Costo reservado: {costo}.");
        }
    }

    public ResultadoAccion ReservarCostoEntrenamiento(
        EntrenarRequest? request,
        out CostoRecursos costo,
        out TipoJugador propietarioTipo)
    {
        lock (sincronizacion)
        {
            costo = null;
            propietarioTipo = TipoJugador.Humano;

            if (partidaActiva == null)
            {
                return ResultadoAccion.Fallido(
                    "No hay una partida activa.");
            }

            if (partidaActiva.Finalizada)
            {
                return ResultadoAccion.Fallido(
                    "La partida ya finalizó.");
            }

            if (request == null)
            {
                return ResultadoAccion.Fallido(
                    "La solicitud de entrenamiento es obligatoria.");
            }

            if (request.EdificioOrigen == null)
            {
                return ResultadoAccion.Fallido(
                    "El edificio de origen es obligatorio.");
            }

            if (!configuracionEconomia
                .IntentarObtenerCostoUnidad(
                    request.TipoUnidad,
                    out costo))
            {
                return ResultadoAccion.Fallido(
                    "El tipo de unidad no tiene un costo configurado.");
            }

            Coordenada origen =
                PartidaRequestMapper.ConvertirCoordenada(
                    request.EdificioOrigen);

            Jugador? propietario =
                BuscarPropietarioCentro(
                    origen,
                    null,
                    out _);

            if (propietario == null)
            {
                return ResultadoAccion.Fallido(
                    "No existe un Centro Urbano propietario único en la posición indicada.");
            }

            if (!propietario.Recursos
                .IntentarGastar(costo))
            {
                return ResultadoAccion.Fallido(
                    $"Recursos insuficientes. Costo: {costo}.");
            }

            propietarioTipo =
                propietario.Tipo;

            return ResultadoAccion.Exitoso(
                $"Costo reservado: {costo}.");
        }
    }

    public void ReembolsarCosto(
        TipoJugador propietarioTipo,
        CostoRecursos costo)
    {
        if (costo == null)
            return;

        lock (sincronizacion)
        {
            partidaActiva?
                .ObtenerJugador(
                    propietarioTipo)?
                .Recursos
                .Reintegrar(
                    costo);
        }
    }

    public void ReembolsarCosto(
        CostoRecursos costo)
    {
        if (costo == null)
            return;

        lock (sincronizacion)
        {
            partidaActiva?.JugadorHumano
                .Recursos.Reintegrar(costo);
        }
    }

    public void ReembolsarCosto(
        Guid unidadId,
        CostoRecursos costo)
    {
        if (costo == null)
            return;

        lock (sincronizacion)
        {
            Jugador? propietario =
                partidaActiva?.BuscarJugadorPorUnidad(
                    unidadId);

            propietario?.Recursos
                .Reintegrar(costo);
        }
    }

    public ResultadoAccion IniciarObra(
        ConstruirRequest? request,
        out Guid obraId)
    {
        lock (sincronizacion)
        {
            obraId = Guid.Empty;

            if (partidaActiva == null)
            {
                return ResultadoAccion.Fallido(
                    "No hay una partida activa.");
            }

            if (request == null)
            {
                return ResultadoAccion.Fallido(
                    "La solicitud de construcción es obligatoria.");
            }

            if (!Guid.TryParse(
                    request.AldeanoId,
                    out Guid aldeanoId))
            {
                return ResultadoAccion.Fallido(
                    "El ID del Aldeano debe tener formato Guid válido.");
            }

            Jugador? propietario =
                partidaActiva.BuscarJugadorPorUnidad(
                    aldeanoId);

            Aldeano? aldeano =
                propietario?.Unidades
                    .OfType<Aldeano>()
                    .FirstOrDefault(
                        u => u.Id == aldeanoId);

            if (propietario == null ||
                aldeano == null)
            {
                return ResultadoAccion.Fallido(
                    "No existe un Aldeano con ese ID.");
            }

            if (!aldeano.Disponible)
            {
                return ResultadoAccion.Fallido(
                    "El Aldeano no está disponible.");
            }

            if (request.Destino == null)
            {
                return ResultadoAccion.Fallido(
                    "La posición de construcción es obligatoria.");
            }

            if (!string.Equals(
                    request.TipoEdificio,
                    nameof(CentroUrbano),
                    StringComparison.OrdinalIgnoreCase))
            {
                return ResultadoAccion.Fallido(
                    "El tipo de edificio indicado no está permitido.");
            }

            Coordenada destino =
                PartidaRequestMapper.ConvertirCoordenada(
                    request.Destino);

            Mapa mapa =
                propietario.Mapa;

            if (!mapa.EstaDentroDeLimites(destino))
            {
                return ResultadoAccion.Fallido(
                    "La posición está fuera del mapa.");
            }

            if (!mapa.PuedeColocar(destino))
            {
                return ResultadoAccion.Fallido(
                    "La posición indicada no está disponible.");
            }

            Casilla? casilla =
                mapa.ObtenerCasilla(
                    destino.X,
                    destino.Y);

            if (casilla == null ||
                !casilla.Ocupar())
            {
                return ResultadoAccion.Fallido(
                    "No se pudo reservar la casilla de construcción.");
            }

            var obra =
                new ObraConstruccion(
                    aldeanoId,
                    nameof(CentroUrbano),
                    destino);

            propietario
                .AgregarObraConstruccion(
                    obra);

            obraId = obra.Id;

            return ResultadoAccion.Exitoso(
                "Obra reservada correctamente.");
        }
    }

    public ResultadoAproximacionConstruccion
        PrepararAproximacionConstruccion(
            Guid aldeanoId,
            Guid obraId,
            bool permitirOrdenMovimientoActiva = false)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
            {
                return ResultadoAproximacionConstruccion.Fallido(
                    "No hay una partida activa.");
            }

            Jugador? propietario =
                partidaActiva.BuscarJugadorPorUnidad(
                    aldeanoId);

            ObraConstruccion? obra =
                propietario?.ObrasConstruccion
                    .FirstOrDefault(
                        o => o.Id == obraId);

            if (propietario == null ||
                obra == null)
            {
                return ResultadoAproximacionConstruccion.Fallido(
                    "No existe la obra indicada.");
            }

            return new PlanificadorAproximacionConstruccion()
                .Preparar(
                    partidaActiva,
                    aldeanoId,
                    obra.Coordenada,
                    permitirOrdenMovimientoActiva);
        }
    }

    public ResultadoProgresoConstruccion AvanzarObra(
        Guid obraId,
        int incremento)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
            {
                return ResultadoProgresoConstruccion.Fallido(
                    "No hay una partida activa.");
            }

            Jugador? propietario =
                BuscarJugadorPorObra(
                    obraId,
                    out ObraConstruccion? obra);

            if (propietario == null ||
                obra == null)
            {
                return ResultadoProgresoConstruccion.Fallido(
                    "No existe la obra indicada.");
            }

            int progreso =
                obra.Avanzar(
                    incremento);

            bool terminada =
                obra.Terminada;

            if (terminada)
            {
                propietario
                    .EliminarObraConstruccion(
                        obra);

                propietario
                    .AgregarEdificio(
                        new CentroUrbano(
                            obra.Coordenada));
            }

            return ResultadoProgresoConstruccion.Exitoso(
                progreso,
                terminada);
        }
    }

    public bool CancelarObra(
        Guid obraId)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
                return false;

            Jugador? propietario =
                BuscarJugadorPorObra(
                    obraId,
                    out ObraConstruccion? obra);

            if (propietario == null ||
                obra == null)
                return false;

            propietario
                .EliminarObraConstruccion(
                    obra);

            propietario.Mapa
                .ObtenerCasilla(
                    obra.Coordenada.X,
                    obra.Coordenada.Y)
                ?.Liberar();

            return true;
        }
    }

    public ResultadoAccion Construir(ConstruirRequest? request)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
                return RegistrarResultado(
                    "CONSTRUIR",
                    ResultadoAccion.Fallido("No hay una partida activa."));

            if (partidaActiva.Finalizada)
                return RegistrarResultado(
                    "CONSTRUIR",
                    ResultadoAccion.Fallido("La partida ya finalizó."));

            if (request == null)
                return RegistrarResultado(
                    "CONSTRUIR",
                    ResultadoAccion.Fallido("La solicitud de construcción es obligatoria."));

            if (!Guid.TryParse(request.AldeanoId, out Guid aldeanoId))
                return RegistrarResultado(
                    "CONSTRUIR",
                    ResultadoAccion.Fallido("El ID del Aldeano debe tener formato Guid válido."));

            if (request.Destino == null)
                return RegistrarResultado(
                    "CONSTRUIR",
                    ResultadoAccion.Fallido("La posición de construcción es obligatoria."));

            Jugador? propietario =
                partidaActiva.BuscarJugadorPorUnidad(
                    aldeanoId);

            if (propietario == null)
            {
                return RegistrarResultado(
                    "CONSTRUIR",
                    ResultadoAccion.Fallido(
                        "No existe una unidad con ese ID."));
            }

            var solicitud = new SolicitudConstruccion(
                aldeanoId,
                request.TipoEdificio ?? string.Empty,
                PartidaRequestMapper.ConvertirCoordenada(request.Destino));

            if (!configuracionEconomia.IntentarObtenerCostoEdificio(
                    request.TipoEdificio,
                    out CostoRecursos costo))
            {
                return RegistrarResultado(
                    "CONSTRUIR",
                    ResultadoAccion.Fallido(
                        "El tipo de edificio no tiene un costo configurado."));
            }

            if (!propietario.Recursos
                .IntentarGastar(costo))
            {
                return RegistrarResultado(
                    "CONSTRUIR",
                    ResultadoAccion.Fallido(
                        $"Recursos insuficientes. Costo: {costo}."));
            }

            ResultadoAccion resultado =
                new OperacionConstruccion()
                    .Ejecutar(
                        partidaActiva,
                        solicitud);

            if (!resultado.Exito)
            {
                propietario.Recursos
                    .Reintegrar(costo);
            }

            return RegistrarResultado(
                "CONSTRUIR",
                resultado);
        }
    }

    private Jugador? BuscarJugadorPorObra(
        Guid obraId,
        out ObraConstruccion? obra)
    {
        obra =
            partidaActiva?.JugadorHumano.ObrasConstruccion
                .FirstOrDefault(
                    o => o.Id == obraId);

        if (obra != null)
            return partidaActiva!.JugadorHumano;

        obra =
            partidaActiva?.JugadorMaquina.ObrasConstruccion
                .FirstOrDefault(
                    o => o.Id == obraId);

        return obra != null
            ? partidaActiva!.JugadorMaquina
            : null;
    }

    public ResultadoAccion EncolarEntrenamiento(
        EntrenarRequest? request,
        out Guid entrenamientoId,
        out Coordenada centroUrbano)
    {
        return EncolarEntrenamientoInterno(
            request,
            null,
            out entrenamientoId,
            out centroUrbano);
    }

    public ResultadoAccion EncolarEntrenamiento(
        EntrenarRequest? request,
        TipoJugador propietarioTipo,
        out Guid entrenamientoId,
        out Coordenada centroUrbano)
    {
        return EncolarEntrenamientoInterno(
            request,
            propietarioTipo,
            out entrenamientoId,
            out centroUrbano);
    }

    private ResultadoAccion EncolarEntrenamientoInterno(
        EntrenarRequest? request,
        TipoJugador? propietarioTipo,
        out Guid entrenamientoId,
        out Coordenada centroUrbano)
    {
        lock (sincronizacion)
        {
            entrenamientoId = Guid.Empty;
            centroUrbano = null;

            if (partidaActiva == null)
            {
                return ResultadoAccion.Fallido(
                    "No hay una partida activa.");
            }

            if (request == null)
            {
                return ResultadoAccion.Fallido(
                    "La solicitud de entrenamiento es obligatoria.");
            }

            if (request.EdificioOrigen == null)
            {
                return ResultadoAccion.Fallido(
                    "El edificio de origen es obligatorio.");
            }

            if (!configuracionEconomia
                .IntentarObtenerCostoUnidad(
                    request.TipoUnidad,
                    out _))
            {
                return ResultadoAccion.Fallido(
                    "El tipo de unidad indicado no está permitido.");
            }

            Coordenada origen =
                PartidaRequestMapper.ConvertirCoordenada(
                    request.EdificioOrigen);

            Jugador? propietario =
                BuscarPropietarioCentro(
                    origen,
                    propietarioTipo,
                    out CentroUrbano? centro);

            if (propietario == null ||
                centro == null)
            {
                return ResultadoAccion.Fallido(
                    "No existe un Centro Urbano del propietario indicado en la posición.");
            }

            Coordenada reunion =
                request.Destino == null
                    ? null
                    : PartidaRequestMapper.ConvertirCoordenada(
                        request.Destino);

            EntrenamientoPendiente pendiente =
                centro.EncolarEntrenamiento(
                    request.TipoUnidad ?? string.Empty,
                    reunion);

            entrenamientoId =
                pendiente.Id;

            centroUrbano =
                centro.Coordenada;

            return ResultadoAccion.Exitoso(
                "Entrenamiento agregado a la cola.");
        }
    }

    public bool EsTurnoEntrenamiento(
        Coordenada centroUrbano,
        Guid entrenamientoId)
    {
        lock (sincronizacion)
        {
            BuscarPropietarioCentro(
                centroUrbano,
                null,
                out CentroUrbano? centro);

            return centro != null &&
                   centro.EsPrimero(
                       entrenamientoId);
        }
    }

    public bool EsTurnoEntrenamiento(
        Coordenada centroUrbano,
        Guid entrenamientoId,
        TipoJugador propietarioTipo)
    {
        lock (sincronizacion)
        {
            BuscarPropietarioCentro(
                centroUrbano,
                propietarioTipo,
                out CentroUrbano? centro);

            return centro != null &&
                   centro.EsPrimero(
                       entrenamientoId);
        }
    }

    public ResultadoProgresoEntrenamiento AvanzarEntrenamiento(
        Coordenada centroUrbano,
        Guid entrenamientoId,
        int incremento)
    {
        lock (sincronizacion)
        {
            BuscarPropietarioCentro(
                centroUrbano,
                null,
                out CentroUrbano? centro);

            return centro == null
                ? ResultadoProgresoEntrenamiento.Fallido(
                    "No existe el Centro Urbano indicado.")
                : centro.AvanzarEntrenamiento(
                    entrenamientoId,
                    incremento);
        }
    }

    public ResultadoProgresoEntrenamiento AvanzarEntrenamiento(
        Coordenada centroUrbano,
        Guid entrenamientoId,
        int incremento,
        TipoJugador propietarioTipo)
    {
        lock (sincronizacion)
        {
            BuscarPropietarioCentro(
                centroUrbano,
                propietarioTipo,
                out CentroUrbano? centro);

            return centro == null
                ? ResultadoProgresoEntrenamiento.Fallido(
                    "No existe el Centro Urbano indicado.")
                : centro.AvanzarEntrenamiento(
                    entrenamientoId,
                    incremento);
        }
    }

    public ResultadoSpawnEntrenamiento CompletarEntrenamientoConSpawn(
        Coordenada centroUrbano,
        Guid entrenamientoId,
        string tipoUnidad)
    {
        lock (sincronizacion)
        {
            Jugador? propietario =
                BuscarPropietarioCentro(
                    centroUrbano,
                    null,
                    out CentroUrbano? centro);

            return CompletarEntrenamientoConSpawnInterno(
                propietario,
                centro,
                entrenamientoId,
                tipoUnidad);
        }
    }

    public ResultadoSpawnEntrenamiento CompletarEntrenamientoConSpawn(
        Coordenada centroUrbano,
        Guid entrenamientoId,
        string tipoUnidad,
        TipoJugador propietarioTipo)
    {
        lock (sincronizacion)
        {
            Jugador? propietario =
                BuscarPropietarioCentro(
                    centroUrbano,
                    propietarioTipo,
                    out CentroUrbano? centro);

            return CompletarEntrenamientoConSpawnInterno(
                propietario,
                centro,
                entrenamientoId,
                tipoUnidad);
        }
    }

    private ResultadoSpawnEntrenamiento CompletarEntrenamientoConSpawnInterno(
        Jugador? propietario,
        CentroUrbano? centro,
        Guid entrenamientoId,
        string tipoUnidad)
    {
        if (partidaActiva == null)
        {
            return ResultadoSpawnEntrenamiento.Fallido(
                "No hay una partida activa.");
        }

        if (propietario == null ||
            centro == null ||
            !centro.EsPrimero(
                entrenamientoId))
        {
            return ResultadoSpawnEntrenamiento.Fallido(
                "La orden no está al frente de la cola.");
        }

        EntrenamientoPendiente? pendiente =
            centro.ColaEntrenamiento
                .FirstOrDefault();

        if (pendiente == null ||
            pendiente.Id != entrenamientoId ||
            pendiente.Progreso < 100)
        {
            return ResultadoSpawnEntrenamiento.Fallido(
                "El entrenamiento todavía no está completo.");
        }

        Coordenada spawn =
            new BuscadorCasillaSpawn()
                .Buscar(
                    partidaActiva,
                    propietario.Tipo,
                    centro.Coordenada);

        if (spawn == null)
        {
            return ResultadoSpawnEntrenamiento.Fallido(
                "No existe una casilla libre cercana para crear la unidad.");
        }

        Unidad unidad =
            FabricaUnidades.Crear(
                tipoUnidad,
                spawn);

        if (unidad == null)
        {
            return ResultadoSpawnEntrenamiento.Fallido(
                "El tipo de unidad indicado no está permitido.");
        }

        Casilla? casilla =
            propietario.Mapa
                .ObtenerCasilla(
                    spawn.X,
                    spawn.Y);

        if (casilla == null ||
            !casilla.Ocupar())
        {
            return ResultadoSpawnEntrenamiento.Fallido(
                "La casilla de aparición dejó de estar disponible.");
        }

        propietario.AgregarUnidad(
            unidad);

        if (!centro.CompletarEntrenamiento(
                entrenamientoId))
        {
            propietario.EliminarUnidad(
                unidad);

            casilla.Liberar();

            return ResultadoSpawnEntrenamiento.Fallido(
                "No se pudo retirar la orden completada de la cola.");
        }

        return ResultadoSpawnEntrenamiento.Exitoso(
            unidad.Id,
            spawn,
            tipoUnidad);
    }

    public bool CancelarEntrenamientoCola(
        Coordenada centroUrbano,
        Guid entrenamientoId)
    {
        lock (sincronizacion)
        {
            BuscarPropietarioCentro(
                centroUrbano,
                null,
                out CentroUrbano? centro);

            return centro != null &&
                   centro.CancelarEntrenamiento(
                       entrenamientoId);
        }
    }

    public bool CancelarEntrenamientoCola(
        Coordenada centroUrbano,
        Guid entrenamientoId,
        TipoJugador propietarioTipo)
    {
        lock (sincronizacion)
        {
            BuscarPropietarioCentro(
                centroUrbano,
                propietarioTipo,
                out CentroUrbano? centro);

            return centro != null &&
                   centro.CancelarEntrenamiento(
                       entrenamientoId);
        }
    }

    private Jugador? BuscarPropietarioCentro(
        Coordenada coordenada,
        TipoJugador? propietarioTipo,
        out CentroUrbano? centro)
    {
        centro = null;

        if (partidaActiva == null ||
            coordenada == null)
        {
            return null;
        }

        Jugador? propietario =
            propietarioTipo.HasValue
                ? partidaActiva.ObtenerJugador(
                    propietarioTipo.Value)
                : partidaActiva.BuscarJugadorPorEdificio(
                    coordenada);

        if (propietario == null)
            return null;

        centro =
            propietario.Edificios
                .OfType<CentroUrbano>()
                .FirstOrDefault(
                    e =>
                        e.Coordenada.X == coordenada.X &&
                        e.Coordenada.Y == coordenada.Y);

        return centro == null
            ? null
            : propietario;
    }

    public ResultadoAccion Entrenar(
        EntrenarRequest? request)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
                return RegistrarResultado(
                    "ENTRENAR",
                    ResultadoAccion.Fallido("No hay una partida activa."));

            if (partidaActiva.Finalizada)
                return RegistrarResultado(
                    "ENTRENAR",
                    ResultadoAccion.Fallido("La partida ya finalizó."));

            if (request == null)
                return RegistrarResultado(
                    "ENTRENAR",
                    ResultadoAccion.Fallido("La solicitud de entrenamiento es obligatoria."));

            if (request.EdificioOrigen == null)
                return RegistrarResultado(
                    "ENTRENAR",
                    ResultadoAccion.Fallido("El edificio de origen es obligatorio."));

            if (request.Destino == null)
                return RegistrarResultado(
                    "ENTRENAR",
                    ResultadoAccion.Fallido("La posición de aparición es obligatoria."));

            Coordenada origen =
                PartidaRequestMapper.ConvertirCoordenada(
                    request.EdificioOrigen);

            Jugador? propietario =
                BuscarPropietarioCentro(
                    origen,
                    null,
                    out _);

            if (propietario == null)
            {
                return RegistrarResultado(
                    "ENTRENAR",
                    ResultadoAccion.Fallido(
                        "No existe un Centro Urbano propietario único en la posición indicada."));
            }

            var solicitud =
                new SolicitudEntrenamiento(
                    origen,
                    request.TipoUnidad ?? string.Empty,
                    PartidaRequestMapper.ConvertirCoordenada(
                        request.Destino));

            if (!configuracionEconomia
                .IntentarObtenerCostoUnidad(
                    request.TipoUnidad,
                    out CostoRecursos costo))
            {
                return RegistrarResultado(
                    "ENTRENAR",
                    ResultadoAccion.Fallido(
                        "El tipo de unidad no tiene un costo configurado."));
            }

            if (!propietario.Recursos
                .IntentarGastar(
                    costo))
            {
                return RegistrarResultado(
                    "ENTRENAR",
                    ResultadoAccion.Fallido(
                        $"Recursos insuficientes. Costo: {costo}."));
            }

            ResultadoAccion resultado =
                new OperacionEntrenamiento()
                    .Ejecutar(
                        partidaActiva,
                        solicitud,
                        propietario.Tipo);

            if (!resultado.Exito)
            {
                propietario.Recursos
                    .Reintegrar(
                        costo);
            }

            return RegistrarResultado(
                "ENTRENAR",
                resultado);
        }
    }

    public ResultadoAccion Atacar(AtacarRequest? request)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
                return RegistrarResultado(
                    "ATACAR",
                    ResultadoAccion.Fallido("No hay una partida activa."));

            if (partidaActiva.Finalizada)
                return RegistrarResultado(
                    "ATACAR",
                    ResultadoAccion.Fallido("La partida ya finalizó."));

            if (request == null)
                return RegistrarResultado(
                    "ATACAR",
                    ResultadoAccion.Fallido("La solicitud de ataque es obligatoria."));

            if (!Guid.TryParse(request.AtacanteId, out Guid atacanteId))
                return RegistrarResultado(
                    "ATACAR",
                    ResultadoAccion.Fallido("El ID del atacante debe tener formato Guid válido."));

            if (!Guid.TryParse(request.ObjetivoId, out Guid objetivoId))
                return RegistrarResultado(
                    "ATACAR",
                    ResultadoAccion.Fallido("El ID del objetivo debe tener formato Guid válido."));

            var solicitud = new SolicitudAtaque(
                atacanteId,
                objetivoId);

            ResultadoAccion resultado =
                new OperacionAtaque()
                    .Ejecutar(
                        partidaActiva,
                        solicitud);

            ResultadoAccion registrado =
                RegistrarResultado(
                    "ATACAR",
                    resultado);

            if (partidaActiva.Finalizada)
                RegistrarFinalizacionSeguro();

            return registrado;
        }
    }

    public EstadoPartidaResponse? ObtenerEstado()
    {
        lock (sincronizacion)
        {
            return partidaActiva == null
                ? null
                : PartidaEstadoMapper.Convertir(partidaActiva);
        }
    }

    public void EstablecerPartida(Partida partida)
    {
        ArgumentNullException.ThrowIfNull(partida);

        lock (sincronizacion)
        {
            partidaActiva = partida;
            finalizacionNotificada = false;
            RegistrarEventoSeguro("PARTIDA|EXITO|Partida establecida.");
        }
    }

    public Partida? ObtenerPartida()
    {
        lock (sincronizacion)
        {
            return partidaActiva;
        }
    }
    public Unidad? ObtenerUnidad(Guid id)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
                return null;

            Jugador? propietario =
                partidaActiva.BuscarJugadorPorUnidad(
                    id);

            return propietario?.Unidades
                .FirstOrDefault(u => u.Id == id);
        }
    }

    public CentroUrbano? ObtenerCentroUrbano(Coordenada coordenada)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null || coordenada == null)
                return null;

            return partidaActiva.JugadorHumano.Edificios
                .OfType<CentroUrbano>()
                .FirstOrDefault(e =>
                    e.Coordenada.X == coordenada.X &&
                    e.Coordenada.Y == coordenada.Y);
        }
    }
    public bool HayPartidaActiva()
    {
        lock (sincronizacion)
        {
            return partidaActiva != null;
        }
    }

    public bool EstaFinalizada()
    {
        lock (sincronizacion)
        {
            return partidaActiva?.Finalizada ?? false;
        }
    }

    private void RegistrarFinalizacionSeguro()
    {
        if (partidaActiva == null ||
            !partidaActiva.Finalizada ||
            finalizacionNotificada)
        {
            return;
        }

        finalizacionNotificada = true;

        if (servicioArchivos != null)
        {
            try
            {
                servicioArchivos.GuardarResultadoPartidaFinalizada(
                    partidaActiva);
            }
            catch (IOException ex)
            {
                Console.Error.WriteLine(
                    $"No se pudo escribir resultado_final.txt: {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.Error.WriteLine(
                    $"No se pudo escribir resultado_final.txt: {ex.Message}");
            }
        }

        RegistrarEventoSeguro(
            $"PARTIDA|FINALIZADA|Ganador={partidaActiva.Ganador?.Nombre}; Motivo={partidaActiva.MotivoFinalizacion}");

        PartidaFinalizada?.Invoke();
    }

    private ResultadoAccion RegistrarResultado(
        string accion,
        ResultadoAccion resultado)
    {
        string estado = resultado.Exito ? "EXITO" : "RECHAZADO";
        RegistrarEventoSeguro($"{accion}|{estado}|{resultado.Mensaje}");
        return resultado;
    }

    private void RegistrarEventoSeguro(string contenido)
    {
        if (servicioArchivos == null)
            return;

        try
        {
            servicioArchivos.RegistrarEvento(contenido);
        }
        catch (IOException ex)
        {
            Console.Error.WriteLine(
                $"No se pudo escribir log_partida.txt: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.Error.WriteLine(
                $"No se pudo escribir log_partida.txt: {ex.Message}");
        }
    }
}

