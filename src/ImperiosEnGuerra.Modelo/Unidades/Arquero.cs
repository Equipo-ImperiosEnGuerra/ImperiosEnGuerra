using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Unidades
{
    /// <summary>
    /// Representa la especialización Arquero con la posición y disponibilidad heredadas.
    /// </summary>
    public class Arquero : Soldado
    {
       /// <summary>
       /// Inicializa la unidad delegando la posición y la disponibilidad inicial en su clase base.
       /// </summary>
       /// <param name="coordenada">Posición lógica inicial, conservada sin validación.</param>
       public Arquero(Coordenada coordenada)
       : base(coordenada)
        {
            
        } 
    }
}
