using System;
using System.Collections.Generic;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Edificios
{
    /// <summary>
    /// Devuelve la ruta y la casilla desde la que un Aldeano puede trabajar en una obra.
    /// </summary>
    public sealed class ResultadoAproximacionConstruccion
    {
        private readonly List<Coordenada> pasos;

        public bool Exito { get; }
        public string Mensaje { get; }
        public Coordenada PuntoInteraccion { get; }
        public bool Reintentable { get; }

        public IReadOnlyList<Coordenada> Pasos =>
            pasos.AsReadOnly();

        private ResultadoAproximacionConstruccion(
            bool exito,
            string mensaje,
            Coordenada puntoInteraccion,
            IEnumerable<Coordenada> pasos,
            bool reintentable)
        {
            Exito = exito;
            Mensaje = mensaje ?? string.Empty;
            PuntoInteraccion = puntoInteraccion;
            this.pasos = pasos == null
                ? new List<Coordenada>()
                : new List<Coordenada>(pasos);
            Reintentable = reintentable;
        }

        //Crea un resultado con la ruta válida hacia el punto de interacción.
        public static ResultadoAproximacionConstruccion Exitoso(
            Coordenada puntoInteraccion,
            IEnumerable<Coordenada> pasos)
        {
            if (puntoInteraccion == null)
                throw new ArgumentNullException(nameof(puntoInteraccion));
            if (pasos == null)
                throw new ArgumentNullException(nameof(pasos));

            return new ResultadoAproximacionConstruccion(
                true,
                "Ruta hacia la obra preparada.",
                puntoInteraccion,
                pasos,
                false);
        }

        //Indica si el fallo puede intentarse de nuevo cuando cambie la ocupación del mapa.
        public static ResultadoAproximacionConstruccion Fallido(
            string mensaje,
            bool reintentable = false)
        {
            return new ResultadoAproximacionConstruccion(
                false,
                mensaje,
                null,
                Array.Empty<Coordenada>(),
                reintentable);
        }
    }
}
