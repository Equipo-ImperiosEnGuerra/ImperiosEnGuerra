namespace ImperiosEnGuerra.Api.Configuracion;

/// <summary>
/// Guarda los tiempos usados por las acciones que se ejecutan de forma concurrente.
/// </summary>
public class AccionesConcurrentesOptions
{
    public int MovimientoSegundos { get; set; }
    public int RecoleccionSegundos { get; set; }
    public int ConstruccionSegundos { get; set; }
    public int EntrenamientoSegundos { get; set; }
    public int AtaqueSegundos { get; set; }
}