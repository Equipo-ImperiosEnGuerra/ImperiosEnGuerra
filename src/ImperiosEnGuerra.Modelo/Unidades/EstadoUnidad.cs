namespace ImperiosEnGuerra.Modelo.Unidades
{
    /// <summary>
    /// Estado lógico autoritativo de una unidad durante una orden de gameplay.
    /// </summary>
    public enum EstadoUnidad
    {
        Idle,
        Moviendo,
        Recolectando,
        Construyendo,
        Atacando,
        Curando
    }
}
