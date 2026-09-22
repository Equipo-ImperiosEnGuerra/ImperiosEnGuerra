using System;
using System.Linq;
using ImperiosEnGuerra.Modelo.Combate;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Acciones
{
    /// <summary>
    /// Ejecuta un impacto destructivo sobre una entidad enemiga adyacente.
    /// No introduce puntos de vida, daño, armadura o alcance numéricos no definidos.
    /// </summary>
    public sealed class OperacionAtaque
    {
        public ResultadoAccion Ejecutar(
            Partida partida,
            SolicitudAtaque solicitud)
        {
            if (partida == null)
                return ResultadoAccion.Fallido("No hay una partida activa.");

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

            if (!EsUnidadMilitar(atacante))
                return ResultadoAccion.Fallido("La unidad atacante no es una unidad militar permitida.");

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

            if (distancia != 1)
                return ResultadoAccion.Fallido("El objetivo debe estar en una casilla ortogonal adyacente.");

            string tipoDestruido;

            if (objetivoUnidad != null)
            {
                if (!oponente.EliminarUnidad(objetivoUnidad))
                    return ResultadoAccion.Fallido("No se pudo retirar la unidad objetivo.");

                LiberarCasilla(oponente.Mapa, objetivoUnidad.Coordenada);
                tipoDestruido = $"Unidad {objetivoUnidad.GetType().Name}";
            }
            else
            {
                if (!oponente.EliminarEdificio(objetivoEdificio))
                    return ResultadoAccion.Fallido("No se pudo retirar el edificio objetivo.");

                LiberarCasilla(oponente.Mapa, objetivoEdificio.Coordenada);
                tipoDestruido = $"Edificio {objetivoEdificio.GetType().Name}";
            }

            EvaluacionVictoria evaluacion =
                new EvaluadorVictoria().Evaluar(oponente);

            return ResultadoAccion.Exitoso(
                $"Impacto - {tipoDestruido} enemigo destruido. " +
                $"SinCentroUrbano={evaluacion.SinCentroUrbano}; " +
                $"SinUnidadesMilitares={evaluacion.SinUnidadesMilitares}.");
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
