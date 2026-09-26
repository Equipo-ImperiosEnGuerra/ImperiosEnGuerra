using System;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Edificios
{
    public sealed class ResultadoSpawnEntrenamiento
    {
        public bool Exito { get; }
        public string Mensaje { get; }
        public Guid UnidadId { get; }
        public Coordenada Coordenada { get; }

        private ResultadoSpawnEntrenamiento(
            bool exito,
            string mensaje,
            Guid unidadId,
            Coordenada coordenada)
        {
            Exito = exito;
            Mensaje = mensaje ?? string.Empty;
            UnidadId = unidadId;
            Coordenada = coordenada;
        }

        public static ResultadoSpawnEntrenamiento Exitoso(
            Guid unidadId,
            Coordenada coordenada,
            string tipoUnidad)
        {
            return new ResultadoSpawnEntrenamiento(
                true,
                $"Unidad {tipoUnidad} entrenada correctamente.",
                unidadId,
                coordenada);
        }

        public static ResultadoSpawnEntrenamiento Fallido(
            string mensaje)
        {
            return new ResultadoSpawnEntrenamiento(
                false,
                mensaje,
                Guid.Empty,
                null);
        }
    }
}
