using System;

namespace ImperiosEnGuerra.Modelo.Recursos
{
    /// <summary>
    /// Costo económico inmutable limitado a los tres recursos exigidos por el proyecto.
    /// </summary>
    public sealed class CostoRecursos
    {
        public int Oro { get; }
        public int Madera { get; }
        public int Comida { get; }

        public CostoRecursos(
            int oro,
            int madera,
            int comida)
        {
            if (oro < 0)
                throw new ArgumentOutOfRangeException(nameof(oro));
            if (madera < 0)
                throw new ArgumentOutOfRangeException(nameof(madera));
            if (comida < 0)
                throw new ArgumentOutOfRangeException(nameof(comida));

            Oro = oro;
            Madera = madera;
            Comida = comida;
        }

        public bool EsCero =>
            Oro == 0 &&
            Madera == 0 &&
            Comida == 0;

        public override string ToString()
        {
            return $"Oro {Oro}, Madera {Madera}, Comida {Comida}";
        }
    }
}
