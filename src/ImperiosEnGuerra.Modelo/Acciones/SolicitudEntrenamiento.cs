using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Acciones
{
    /// <summary>
    /// Indica qué unidad debe aparecer, desde qué edificio y en qué casilla.
    /// </summary>
    public sealed class SolicitudEntrenamiento : SolicitudAccion
    {
        public Coordenada EdificioOrigen { get; }
        public string TipoUnidad { get; }
        public Coordenada Destino { get; }

        public SolicitudEntrenamiento(
            Coordenada edificioOrigen,
            string tipoUnidad,
            Coordenada destino)
            : base(TipoAccionJuego.Entrenar)
        {
            EdificioOrigen = edificioOrigen;
            TipoUnidad = tipoUnidad;
            Destino = destino;
        }
    }
}