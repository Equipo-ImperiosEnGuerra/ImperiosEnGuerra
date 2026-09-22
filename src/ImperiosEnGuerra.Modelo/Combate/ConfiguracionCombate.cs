using System;
using System.Collections.Generic;

namespace ImperiosEnGuerra.Modelo.Combate
{
    /// <summary>
    /// Balance numérico propio del prototipo académico.
    /// Estos valores no provienen de la guía ni pretenden ser valores oficiales de Age of Empires.
    /// </summary>
    public sealed class ConfiguracionCombate
    {
        private readonly Dictionary<string, EstadisticasCombate> porTipo =
            new Dictionary<string, EstadisticasCombate>(
                StringComparer.OrdinalIgnoreCase)
            {
                ["Aldeano"] =
                    new EstadisticasCombate(
                        "Aldeano",
                        60,
                        0,
                        0,
                        0d),

                ["Guerrero"] =
                    new EstadisticasCombate(
                        "Guerrero",
                        120,
                        30,
                        1,
                        4d),

                ["Lancero"] =
                    new EstadisticasCombate(
                        "Lancero",
                        100,
                        25,
                        1,
                        3.5d),

                ["Arquero"] =
                    new EstadisticasCombate(
                        "Arquero",
                        80,
                        20,
                        3,
                        3d),

                ["Monje"] =
                    new EstadisticasCombate(
                        "Monje",
                        70,
                        15,
                        2,
                        5d),

                ["CentroUrbano"] =
                    new EstadisticasCombate(
                        "CentroUrbano",
                        300,
                        0,
                        0,
                        0d)
            };

        public EstadisticasCombate Obtener(
            string tipo)
        {
            if (string.IsNullOrWhiteSpace(tipo) ||
                !porTipo.TryGetValue(
                    tipo,
                    out EstadisticasCombate estadisticas))
            {
                throw new ArgumentException(
                    $"No existe configuración de combate para '{tipo}'.",
                    nameof(tipo));
            }

            return estadisticas;
        }
    }

    public sealed class EstadisticasCombate
    {
        public string Tipo { get; }
        public int VidaMaxima { get; }
        public int Danio { get; }
        public int Alcance { get; }
        public double IntervaloAtaqueSegundos { get; }

        public EstadisticasCombate(
            string tipo,
            int vidaMaxima,
            int danio,
            int alcance,
            double intervaloAtaqueSegundos)
        {
            if (string.IsNullOrWhiteSpace(tipo))
                throw new ArgumentException(
                    "El tipo es obligatorio.",
                    nameof(tipo));

            if (vidaMaxima <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(vidaMaxima));

            if (danio < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(danio));

            if (alcance < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(alcance));

            if (intervaloAtaqueSegundos < 0d ||
                double.IsNaN(intervaloAtaqueSegundos) ||
                double.IsInfinity(intervaloAtaqueSegundos))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(intervaloAtaqueSegundos));
            }

            Tipo = tipo;
            VidaMaxima = vidaMaxima;
            Danio = danio;
            Alcance = alcance;
            IntervaloAtaqueSegundos = intervaloAtaqueSegundos;
        }
    }
}
