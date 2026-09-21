using ImperiosEnGuerra.Api.Mapeadores;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;
using NUnit.Framework;

namespace ImperiosEnGuerra.Tests.Editor
{
    public class IdentidadUnidadMapperTests
    {
        [Test]
        public void Convertir_ConservaIdEstableOriginadoEnModelo()
        {
            var mapa = new Mapa(6, 6);
            Partida partida = new InicializadorPartida().Crear(
                "Ana", mapa, new Coordenada(0, 0), CrearRecursos(1),
                "Máquina", mapa, new Coordenada(5, 5), CrearRecursos(4));
            var unidad = new Guerrero(new Coordenada(2, 2));
            partida.JugadorHumano.AgregarUnidad(unidad);

            var primeraRespuesta = PartidaEstadoMapper.Convertir(partida);
            var segundaRespuesta = PartidaEstadoMapper.Convertir(partida);

            string idEsperado = unidad.Id.ToString("D");
            Assert.That(
                primeraRespuesta.JugadorHumano.Unidades.Any(u => u.Id == idEsperado),
                Is.True);
            Assert.That(
                segundaRespuesta.JugadorHumano.Unidades.Any(u => u.Id == idEsperado),
                Is.True);
        }

        private static IReadOnlyList<Recurso> CrearRecursos(int y)
        {
            return new List<Recurso>
            {
                new Recurso(TipoRecurso.Oro, new Coordenada(1, y)),
                new Recurso(TipoRecurso.Madera, new Coordenada(2, y)),
                new Recurso(TipoRecurso.Comida, new Coordenada(3, y))
            };
        }
    }
}
