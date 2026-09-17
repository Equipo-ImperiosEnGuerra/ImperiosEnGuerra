namespace ImperiosEnGuerra.Modelo.Acciones
{
    /// <summary>Resultado de una acción, independiente del transporte y de Unity.</summary>
    public class ResultadoAccion
    {
        public bool Exito { get; }
        public string Mensaje { get; }

        private ResultadoAccion(bool exito, string mensaje)
        {
            Exito = exito;
            Mensaje = mensaje;
        }

        public static ResultadoAccion Exitoso(string mensaje)
        {
            return new ResultadoAccion(true, mensaje);
        }

        public static ResultadoAccion Fallido(string mensaje)
        {
            return new ResultadoAccion(false, mensaje);
        }
    }
}
