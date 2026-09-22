using ImperiosEnGuerra.Modelo.Recursos;

namespace ImperiosEnGuerra.Modelo.Recoleccion
{
    /// <summary>
    /// Resultado de un ciclo lógico de extracción desde un nodo hacia la carga del Aldeano.
    /// </summary>
    public sealed class ResultadoPasoRecoleccion
    {
        public bool Exito { get; }
        public string Mensaje { get; }
        public int CantidadExtraida { get; }
        public int CargaActual { get; }
        public int CapacidadCarga { get; }
        public bool CapacidadCompleta { get; }
        public bool RecursoAgotado { get; }
        public TipoRecurso? TipoRecurso { get; }

        private ResultadoPasoRecoleccion(
            bool exito,
            string mensaje,
            int cantidadExtraida,
            int cargaActual,
            int capacidadCarga,
            bool capacidadCompleta,
            bool recursoAgotado,
            TipoRecurso? tipoRecurso)
        {
            Exito = exito;
            Mensaje = mensaje ?? string.Empty;
            CantidadExtraida = cantidadExtraida;
            CargaActual = cargaActual;
            CapacidadCarga = capacidadCarga;
            CapacidadCompleta = capacidadCompleta;
            RecursoAgotado = recursoAgotado;
            TipoRecurso = tipoRecurso;
        }

        public static ResultadoPasoRecoleccion Exitoso(
            int cantidadExtraida,
            int cargaActual,
            int capacidadCarga,
            bool recursoAgotado,
            TipoRecurso tipoRecurso)
        {
            return new ResultadoPasoRecoleccion(
                true,
                $"Se recolectaron {cantidadExtraida} de {tipoRecurso}.",
                cantidadExtraida,
                cargaActual,
                capacidadCarga,
                cargaActual >= capacidadCarga,
                recursoAgotado,
                tipoRecurso);
        }

        public static ResultadoPasoRecoleccion Fallido(
            string mensaje)
        {
            return new ResultadoPasoRecoleccion(
                false,
                mensaje,
                0,
                0,
                0,
                false,
                false,
                null);
        }
    }
}
