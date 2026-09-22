namespace ImperiosEnGuerra.Modelo.Edificios
{
    public sealed class ResultadoProgresoEntrenamiento
    {
        public bool Exito { get; }
        public string Mensaje { get; }
        public int Progreso { get; }
        public bool Terminado { get; }

        private ResultadoProgresoEntrenamiento(
            bool exito,
            string mensaje,
            int progreso,
            bool terminado)
        {
            Exito = exito;
            Mensaje = mensaje ?? string.Empty;
            Progreso = progreso;
            Terminado = terminado;
        }

        public static ResultadoProgresoEntrenamiento Exitoso(
            int progreso)
        {
            return new ResultadoProgresoEntrenamiento(
                true,
                progreso >= 100
                    ? "Entrenamiento listo para aparición."
                    : $"Entrenamiento al {progreso}%.",
                progreso,
                progreso >= 100);
        }

        public static ResultadoProgresoEntrenamiento Fallido(
            string mensaje)
        {
            return new ResultadoProgresoEntrenamiento(
                false,
                mensaje,
                0,
                false);
        }
    }
}
