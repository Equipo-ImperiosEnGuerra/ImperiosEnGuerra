using NUnit.Framework;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Tests.Editor
{
    public class EstadoUnidadTests
    {
        [Test]
        public void UnidadNueva_IniciaIdleYSinOrden()
        {
            Aldeano unidad = new Aldeano(new Coordenada(1, 1));

            Assert.That(unidad.Estado, Is.EqualTo(EstadoUnidad.Idle));
            Assert.That(unidad.OrdenActiva, Is.Null);
            Assert.That(unidad.Disponible, Is.True);
        }

        [TestCase(TipoAccionJuego.Mover, EstadoUnidad.Moviendo)]
        [TestCase(TipoAccionJuego.Recolectar, EstadoUnidad.Recolectando)]
        [TestCase(TipoAccionJuego.Construir, EstadoUnidad.Construyendo)]
        public void OrdenValida_CambiaEstado(
            TipoAccionJuego orden,
            EstadoUnidad estadoEsperado)
        {
            Aldeano unidad = new Aldeano(new Coordenada(1, 1));

            bool iniciada = unidad.IntentarIniciarOrden(orden);

            Assert.That(iniciada, Is.True);
            Assert.That(unidad.OrdenActiva, Is.EqualTo(orden));
            Assert.That(unidad.Estado, Is.EqualTo(estadoEsperado));
            Assert.That(unidad.Disponible, Is.False);
        }

        [Test]
        public void SegundaOrdenSinReemplazo_EsRechazada()
        {
            Aldeano unidad = new Aldeano(new Coordenada(1, 1));
            Assert.That(unidad.IntentarIniciarOrden(TipoAccionJuego.Mover), Is.True);

            bool segunda = unidad.IntentarIniciarOrden(TipoAccionJuego.Recolectar);

            Assert.That(segunda, Is.False);
            Assert.That(unidad.OrdenActiva, Is.EqualTo(TipoAccionJuego.Mover));
            Assert.That(unidad.Estado, Is.EqualTo(EstadoUnidad.Moviendo));
        }

        [Test]
        public void CancelarOrden_VuelveAIdle()
        {
            Aldeano unidad = new Aldeano(new Coordenada(1, 1));
            unidad.IntentarIniciarOrden(TipoAccionJuego.Construir);

            unidad.CancelarOrden();

            Assert.That(unidad.Estado, Is.EqualTo(EstadoUnidad.Idle));
            Assert.That(unidad.OrdenActiva, Is.Null);
            Assert.That(unidad.Disponible, Is.True);
        }

        [Test]
        public void ReemplazarOrden_NoDejaEstadoHuerfano()
        {
            Aldeano unidad = new Aldeano(new Coordenada(1, 1));
            unidad.IntentarIniciarOrden(TipoAccionJuego.Mover);

            bool reemplazada =
                unidad.IntentarReemplazarOrden(TipoAccionJuego.Recolectar);

            Assert.That(reemplazada, Is.True);
            Assert.That(unidad.OrdenActiva, Is.EqualTo(TipoAccionJuego.Recolectar));
            Assert.That(unidad.Estado, Is.EqualTo(EstadoUnidad.Recolectando));
            Assert.That(unidad.Disponible, Is.False);

            unidad.CompletarOrden();

            Assert.That(unidad.Estado, Is.EqualTo(EstadoUnidad.Idle));
            Assert.That(unidad.OrdenActiva, Is.Null);
            Assert.That(unidad.Disponible, Is.True);
        }

        [TestCase(TipoAccionJuego.Entrenar)]
        [TestCase(TipoAccionJuego.Atacar)]
        public void OrdenSinEstadoDefinido_EsRechazada(TipoAccionJuego orden)
        {
            Aldeano unidad = new Aldeano(new Coordenada(1, 1));

            bool iniciada = unidad.IntentarIniciarOrden(orden);

            Assert.That(iniciada, Is.False);
            Assert.That(unidad.Estado, Is.EqualTo(EstadoUnidad.Idle));
            Assert.That(unidad.OrdenActiva, Is.Null);
        }
    }
}
