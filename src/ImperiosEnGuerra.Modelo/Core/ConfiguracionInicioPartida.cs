using System;
using ImperiosEnGuerra.Modelo.Recursos;

namespace ImperiosEnGuerra.Modelo.Core
{
    /// <summary>
    /// Balance inicial propio del prototipo académico.
    /// La guía exige mapa, recursos y Centro Urbano, pero no fija estos valores.
    /// </summary>
    public sealed class ConfiguracionInicioPartida
    {
        public const int AldeanosInicialesPredeterminados = 2;
        public const int OroInicialPredeterminado = 0;
        public const int MaderaInicialPredeterminada = 20;
        public const int ComidaInicialPredeterminada = 30;

        public int AldeanosIniciales { get; }
        public int OroInicial { get; }
        public int MaderaInicial { get; }
        public int ComidaInicial { get; }

        public ConfiguracionInicioPartida(
            int aldeanosIniciales = AldeanosInicialesPredeterminados,
            int oroInicial = OroInicialPredeterminado,
            int maderaInicial = MaderaInicialPredeterminada,
            int comidaInicial = ComidaInicialPredeterminada)
        {
            if (aldeanosIniciales <= 0)
                throw new ArgumentOutOfRangeException(nameof(aldeanosIniciales));
            if (oroInicial < 0)
                throw new ArgumentOutOfRangeException(nameof(oroInicial));
            if (maderaInicial < 0)
                throw new ArgumentOutOfRangeException(nameof(maderaInicial));
            if (comidaInicial < 0)
                throw new ArgumentOutOfRangeException(nameof(comidaInicial));

            AldeanosIniciales = aldeanosIniciales;
            OroInicial = oroInicial;
            MaderaInicial = maderaInicial;
            ComidaInicial = comidaInicial;
        }

        public void AplicarSaldoInicial(
            RecursosJugador recursos)
        {
            if (recursos == null)
                throw new ArgumentNullException(nameof(recursos));

            recursos.Agregar(TipoRecurso.Oro, OroInicial);
            recursos.Agregar(TipoRecurso.Madera, MaderaInicial);
            recursos.Agregar(TipoRecurso.Comida, ComidaInicial);
        }
    }
}
