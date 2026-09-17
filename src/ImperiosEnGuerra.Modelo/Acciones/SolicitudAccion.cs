namespace ImperiosEnGuerra.Modelo.Acciones
{
    /// <summary>Intención mínima; no identifica aún ejecutor, objetivo ni destino.</summary>
    public class SolicitudAccion
    {
        public TipoAccionJuego Tipo { get; }

        public SolicitudAccion(TipoAccionJuego tipo)
        {
            Tipo = tipo;
        }
    }
}
