using System.Linq;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Acciones
{
    /// <summary>Valida y aplica un movimiento inmediato del humano, sin trayectoria ni temporización.</summary>
    public sealed class OperacionMovimiento
    {
        public ResultadoAccion Ejecutar(Partida partida, SolicitudMovimiento solicitud)
        {
            if (partida == null)
                return ResultadoAccion.Fallido("No hay una partida activa.");
            if (solicitud == null)
                return ResultadoAccion.Fallido("La solicitud de movimiento es obligatoria.");

            if (partida.JugadorMaquina.Unidades.Any(u => u.Id == solicitud.UnidadId))
                return ResultadoAccion.Fallido("No se puede mover una unidad de la máquina.");

            Unidad unidad = partida.JugadorHumano.Unidades.FirstOrDefault(u => u.Id == solicitud.UnidadId);
            if (unidad == null)
                return ResultadoAccion.Fallido("No existe una unidad humana con ese ID.");
            if (!unidad.Disponible)
                return ResultadoAccion.Fallido("La unidad no está disponible.");

            Coordenada destino = solicitud.Destino;
            Mapa mapa = partida.JugadorHumano.Mapa;
            if (destino == null)
                return ResultadoAccion.Fallido("El destino es obligatorio.");
            if (!mapa.EstaDentroDeLimites(destino))
                return ResultadoAccion.Fallido("El destino está fuera del mapa.");

            Casilla casilla = mapa.ObtenerCasilla(destino.X, destino.Y);
            if (!casilla.EsTransitable)
                return ResultadoAccion.Fallido("La casilla destino no es transitable.");
            if (casilla.EstaOcupada)
                return ResultadoAccion.Fallido("La casilla destino está ocupada.");
            if (mapa.ObtenerRecursoEn(destino) != null)
                return ResultadoAccion.Fallido("La casilla destino contiene un recurso físico.");

            if (TieneEntidadEn(partida.JugadorHumano, unidad, destino)
                || (ReferenceEquals(mapa, partida.JugadorMaquina.Mapa)
                    && TieneEntidadEn(partida.JugadorMaquina, unidad, destino)))
                return ResultadoAccion.Fallido("La posición destino contiene una unidad o un edificio.");

            // AgregarUnidad no ocupa Casilla. Conservamos ese contrato: no liberamos
            // marcas sin identidad de ocupante que podrían corresponder a edificios.
            unidad.EstablecerDestino(destino);
            return ResultadoAccion.Exitoso("Movimiento realizado.");
        }

        private static bool TieneEntidadEn(Jugador jugador, Unidad unidad, Coordenada destino)
        {
            return jugador.Unidades.Any(u => !ReferenceEquals(u, unidad) && Coincide(u.Coordenada, destino))
                || jugador.Edificios.Any(e => Coincide(e.Coordenada, destino));
        }

        private static bool Coincide(Coordenada posicion, Coordenada destino)
        {
            return posicion != null && posicion.X == destino.X && posicion.Y == destino.Y;
        }
    }
}
