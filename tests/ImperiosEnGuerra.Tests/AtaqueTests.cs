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
        var mapa = new Mapa(6, 6);

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
    public void AtaqueAdyacente_DestruyeUnidadYReportaEstadoDeVictoria()
    {
        ResultadoAccion resultado = operacion.Ejecutar(
            partida,
            new SolicitudAtaque(atacante.Id, objetivo.Id));

        Assert.That(resultado.Exito, Is.True, resultado.Mensaje);
        Assert.That(resultado.Mensaje, Does.Contain("destruido"));
        Assert.That(partida.JugadorMaquina.Unidades, Is.Empty);

        EvaluacionVictoria evaluacion =
            new EvaluadorVictoria().Evaluar(partida.JugadorMaquina);

        Assert.That(evaluacion.SinUnidadesMilitares, Is.True);
    }

    [Test]
    public void AtacanteMaquina_PuedeDestruirUnidadHumana()
    {
        ResultadoAccion resultado = operacion.Ejecutar(
            partida,
            new SolicitudAtaque(objetivo.Id, atacante.Id));

        Assert.That(resultado.Exito, Is.True, resultado.Mensaje);
        Assert.That(partida.JugadorHumano.Unidades, Is.Empty);
    }

    [Test]
    public void ObjetivoLejano_FallaSinDestruir()
    {
        var lejano = new Guerrero(new Coordenada(5, 5));
        partida.JugadorMaquina.AgregarUnidad(lejano);

        ResultadoAccion resultado = operacion.Ejecutar(
            partida,
            new SolicitudAtaque(atacante.Id, lejano.Id));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(resultado.Mensaje, Does.Contain("adyacente"));
        Assert.That(partida.JugadorMaquina.Unidades, Does.Contain(lejano));
    }

    [Test]
    public void CentroUrbanoEnemigo_PuedeSerDestruidoPorId()
    {
        var centro = new CentroUrbano(new Coordenada(1, 2));
        partida.JugadorMaquina.AgregarEdificio(centro);
        partida.JugadorMaquina.Mapa.ObtenerCasilla(1, 2).Ocupar();

        ResultadoAccion resultado = operacion.Ejecutar(
            partida,
            new SolicitudAtaque(atacante.Id, centro.Id));

        Assert.That(resultado.Exito, Is.True, resultado.Mensaje);
        Assert.That(partida.JugadorMaquina.Edificios, Is.Empty);
        Assert.That(
            partida.JugadorMaquina.Mapa.ObtenerCasilla(1, 2).EstaOcupada,
            Is.False);

        EvaluacionVictoria evaluacion =
            new EvaluadorVictoria().Evaluar(partida.JugadorMaquina);

        Assert.That(evaluacion.SinCentroUrbano, Is.True);
    }

    [Test]
    public void DestruirUnoDeDosCentros_NoReportaCeroCentros()
    {
        var centroAdyacente = new CentroUrbano(new Coordenada(1, 2));
        var centroLejano = new CentroUrbano(new Coordenada(5, 5));

        partida.JugadorMaquina.AgregarEdificio(centroAdyacente);
        partida.JugadorMaquina.AgregarEdificio(centroLejano);
        partida.JugadorMaquina.Mapa.ObtenerCasilla(1, 2).Ocupar();
        partida.JugadorMaquina.Mapa.ObtenerCasilla(5, 5).Ocupar();

        ResultadoAccion resultado = operacion.Ejecutar(
            partida,
            new SolicitudAtaque(atacante.Id, centroAdyacente.Id));

        Assert.That(resultado.Exito, Is.True, resultado.Mensaje);

        EvaluacionVictoria evaluacion =
            new EvaluadorVictoria().Evaluar(partida.JugadorMaquina);

        Assert.That(evaluacion.SinCentroUrbano, Is.False);
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
        Assert.That(resultado.Mensaje, Does.Contain("militar"));
    }

    [Test]
    public void AtacanteNoDisponible_Falla()
    {
        atacante.MarcarNoDisponible();

        ResultadoAccion resultado = operacion.Ejecutar(
            partida,
            new SolicitudAtaque(atacante.Id, objetivo.Id));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(resultado.Mensaje, Does.Contain("disponible"));
    }

    [Test]
    public void ObjetivoPropio_FallaYConservaModelo()
    {
        var aliado = new Arquero(new Coordenada(1, 2));
        partida.JugadorHumano.AgregarUnidad(aliado);

        ResultadoAccion resultado = operacion.Ejecutar(
            partida,
            new SolicitudAtaque(atacante.Id, aliado.Id));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(resultado.Mensaje, Is.EqualTo("El objetivo pertenece al mismo jugador que el atacante."));
        Assert.That(partida.JugadorHumano.Unidades, Does.Contain(aliado));
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

    [Test]
    public void Servicio_AtaqueValido_ActualizaSnapshot()
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
        Assert.That(estado, Is.Not.Null);
        Assert.That(estado!.JugadorMaquina.Unidades, Is.Empty);
    }

    [Test]
    public void Servicio_IdsInvalidos_DevuelvenMensajeClaro()
    {
        var servicio = CrearServicio();

        ResultadoAccion atacanteInvalido = servicio.Atacar(
            new AtacarRequest
            {
                AtacanteId = "no-es-guid",
                ObjetivoId = objetivo.Id.ToString("D")
            });

        ResultadoAccion objetivoInvalido = servicio.Atacar(
            new AtacarRequest
            {
                AtacanteId = atacante.Id.ToString("D"),
                ObjetivoId = "no-es-guid"
            });

        Assert.That(atacanteInvalido.Exito, Is.False);
        Assert.That(atacanteInvalido.Mensaje, Does.Contain("atacante"));
        Assert.That(objetivoInvalido.Exito, Is.False);
        Assert.That(objetivoInvalido.Mensaje, Does.Contain("objetivo"));
    }

    [Test]
    public void SinPartidaOSolicitud_DevuelveFallo()
    {
        Assert.That(
            operacion.Ejecutar(null, new SolicitudAtaque(atacante.Id, objetivo.Id)).Exito,
            Is.False);

        Assert.That(operacion.Ejecutar(partida, null).Exito, Is.False);

        Assert.That(
            new EstadoPartidaService().Atacar(
                new AtacarRequest
                {
                    AtacanteId = atacante.Id.ToString("D"),
                    ObjetivoId = objetivo.Id.ToString("D")
                }).Mensaje,
            Is.EqualTo("No hay una partida activa."));
    }

    private EstadoPartidaService CrearServicio()
    {
        var servicio = new EstadoPartidaService();
        servicio.EstablecerPartida(partida);
        return servicio;
    }
}
