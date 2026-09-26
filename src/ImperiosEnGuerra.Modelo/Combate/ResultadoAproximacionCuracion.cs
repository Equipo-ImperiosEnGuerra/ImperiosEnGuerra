using System;
using System.Collections.Generic;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Combate
{
    public sealed class ResultadoAproximacionCuracion
    {
        private readonly List<Coordenada> pasos;

        public bool Exito { get; }
        public string Mensaje { get; }
        public bool YaEnAlcance { get; }
        public Coordenada PuntoCuracion { get; }

        public IReadOnlyList<Coordenada> Pasos =>
            pasos.AsReadOnly();

        private ResultadoAproximacionCuracion(
            bool exito,
            string mensaje,
            bool yaEnAlcance,
            Coordenada puntoCuracion,
            IEnumerable<Coordenada> pasos)
        {
            Exito = exito;
            Mensaje = mensaje ?? string.Empty;
            YaEnAlcance = yaEnAlcance;
            PuntoCuracion = puntoCuracion;

            this.pasos =
                pasos == null
                    ? new List<Coordenada>()
                    : new List<Coordenada>(
                        pasos);
        }

        public static ResultadoAproximacionCuracion EnAlcance(
            Coordenada posicionActual)
        {
            return new ResultadoAproximacionCuracion(
                true,
                "El aliado ya está dentro del alcance de curación.",
                true,
                posicionActual,
                Array.Empty<Coordenada>());
        }

        public static ResultadoAproximacionCuracion Exitoso(
            Coordenada puntoCuracion,
            IEnumerable<Coordenada> pasos)
        {
            if (puntoCuracion == null)
                throw new ArgumentNullException(
                    nameof(puntoCuracion));

            if (pasos == null)
                throw new ArgumentNullException(
                    nameof(pasos));

            return new ResultadoAproximacionCuracion(
                true,
                "Ruta de aproximación de curación preparada.",
                false,
                puntoCuracion,
                pasos);
        }

        public static ResultadoAproximacionCuracion Fallido(
            string mensaje)
        {
            return new ResultadoAproximacionCuracion(
                false,
                mensaje,
                false,
                null,
                Array.Empty<Coordenada>());
        }
    }
}
