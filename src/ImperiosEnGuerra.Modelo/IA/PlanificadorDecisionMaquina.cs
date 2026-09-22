using System;
using System.Collections.Generic;
using System.Linq;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.IA
{
    /// <summary>
    /// Política inicial de IA: asigna un Aldeano disponible al recurso físico
    /// no agotado más cercano. Solo decide; no modifica el Modelo.
    /// </summary>
    public sealed class PlanificadorDecisionMaquina
    {
        public DecisionMaquina Preparar(
            Partida partida,
            IReadOnlyCollection<Guid> unidadesExcluidas = null)
        {
            if (partida == null)
                return DecisionMaquina.SinAccion(
                    "No hay una partida activa.");

            var excluidas =
                unidadesExcluidas == null
                    ? new HashSet<Guid>()
                    : new HashSet<Guid>(unidadesExcluidas);

            Aldeano[] aldeanos =
                partida.JugadorMaquina.Unidades
                    .OfType<Aldeano>()
                    .Where(a =>
                        a.Disponible &&
                        !excluidas.Contains(a.Id))
                    .OrderBy(a => a.Coordenada.X)
                    .ThenBy(a => a.Coordenada.Y)
                    .ThenBy(a => a.Id)
                    .ToArray();

            if (aldeanos.Length == 0)
                return DecisionMaquina.SinAccion(
                    "No hay Aldeanos de la Máquina disponibles.");

            Recurso[] recursos =
                partida.JugadorMaquina.Mapa.Recursos
                    .Where(r => !r.Agotado)
                    .OrderBy(r => r.Coordenada.X)
                    .ThenBy(r => r.Coordenada.Y)
                    .ThenBy(r => r.Tipo)
                    .ToArray();

            if (recursos.Length == 0)
                return DecisionMaquina.SinAccion(
                    "No hay recursos físicos disponibles.");

            Aldeano mejorAldeano = null;
            Recurso mejorRecurso = null;
            int mejorDistancia = int.MaxValue;

            foreach (Aldeano aldeano in aldeanos)
            {
                foreach (Recurso recurso in recursos)
                {
                    int distancia =
                        Math.Abs(aldeano.Coordenada.X - recurso.Coordenada.X) +
                        Math.Abs(aldeano.Coordenada.Y - recurso.Coordenada.Y);

                    if (distancia >= mejorDistancia)
                        continue;

                    mejorDistancia = distancia;
                    mejorAldeano = aldeano;
                    mejorRecurso = recurso;
                }
            }

            return mejorAldeano == null || mejorRecurso == null
                ? DecisionMaquina.SinAccion(
                    "No fue posible preparar una decisión.")
                : DecisionMaquina.Recolectar(
                    mejorAldeano.Id,
                    mejorRecurso.Coordenada);
        }
    }
}
