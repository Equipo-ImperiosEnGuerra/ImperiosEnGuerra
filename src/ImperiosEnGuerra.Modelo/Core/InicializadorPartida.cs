using System;
using System.Collections.Generic;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Core
{
    /// <summary>
    /// Valida y configura los mapas y participantes que forman el estado inicial de una partida.
    /// </summary>
    public class InicializadorPartida
    {
        private readonly ConfiguracionInicioPartida configuracionInicio;

        public InicializadorPartida()
            : this(new ConfiguracionInicioPartida())
        {
        }

        public InicializadorPartida(
            ConfiguracionInicioPartida configuracionInicio)
        {
            this.configuracionInicio =
                configuracionInicio
                ?? throw new ArgumentNullException(nameof(configuracionInicio));
        }

        /// <summary>
        /// Crea ambos participantes con Centro Urbano, nodos físicos, saldo inicial
        /// y Aldeanos iniciales colocados de forma determinista en casillas libres.
        /// </summary>
        public Partida Crear(
            string nombreHumano,
            Mapa mapaHumano,
            Coordenada centroHumano,
            IReadOnlyList<Recurso> recursosHumano,
            string nombreMaquina,
            Mapa mapaMaquina,
            Coordenada centroMaquina,
            IReadOnlyList<Recurso> recursosMaquina)
        {
            var posicionesPorMapa =
                new Dictionary<Mapa, HashSet<(int, int)>>();

            ValidarMapa(
                mapaHumano,
                centroHumano,
                recursosHumano,
                posicionesPorMapa);

            ValidarMapa(
                mapaMaquina,
                centroMaquina,
                recursosMaquina,
                posicionesPorMapa);

            IReadOnlyList<Coordenada> aldeanosHumano =
                PlanificarAldeanosIniciales(
                    mapaHumano,
                    centroHumano,
                    posicionesPorMapa);

            IReadOnlyList<Coordenada> aldeanosMaquina =
                PlanificarAldeanosIniciales(
                    mapaMaquina,
                    centroMaquina,
                    posicionesPorMapa);

            Jugador jugadorHumano =
                new Jugador(
                    nombreHumano,
                    TipoJugador.Humano,
                    mapaHumano,
                    new RecursosJugador());

            Jugador jugadorMaquina =
                new Jugador(
                    nombreMaquina,
                    TipoJugador.Maquina,
                    mapaMaquina,
                    new RecursosJugador());

            ConfigurarMapa(
                jugadorHumano,
                centroHumano,
                recursosHumano);

            ConfigurarMapa(
                jugadorMaquina,
                centroMaquina,
                recursosMaquina);

            ConfigurarInicioJugador(
                jugadorHumano,
                aldeanosHumano);

            ConfigurarInicioJugador(
                jugadorMaquina,
                aldeanosMaquina);

            return new Partida(
                jugadorHumano,
                jugadorMaquina);
        }

        /// <summary>
        /// Crea una partida compartiendo un único mapa físico entre un humano
        /// y varias IAs. Los recursos existen una sola vez en el mapa, por lo
        /// que todas las facciones compiten realmente por los mismos nodos.
        /// </summary>
        public Partida CrearCuatroJugadores(
            string nombreHumano,
            Mapa mapa,
            Coordenada centroHumano,
            IReadOnlyList<Recurso> recursosCompartidos,
            IReadOnlyList<(string Nombre, Coordenada Centro)> maquinas)
        {
            if (mapa == null)
                throw new ArgumentNullException(nameof(mapa));

            if (centroHumano == null)
                throw new ArgumentNullException(nameof(centroHumano));

            if (recursosCompartidos == null)
                throw new ArgumentNullException(nameof(recursosCompartidos));

            if (maquinas == null ||
                maquinas.Count != 3)
            {
                throw new ArgumentException(
                    "La partida de cuatro jugadores requiere exactamente tres IAs.",
                    nameof(maquinas));
            }

            var posicionesPorMapa =
                new Dictionary<Mapa, HashSet<(int, int)>>
                {
                    [mapa] =
                        new HashSet<(int, int)>()
                };

            HashSet<(int, int)> reservadas =
                posicionesPorMapa[mapa];

            ReservarCentroCompartido(
                mapa,
                centroHumano,
                reservadas);

            foreach (var maquina in maquinas)
            {
                if (maquina.Centro == null)
                {
                    throw new ArgumentException(
                        "Cada IA requiere una posición de Centro Urbano.",
                        nameof(maquinas));
                }

                ReservarCentroCompartido(
                    mapa,
                    maquina.Centro,
                    reservadas);
            }

            ValidarRecursosCompartidos(
                mapa,
                recursosCompartidos,
                reservadas);

            IReadOnlyList<Coordenada> aldeanosHumano =
                PlanificarAldeanosIniciales(
                    mapa,
                    centroHumano,
                    posicionesPorMapa);

            var aldeanosMaquina =
                new List<IReadOnlyList<Coordenada>>();

            foreach (var maquina in maquinas)
            {
                aldeanosMaquina.Add(
                    PlanificarAldeanosIniciales(
                        mapa,
                        maquina.Centro,
                        posicionesPorMapa));
            }

            var jugadorHumano =
                new Jugador(
                    nombreHumano,
                    TipoJugador.Humano,
                    mapa,
                    new RecursosJugador());

            var jugadoresMaquina =
                new List<Jugador>();

            foreach (var maquina in maquinas)
            {
                jugadoresMaquina.Add(
                    new Jugador(
                        maquina.Nombre,
                        TipoJugador.Maquina,
                        mapa,
                        new RecursosJugador()));
            }

            ConfigurarMapa(
                jugadorHumano,
                centroHumano,
                recursosCompartidos);

            for (int i = 0;
                 i < jugadoresMaquina.Count;
                 i++)
            {
                ConfigurarMapa(
                    jugadoresMaquina[i],
                    maquinas[i].Centro,
                    Array.Empty<Recurso>());
            }

            ConfigurarInicioJugador(
                jugadorHumano,
                aldeanosHumano);

            for (int i = 0;
                 i < jugadoresMaquina.Count;
                 i++)
            {
                ConfigurarInicioJugador(
                    jugadoresMaquina[i],
                    aldeanosMaquina[i]);
            }

            return new Partida(
                jugadorHumano,
                jugadoresMaquina);
        }

        private static void ReservarCentroCompartido(
            Mapa mapa,
            Coordenada centro,
            HashSet<(int, int)> reservadas)
        {
            if (!mapa.PuedeColocar(centro))
            {
                throw new ArgumentException(
                    "La posición de un Centro Urbano no está disponible.");
            }

            if (!reservadas.Add(
                    (centro.X, centro.Y)))
            {
                throw new ArgumentException(
                    "Dos Centros Urbanos no pueden compartir la misma casilla.");
            }
        }

        private static void ValidarRecursosCompartidos(
            Mapa mapa,
            IReadOnlyList<Recurso> recursos,
            HashSet<(int, int)> reservadas)
        {
            bool tieneOro = false;
            bool tieneMadera = false;
            bool tieneComida = false;

            foreach (Recurso recurso in recursos)
            {
                if (recurso == null)
                {
                    throw new ArgumentException(
                        "La lista no puede contener recursos nulos.",
                        nameof(recursos));
                }

                if (!mapa.PuedeColocar(
                        recurso.Coordenada))
                {
                    throw new ArgumentException(
                        "La posición de un recurso no está disponible.",
                        nameof(recursos));
                }

                if (!reservadas.Add(
                        (recurso.Coordenada.X,
                         recurso.Coordenada.Y)))
                {
                    throw new ArgumentException(
                        "Un recurso coincide con un Centro Urbano u otro recurso.",
                        nameof(recursos));
                }

                tieneOro =
                    tieneOro ||
                    recurso.Tipo == TipoRecurso.Oro;

                tieneMadera =
                    tieneMadera ||
                    recurso.Tipo == TipoRecurso.Madera;

                tieneComida =
                    tieneComida ||
                    recurso.Tipo == TipoRecurso.Comida;
            }

            if (!tieneOro ||
                !tieneMadera ||
                !tieneComida)
            {
                throw new ArgumentException(
                    "El mapa compartido requiere Oro, Madera y Comida.",
                    nameof(recursos));
            }
        }

        private void ValidarMapa(
            Mapa mapa,
            Coordenada centro,
            IReadOnlyList<Recurso> recursos,
            Dictionary<Mapa, HashSet<(int, int)>> posicionesPorMapa)
        {
            if (mapa == null)
                throw new ArgumentNullException(nameof(mapa));

            if (centro == null)
                throw new ArgumentNullException(nameof(centro));

            if (recursos == null)
                throw new ArgumentNullException(nameof(recursos));

            if (!mapa.PuedeColocar(centro))
            {
                throw new ArgumentException(
                    "La posición del Centro Urbano no está disponible.",
                    nameof(centro));
            }

            if (!posicionesPorMapa.TryGetValue(
                    mapa,
                    out HashSet<(int, int)> posiciones))
            {
                posiciones =
                    new HashSet<(int, int)>();

                posicionesPorMapa.Add(
                    mapa,
                    posiciones);
            }

            if (!posiciones.Add(
                    (centro.X, centro.Y)))
            {
                throw new ArgumentException(
                    "La posición del Centro Urbano está repetida.",
                    nameof(centro));
            }

            bool tieneOro = false;
            bool tieneMadera = false;
            bool tieneComida = false;

            foreach (Recurso recurso in recursos)
            {
                if (recurso == null)
                {
                    throw new ArgumentException(
                        "La lista no puede contener recursos nulos.",
                        nameof(recursos));
                }

                if (!mapa.PuedeColocar(
                        recurso.Coordenada))
                {
                    throw new ArgumentException(
                        "La posición de un recurso no está disponible.",
                        nameof(recursos));
                }

                if (!posiciones.Add(
                        (recurso.Coordenada.X,
                         recurso.Coordenada.Y)))
                {
                    throw new ArgumentException(
                        "Un recurso coincide con un Centro Urbano u otro recurso.",
                        nameof(recursos));
                }

                tieneOro =
                    tieneOro ||
                    recurso.Tipo == TipoRecurso.Oro;

                tieneMadera =
                    tieneMadera ||
                    recurso.Tipo == TipoRecurso.Madera;

                tieneComida =
                    tieneComida ||
                    recurso.Tipo == TipoRecurso.Comida;
            }

            if (!tieneOro ||
                !tieneMadera ||
                !tieneComida)
            {
                throw new ArgumentException(
                    "Se requiere al menos un recurso de Oro, Madera y Comida.",
                    nameof(recursos));
            }
        }

        private IReadOnlyList<Coordenada> PlanificarAldeanosIniciales(
            Mapa mapa,
            Coordenada centro,
            Dictionary<Mapa, HashSet<(int, int)>> posicionesPorMapa)
        {
            HashSet<(int, int)> reservadas =
                posicionesPorMapa[mapa];

            var resultado =
                new List<Coordenada>();

            for (int i = 0;
                 i < configuracionInicio.AldeanosIniciales;
                 i++)
            {
                Coordenada posicion =
                    BuscarCasillaLibreCercana(
                        mapa,
                        centro,
                        reservadas);

                if (posicion == null)
                {
                    throw new InvalidOperationException(
                        "No hay espacio suficiente para colocar los Aldeanos iniciales.");
                }

                reservadas.Add(
                    (posicion.X, posicion.Y));

                resultado.Add(
                    posicion);
            }

            return resultado;
        }

        private static Coordenada BuscarCasillaLibreCercana(
            Mapa mapa,
            Coordenada origen,
            HashSet<(int, int)> reservadas)
        {
            int distanciaMaxima =
                mapa.Ancho + mapa.Alto;

            for (int distancia = 1;
                 distancia <= distanciaMaxima;
                 distancia++)
            {
                for (int x = 0;
                     x < mapa.Ancho;
                     x++)
                {
                    for (int y = 0;
                         y < mapa.Alto;
                         y++)
                    {
                        if (Math.Abs(x - origen.X) +
                            Math.Abs(y - origen.Y) != distancia)
                        {
                            continue;
                        }

                        if (reservadas.Contains((x, y)))
                            continue;

                        Coordenada candidata =
                            new Coordenada(x, y);

                        Casilla casilla =
                            mapa.ObtenerCasilla(x, y);

                        if (casilla == null ||
                            !casilla.EsTransitable ||
                            !mapa.PuedeColocar(candidata))
                        {
                            continue;
                        }

                        return candidata;
                    }
                }
            }

            return null;
        }

        private void ConfigurarInicioJugador(
            Jugador jugador,
            IReadOnlyList<Coordenada> aldeanos)
        {
            configuracionInicio.AplicarSaldoInicial(
                jugador.Recursos);

            foreach (Coordenada posicion in aldeanos)
            {
                jugador.AgregarUnidad(
                    new Aldeano(posicion));
            }
        }

        private void ConfigurarMapa(
            Jugador jugador,
            Coordenada centro,
            IReadOnlyList<Recurso> recursos)
        {
            if (!jugador.Mapa
                .ObtenerCasilla(
                    centro.X,
                    centro.Y)
                .Ocupar())
            {
                throw new InvalidOperationException(
                    "No se pudo ocupar la casilla del Centro Urbano.");
            }

            jugador.AgregarEdificio(
                new CentroUrbano(centro));

            foreach (Recurso recurso in recursos)
            {
                if (!jugador.Mapa.ColocarRecurso(
                        recurso))
                {
                    throw new InvalidOperationException(
                        "No se pudo colocar un recurso validado.");
                }
            }
        }
    }
}
