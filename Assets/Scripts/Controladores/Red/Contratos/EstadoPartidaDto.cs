using System;

namespace ImperiosEnGuerra.Controladores.Red.Contratos
{
    [Serializable]
    public class EstadoPartidaDto
    {
        public string estado;
        public MapaEstadoDto mapa;
        public JugadorEstadoDto jugadorHumano;
        public JugadorEstadoDto jugadorMaquina;
    }

    [Serializable]
    public class MapaEstadoDto
    {
        public int ancho;
        public int alto;
        public RecursoEstadoDto[] recursos;
    }

    [Serializable]
    public class JugadorEstadoDto
    {
        public string nombre;
        public string tipo;
        public RecursosJugadorEstadoDto recursos;
        public EdificioEstadoDto[] edificios;
        public UnidadEstadoDto[] unidades;
    }

    [Serializable]
    public class RecursosJugadorEstadoDto
    {
        public int oro;
        public int madera;
        public int comida;
    }

    [Serializable]
    public class RecursoEstadoDto
    {
        public string tipo;
        public CoordenadaEstadoDto coordenada;
    }

    [Serializable]
    public class EdificioEstadoDto
    {
        public string tipo;
        public CoordenadaEstadoDto coordenada;
    }

    [Serializable]
    public class UnidadEstadoDto
    {
        public string tipo;
        public CoordenadaEstadoDto coordenada;
        public bool disponible;
    }

    [Serializable]
    public class CoordenadaEstadoDto
    {
        public int x;
        public int y;
    }
}
