using System;
using System.Collections.Generic;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Combate
{
    public sealed class ResultadoAproximacionAtaque
    {
        private readonly List<Coordenada> pasos;

        public bool Exito { get; }
        public string Mensaje { get; }
        public bool YaEnAlcance { get; }
        public Coordenada PuntoAtaque { get; }

        public IReadOnlyList<Coordenada> Pasos =>
            pasos.AsReadOnly();

        private ResultadoAproximacionAtaque(
            bool exito,
            string mensaje,
            bool yaEnAlcance,
            Coordenada puntoAtaque,
            IEnumerable<Coordenada> pasos)
        {
            Exito = exito;
            Mensaje = mensaje ?? string.Empty;
            YaEnAlcance = yaEnAlcance;
            PuntoAtaque = puntoAtaque;
            this.pasos = pasos == null
                ? new List<Coordenada>()
                : new List<Coordenada>(pasos);
        }

        public static ResultadoAproximacionAtaque EnAlcance(
            Coordenada posicionActual)
        {
            return new ResultadoAproximacionAtaque(
                true,
                "El objetivo ya está dentro del alcance.",
                true,
                posicionActual,
                Array.Empty<Coordenada>());
        }

        public static ResultadoAproximacionAtaque Exitoso(
            Coordenada puntoAtaque,
            IEnumerable<Coordenada> pasos)
        {
            if (puntoAtaque == null)
                throw new ArgumentNullException(nameof(puntoAtaque));

            if (pasos == null)
                throw new ArgumentNullException(nameof(pasos));

            return new ResultadoAproximacionAtaque(
                true,
                "Ruta de aproximación de ataque preparada.",
                false,
                puntoAtaque,
                pasos);
        }

        public static ResultadoAproximacionAtaque Fallido(
            string mensaje)
        {
            return new ResultadoAproximacionAtaque(
                false,
                mensaje,
                false,
                null,
                Array.Empty<Coordenada>());
        }
    }
}
