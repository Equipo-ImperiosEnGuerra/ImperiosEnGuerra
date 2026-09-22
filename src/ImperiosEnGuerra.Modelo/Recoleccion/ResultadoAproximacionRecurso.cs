using System;
using System.Collections.Generic;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;

namespace ImperiosEnGuerra.Modelo.Recoleccion
{
    /// <summary>
    /// Resultado de calcular una ruta hasta una casilla de interacción
    /// adyacente a un nodo de recurso.
    /// </summary>
    public sealed class ResultadoAproximacionRecurso
    {
        private readonly List<Coordenada> pasos;

        public bool Exito { get; }
        public string Mensaje { get; }
        public TipoRecurso? TipoRecurso { get; }
        public Coordenada PuntoInteraccion { get; }
        public bool Reintentable { get; }

        public IReadOnlyList<Coordenada> Pasos
        {
            get { return pasos.AsReadOnly(); }
        }

        private ResultadoAproximacionRecurso(
            bool exito,
            string mensaje,
            TipoRecurso? tipoRecurso,
            Coordenada puntoInteraccion,
            IEnumerable<Coordenada> pasos,
            bool reintentable)
        {
            Exito = exito;
            Mensaje = mensaje ?? string.Empty;
            TipoRecurso = tipoRecurso;
            PuntoInteraccion = puntoInteraccion;
            Reintentable = reintentable;
            this.pasos = pasos == null
                ? new List<Coordenada>()
                : new List<Coordenada>(pasos);
        }

        public static ResultadoAproximacionRecurso Exitoso(
            TipoRecurso tipoRecurso,
            Coordenada puntoInteraccion,
            IEnumerable<Coordenada> pasos)
        {
            if (puntoInteraccion == null)
            {
                throw new ArgumentNullException(
                    nameof(puntoInteraccion));
            }

            if (pasos == null)
            {
                throw new ArgumentNullException(
                    nameof(pasos));
            }

            return new ResultadoAproximacionRecurso(
                true,
                $"Ruta hacia recurso {tipoRecurso} preparada.",
                tipoRecurso,
                puntoInteraccion,
                pasos,
                false);
        }

        public static ResultadoAproximacionRecurso Fallido(
            string mensaje,
            bool reintentable = false)
        {
            return new ResultadoAproximacionRecurso(
                false,
                mensaje,
                null,
                null,
                Array.Empty<Coordenada>(),
                reintentable);
        }
    }
}
