using System;
using System.Collections.Generic;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Movimiento
{
    /// <summary>
    /// Resultado inmutable de preparar una orden de movimiento sin modificar todavía la posición.
    /// </summary>
    public sealed class ResultadoPlanMovimiento
    {
        private readonly List<Coordenada> pasos;

        public bool Exito { get; }
        public string Mensaje { get; }

        public IReadOnlyList<Coordenada> Pasos
        {
            get { return pasos.AsReadOnly(); }
        }

        private ResultadoPlanMovimiento(
            bool exito,
            string mensaje,
            IEnumerable<Coordenada> pasos)
        {
            Exito = exito;
            Mensaje = mensaje ?? string.Empty;
            this.pasos = pasos == null
                ? new List<Coordenada>()
                : new List<Coordenada>(pasos);
        }

        public static ResultadoPlanMovimiento Exitoso(
            IEnumerable<Coordenada> pasos)
        {
            if (pasos == null)
                throw new ArgumentNullException(nameof(pasos));

            return new ResultadoPlanMovimiento(
                true,
                "Ruta de movimiento preparada.",
                pasos);
        }

        public static ResultadoPlanMovimiento Fallido(
            string mensaje)
        {
            return new ResultadoPlanMovimiento(
                false,
                mensaje,
                Array.Empty<Coordenada>());
        }
    }
}
