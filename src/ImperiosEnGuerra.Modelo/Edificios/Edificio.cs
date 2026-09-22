using System;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Edificios
{
    public abstract class Edificio
    {
        public Guid Id { get; }
        public Coordenada Coordenada { get; }

        protected Edificio(Coordenada coordenada)
        {
            if (coordenada == null)
                throw new ArgumentNullException(nameof(coordenada));

            Id = Guid.NewGuid();
            Coordenada = coordenada;
        }
    }
}
