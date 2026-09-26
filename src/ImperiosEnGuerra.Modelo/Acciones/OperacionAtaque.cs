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

            Jugador objetivoPropietario =
                partida.BuscarJugadorPorUnidad(
                    solicitud.ObjetivoId)
                ??
                partida.BuscarJugadorPorEdificio(
                    solicitud.ObjetivoId);

            if (objetivoPropietario == null)
            {
                return ResultadoAccion.Fallido(
                    "No existe la entidad objetivo indicada.");
            }

            if (!partida.SonEnemigos(
                    propietario,
                    objetivoPropietario))
            {
                return ResultadoAccion.Fallido(
                    "No se puede atacar una entidad aliada.");
            }

            if (!ReferenceEquals(
                    propietario.Mapa,
                    objetivoPropietario.Mapa))
            {
                return ResultadoAccion.Fallido(
                    "El atacante y el objetivo no comparten el mismo mapa lógico.");
            }

            Unidad objetivoUnidad =
                objetivoPropietario.Unidades
                    .FirstOrDefault(
                        u => u.Id == solicitud.ObjetivoId);

            Edificio objetivoEdificio =
                objetivoPropietario.Edificios
                    .FirstOrDefault(
                        e => e.Id == solicitud.ObjetivoId);

            if (objetivoUnidad == null &&
                objetivoEdificio == null)
            {
                return ResultadoAccion.Fallido(
                    "No existe la entidad enemiga objetivo indicada.");
            }

            Coordenada objetivo =
                objetivoUnidad?.Coordenada ??
                objetivoEdificio.Coordenada;

            int distancia =
                DistanciaCombate(
                    atacante.Coordenada,
                    objetivo);

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
                if (!objetivoPropietario.EliminarUnidad(objetivoUnidad))
                    return ResultadoAccion.Fallido("No se pudo retirar la unidad objetivo.");

                LiberarCasilla(objetivoPropietario.Mapa, objetivoUnidad.Coordenada);
            }
            else
            {
                if (!objetivoPropietario.EliminarEdificio(objetivoEdificio))
                    return ResultadoAccion.Fallido("No se pudo retirar el edificio objetivo.");

                LiberarCasilla(objetivoPropietario.Mapa, objetivoEdificio.Coordenada);
            }

            EvaluacionVictoria evaluacion =
                new EvaluadorVictoria()
                    .Evaluar(
                        partida,
                        objetivoPropietario);

            bool centroConquistaCreado =
                evaluacion.HayVictoria &&
                ReferenceEquals(
                    propietario,
                    partida.JugadorHumano) &&
                objetivoPropietario.Tipo ==
                    TipoJugador.Maquina &&
                IntentarCrearCentroConquista(
                    partida,
                    objetivo);

            string mensaje =
                $"Impacto de {atacante.GetType().Name}: daño {atacante.DanioAtaque}. " +
                $"{tipoObjetivo} enemigo destruido.";

            if (centroConquistaCreado)
            {
                mensaje +=
                    " Facción conquistada: se estableció un nuevo Centro Urbano para tu imperio.";
            }

            if (partida.Finalizada &&
                partida.Ganador != null)
            {
                mensaje +=
                    $" Ganador: {partida.Ganador.Nombre}.";
            }

            return ResultadoAccion.Exitoso(
                mensaje);
        }

        private static bool IntentarCrearCentroConquista(
            Partida partida,
            Coordenada posicion)
        {
            if (partida == null ||
                posicion == null)
            {
                return false;
            }

            Mapa mapa =
                partida.JugadorHumano.Mapa;

            if (!mapa.PuedeColocar(
                    posicion))
            {
                return false;
            }

            Casilla casilla =
                mapa.ObtenerCasilla(
                    posicion.X,
                    posicion.Y);

            if (casilla == null ||
                !casilla.Ocupar())
            {
                return false;
            }

            partida.JugadorHumano
                .AgregarEdificio(
                    new CentroUrbano(
                        new Coordenada(
                            posicion.X,
                            posicion.Y)));

            return true;
        }

        private static int DistanciaCombate(
            Coordenada origen,
            Coordenada objetivo)
        {
            int deltaX =
                Math.Abs(
                    origen.X -
                    objetivo.X);

            int deltaY =
                Math.Abs(
                    origen.Y -
                    objetivo.Y);

            // En combate una diagonal también cuenta como casilla adyacente.
            // El movimiento continúa siendo ortogonal y conserva su A* actual.
            return Math.Max(
                deltaX,
                deltaY);
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
            return unidad is Soldado;
        }
    }
}
