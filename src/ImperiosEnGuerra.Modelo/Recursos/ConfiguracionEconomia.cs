using System;
using System.Collections.Generic;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Recursos
{
    /// <summary>
    /// Balance económico propio del prototipo académico.
    /// Comida sostiene crecimiento y ejército; Madera se reserva a construcción;
    /// Oro financia tropas y expansión.
    /// </summary>
    public sealed class ConfiguracionEconomia
    {
        private readonly Dictionary<string, CostoRecursos> costosEdificios;
        private readonly Dictionary<string, CostoRecursos> costosUnidades;

        public ConfiguracionEconomia()
        {
            costosEdificios =
                new Dictionary<string, CostoRecursos>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    {
                        nameof(CentroUrbano),
                        new CostoRecursos(
                            oro: 20,
                            madera: 50,
                            comida: 0)
                    }
                };

            costosUnidades =
                new Dictionary<string, CostoRecursos>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    { nameof(Aldeano), new CostoRecursos(0, 0, 10) },
                    { nameof(Guerrero), new CostoRecursos(5, 0, 15) },
                    { nameof(Lancero), new CostoRecursos(8, 0, 15) },
                    { nameof(Arquero), new CostoRecursos(10, 0, 10) },
                    { nameof(Monje), new CostoRecursos(20, 0, 10) }
                };
        }

        public bool IntentarObtenerCostoEdificio(
            string tipoEdificio,
            out CostoRecursos costo)
        {
            if (string.IsNullOrWhiteSpace(tipoEdificio))
            {
                costo = null;
                return false;
            }

            return costosEdificios.TryGetValue(
                tipoEdificio,
                out costo);
        }

        public bool IntentarObtenerCostoUnidad(
            string tipoUnidad,
            out CostoRecursos costo)
        {
            if (string.IsNullOrWhiteSpace(tipoUnidad))
            {
                costo = null;
                return false;
            }

            return costosUnidades.TryGetValue(
                tipoUnidad,
                out costo);
        }
    }
}
