using System;
using System.Collections.Generic;
using System.Linq;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.IA
{
    /// <summary>
    /// Política determinista y sencilla de la Máquina:
    /// crecer a tres Aldeanos, intentar una expansión, crear una primera
    /// unidad militar y, si ninguna prioridad aplica, continuar recolectando.
    /// Solo decide; nunca modifica directamente el Modelo.
    /// </summary>
    public sealed class PlanificadorDecisionMaquina
    {
        private readonly ConfiguracionEconomia economia =
            new ConfiguracionEconomia();

        public DecisionMaquina Preparar(
            Partida partida,
            IReadOnlyCollection<Guid> unidadesExcluidas = null,
            IReadOnlyCollection<Coordenada> centrosExcluidos = null)
        {
            if (partida == null)
            {
                return DecisionMaquina.SinAccion(
                    "No hay una partida activa.");
            }

            Jugador maquina =
                partida.JugadorMaquina;

            var excluidas =
                unidadesExcluidas == null
                    ? new HashSet<Guid>()
                    : new HashSet<Guid>(
                        unidadesExcluidas);

            Aldeano[] aldeanos =
                maquina.Unidades
                    .OfType<Aldeano>()
                    .OrderBy(
                        a => a.Coordenada.X)
                    .ThenBy(
                        a => a.Coordenada.Y)
                    .ThenBy(
                        a => a.Id)
                    .ToArray();

            Aldeano[] disponibles =
                aldeanos
                    .Where(
                        a =>
                            a.Disponible &&
                            !excluidas.Contains(
                                a.Id))
                    .ToArray();

            CentroUrbano[] centros =
                maquina.Edificios
                    .OfType<CentroUrbano>()
                    .OrderBy(
                        c => c.Coordenada.X)
                    .ThenBy(
                        c => c.Coordenada.Y)
                    .ToArray();

            CentroUrbano centroDisponible =
                centros.FirstOrDefault(
                    c =>
                        !c.EstaEntrenando &&
                        !EstaCentroExcluido(
                            c.Coordenada,
                            centrosExcluidos));

            if (aldeanos.Length < 3 &&
                centroDisponible != null &&
                PuedePagarUnidad(
                    maquina,
                    nameof(Aldeano)))
            {
                return DecisionMaquina.Entrenar(
                    centroDisponible.Coordenada,
                    nameof(Aldeano));
            }

            if (centros.Length < 2 &&
                disponibles.Length > 0 &&
                PuedePagarEdificio(
                    maquina,
                    nameof(CentroUrbano)))
            {
                Coordenada destino =
                    BuscarCasillaExpansion(
                        partida,
                        maquina,
                        centros);

                if (destino != null)
                {
                    Aldeano constructor =
                        disponibles
                            .OrderBy(
                                a => Distancia(
                                    a.Coordenada,
                                    destino))
                            .ThenBy(
                                a => a.Id)
                            .First();

                    return DecisionMaquina.Construir(
                        constructor.Id,
                        nameof(CentroUrbano),
                        destino);
                }
            }

            bool tieneMilitar =
                maquina.Unidades.Any(
                    u =>
                        u is Soldado ||
                        u is Monje);

            if (!tieneMilitar &&
                centroDisponible != null &&
                PuedePagarUnidad(
                    maquina,
                    nameof(Guerrero)))
            {
                return DecisionMaquina.Entrenar(
                    centroDisponible.Coordenada,
                    nameof(Guerrero));
            }

            DecisionMaquina combate =
                PrepararCombate(
                    partida,
                    maquina,
                    excluidas);

            if (combate.Tipo != TipoDecisionMaquina.Ninguna)
                return combate;

            if (disponibles.Length == 0)
            {
                return DecisionMaquina.SinAccion(
                    "No hay Aldeanos de la Máquina disponibles.");
            }

            Recurso[] recursos =
                maquina.Mapa.Recursos
                    .Where(
                        r => !r.Agotado)
                    .OrderBy(
                        r => r.Coordenada.X)
                    .ThenBy(
                        r => r.Coordenada.Y)
                    .ThenBy(
                        r => r.Tipo)
                    .ToArray();

            if (recursos.Length == 0)
            {
                return DecisionMaquina.SinAccion(
                    "No hay recursos físicos disponibles.");
            }

            Aldeano mejorAldeano = null;
            Recurso mejorRecurso = null;
            int mejorDistancia = int.MaxValue;

            foreach (Aldeano aldeano in disponibles)
            {
                foreach (Recurso recurso in recursos)
                {
                    int distancia =
                        Distancia(
                            aldeano.Coordenada,
                            recurso.Coordenada);

                    if (distancia >= mejorDistancia)
                        continue;

                    mejorDistancia =
                        distancia;

                    mejorAldeano =
                        aldeano;

                    mejorRecurso =
                        recurso;
                }
            }

            return mejorAldeano == null ||
                   mejorRecurso == null
                ? DecisionMaquina.SinAccion(
                    "No fue posible preparar una decisión.")
                : DecisionMaquina.Recolectar(
                    mejorAldeano.Id,
                    mejorRecurso.Coordenada);
        }

        private static DecisionMaquina PrepararCombate(
            Partida partida,
            Jugador maquina,
            HashSet<Guid> excluidas)
        {
            if (!ReferenceEquals(
                    maquina.Mapa,
                    partida.JugadorHumano.Mapa))
            {
                return DecisionMaquina.SinAccion(
                    "Los jugadores no comparten el mismo mapa lógico.");
            }

            Unidad[] militares =
                maquina.Unidades
                    .Where(
                        u =>
                            (u is Soldado || u is Monje) &&
                            u.Disponible &&
                            !excluidas.Contains(u.Id))
                    .OrderBy(u => u.Coordenada.X)
                    .ThenBy(u => u.Coordenada.Y)
                    .ThenBy(u => u.Id)
                    .ToArray();

            Unidad[] enemigos =
                partida.JugadorHumano.Unidades
                    .OrderBy(u => u.Coordenada.X)
                    .ThenBy(u => u.Coordenada.Y)
                    .ThenBy(u => u.Id)
                    .ToArray();

            if (militares.Length == 0 ||
                enemigos.Length == 0)
            {
                return DecisionMaquina.SinAccion(
                    "No hay combate disponible.");
            }

            Unidad atacante = null;
            Unidad objetivo = null;
            int mejorDistancia = int.MaxValue;

            foreach (Unidad militar in militares)
            {
                foreach (Unidad enemigo in enemigos)
                {
                    int distancia =
                        Distancia(
                            militar.Coordenada,
                            enemigo.Coordenada);

                    if (distancia >= mejorDistancia)
                        continue;

                    mejorDistancia = distancia;
                    atacante = militar;
                    objetivo = enemigo;
                }
            }

            if (atacante == null ||
                objetivo == null)
            {
                return DecisionMaquina.SinAccion(
                    "No se pudo seleccionar un objetivo militar.");
            }

            if (mejorDistancia <= 1)
            {
                return DecisionMaquina.Atacar(
                    atacante.Id,
                    objetivo.Id);
            }

            Coordenada aproximacion =
                BuscarCasillaAproximacion(
                    partida,
                    atacante,
                    objetivo.Coordenada);

            return aproximacion == null
                ? DecisionMaquina.SinAccion(
                    "No existe una casilla de aproximación disponible.")
                : DecisionMaquina.Mover(
                    atacante.Id,
                    aproximacion,
                    objetivo.Id);
        }

        private static Coordenada BuscarCasillaAproximacion(
            Partida partida,
            Unidad atacante,
            Coordenada objetivo)
        {
            Mapa mapa =
                partida.JugadorMaquina.Mapa;

            Coordenada[] candidatas =
            {
                new Coordenada(objetivo.X - 1, objetivo.Y),
                new Coordenada(objetivo.X + 1, objetivo.Y),
                new Coordenada(objetivo.X, objetivo.Y - 1),
                new Coordenada(objetivo.X, objetivo.Y + 1)
            };

            return candidatas
                .Where(
                    c =>
                        mapa.EstaDentroDeLimites(c) &&
                        mapa.PuedeColocar(c) &&
                        !HayEntidadEn(
                            partida.JugadorHumano,
                            mapa,
                            c) &&
                        !HayEntidadEn(
                            partida.JugadorMaquina,
                            mapa,
                            c))
                .OrderBy(
                    c => Distancia(
                        atacante.Coordenada,
                        c))
                .ThenBy(c => c.X)
                .ThenBy(c => c.Y)
                .FirstOrDefault();
        }

        private bool PuedePagarUnidad(
            Jugador jugador,
            string tipoUnidad)
        {
            return economia.IntentarObtenerCostoUnidad(
                       tipoUnidad,
                       out CostoRecursos costo)
                   &&
                   PuedePagar(
                       jugador,
                       costo);
        }

        private bool PuedePagarEdificio(
            Jugador jugador,
            string tipoEdificio)
        {
            return economia.IntentarObtenerCostoEdificio(
                       tipoEdificio,
                       out CostoRecursos costo)
                   &&
                   PuedePagar(
                       jugador,
                       costo);
        }

        private static bool PuedePagar(
            Jugador jugador,
            CostoRecursos costo)
        {
            return jugador.Recursos.PuedePagar(
                       TipoRecurso.Oro,
                       costo.Oro)
                   &&
                   jugador.Recursos.PuedePagar(
                       TipoRecurso.Madera,
                       costo.Madera)
                   &&
                   jugador.Recursos.PuedePagar(
                       TipoRecurso.Comida,
                       costo.Comida);
        }

        private static Coordenada BuscarCasillaExpansion(
            Partida partida,
            Jugador maquina,
            IReadOnlyList<CentroUrbano> centros)
        {
            if (centros.Count == 0)
                return null;

            Mapa mapa =
                maquina.Mapa;

            Coordenada origen =
                centros[0].Coordenada;

            for (int distancia = 2;
                 distancia <= mapa.Ancho + mapa.Alto;
                 distancia++)
            {
                for (int dx = -distancia;
                     dx <= distancia;
                     dx++)
                {
                    int dy =
                        distancia -
                        Math.Abs(dx);

                    foreach (int signo in
                        dy == 0
                            ? new[] { 1 }
                            : new[] { 1, -1 })
                    {
                        Coordenada candidata =
                            new Coordenada(
                                origen.X + dx,
                                origen.Y + dy * signo);

                        if (PuedeConstruirEn(
                                partida,
                                maquina,
                                candidata))
                        {
                            return candidata;
                        }
                    }
                }
            }

            return null;
        }

        private static bool PuedeConstruirEn(
            Partida partida,
            Jugador maquina,
            Coordenada candidata)
        {
            Mapa mapa =
                maquina.Mapa;

            Casilla casilla =
                mapa.ObtenerCasilla(
                    candidata.X,
                    candidata.Y);

            if (casilla == null ||
                !casilla.EsTransitable ||
                !mapa.PuedeColocar(
                    candidata))
            {
                return false;
            }

            bool entidad =
                HayEntidadEn(
                    partida.JugadorHumano,
                    mapa,
                    candidata)
                ||
                HayEntidadEn(
                    partida.JugadorMaquina,
                    mapa,
                    candidata);

            if (entidad)
                return false;

            (int X, int Y)[] direcciones =
            {
                (1, 0),
                (-1, 0),
                (0, 1),
                (0, -1)
            };

            return direcciones.Any(
                d =>
                {
                    Coordenada vecina =
                        new Coordenada(
                            candidata.X + d.X,
                            candidata.Y + d.Y);

                    Casilla adyacente =
                        mapa.ObtenerCasilla(
                            vecina.X,
                            vecina.Y);

                    return adyacente != null &&
                           adyacente.EsTransitable &&
                           mapa.PuedeColocar(
                               vecina) &&
                           !HayEntidadEn(
                               partida.JugadorHumano,
                               mapa,
                               vecina) &&
                           !HayEntidadEn(
                               partida.JugadorMaquina,
                               mapa,
                               vecina);
                });
        }

        private static bool HayEntidadEn(
            Jugador jugador,
            Mapa mapa,
            Coordenada coordenada)
        {
            if (!ReferenceEquals(
                    jugador.Mapa,
                    mapa))
            {
                return false;
            }

            return jugador.Unidades.Any(
                       u => Coincide(
                           u.Coordenada,
                           coordenada))
                   ||
                   jugador.Edificios.Any(
                       e => Coincide(
                           e.Coordenada,
                           coordenada))
                   ||
                   jugador.ObrasConstruccion.Any(
                       o => Coincide(
                           o.Coordenada,
                           coordenada));
        }

        private static bool EstaCentroExcluido(
            Coordenada centro,
            IReadOnlyCollection<Coordenada> excluidos)
        {
            return excluidos != null &&
                   excluidos.Any(
                       e => Coincide(
                           e,
                           centro));
        }

        private static int Distancia(
            Coordenada a,
            Coordenada b)
        {
            return Math.Abs(
                       a.X -
                       b.X)
                   +
                   Math.Abs(
                       a.Y -
                       b.Y);
        }

        private static bool Coincide(
            Coordenada a,
            Coordenada b)
        {
            return a != null &&
                   b != null &&
                   a.X == b.X &&
                   a.Y == b.Y;
        }
    }
}
