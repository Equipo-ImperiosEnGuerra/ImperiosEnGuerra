using System;
using System.Collections.Generic;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Edificios
{
    /// <summary>
    /// Multiplicadores propios del prototipo para tiempos de entrenamiento.
    /// </summary>
    public sealed class ConfiguracionEntrenamiento
    {
        private readonly Dictionary<string, double> factores;

        public ConfiguracionEntrenamiento()
        {
            factores =
                new Dictionary<string, double>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    { nameof(Aldeano), 1.00d },
                    { nameof(Guerrero), 1.20d },
                    { nameof(Lancero), 1.10d },
                    { nameof(Arquero), 1.30d },
                    { nameof(Monje), 1.50d }
                };
        }

        public bool IntentarObtenerFactor(
            string tipoUnidad,
            out double factor)
        {
            if (string.IsNullOrWhiteSpace(tipoUnidad))
            {
                factor = 0d;
                return false;
            }

            return factores.TryGetValue(
                tipoUnidad,
                out factor);
        }
    }
}
