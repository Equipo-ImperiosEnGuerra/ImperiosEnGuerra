using System;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;

namespace ImperiosEnGuerra.Modelo.Unidades
{
    /// <summary>
    /// Representa la especialización Aldeano con carga transportada separada
    /// del saldo económico del jugador.
    /// </summary>
    public class Aldeano : Unidad
    {
        public const int CapacidadCargaPredeterminada = 10;

        private readonly object sincronizacionCarga =
            new object();

        private int cargaActual;
        private TipoRecurso? tipoCarga;

        public int CapacidadCarga { get; }

        public int CargaActual
        {
            get
            {
                lock (sincronizacionCarga)
                {
                    return cargaActual;
                }
            }
        }

        public TipoRecurso? TipoCarga
        {
            get
            {
                lock (sincronizacionCarga)
                {
                    return tipoCarga;
                }
            }
        }

        public int CapacidadDisponible
        {
            get
            {
                lock (sincronizacionCarga)
                {
                    return CapacidadCarga - cargaActual;
                }
            }
        }

        public Aldeano(
            Coordenada coordenada)
            : this(
                coordenada,
                CapacidadCargaPredeterminada)
        {
        }

        public Aldeano(
            Coordenada coordenada,
            int capacidadCarga)
            : base(
                coordenada,
                1.00d)
        {
            if (capacidadCarga <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(capacidadCarga),
                    "La capacidad de carga debe ser positiva.");
            }

            CapacidadCarga = capacidadCarga;
            cargaActual = 0;
            tipoCarga = null;
        }

        /// <summary>
        /// Extrae del nodo y añade a la carga del Aldeano de forma sincronizada.
        /// No mezcla tipos de recurso en una misma carga.
        /// </summary>
        public int RecolectarDesde(
            Recurso recurso,
            int cantidadSolicitada)
        {
            if (recurso == null)
            {
                throw new ArgumentNullException(
                    nameof(recurso));
            }

            if (cantidadSolicitada < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(cantidadSolicitada),
                    "La cantidad solicitada no puede ser negativa.");
            }

            if (cantidadSolicitada == 0)
            {
                return 0;
            }

            lock (sincronizacionCarga)
            {
                if (cargaActual > 0 &&
                    tipoCarga.HasValue &&
                    tipoCarga.Value != recurso.Tipo)
                {
                    return 0;
                }

                int capacidadDisponible =
                    CapacidadCarga - cargaActual;

                if (capacidadDisponible <= 0)
                {
                    return 0;
                }

                int cantidadAExtraer =
                    Math.Min(
                        cantidadSolicitada,
                        capacidadDisponible);

                int extraida =
                    recurso.Extraer(
                        cantidadAExtraer);

                if (extraida > 0)
                {
                    tipoCarga = recurso.Tipo;
                    cargaActual += extraida;
                }

                return extraida;
            }
        }

        /// <summary>
        /// Vacía la carga para que una operación de depósito pueda transferirla
        /// al saldo del jugador.
        /// </summary>
        public int VaciarCarga(
            out TipoRecurso? tipo)
        {
            lock (sincronizacionCarga)
            {
                tipo = tipoCarga;

                int cantidad =
                    cargaActual;

                cargaActual = 0;
                tipoCarga = null;

                return cantidad;
            }
        }
    }
}
