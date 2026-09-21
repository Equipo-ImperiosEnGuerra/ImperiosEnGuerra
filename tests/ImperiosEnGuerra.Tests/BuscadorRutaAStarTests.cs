using System.Linq;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Movimiento;
using ImperiosEnGuerra.Modelo.Recursos;

namespace ImperiosEnGuerra.Tests;

public class BuscadorRutaAStarTests
{
    private BuscadorRutaAStar buscador;
    private Mapa mapa;

    [SetUp]
    public void Preparar()
    {
        buscador =
            new BuscadorRutaAStar();

        mapa =
            new Mapa(6, 6);
    }

    [Test]
    public void RutaDirecta_EncuentraCamino()
    {
        ResultadoRuta resultado =
            buscador.Buscar(
                mapa,
                new Coordenada(1, 1),
                new Coordenada(4, 1));

        Assert.That(
            resultado.Encontrada,
            Is.True);

        Assert.That(
            resultado.Pasos.Count,
            Is.EqualTo(3));

        Assert.That(
            resultado.Pasos.Last().X,
            Is.EqualTo(4));

        Assert.That(
            resultado.Pasos.Last().Y,
            Is.EqualTo(1));
    }

    [Test]
    public void OrigenIgualDestino_RutaVacia()
    {
        ResultadoRuta resultado =
            buscador.Buscar(
                mapa,
                new Coordenada(2, 2),
                new Coordenada(2, 2));

        Assert.That(
            resultado.Encontrada,
            Is.True);

        Assert.That(
            resultado.Pasos,
            Is.Empty);
    }

    [Test]
    public void Obstaculo_ObligaARodear()
    {
        mapa.ObtenerCasilla(
            2,
            1)
            .CambiarTransitabilidad(false);

        ResultadoRuta resultado =
            buscador.Buscar(
                mapa,
                new Coordenada(1, 1),
                new Coordenada(3, 1));

        Assert.That(
            resultado.Encontrada,
            Is.True);

        Assert.That(
            resultado.Pasos.Count,
            Is.EqualTo(4));

        Assert.That(
            resultado.Pasos.Any(
                paso =>
                    paso.X == 2 &&
                    paso.Y == 1),
            Is.False);
    }

    [Test]
    public void CasillaOcupada_NoSeAtraviesa()
    {
        mapa.ObtenerCasilla(
            2,
            1)
            .Ocupar();

        ResultadoRuta resultado =
            buscador.Buscar(
                mapa,
                new Coordenada(1, 1),
                new Coordenada(3, 1));

        Assert.That(
            resultado.Encontrada,
            Is.True);

        Assert.That(
            resultado.Pasos.Any(
                paso =>
                    paso.X == 2 &&
                    paso.Y == 1),
            Is.False);
    }

    [Test]
    public void Recurso_NoSeAtraviesa()
    {
        mapa.ColocarRecurso(
            new Recurso(
                TipoRecurso.Oro,
                new Coordenada(2, 1)));

        ResultadoRuta resultado =
            buscador.Buscar(
                mapa,
                new Coordenada(1, 1),
                new Coordenada(3, 1));

        Assert.That(
            resultado.Encontrada,
            Is.True);

        Assert.That(
            resultado.Pasos.Any(
                paso =>
                    paso.X == 2 &&
                    paso.Y == 1),
            Is.False);
    }

    [Test]
    public void DestinoBloqueado_RutaImposible()
    {
        mapa.ObtenerCasilla(
            3,
            1)
            .CambiarTransitabilidad(false);

        ResultadoRuta resultado =
            buscador.Buscar(
                mapa,
                new Coordenada(1, 1),
                new Coordenada(3, 1));

        Assert.That(
            resultado.Encontrada,
            Is.False);

        Assert.That(
            resultado.Pasos,
            Is.Empty);
    }

    [Test]
    public void Encerrado_RutaImposible()
    {
        mapa.ObtenerCasilla(
            0,
            1)
            .CambiarTransitabilidad(false);

        mapa.ObtenerCasilla(
            2,
            1)
            .CambiarTransitabilidad(false);

        mapa.ObtenerCasilla(
            1,
            0)
            .CambiarTransitabilidad(false);

        mapa.ObtenerCasilla(
            1,
            2)
            .CambiarTransitabilidad(false);

        ResultadoRuta resultado =
            buscador.Buscar(
                mapa,
                new Coordenada(1, 1),
                new Coordenada(4, 4));

        Assert.That(
            resultado.Encontrada,
            Is.False);
    }

    [TestCase(-1, 0)]
    [TestCase(0, -1)]
    [TestCase(6, 0)]
    [TestCase(0, 6)]
    public void DestinoFueraDelMapa_RutaImposible(
        int x,
        int y)
    {
        ResultadoRuta resultado =
            buscador.Buscar(
                mapa,
                new Coordenada(1, 1),
                new Coordenada(x, y));

        Assert.That(
            resultado.Encontrada,
            Is.False);
    }
}
