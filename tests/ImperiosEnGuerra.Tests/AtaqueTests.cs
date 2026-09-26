using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Api.Servicios;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Combate;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Tests;

public class AtaqueTests
{
    private Partida partida;
    private Guerrero atacante;
    private Lancero objetivo;
    private OperacionAtaque operacion;

    [SetUp]
    public void Preparar()
    {
        var mapa = new Mapa(8, 8);

        partida = new Partida(
            new Jugador("Humano", TipoJugador.Humano, mapa, new RecursosJugador()),
            new Jugador("Máquina", TipoJugador.Maquina, mapa, new RecursosJugador()));

        atacante = new Guerrero(new Coordenada(1, 1));
        objetivo = new Lancero(new Coordenada(2, 1));

        partida.JugadorHumano.AgregarUnidad(atacante);
        partida.JugadorMaquina.AgregarUnidad(objetivo);

        operacion = new OperacionAtaque();
    }

    [Test]
    public void PrimerImpacto_ReduceVidaSinDestruir()
    {
        ResultadoAccion resultado = operacion.Ejecutar(
            partida,
            new SolicitudAtaque(atacante.Id, objetivo.Id));

        Assert.That(resultado.Exito, Is.True, resultado.Mensaje);
        Assert.That(objetivo.VidaActual, Is.EqualTo(70));
        Assert.That(partida.JugadorMaquina.Unidades, Does.Contain(objetivo));
        Assert.That(resultado.Mensaje, Does.Contain("70/100"));
    }

    [Test]
    public void ImpactosRepetidos_DestruyenSoloAlLlegarACero()
    {
        AtacarHastaDestruir(
            partida,
            atacante.Id,
            objetivo.Id,
            () => partida.JugadorMaquina.Unidades.Contains(objetivo));

        Assert.That(objetivo.VidaActual, Is.Zero);
        Assert.That(partida.JugadorMaquina.Unidades, Does.Not.Contain(objetivo));
    }

    [Test]
    public void Arquero_PuedeAtacarATresCasillas()
    {
        var arquero = new Arquero(new Coordenada(1, 4));
        var enemigo = new Guerrero(new Coordenada(4, 4));

        partida.JugadorHumano.AgregarUnidad(arquero);
        partida.JugadorMaquina.AgregarUnidad(enemigo);

        ResultadoAccion resultado = operacion.Ejecutar(
            partida,
            new SolicitudAtaque(arquero.Id, enemigo.Id));

        Assert.That(resultado.Exito, Is.True, resultado.Mensaje);
        Assert.That(enemigo.VidaActual, Is.EqualTo(100));
        Assert.That(arquero.AlcanceAtaque, Is.EqualTo(3));
    }

    [Test]
    public void Guerrero_PuedeAtacarEnDiagonalAdyacente()
    {
        var diagonal =
            new Guerrero(
                new Coordenada(2, 2));

        partida.JugadorMaquina.AgregarUnidad(
            diagonal);

        ResultadoAccion resultado =
            operacion.Ejecutar(
                partida,
                new SolicitudAtaque(
                    atacante.Id,
                    diagonal.Id));

        Assert.That(
            resultado.Exito,
            Is.True,
            resultado.Mensaje);

        Assert.That(
            diagonal.VidaActual,
            Is.EqualTo(90));
    }

    [Test]
    public void Arquero_AlcanceTres_IncluyeDiagonal()
    {
        var arquero =
            new Arquero(
                new Coordenada(1, 1));

        var enemigo =
            new Guerrero(
                new Coordenada(4, 4));

        partida.JugadorHumano.AgregarUnidad(
            arquero);

        partida.JugadorMaquina.AgregarUnidad(
            enemigo);

        ResultadoAccion resultado =
            operacion.Ejecutar(
                partida,
                new SolicitudAtaque(
                    arquero.Id,
                    enemigo.Id));

        Assert.That(
            resultado.Exito,
            Is.True,
            resultado.Mensaje);

        Assert.That(
            enemigo.VidaActual,
            Is.EqualTo(100));
    }

    [Test]
    public void Guerrero_NoPuedeAtacarADosCasillas()
    {
        var lejano = new Guerrero(new Coordenada(3, 1));
        partida.JugadorMaquina.AgregarUnidad(lejano);

        ResultadoAccion resultado = operacion.Ejecutar(
            partida,
            new SolicitudAtaque(atacante.Id, lejano.Id));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(resultado.Mensaje, Does.Contain("fuera de alcance"));
        Assert.That(lejano.VidaActual, Is.EqualTo(lejano.VidaMaxima));
    }

    [Test]
    public void CentroUrbano_RequiereVariosImpactos()
    {
        var centro = new CentroUrbano(new Coordenada(1, 2));
        partida.JugadorMaquina.AgregarEdificio(centro);
        partida.JugadorMaquina.Mapa.ObtenerCasilla(1, 2).Ocupar();

        ResultadoAccion primerImpacto = operacion.Ejecutar(
            partida,
            new SolicitudAtaque(atacante.Id, centro.Id));

        Assert.That(primerImpacto.Exito, Is.True, primerImpacto.Mensaje);
        Assert.That(centro.VidaActual, Is.EqualTo(270));
        Assert.That(partida.JugadorMaquina.Edificios, Does.Contain(centro));

        AtacarHastaDestruir(
            partida,
            atacante.Id,
            centro.Id,
            () => partida.JugadorMaquina.Edificios.Contains(centro));

        Assert.That(centro.VidaActual, Is.Zero);
        Assert.That(partida.JugadorMaquina.Edificios, Does.Not.Contain(centro));
        Assert.That(partida.JugadorMaquina.Mapa.ObtenerCasilla(1, 2).EstaOcupada, Is.False);
    }

    [Test]
    public void Aldeano_NoPuedeAtacar()
    {
        var aldeano = new Aldeano(new Coordenada(2, 2));
        partida.JugadorHumano.AgregarUnidad(aldeano);

        ResultadoAccion resultado = operacion.Ejecutar(
            partida,
            new SolicitudAtaque(aldeano.Id, objetivo.Id));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(resultado.Mensaje, Does.Contain("ofensiva"));
    }

    [Test]
    public void ObjetivoPropio_FallaYConservaVida()
    {
        var aliado = new Arquero(new Coordenada(1, 2));
        partida.JugadorHumano.AgregarUnidad(aliado);

        ResultadoAccion resultado = operacion.Ejecutar(
            partida,
            new SolicitudAtaque(atacante.Id, aliado.Id));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(aliado.VidaActual, Is.EqualTo(aliado.VidaMaxima));
    }

    [Test]
    public void Servicio_AtaqueValido_ExponeVidaEnSnapshot()
    {
        var servicio = CrearServicio();

        ResultadoAccion resultado = servicio.Atacar(
            new AtacarRequest
            {
                AtacanteId = atacante.Id.ToString("D"),
                ObjetivoId = objetivo.Id.ToString("D")
            });

        Assert.That(resultado.Exito, Is.True);

        var estado = servicio.ObtenerEstado();
        var unidad = estado!.JugadorMaquina.Unidades.Single(u => u.Id == objetivo.Id.ToString("D"));

        Assert.That(unidad.VidaActual, Is.EqualTo(70));
        Assert.That(unidad.VidaMaxima, Is.EqualTo(100));
        Assert.That(unidad.Danio, Is.EqualTo(25));
        Assert.That(unidad.Alcance, Is.EqualTo(1));
    }

    [Test]
    public void ConfiguracionCombate_ValoresDelPrototipoSonCentralizados()
    {
        var config = new ConfiguracionCombate();

        EstadisticasCombate guerrero = config.Obtener("Guerrero");
        EstadisticasCombate arquero = config.Obtener("Arquero");
        EstadisticasCombate centro = config.Obtener("CentroUrbano");

        Assert.That(guerrero.VidaMaxima, Is.EqualTo(120));
        Assert.That(guerrero.Danio, Is.EqualTo(30));
        Assert.That(guerrero.Alcance, Is.EqualTo(1));
        Assert.That(arquero.Alcance, Is.EqualTo(3));
        Assert.That(centro.VidaMaxima, Is.EqualTo(300));
    }

    [Test]
    public void AtacanteInexistente_Falla()
    {
        ResultadoAccion resultado = operacion.Ejecutar(
            partida,
            new SolicitudAtaque(Guid.NewGuid(), objetivo.Id));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(resultado.Mensaje, Does.Contain("atacante"));
    }

    [Test]
    public void ObjetivoInexistente_Falla()
    {
        ResultadoAccion resultado = operacion.Ejecutar(
            partida,
            new SolicitudAtaque(atacante.Id, Guid.NewGuid()));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(resultado.Mensaje, Does.Contain("objetivo"));
    }

    private void AtacarHastaDestruir(
        Partida partidaObjetivo,
        Guid atacanteId,
        Guid objetivoId,
        Func<bool> sigueExistiendo)
    {
        int seguridad = 0;

        while (sigueExistiendo() &&
               seguridad++ < 30)
        {
            ResultadoAccion resultado =
                operacion.Ejecutar(
                    partidaObjetivo,
                    new SolicitudAtaque(
                        atacanteId,
                        objetivoId));

            Assert.That(
                resultado.Exito,
                Is.True,
                resultado.Mensaje);
        }

        Assert.That(seguridad, Is.LessThan(30));
    }

    private EstadoPartidaService CrearServicio()
    {
        var servicio = new EstadoPartidaService();
        servicio.EstablecerPartida(partida);
        return servicio;
    }
}
