using System;
using System.Collections.Generic;

namespace ImperiosEnGuerra.Modelo.Recursos
{
    /// <summary>
    /// Configuración lógica de cantidad recolectada por ciclo según el tipo de recurso.
    /// </summary>
    public sealed class ConfiguracionRecoleccion
    {
        private readonly Dictionary<TipoRecurso, int> tasas;

        public ConfiguracionRecoleccion(
            int tasaOro = 1,
            int tasaMadera = 1,
            int tasaComida = 1)
        {
            ValidarTasa(
                tasaOro,
                nameof(tasaOro));

            ValidarTasa(
                tasaMadera,
                nameof(tasaMadera));

            ValidarTasa(
                tasaComida,
                nameof(tasaComida));

            tasas =
                new Dictionary<TipoRecurso, int>
                {
                    { TipoRecurso.Oro, tasaOro },
                    { TipoRecurso.Madera, tasaMadera },
                    { TipoRecurso.Comida, tasaComida }
                };
        }

        public int ObtenerTasa(
            TipoRecurso tipo)
        {
            if (!tasas.TryGetValue(
                    tipo,
                    out int tasa))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(tipo),
                    "El tipo de recurso no tiene una tasa de recolección configurada.");
            }

            return tasa;
        }

        private static void ValidarTasa(
            int tasa,
            string nombreParametro)
        {
            if (tasa <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nombreParametro,
                    "La tasa de recolección debe ser positiva.");
            }
        }
    }
}
