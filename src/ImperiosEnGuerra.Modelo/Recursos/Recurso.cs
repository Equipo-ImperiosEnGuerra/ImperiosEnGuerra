using System;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Recursos
{
    /// <summary>
    /// Representa un nodo físico de recurso con cantidad restante sincronizada.
    /// </summary>
    public class Recurso
    {
        public const int CantidadInicialPredeterminada = 100;

        private readonly object sincronizacion =
            new object();

        private int cantidadRestante;

        /// <summary>
        /// Tipo del recurso físico.
        /// </summary>
        public TipoRecurso Tipo { get; }

        /// <summary>
        /// Posición lógica del recurso físico.
        /// </summary>
        public Coordenada Coordenada { get; }

        /// <summary>
        /// Cantidad que todavía puede extraerse del nodo.
        /// </summary>
        public int CantidadRestante
        {
            get
            {
                lock (sincronizacion)
                {
                    return cantidadRestante;
                }
            }
        }

        public bool Agotado
        {
            get
            {
                lock (sincronizacion)
                {
                    return cantidadRestante == 0;
                }
            }
        }

        public Recurso(
            TipoRecurso tipo,
            Coordenada coordenada)
            : this(
                tipo,
                coordenada,
                CantidadInicialPredeterminada)
        {
        }

        public Recurso(
            TipoRecurso tipo,
            Coordenada coordenada,
            int cantidadInicial)
        {
            if (coordenada == null)
            {
                throw new ArgumentNullException(
                    nameof(coordenada));
            }

            if (cantidadInicial <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(cantidadInicial),
                    "La cantidad inicial del recurso debe ser positiva.");
            }

            Tipo = tipo;
            Coordenada = coordenada;
            cantidadRestante = cantidadInicial;
        }

        /// <summary>
        /// Extrae hasta la cantidad solicitada sin permitir valores negativos
        /// ni entregar más recurso del disponible.
        /// </summary>
        public int Extraer(
            int cantidadSolicitada)
        {
            if (cantidadSolicitada < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(cantidadSolicitada),
                    "La cantidad a extraer no puede ser negativa.");
            }

            if (cantidadSolicitada == 0)
            {
                return 0;
            }

            lock (sincronizacion)
            {
                int extraida =
                    Math.Min(
                        cantidadSolicitada,
                        cantidadRestante);

                cantidadRestante -= extraida;

                return extraida;
            }
        }
    }
}
