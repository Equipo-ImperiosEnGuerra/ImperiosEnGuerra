using System;
using System.Linq;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Combate
{
    /// <summary>
    /// Calcula por separado los dos hechos que la guía menciona para victoria.
    /// No decide la semántica de "y/o" hasta que el equipo la fije explícitamente.
    /// </summary>
    public sealed class EvaluadorVictoria
    {
        public EvaluacionVictoria Evaluar(
            Jugador jugador)
        {
            if (jugador == null)
                throw new ArgumentNullException(nameof(jugador));

            bool sinCentroUrbano =
                !jugador.Edificios
                    .OfType<CentroUrbano>()
                    .Any();

            bool sinUnidadesMilitares =
                !jugador.Unidades
                    .Any(EsUnidadMilitar);

            return new EvaluacionVictoria(
                sinCentroUrbano,
                sinUnidadesMilitares);
        }

        private static bool EsUnidadMilitar(
            Unidad unidad)
        {
            return unidad is Soldado ||
                   unidad is Monje;
        }
    }

    public sealed class EvaluacionVictoria
    {
        public bool SinCentroUrbano { get; }
        public bool SinUnidadesMilitares { get; }

        public EvaluacionVictoria(
            bool sinCentroUrbano,
            bool sinUnidadesMilitares)
        {
            SinCentroUrbano = sinCentroUrbano;
            SinUnidadesMilitares = sinUnidadesMilitares;
        }
    }
}
