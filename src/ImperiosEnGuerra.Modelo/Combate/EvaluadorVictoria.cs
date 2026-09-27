using System;
using System.Linq;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Combate
{
    /// <summary>
    /// Regla terminal confirmada por el equipo: AND.
    /// Un jugador pierde únicamente cuando no conserva ningún Centro Urbano
    /// y tampoco conserva unidades militares.
    /// </summary>
    public sealed class EvaluadorVictoria
    {
        public EvaluacionVictoria Evaluar(
            Jugador jugador)
        {
            if (jugador == null)
                throw new ArgumentNullException(nameof(jugador));

            //La derrota exige cumplir las dos condiciones: sin Centro Urbano y sin militares.
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

        public EvaluacionVictoria Evaluar(
            Partida partida,
            Jugador jugadorAfectado)
        {
            if (partida == null)
                throw new ArgumentNullException(nameof(partida));

            if (jugadorAfectado == null)
                throw new ArgumentNullException(nameof(jugadorAfectado));

            EvaluacionVictoria evaluacion =
                Evaluar(
                    jugadorAfectado);

            if (!evaluacion.HayVictoria ||
                partida.Finalizada)
            {
                return evaluacion;
            }

            //Si cae el Humano, gana una facción de Máquina que todavía siga activa.
            if (ReferenceEquals(
                    jugadorAfectado,
                    partida.JugadorHumano))
            {
                Jugador ganador =
                    partida.JugadoresMaquina
                        .FirstOrDefault(
                            maquina =>
                                !Evaluar(
                                    maquina)
                                    .HayVictoria);

                if (ganador != null)
                {
                    partida.IntentarFinalizar(
                        ganador,
                        "Tu imperio se quedó sin Centros Urbanos y sin unidades militares.");
                }

                return evaluacion;
            }

            //El Humano solo gana cuando las tres facciones enemigas ya fueron eliminadas.
            bool todasLasIasEliminadas =
                partida.JugadoresMaquina
                    .All(
                        maquina =>
                            Evaluar(
                                maquina)
                                .HayVictoria);

            if (todasLasIasEliminadas)
            {
                partida.IntentarFinalizar(
                    partida.JugadorHumano,
                    "Las tres facciones enemigas se quedaron sin Centros Urbanos y sin unidades militares.");
            }

            return evaluacion;
        }

        private static bool EsUnidadMilitar(
            Unidad unidad)
        {
            return unidad is Soldado ||
                   unidad is Monje;
        }
    }

    /// <summary>
    /// Guarda por separado las dos condiciones usadas por la regla AND de victoria.
    /// </summary>
    public sealed class EvaluacionVictoria
    {
        public bool SinCentroUrbano { get; }
        public bool SinUnidadesMilitares { get; }
        public bool HayVictoria =>
            SinCentroUrbano &&
            SinUnidadesMilitares;

        public EvaluacionVictoria(
            bool sinCentroUrbano,
            bool sinUnidadesMilitares)
        {
            SinCentroUrbano = sinCentroUrbano;
            SinUnidadesMilitares = sinUnidadesMilitares;
        }
    }
}
