using ImperiosEnGuerra.Modelo.Recursos;

namespace ImperiosEnGuerra.Tests;

public class ConfiguracionRecoleccionTests
{
    [Test]
    public void TasasPuedenConfigurarsePorTipo()
    {
        var configuracion =
            new ConfiguracionRecoleccion(
                tasaOro: 2,
                tasaMadera: 3,
                tasaComida: 4);

        Assert.That(
            configuracion.ObtenerTasa(
                TipoRecurso.Oro),
            Is.EqualTo(2));

        Assert.That(
            configuracion.ObtenerTasa(
                TipoRecurso.Madera),
            Is.EqualTo(3));

        Assert.That(
            configuracion.ObtenerTasa(
                TipoRecurso.Comida),
            Is.EqualTo(4));
    }

    [TestCase(0, 1, 1)]
    [TestCase(1, 0, 1)]
    [TestCase(1, 1, 0)]
    [TestCase(-1, 1, 1)]
    public void TasaNoPositiva_Lanza(
        int oro,
        int madera,
        int comida)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new ConfiguracionRecoleccion(
                    oro,
                    madera,
                    comida));
    }

    [Test]
    public void TipoNoConfigurado_Lanza()
    {
        var configuracion =
            new ConfiguracionRecoleccion();

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                configuracion.ObtenerTasa(
                    (TipoRecurso)999));
    }
}
