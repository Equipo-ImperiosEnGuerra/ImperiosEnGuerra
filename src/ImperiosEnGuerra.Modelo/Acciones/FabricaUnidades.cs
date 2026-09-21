using System;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Acciones
{
    public static class FabricaUnidades
    {
        public static Unidad Crear(
            string tipoUnidad,
            Coordenada coordenada)
        {
            if (string.IsNullOrWhiteSpace(tipoUnidad) ||
                coordenada == null)
            {
                return null;
            }

            if (string.Equals(tipoUnidad, nameof(Aldeano), StringComparison.OrdinalIgnoreCase))
                return new Aldeano(coordenada);
            if (string.Equals(tipoUnidad, nameof(Guerrero), StringComparison.OrdinalIgnoreCase))
                return new Guerrero(coordenada);
            if (string.Equals(tipoUnidad, nameof(Lancero), StringComparison.OrdinalIgnoreCase))
                return new Lancero(coordenada);
            if (string.Equals(tipoUnidad, nameof(Arquero), StringComparison.OrdinalIgnoreCase))
                return new Arquero(coordenada);
            if (string.Equals(tipoUnidad, nameof(Monje), StringComparison.OrdinalIgnoreCase))
                return new Monje(coordenada);

            return null;
        }
    }
}
