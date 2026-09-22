using System;
using System.Collections.Generic;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Recoleccion
{
    public sealed class ResultadoAproximacionDeposito
    {
        private readonly List<Coordenada> pasos;

        public bool Exito { get; }
        public string Mensaje { get; }
        public Coordenada CentroUrbano { get; }
        public Coordenada PuntoInteraccion { get; }
        public bool Reintentable { get; }

        public IReadOnlyList<Coordenada> Pasos =>
            pasos.AsReadOnly();

        private ResultadoAproximacionDeposito(
            bool exito,
            string mensaje,
            Coordenada centroUrbano,
            Coordenada puntoInteraccion,
            IEnumerable<Coordenada> pasos,
            bool reintentable)
        {
            Exito = exito;
            Mensaje = mensaje ?? string.Empty;
            CentroUrbano = centroUrbano;
            PuntoInteraccion = puntoInteraccion;
            this.pasos = pasos == null
                ? new List<Coordenada>()
                : new List<Coordenada>(pasos);
            Reintentable = reintentable;
        }

        public static ResultadoAproximacionDeposito Exitoso(
            Coordenada centroUrbano,
            Coordenada puntoInteraccion,
            IEnumerable<Coordenada> pasos)
        {
            if (centroUrbano == null)
                throw new ArgumentNullException(nameof(centroUrbano));

            if (puntoInteraccion == null)
                throw new ArgumentNullException(nameof(puntoInteraccion));

            if (pasos == null)
                throw new ArgumentNullException(nameof(pasos));

            return new ResultadoAproximacionDeposito(
                true,
                "Ruta hacia el punto de depósito preparada.",
                centroUrbano,
                puntoInteraccion,
                pasos,
                false);
        }

        public static ResultadoAproximacionDeposito Fallido(
            string mensaje,
            bool reintentable = false)
        {
            return new ResultadoAproximacionDeposito(
                false,
                mensaje,
                null,
                null,
                Array.Empty<Coordenada>(),
                reintentable);
        }
    }
}
