using System;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Acciones
{
    public sealed class SolicitudConstruccion : SolicitudAccion
    {
        public Guid AldeanoId { get; }
        public string TipoEdificio { get; }
        public Coordenada Destino { get; }

        public SolicitudConstruccion(
            Guid aldeanoId,
            string tipoEdificio,
            Coordenada destino)
            : base(TipoAccionJuego.Construir)
        {
            AldeanoId = aldeanoId;
            TipoEdificio = tipoEdificio;
            Destino = destino;
        }
    }
}