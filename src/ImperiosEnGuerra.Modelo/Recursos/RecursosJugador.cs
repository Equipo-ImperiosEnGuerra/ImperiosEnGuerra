using System;
using System.Collections.Generic;

namespace ImperiosEnGuerra.Modelo.Recursos
{
    /// <summary>
    /// Administra los saldos de oro, madera y comida de un participante.
    /// </summary>
    public class RecursosJugador
    {
        private readonly Dictionary<TipoRecurso, int> cantidades;

        /// <summary>
        /// Inicializa los saldos de oro, madera y comida en cero.
        /// </summary>
        public RecursosJugador()
        {
            cantidades = new Dictionary<TipoRecurso, int>
            {
                { TipoRecurso.Oro, 0 },
                { TipoRecurso.Madera, 0 },
                { TipoRecurso.Comida, 0 }
            };
        }

        /// <summary>
        /// Consulta el saldo almacenado para un tipo de recurso.
        /// </summary>
        /// <param name="tipo">Tipo de recurso que se consulta.</param>
        /// <returns>Cantidad almacenada.</returns>
        /// <exception cref="KeyNotFoundException">El tipo no corresponde a una clave registrada.</exception>
        public int ObtenerCantidad(TipoRecurso tipo)
        {
            return cantidades[tipo];
        }

        /// <summary>
        /// Incrementa el saldo del tipo indicado en una cantidad no negativa.
        /// </summary>
        /// <param name="tipo">Tipo de recurso que se incrementa.</param>
        /// <param name="cantidad">Cantidad que se añade; puede ser cero.</param>
        /// <exception cref="ArgumentOutOfRangeException">La cantidad es negativa.</exception>
        /// <exception cref="KeyNotFoundException">El tipo no corresponde a una clave registrada.</exception>
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

        /// <summary>
        /// Comprueba si el saldo cubre una cantidad sin descontarla.
        /// </summary>
        /// <param name="tipo">Tipo de recurso que se consulta.</param>
        /// <param name="cantidad">Cantidad requerida.</param>
        /// <returns>true si la cantidad es no negativa y hay saldo suficiente; false en caso contrario.</returns>
        /// <exception cref="KeyNotFoundException">La cantidad es no negativa y el tipo no corresponde a una clave registrada.</exception>
        public bool PuedePagar(TipoRecurso tipo, int cantidad)
        {
            if (cantidad < 0)
            {
                return false;
            }

            return cantidades[tipo] >= cantidad;
        }

        /// <summary>
        /// Descuenta la cantidad si hay saldo suficiente; conserva el saldo si es negativa o insuficiente.
        /// </summary>
        /// <param name="tipo">Tipo de recurso que se consulta.</param>
        /// <param name="cantidad">Cantidad requerida.</param>
        /// <returns>true si se realizó el descuento, incluido un gasto de cero; false si la cantidad es negativa o falta saldo.</returns>
        /// <exception cref="KeyNotFoundException">La cantidad es no negativa y el tipo no corresponde a una clave registrada.</exception>
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