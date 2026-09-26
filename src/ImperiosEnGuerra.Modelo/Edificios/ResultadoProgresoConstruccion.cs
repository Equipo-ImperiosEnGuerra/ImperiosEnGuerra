namespace ImperiosEnGuerra.Modelo.Edificios
{
    public sealed class ResultadoProgresoConstruccion
    {
        public bool Exito { get; }
        public string Mensaje { get; }
        public int Progreso { get; }
        public bool Terminada { get; }

        private ResultadoProgresoConstruccion(
            bool exito,
            string mensaje,
            int progreso,
            bool terminada)
        {
            Exito = exito;
            Mensaje = mensaje ?? string.Empty;
            Progreso = progreso;
            Terminada = terminada;
        }

        public static ResultadoProgresoConstruccion Exitoso(
            int progreso,
            bool terminada)
        {
            return new ResultadoProgresoConstruccion(
                true,
                terminada
                    ? "Construcción terminada."
                    : $"Construcción al {progreso}%.",
                progreso,
                terminada);
        }

        public static ResultadoProgresoConstruccion Fallido(
            string mensaje)
        {
            return new ResultadoProgresoConstruccion(
                false,
                mensaje,
                0,
                false);
        }
    }
}
