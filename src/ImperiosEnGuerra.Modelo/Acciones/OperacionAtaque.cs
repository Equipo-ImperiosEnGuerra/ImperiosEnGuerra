using System.Linq;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Acciones
{
    /// <summary>
    /// Valida la intención base de ataque para cualquiera de los dos jugadores.
    /// No aplica daño mientras las estadísticas de combate sigan pendientes.
    /// </summary>
    public sealed class OperacionAtaque
    {
        public ResultadoAccion Ejecutar(
            Partida partida,
            SolicitudAtaque solicitud)
        {
            if (partida == null)
                return ResultadoAccion.Fallido(
                    "No hay una partida activa.");

            if (solicitud == null)
                return ResultadoAccion.Fallido(
                    "La solicitud de ataque es obligatoria.");

            Jugador propietario =
                partida.BuscarJugadorPorUnidad(
                    solicitud.AtacanteId);

            if (propietario == null)
            {
                return ResultadoAccion.Fallido(
                    "No existe la unidad atacante indicada.");
            }

            Unidad atacante =
                propietario.Unidades
                    .First(
                        unidad =>
                            unidad.Id ==
                            solicitud.AtacanteId);

            if (!atacante.Disponible)
            {
                return ResultadoAccion.Fallido(
                    "La unidad atacante no está disponible.");
            }

            if (!EsUnidadMilitar(
                    atacante))
            {
                return ResultadoAccion.Fallido(
                    "La unidad atacante no es una unidad militar permitida.");
            }

            if (propietario.Unidades.Any(
                    unidad =>
                        unidad.Id ==
                        solicitud.ObjetivoId))
            {
                return ResultadoAccion.Fallido(
                    "El objetivo pertenece al mismo jugador que el atacante.");
            }

            Jugador oponente =
                partida.ObtenerOponente(
                    propietario);

            Unidad objetivo =
                oponente?.Unidades
                    .FirstOrDefault(
                        unidad =>
                            unidad.Id ==
                            solicitud.ObjetivoId);

            if (objetivo == null)
            {
                return ResultadoAccion.Fallido(
                    "No existe la unidad enemiga objetivo indicada.");
            }

            return ResultadoAccion.Exitoso(
                "Ataque preparado correctamente. El daño queda pendiente hasta definir estadísticas de combate.");
        }

        private static bool EsUnidadMilitar(
            Unidad unidad)
        {
            return unidad is Soldado ||
                   unidad is Monje;
        }
    }
}
