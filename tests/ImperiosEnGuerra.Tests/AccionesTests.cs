using ImperiosEnGuerra.Modelo.Acciones;
using NUnit.Framework;

namespace ImperiosEnGuerra.Tests
{
    public class AccionesTests
    {
        [TestCase("Acción aceptada")]
        [TestCase("")]
        public void ResultadoExitoso_ConservaMensaje(string mensaje)
        {
            var resultado = ResultadoAccion.Exitoso(mensaje);
            Assert.That(resultado.Exito, Is.True);
            Assert.That(resultado.Mensaje, Is.EqualTo(mensaje));
        }

        [TestCase("Acción rechazada")]
        [TestCase("")]
        public void ResultadoFallido_ConservaMensaje(string mensaje)
        {
            var resultado = ResultadoAccion.Fallido(mensaje);
            Assert.That(resultado.Exito, Is.False);
            Assert.That(resultado.Mensaje, Is.EqualTo(mensaje));
        }

        [TestCase(TipoAccionJuego.Mover)]
        [TestCase(TipoAccionJuego.Recolectar)]
        [TestCase(TipoAccionJuego.Construir)]
        [TestCase(TipoAccionJuego.Entrenar)]
        [TestCase(TipoAccionJuego.Atacar)]
        public void Solicitud_ConservaTipo(TipoAccionJuego tipo)
        {
            Assert.That(new SolicitudAccion(tipo).Tipo, Is.EqualTo(tipo));
        }
    }
}
