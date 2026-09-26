using System;
using System.Linq;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Acciones
{
    /// <summary>
    /// Aplica una pulsación de curación del Monje sobre una unidad aliada.
    /// La operación solo modifica el Modelo y nunca toca UnityEngine.
    /// </summary>
    public sealed class OperacionCuracion
    {
        public ResultadoAccion Ejecutar(
            Partida partida,
            SolicitudCuracion solicitud)
        {
            if (partida == null)
                return ResultadoAccion.Fallido("No hay una partida activa.");

            if (partida.Finalizada)
                return ResultadoAccion.Fallido("La partida ya finalizó.");

            if (solicitud == null)
                return ResultadoAccion.Fallido("La solicitud de curación es obligatoria.");

            Jugador propietario =
                partida.BuscarJugadorPorUnidad(
                    solicitud.CuradorId);

            if (propietario == null)
                return ResultadoAccion.Fallido("No existe la unidad curadora indicada.");

            Monje curador =
                propietario.Unidades
                    .OfType<Monje>()
                    .FirstOrDefault(
                        u => u.Id == solicitud.CuradorId);

            if (curador == null)
                return ResultadoAccion.Fallido("Solo un Monje puede ejecutar la acción Curar.");

            if (!curador.Disponible &&
                curador.OrdenActiva != TipoAccionJuego.Curar)
            {
                return ResultadoAccion.Fallido(
                    "El Monje no está disponible.");
            }

            if (solicitud.CuradorId == solicitud.ObjetivoId)
                return ResultadoAccion.Fallido("El Monje no puede curarse a sí mismo.");

            Unidad objetivo =
                propietario.Unidades
                    .FirstOrDefault(
                        u => u.Id == solicitud.ObjetivoId);

            if (objetivo == null)
            {
                return ResultadoAccion.Fallido(
                    "La curación solo puede aplicarse a una unidad aliada.");
            }

            if (objetivo.Destruida)
                return ResultadoAccion.Fallido("No se puede curar una unidad destruida.");

            if (objetivo.VidaActual >= objetivo.VidaMaxima)
                return ResultadoAccion.Fallido("La unidad aliada ya tiene la vida completa.");

            int distancia =
                Distancia(
                    curador.Coordenada,
                    objetivo.Coordenada);

            if (distancia <= 0 ||
                distancia > curador.AlcanceCuracion)
            {
                return ResultadoAccion.Fallido(
                    $"La unidad aliada está fuera de alcance. Alcance de curación del Monje: {curador.AlcanceCuracion} casilla(s).");
            }

            int vidaAnterior =
                objetivo.VidaActual;

            int vidaRestante =
                objetivo.RecuperarVida(
                    curador.CantidadCuracion);

            int curado =
                vidaRestante - vidaAnterior;

            return ResultadoAccion.Exitoso(
                $"Curación de Monje: +{curado} vida. " +
                $"Unidad {objetivo.GetType().Name} queda con {vidaRestante}/{objetivo.VidaMaxima} de vida.");
        }

        private static int Distancia(
            Coordenada origen,
            Coordenada objetivo)
        {
            int deltaX =
                Math.Abs(
                    origen.X - objetivo.X);

            int deltaY =
                Math.Abs(
                    origen.Y - objetivo.Y);

            return Math.Max(
                deltaX,
                deltaY);
        }
    }
}
