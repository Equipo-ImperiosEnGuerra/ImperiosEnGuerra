using System;

namespace ImperiosEnGuerra.Controladores.Red.Contratos
{
    [Serializable]
    public class IniciarPartidaDto
    {
        public string nombreHumano;
        public string nombreMaquina;

        public int anchoMapa;
        public int altoMapa;

        public CoordenadaDto centroHumano;
        public CoordenadaDto centroMaquina;

        public RecursoInicialDto[] recursosHumano;
        public RecursoInicialDto[] recursosMaquina;
    }

    [Serializable]
    public class CoordenadaDto
    {
        public int x;
        public int y;

        public CoordenadaDto(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    [Serializable]
    public class RecursoInicialDto
    {
        public string tipo;
        public int x;
        public int y;

        public RecursoInicialDto(string tipo, int x, int y)
        {
            this.tipo = tipo;
            this.x = x;
            this.y = y;
        }
    }
}