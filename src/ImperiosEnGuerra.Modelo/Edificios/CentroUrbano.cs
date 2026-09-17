using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Edificios
{
    /// <summary>
    /// Especializa un edificio como Centro Urbano, conservando la posición definida por la clase base.
    /// </summary>
    public class CentroUrbano : Edificio
    {
        /// <summary>
        /// Crea un Centro Urbano delegando la inicialización de su posición en Edificio.
        /// </summary>
        /// <param name="coordenada">Posición lógica del Centro Urbano.</param>
        /// <exception cref="System.ArgumentNullException">La coordenada es nula.</exception>
        public CentroUrbano(Coordenada coordenada)
            : base(coordenada)
        {
        }
    }
}
