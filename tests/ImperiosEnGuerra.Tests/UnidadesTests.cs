using System;
using NUnit.Framework;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Tests.Editor
{
    public class UnidadesTests
    {
        [Test]
        public void Guerrero_EsSoldadoYConservaCoordenada()
        {
            Coordenada coordenada = new Coordenada(1, 2);

            Guerrero guerrero = new Guerrero(coordenada);

            Assert.That(guerrero, Is.InstanceOf<Soldado>());
            Assert.That(guerrero.Coordenada, Is.SameAs(coordenada));
            Assert.That(guerrero.Disponible, Is.True);
        }

        [Test]
        public void Lancero_EsSoldadoYConservaCoordenada()
        {
            Coordenada coordenada = new Coordenada(2, 3);

            Lancero lancero = new Lancero(coordenada);

            Assert.That(lancero, Is.InstanceOf<Soldado>());
            Assert.That(lancero.Coordenada, Is.SameAs(coordenada));
            Assert.That(lancero.Disponible, Is.True);
        }

        [Test]
        public void Arquero_ContinuaSiendoSoldado()
        {
            Coordenada coordenada = new Coordenada(3, 4);

            Arquero arquero = new Arquero(coordenada);

            Assert.That(arquero, Is.InstanceOf<Soldado>());
            Assert.That(arquero.Coordenada, Is.SameAs(coordenada));
            Assert.That(arquero.Disponible, Is.True);
        }

        [Test]
        public void Monje_EsUnidadPeroNoSoldado()
        {
            Coordenada coordenada = new Coordenada(4, 5);

            Monje monje = new Monje(coordenada);

            Assert.That(monje, Is.InstanceOf<Unidad>());
            Assert.That(monje, Is.Not.InstanceOf<Soldado>());
            Assert.That(monje.Coordenada, Is.SameAs(coordenada));
            Assert.That(monje.Disponible, Is.True);
        }

        [Test]
        public void VelocidadesMovimiento_SonPositivas()
        {
            Unidad[] unidades =
            {
                new Aldeano(new Coordenada(0, 0)),
                new Guerrero(new Coordenada(0, 0)),
                new Lancero(new Coordenada(0, 0)),
                new Arquero(new Coordenada(0, 0)),
                new Monje(new Coordenada(0, 0))
            };

            foreach (Unidad unidad in unidades)
            {
                Assert.That(
                    unidad.VelocidadMovimiento,
                    Is.GreaterThan(0d));
            }
        }

        [Test]
        public void VelocidadMovimiento_PuedeVariarSegunTipo()
        {
            var aldeano =
                new Aldeano(new Coordenada(0, 0));

            var lancero =
                new Lancero(new Coordenada(0, 0));

            var monje =
                new Monje(new Coordenada(0, 0));

            Assert.That(
                lancero.VelocidadMovimiento,
                Is.GreaterThan(
                    aldeano.VelocidadMovimiento));

            Assert.That(
                monje.VelocidadMovimiento,
                Is.LessThan(
                    aldeano.VelocidadMovimiento));
        }

        [Test]
        public void UnidadesDistintas_TienenIdsDistintosYNoVacios()
        {
            var primera = new Aldeano(new Coordenada(1, 1));
            var segunda = new Aldeano(new Coordenada(2, 2));

            Assert.That(primera.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(segunda.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(primera.Id, Is.Not.EqualTo(segunda.Id));
        }

        [Test]
        public void Id_PermaneceAlCambiarCoordenada()
        {
            var unidad = new UnidadPrueba(new Coordenada(1, 1));
            Guid idOriginal = unidad.Id;

            unidad.CambiarCoordenada(new Coordenada(4, 5));

            Assert.That(unidad.Id, Is.EqualTo(idOriginal));
            Assert.That(unidad.Coordenada.X, Is.EqualTo(4));
            Assert.That(unidad.Coordenada.Y, Is.EqualTo(5));
        }

        private sealed class UnidadPrueba : Unidad
        {
            public UnidadPrueba(Coordenada coordenada) : base(coordenada)
            {
            }

            public void CambiarCoordenada(Coordenada coordenada)
            {
                Coordenada = coordenada;
            }
        }
    }
}
