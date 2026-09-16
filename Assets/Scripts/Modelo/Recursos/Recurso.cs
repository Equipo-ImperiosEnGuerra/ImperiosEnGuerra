using System;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Recursos
{
    public class Recurso
    {
        public TipoRecurso Tipo { get; }
        public Coordenada Coordenada { get; }

        public Recurso(TipoRecurso tipo, Coordenada coordenada)
        {
            if (coordenada == null)
            {
                throw new ArgumentNullException(nameof(coordenada));
            }

            Tipo = tipo;
            Coordenada = coordenada;
        }
    }
}
