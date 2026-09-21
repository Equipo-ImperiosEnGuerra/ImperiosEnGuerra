using System;
using System.Collections.Generic;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Movimiento
{
    public sealed class ResultadoRuta
    {
        private readonly List<Coordenada> pasos;

        public bool Encontrada { get; }

        public IReadOnlyList<Coordenada> Pasos
        {
            get { return pasos.AsReadOnly(); }
        }

        private ResultadoRuta(
            bool encontrada,
            IEnumerable<Coordenada> pasos)
        {
            Encontrada = encontrada;

            this.pasos = pasos == null
                ? new List<Coordenada>()
                : new List<Coordenada>(pasos);
        }

        public static ResultadoRuta Exitosa(
            IEnumerable<Coordenada> pasos)
        {
            if (pasos == null)
            {
                throw new ArgumentNullException(nameof(pasos));
            }

            return new ResultadoRuta(
                true,
                pasos);
        }

        public static ResultadoRuta Imposible()
        {
            return new ResultadoRuta(
                false,
                Array.Empty<Coordenada>());
        }
    }
}
