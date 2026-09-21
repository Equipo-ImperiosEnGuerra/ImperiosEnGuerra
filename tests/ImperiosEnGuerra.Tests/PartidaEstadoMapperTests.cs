using ImperiosEnGuerra.Api.Mapeadores;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;
using NUnit.Framework;

namespace ImperiosEnGuerra.Tests.Editor
{
    public class PartidaEstadoMapperTests
    {
        [Test]
        public void Convertir_ExponeEstadoNecesarioParaLaVista()
        {
            Partida partida = CrearPartida();
            partida.JugadorHumano.Recursos.Agregar(TipoRecurso.Oro, 100);
            partida.JugadorHumano.Recursos.Agregar(TipoRecurso.Madera, 200);
            partida.JugadorHumano.Recursos.Agregar(TipoRecurso.Comida, 300);
            partida.JugadorMaquina.Recursos.Agregar(TipoRecurso.Oro, 40);
            partida.JugadorMaquina.Recursos.Agregar(TipoRecurso.Madera, 50);
            partida.JugadorMaquina.Recursos.Agregar(TipoRecurso.Comida, 60);
            partida.JugadorHumano.AgregarUnidad(new Guerrero(new Coordenada(2, 2)));
            var monje = new Monje(new Coordenada(7, 6));
            monje.MarcarNoDisponible();
            partida.JugadorMaquina.AgregarUnidad(monje);

            var respuesta = PartidaEstadoMapper.Convertir(partida);

            Assert.That(respuesta.Estado, Is.EqualTo("activa"));
            Assert.That(respuesta.Mapa.Ancho, Is.EqualTo(10));
            Assert.That(respuesta.Mapa.Alto, Is.EqualTo(8));
            Assert.That(respuesta.Mapa.Recursos, Has.Count.EqualTo(6));
            Assert.That(
                respuesta.Mapa.Recursos.All(
                    r => r.CantidadRestante == Recurso.CantidadInicialPredeterminada),
                Is.True);
            Assert.That(respuesta.Mapa.Recursos.Select(r => (r.Tipo, r.Coordenada.X, r.Coordenada.Y)),
                Is.EquivalentTo(new[]
                {
                    ("Oro", 1, 1), ("Madera", 2, 1), ("Comida", 3, 1),
                    ("Oro", 1, 5), ("Madera", 2, 5), ("Comida", 3, 5)
                }));

            var humano = respuesta.JugadorHumano;
            Assert.That(humano.Nombre, Is.EqualTo("Ana"));
            Assert.That(humano.Tipo, Is.EqualTo("Humano"));
            Assert.That(humano.Recursos.Oro, Is.EqualTo(100));
            Assert.That(humano.Recursos.Madera, Is.EqualTo(200));
            Assert.That(humano.Recursos.Comida, Is.EqualTo(300));
            Assert.That(humano.Edificios, Has.Count.EqualTo(1));
            Assert.That(humano.Edificios[0].Tipo, Is.EqualTo("CentroUrbano"));
            Assert.That(humano.Edificios[0].Coordenada.X, Is.EqualTo(0));
            Assert.That(humano.Edificios[0].Coordenada.Y, Is.EqualTo(0));
            Assert.That(humano.Edificios[0].ColaEntrenamiento, Is.Empty);
            Assert.That(humano.ObrasConstruccion, Is.Empty);

            Assert.That(respuesta.Economia.CentroUrbano.Oro, Is.EqualTo(20));
            Assert.That(respuesta.Economia.CentroUrbano.Madera, Is.EqualTo(50));
            Assert.That(respuesta.Economia.Guerrero.Oro, Is.EqualTo(5));
            Assert.That(respuesta.Economia.Guerrero.Comida, Is.EqualTo(15));
            Assert.That(respuesta.Economia.Guerrero.Madera, Is.Zero);

            Assert.That(humano.Unidades, Has.Count.EqualTo(3));
            var guerreroEstado = humano.Unidades.Single(u => u.Tipo == "Guerrero");
            Assert.That(guerreroEstado.Coordenada.X, Is.EqualTo(2));
            Assert.That(guerreroEstado.Coordenada.Y, Is.EqualTo(2));
            Assert.That(guerreroEstado.Disponible, Is.True);

            var maquina = respuesta.JugadorMaquina;
            Assert.That(maquina.Nombre, Is.EqualTo("Máquina"));
            Assert.That(maquina.Tipo, Is.EqualTo("Maquina"));
            Assert.That(maquina.Recursos.Oro, Is.EqualTo(40));
            Assert.That(maquina.Recursos.Madera, Is.EqualTo(50));
            Assert.That(maquina.Recursos.Comida, Is.EqualTo(60));
            Assert.That(maquina.Edificios, Has.Count.EqualTo(1));
            Assert.That(maquina.Edificios[0].Tipo, Is.EqualTo("CentroUrbano"));
            Assert.That(maquina.Edificios[0].Coordenada.X, Is.EqualTo(9));
            Assert.That(maquina.Edificios[0].Coordenada.Y, Is.EqualTo(7));
            Assert.That(maquina.Unidades, Has.Count.EqualTo(3));
            var monjeEstado = maquina.Unidades.Single(u => u.Tipo == "Monje");
            Assert.That(monjeEstado.Coordenada.X, Is.EqualTo(7));
            Assert.That(monjeEstado.Coordenada.Y, Is.EqualTo(6));
            Assert.That(monjeEstado.Disponible, Is.False);
        }

        [Test]
        public void Convertir_PartidaNula_LanzaExcepcion()
        {
            Assert.Throws<ArgumentNullException>(() => PartidaEstadoMapper.Convertir(null));
        }

        [Test]
        public void Convertir_UnidadSinCoordenada_ConservaNull()
        {
            Partida partida = CrearPartida();
            partida.JugadorHumano.AgregarUnidad(new Aldeano(null));

            var respuesta = PartidaEstadoMapper.Convertir(partida);

            var unidadSinCoordenada =
                respuesta.JugadorHumano.Unidades.Single(
                    u => u.Coordenada == null);

            Assert.That(unidadSinCoordenada.Tipo, Is.EqualTo("Aldeano"));
            Assert.That(unidadSinCoordenada.Disponible, Is.True);
            Assert.That(
                respuesta.JugadorMaquina.Unidades.Count,
                Is.EqualTo(ConfiguracionInicioPartida.AldeanosInicialesPredeterminados));
        }

        [Test]
        public void Convertir_MapasDistintos_ExponeSoloMapaHumano()
        {
            var mapaHumano = new Mapa(10, 8);
            var mapaMaquina = new Mapa(12, 9);
            Partida partida = new InicializadorPartida().Crear(
                "Ana", mapaHumano, new Coordenada(0, 0), CrearRecursos(1),
                "Máquina", mapaMaquina, new Coordenada(9, 7), CrearRecursos(5));

            var respuesta = PartidaEstadoMapper.Convertir(partida);

            Assert.That(respuesta.Mapa.Ancho, Is.EqualTo(10));
            Assert.That(respuesta.Mapa.Alto, Is.EqualTo(8));
            Assert.That(respuesta.Mapa.Recursos, Has.Count.EqualTo(3));
            Assert.That(respuesta.Mapa.Recursos.All(r => r.Coordenada.Y == 1), Is.True);
        }

        private static Partida CrearPartida()
        {
            var mapa = new Mapa(10, 8);
            return new InicializadorPartida().Crear(
                "Ana", mapa, new Coordenada(0, 0), CrearRecursos(1),
                "Máquina", mapa, new Coordenada(9, 7), CrearRecursos(5));
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
