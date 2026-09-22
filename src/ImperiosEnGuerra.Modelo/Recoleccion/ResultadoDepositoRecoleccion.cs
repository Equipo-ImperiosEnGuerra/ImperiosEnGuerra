using ImperiosEnGuerra.Modelo.Recursos;

namespace ImperiosEnGuerra.Modelo.Recoleccion
{
    public sealed class ResultadoDepositoRecoleccion
    {
        public bool Exito { get; }
        public string Mensaje { get; }
        public int CantidadDepositada { get; }
        public TipoRecurso? TipoRecurso { get; }

        private ResultadoDepositoRecoleccion(
            bool exito,
            string mensaje,
            int cantidadDepositada,
            TipoRecurso? tipoRecurso)
        {
            Exito = exito;
            Mensaje = mensaje ?? string.Empty;
            CantidadDepositada = cantidadDepositada;
            TipoRecurso = tipoRecurso;
        }

        public static ResultadoDepositoRecoleccion Exitoso(
            int cantidad,
            TipoRecurso tipo)
        {
            return new ResultadoDepositoRecoleccion(
                true,
                $"Se depositaron {cantidad} de {tipo}.",
                cantidad,
                tipo);
        }

        public static ResultadoDepositoRecoleccion Fallido(
            string mensaje)
        {
            return new ResultadoDepositoRecoleccion(
                false,
                mensaje,
                0,
                null);
        }
    }
}
