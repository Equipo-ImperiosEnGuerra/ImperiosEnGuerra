using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Api.Mapeadores;

public static class PartidaEstadoMapper
{
    public static EstadoPartidaResponse Convertir(Partida partida)
    {
        ArgumentNullException.ThrowIfNull(partida);

        Mapa mapa = partida.JugadorHumano.Mapa;

        return new EstadoPartidaResponse
        {
            Estado = partida.Finalizada
                ? "finalizada"
                : "activa",
            Ganador = partida.Ganador?.Tipo.ToString(),
            GanadorNombre = partida.Ganador?.Nombre,
            MotivoFinalizacion = partida.MotivoFinalizacion,
            Mapa = new MapaEstadoResponse
            {
                Ancho = mapa.Ancho,
                Alto = mapa.Alto,
                Recursos = mapa.Recursos.Select(recurso => new RecursoEstadoResponse
                {
                    Tipo = recurso.Tipo.ToString(),
                    Coordenada = ConvertirCoordenada(recurso.Coordenada),
                    CantidadRestante = recurso.CantidadRestante
                }).ToList()
            },
            JugadorHumano = ConvertirJugador(partida.JugadorHumano),
            JugadorMaquina = ConvertirJugador(partida.JugadorMaquina),
            Economia = ConvertirEconomia()
        };
    }

    private static EconomiaEstadoResponse ConvertirEconomia()
    {
        var configuracion =
            new ConfiguracionEconomia();

        configuracion.IntentarObtenerCostoEdificio(
            "CentroUrbano",
            out CostoRecursos centro);

        configuracion.IntentarObtenerCostoUnidad(
            "Aldeano",
            out CostoRecursos aldeano);

        configuracion.IntentarObtenerCostoUnidad(
            "Guerrero",
            out CostoRecursos guerrero);

        configuracion.IntentarObtenerCostoUnidad(
            "Lancero",
            out CostoRecursos lancero);

        configuracion.IntentarObtenerCostoUnidad(
            "Arquero",
            out CostoRecursos arquero);

        configuracion.IntentarObtenerCostoUnidad(
            "Monje",
            out CostoRecursos monje);

        return new EconomiaEstadoResponse
        {
            CentroUrbano = ConvertirCosto(centro),
            Aldeano = ConvertirCosto(aldeano),
            Guerrero = ConvertirCosto(guerrero),
            Lancero = ConvertirCosto(lancero),
            Arquero = ConvertirCosto(arquero),
            Monje = ConvertirCosto(monje)
        };
    }

    private static CostoEstadoResponse ConvertirCosto(
        CostoRecursos costo)
    {
        return new CostoEstadoResponse
        {
            Oro = costo.Oro,
            Madera = costo.Madera,
            Comida = costo.Comida
        };
    }

    private static JugadorEstadoResponse ConvertirJugador(Jugador jugador)
    {
        return new JugadorEstadoResponse
        {
            Nombre = jugador.Nombre,
            Tipo = jugador.Tipo.ToString(),
            Recursos = new RecursosJugadorEstadoResponse
            {
                Oro = jugador.Recursos.ObtenerCantidad(TipoRecurso.Oro),
                Madera = jugador.Recursos.ObtenerCantidad(TipoRecurso.Madera),
                Comida = jugador.Recursos.ObtenerCantidad(TipoRecurso.Comida)
            },
            Edificios = jugador.Edificios.Select(edificio => new EdificioEstadoResponse
            {
                Id = edificio.Id.ToString("D"),
                Tipo = edificio.GetType().Name,
                Coordenada = ConvertirCoordenada(edificio.Coordenada),
                ColaEntrenamiento = edificio is CentroUrbano centro
                    ? centro.ColaEntrenamiento.Select(p => new EntrenamientoEstadoResponse
                    {
                        Id = p.Id.ToString("D"),
                        TipoUnidad = p.TipoUnidad,
                        Progreso = p.Progreso
                    }).ToList()
                    : new List<EntrenamientoEstadoResponse>()
            }).ToList(),
            ObrasConstruccion = jugador.ObrasConstruccion.Select(obra => new ObraConstruccionEstadoResponse
            {
                Id = obra.Id.ToString("D"),
                Tipo = obra.TipoEdificio,
                Coordenada = ConvertirCoordenada(obra.Coordenada),
                Progreso = obra.Progreso
            }).ToList(),
            Unidades = jugador.Unidades.Select(unidad => new UnidadEstadoResponse
            {
                Id = unidad.Id.ToString("D"),
                Tipo = unidad.GetType().Name,
                Coordenada = unidad.Coordenada == null
                    ? null
                    : ConvertirCoordenada(unidad.Coordenada),
                Disponible = unidad.Disponible,
                Estado = unidad.Estado.ToString(),
                OrdenActiva = unidad.OrdenActiva?.ToString(),
                CapacidadCarga = unidad is Aldeano aldeano
                    ? aldeano.CapacidadCarga
                    : 0,
                CargaActual = unidad is Aldeano aldeanoCarga
                    ? aldeanoCarga.CargaActual
                    : 0,
                TipoCarga = unidad is Aldeano aldeanoTipo
                    ? aldeanoTipo.TipoCarga?.ToString()
                    : null
            }).ToList()
        };
    }

    private static CoordenadaEstadoResponse ConvertirCoordenada(Coordenada coordenada)
    {
        return new CoordenadaEstadoResponse
        {
            X = coordenada.X,
            Y = coordenada.Y
        };
    }
}
