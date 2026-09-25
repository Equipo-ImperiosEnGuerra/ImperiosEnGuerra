using System;
using System.Collections.Generic;
using System.Linq;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Combate
{
    /// <summary>
    /// Detecta reacciones automáticas del bando humano sin ejecutar acciones.
    /// Las órdenes manuales tienen prioridad: solo considera unidades Idle.
    /// </summary>
    public sealed class PlanificadorReaccionAutomatica
    {
        public const int RadioDeteccionMilitarPredeterminado = 4;

        public IReadOnlyList<ReaccionAutomatica> Preparar(
            Partida partida,
            int radioDeteccionMilitar =
                RadioDeteccionMilitarPredeterminado)
        {
            if (partida == null ||
                partida.Finalizada)
            {
                return Array.Empty<ReaccionAutomatica>();
            }

            if (radioDeteccionMilitar <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(radioDeteccionMilitar));
            }

            var reacciones =
                new List<ReaccionAutomatica>();

            Jugador humano =
                partida.JugadorHumano;

            Jugador maquina =
                partida.JugadorMaquina;

            foreach (Unidad unidad in humano.Unidades)
            {
                if (!EstaLibre(
                        unidad))
                {
                    continue;
                }

                if (unidad is Monje monje)
                {
                    Unidad aliado =
                        BuscarAliadoHerido(
                            humano,
                            monje);

                    if (aliado != null)
                    {
                        reacciones.Add(
                            new ReaccionAutomatica(
                                TipoReaccionAutomatica.Curar,
                                monje.Id,
                                aliado.Id));
                    }

                    continue;
                }

                if (unidad is not Soldado)
                    continue;

                Guid objetivoId =
                    BuscarObjetivoEnemigo(
                        unidad,
                        maquina,
                        radioDeteccionMilitar);

                if (objetivoId != Guid.Empty)
                {
                    reacciones.Add(
                        new ReaccionAutomatica(
                            TipoReaccionAutomatica.Atacar,
                            unidad.Id,
                            objetivoId));
                }
            }

            return reacciones.AsReadOnly();
        }

        private static bool EstaLibre(
            Unidad unidad)
        {
            return unidad != null &&
                   !unidad.Destruida &&
                   unidad.Disponible &&
                   !unidad.OrdenActiva.HasValue;
        }

        private static Unidad BuscarAliadoHerido(
            Jugador propietario,
            Monje monje)
        {
            return propietario.Unidades
                .Where(
                    u =>
                        u != null &&
                        u.Id != monje.Id &&
                        !u.Destruida &&
                        u.VidaActual < u.VidaMaxima &&
                        Distancia(
                            monje.Coordenada,
                            u.Coordenada) <=
                            monje.AlcanceCuracion)
                .OrderBy(
                    u =>
                        Distancia(
                            monje.Coordenada,
                            u.Coordenada))
                .ThenBy(
                    u =>
                        (double)u.VidaActual /
                        u.VidaMaxima)
                .FirstOrDefault();
        }

        private static Guid BuscarObjetivoEnemigo(
            Unidad atacante,
            Jugador enemigo,
            int radioDeteccion)
        {
            var unidades =
                enemigo.Unidades
                    .Where(
                        u =>
                            u != null &&
                            !u.Destruida)
                    .Select(
                        u =>
                            new ObjetivoDetectado(
                                u.Id,
                                u.Coordenada,
                                0));

            var edificios =
                enemigo.Edificios
                    .Where(
                        e =>
                            e != null &&
                            !e.Destruido)
                    .Select(
                        e =>
                            new ObjetivoDetectado(
                                e.Id,
                                e.Coordenada,
                                1));

            ObjetivoDetectado objetivo =
                unidades
                    .Concat(
                        edificios)
                    .Where(
                        candidato =>
                            Distancia(
                                atacante.Coordenada,
                                candidato.Coordenada) <=
                                radioDeteccion)
                    .OrderBy(
                        candidato =>
                            Distancia(
                                atacante.Coordenada,
                                candidato.Coordenada))
                    .ThenBy(
                        candidato =>
                            candidato.Prioridad)
                    .FirstOrDefault();

            return objetivo?.Id ??
                   Guid.Empty;
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

        private sealed class ObjetivoDetectado
        {
            public Guid Id { get; }
            public Coordenada Coordenada { get; }
            public int Prioridad { get; }

            public ObjetivoDetectado(
                Guid id,
                Coordenada coordenada,
                int prioridad)
            {
                Id = id;
                Coordenada = coordenada;
                Prioridad = prioridad;
            }
        }
    }
}
