using ImperiosEnGuerra.Modelo.Recursos;

namespace ImperiosEnGuerra.Tests;

public class EconomiaTests
{
    [Test]
    public void GastoCompuesto_EsAtomico()
    {
        var recursos = new RecursosJugador();
        recursos.Agregar(TipoRecurso.Oro, 20);
        recursos.Agregar(TipoRecurso.Madera, 30);
        recursos.Agregar(TipoRecurso.Comida, 10);

        var costo =
            new CostoRecursos(20, 30, 10);

        Assert.That(
            recursos.IntentarGastar(costo),
            Is.True);

        Assert.That(
            recursos.ObtenerCantidad(TipoRecurso.Oro),
            Is.Zero);
        Assert.That(
            recursos.ObtenerCantidad(TipoRecurso.Madera),
            Is.Zero);
        Assert.That(
            recursos.ObtenerCantidad(TipoRecurso.Comida),
            Is.Zero);
    }

    [Test]
    public void GastoCompuesto_Insuficiente_NoDescuentaNada()
    {
        var recursos = new RecursosJugador();
        recursos.Agregar(TipoRecurso.Oro, 20);
        recursos.Agregar(TipoRecurso.Madera, 29);
        recursos.Agregar(TipoRecurso.Comida, 10);

        Assert.That(
            recursos.IntentarGastar(
                new CostoRecursos(20, 30, 10)),
            Is.False);

        Assert.That(
            recursos.ObtenerCantidad(TipoRecurso.Oro),
            Is.EqualTo(20));
        Assert.That(
            recursos.ObtenerCantidad(TipoRecurso.Madera),
            Is.EqualTo(29));
        Assert.That(
            recursos.ObtenerCantidad(TipoRecurso.Comida),
            Is.EqualTo(10));
    }

    [Test]
    public async Task DosGastosConcurrentes_ConSaldoParaUno_SoloUnoGana()
    {
        var recursos = new RecursosJugador();
        recursos.Agregar(TipoRecurso.Oro, 20);
        recursos.Agregar(TipoRecurso.Madera, 30);

        var costo =
            new CostoRecursos(20, 30, 0);

        bool[] resultados =
            await Task.WhenAll(
                Task.Run(() => recursos.IntentarGastar(costo)),
                Task.Run(() => recursos.IntentarGastar(costo)));

        Assert.That(
            resultados.Count(x => x),
            Is.EqualTo(1));

        Assert.That(
            recursos.ObtenerCantidad(TipoRecurso.Oro),
            Is.Zero);

        Assert.That(
            recursos.ObtenerCantidad(TipoRecurso.Madera),
            Is.Zero);
    }

    [Test]
    public void ConfiguracionEconomia_DefineSoloTiposActuales()
    {
        var config =
            new ConfiguracionEconomia();

        Assert.That(
            config.IntentarObtenerCostoEdificio(
                "CentroUrbano",
                out CostoRecursos edificio),
            Is.True);

        Assert.That(
            edificio.EsCero,
            Is.False);

        foreach (string tipo in new[]
        {
            "Aldeano",
            "Guerrero",
            "Lancero",
            "Arquero",
            "Monje"
        })
        {
            Assert.That(
                config.IntentarObtenerCostoUnidad(
                    tipo,
                    out CostoRecursos costo),
                Is.True);

            Assert.That(
                costo.EsCero,
                Is.False);
        }
    }
}
