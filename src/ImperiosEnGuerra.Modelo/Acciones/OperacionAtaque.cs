using System;
using System.Linq;
using ImperiosEnGuerra.Modelo.Combate;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Acciones
{
    public sealed class OperacionAtaque
    {
        public ResultadoAccion Ejecutar(
            Partida partida,
            SolicitudAtaque solicitud)
        {
            if (partida == null)
                return ResultadoAccion.Fallido("No hay una partida activa.");

            if (partida.Finalizada)
                return ResultadoAccion.Fallido("La partida ya finalizó.");

            if (solicitud == null)
                return ResultadoAccion.Fallido("La solicitud de ataque es obligatoria.");

            Jugador propietario =
                partida.BuscarJugadorPorUnidad(
                    solicitud.AtacanteId);

            if (propietario == null)
                return ResultadoAccion.Fallido("No existe la unidad atacante indicada.");

            Unidad atacante =
                propietario.Unidades
                    .First(u => u.Id == solicitud.AtacanteId);

            if (!atacante.Disponible &&
                atacante.OrdenActiva != TipoAccionJuego.Atacar)
                return ResultadoAccion.Fallido("La unidad atacante no está disponible.");

            if (!EsUnidadMilitar(atacante) ||
                atacante.DanioAtaque <= 0 ||
                atacante.AlcanceAtaque <= 0)
            {
                return ResultadoAccion.Fallido(
                    "La unidad atacante no es una unidad militar ofensiva permitida.");
            }

            if (propietario.Unidades.Any(u => u.Id == solicitud.ObjetivoId) ||
                propietario.Edificios.Any(e => e.Id == solicitud.ObjetivoId))
                return ResultadoAccion.Fallido("El objetivo pertenece al mismo jugador que el atacante.");

            Jugador oponente =
                partida.ObtenerOponente(
                    propietario);

            if (oponente == null)
                return ResultadoAccion.Fallido("No existe un oponente válido.");

            if (!ReferenceEquals(propietario.Mapa, oponente.Mapa))
                return ResultadoAccion.Fallido("El atacante y el objetivo no comparten el mismo mapa lógico.");

            Unidad objetivoUnidad =
                oponente.Unidades
                    .FirstOrDefault(u => u.Id == solicitud.ObjetivoId);

            Edificio objetivoEdificio =
                oponente.Edificios
                    .FirstOrDefault(e => e.Id == solicitud.ObjetivoId);

            if (objetivoUnidad == null &&
                objetivoEdificio == null)
                return ResultadoAccion.Fallido("No existe la entidad enemiga objetivo indicada.");

            Coordenada objetivo =
                objetivoUnidad?.Coordenada ??
                objetivoEdificio.Coordenada;

            int distancia =
                Math.Abs(atacante.Coordenada.X - objetivo.X) +
                Math.Abs(atacante.Coordenada.Y - objetivo.Y);

            if (distancia <= 0 ||
                distancia > atacante.AlcanceAtaque)
            {
                return ResultadoAccion.Fallido(
                    $"El objetivo está fuera de alcance. Alcance de {atacante.GetType().Name}: {atacante.AlcanceAtaque} casilla(s).");
            }

            int vidaRestante;
            int vidaMaxima;
            string tipoObjetivo;

            if (objetivoUnidad != null)
            {
                vidaRestante =
                    objetivoUnidad.RecibirDanio(
                        atacante.DanioAtaque);

                vidaMaxima =
                    objetivoUnidad.VidaMaxima;

                tipoObjetivo =
                    $"Unidad {objetivoUnidad.GetType().Name}";
            }
            else
            {
                vidaRestante =
                    objetivoEdificio.RecibirDanio(
                        atacante.DanioAtaque);

                vidaMaxima =
                    objetivoEdificio.VidaMaxima;

                tipoObjetivo =
                    $"Edificio {objetivoEdificio.GetType().Name}";
            }

            if (vidaRestante > 0)
            {
                return ResultadoAccion.Exitoso(
                    $"Impacto de {atacante.GetType().Name}: daño {atacante.DanioAtaque}. " +
                    $"{tipoObjetivo} conserva {vidaRestante}/{vidaMaxima} de vida.");
            }

            if (objetivoUnidad != null)
            {
                if (!oponente.EliminarUnidad(objetivoUnidad))
                    return ResultadoAccion.Fallido("No se pudo retirar la unidad objetivo.");

                LiberarCasilla(oponente.Mapa, objetivoUnidad.Coordenada);
            }
            else
            {
                if (!oponente.EliminarEdificio(objetivoEdificio))
                    return ResultadoAccion.Fallido("No se pudo retirar el edificio objetivo.");

                LiberarCasilla(oponente.Mapa, objetivoEdificio.Coordenada);
            }

            EvaluacionVictoria evaluacion =
                new EvaluadorVictoria()
                    .Evaluar(
                        partida,
                        oponente);

            string mensaje =
                $"Impacto de {atacante.GetType().Name}: daño {atacante.DanioAtaque}. " +
                $"{tipoObjetivo} enemigo destruido.";

            if (partida.Finalizada &&
                partida.Ganador != null)
            {
                mensaje +=
                    $" Ganador: {partida.Ganador.Nombre}.";
            }

            return ResultadoAccion.Exitoso(
                mensaje);
        }

        private static void LiberarCasilla(
            Mapa mapa,
            Coordenada coordenada)
        {
            Casilla casilla =
                mapa?.ObtenerCasilla(
                    coordenada.X,
                    coordenada.Y);

            casilla?.Liberar();
        }

        private static bool EsUnidadMilitar(
            Unidad unidad)
        {
            return unidad is Soldado ||
                   unidad is Monje;
        }
    }
}
