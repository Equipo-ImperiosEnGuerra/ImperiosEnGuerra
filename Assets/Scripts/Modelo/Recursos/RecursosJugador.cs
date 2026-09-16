using System;
using System.Collections.Generic;

namespace ImperiosEnGuerra.Modelo.Recursos
{
    public class RecursosJugador
    {
        private readonly Dictionary<TipoRecurso, int> cantidades;

        public RecursosJugador()
        {
            cantidades = new Dictionary<TipoRecurso, int>
            {
                { TipoRecurso.Oro, 0 },
                { TipoRecurso.Madera, 0 },
                { TipoRecurso.Comida, 0 }
            };
        }

        public int ObtenerCantidad(TipoRecurso tipo)
        {
            return cantidades[tipo];
        }

        public void Agregar(TipoRecurso tipo, int cantidad)
        {
            if (cantidad < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(cantidad),
                    "La cantidad a agregar no puede ser negativa."
                );
            }

            cantidades[tipo] += cantidad;
        }

        public bool PuedePagar(TipoRecurso tipo, int cantidad)
        {
            if (cantidad < 0)
            {
                return false;
            }

            return cantidades[tipo] >= cantidad;
        }

        public bool IntentarGastar(TipoRecurso tipo, int cantidad)
        {
            if (cantidad < 0)
            {
                return false;
            }

            if (cantidades[tipo] < cantidad)
            {
                return false;
            }

            cantidades[tipo] -= cantidad;
            return true;
        }
    }
}