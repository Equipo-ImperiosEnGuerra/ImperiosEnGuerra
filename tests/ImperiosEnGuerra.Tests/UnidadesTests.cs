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
    }
}
