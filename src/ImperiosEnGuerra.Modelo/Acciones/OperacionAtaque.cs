using System.Linq;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Acciones
{
    /// <summary>
    /// Valida la intención base de ataque sin aplicar daño mientras no existan estadísticas definidas.
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

            var atacanteMaquina = partida.JugadorMaquina.Unidades
                .FirstOrDefault(unidad => unidad.Id == solicitud.AtacanteId);

            if (atacanteMaquina != null)
                return ResultadoAccion.Fallido(
                    "La unidad atacante pertenece a la máquina y no puede controlarse.");

            var atacante = partida.JugadorHumano.Unidades
                .FirstOrDefault(unidad => unidad.Id == solicitud.AtacanteId);

            if (atacante == null)
                return ResultadoAccion.Fallido(
                    "No existe la unidad atacante humana indicada.");

            if (!atacante.Disponible)
                return ResultadoAccion.Fallido(
                    "La unidad atacante no está disponible.");

            if (!EsUnidadMilitar(atacante))
                return ResultadoAccion.Fallido(
                    "La unidad atacante no es una unidad militar permitida.");

            var objetivoPropio = partida.JugadorHumano.Unidades
                .FirstOrDefault(unidad => unidad.Id == solicitud.ObjetivoId);

            if (objetivoPropio != null)
                return ResultadoAccion.Fallido(
                    "El objetivo pertenece al jugador humano.");

            var objetivo = partida.JugadorMaquina.Unidades
                .FirstOrDefault(unidad => unidad.Id == solicitud.ObjetivoId);

            if (objetivo == null)
                return ResultadoAccion.Fallido(
                    "No existe la unidad enemiga objetivo indicada.");

            return ResultadoAccion.Exitoso(
                "Ataque preparado correctamente. El daño queda pendiente hasta definir estadísticas de combate.");
        }

        private static bool EsUnidadMilitar(Unidad unidad)
        {
            return unidad is Soldado || unidad is Monje;
        }
    }
}
