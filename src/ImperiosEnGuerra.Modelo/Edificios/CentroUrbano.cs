using System;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Edificios
{
    public class CentroUrbano : Edificio
    {
        private bool entrenando;
        private string? tipoUnidadEntrenando;


        public bool EstaEntrenando => entrenando;


        public string? TipoUnidadEntrenando =>
            tipoUnidadEntrenando;


        public CentroUrbano(Coordenada coordenada)
            : base(coordenada)
        {
            entrenando = false;
            tipoUnidadEntrenando = null;
        }


        public bool IniciarEntrenamiento(string tipoUnidad)
        {
            if (entrenando)
                return false;

            if (string.IsNullOrWhiteSpace(tipoUnidad))
                return false;


            entrenando = true;
            tipoUnidadEntrenando = tipoUnidad;

            return true;
        }


        public void CompletarEntrenamiento()
        {
            entrenando = false;
            tipoUnidadEntrenando = null;
        }
    }
}