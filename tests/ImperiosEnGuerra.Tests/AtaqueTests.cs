using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Api.Servicios;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Core;
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
            new Jugador(
                "Humano",
                TipoJugador.Humano,
                mapa,
                new RecursosJugador()),
            new Jugador(
                "Máquina",
                TipoJugador.Maquina,
                mapa,
                new RecursosJugador()));

        atacante = new Guerrero(new Coordenada(1, 1));
        objetivo = new Lancero(new Coordenada(4, 4));

        partida.JugadorHumano.AgregarUnidad(atacante);
        partida.JugadorMaquina.AgregarUnidad(objetivo);

        operacion = new OperacionAtaque();
    }

    [Test]
    public void AtaqueValido_PreparaSinAplicarDanoInventado()
    {
        int unidadesHumano = partida.JugadorHumano.Unidades.Count;
        int unidadesMaquina = partida.JugadorMaquina.Unidades.Count;

        ResultadoAccion resultado = operacion.Ejecutar(
            partida,
            new SolicitudAtaque(
                atacante.Id,
                objetivo.Id));

        Assert.That(resultado.Exito, Is.True);
        Assert.That(resultado.Mensaje, Does.Contain("pendiente"));
        Assert.That(partida.JugadorHumano.Unidades.Count, Is.EqualTo(unidadesHumano));
        Assert.That(partida.JugadorMaquina.Unidades.Count, Is.EqualTo(unidadesMaquina));
        Assert.That(objetivo.Coordenada.X, Is.EqualTo(4));
        Assert.That(objetivo.Coordenada.Y, Is.EqualTo(4));
    }

    [Test]
    public void AtacanteMaquina_NoEsControlable()
    {
        ResultadoAccion resultado = operacion.Ejecutar(
            partida,
            new SolicitudAtaque(
                objetivo.Id,
                atacante.Id));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(resultado.Mensaje, Does.Contain("máquina"));
    }

    [Test]
    public void Aldeano_NoPuedeAtacar()
    {
        var aldeano = new Aldeano(new Coordenada(2, 2));
        partida.JugadorHumano.AgregarUnidad(aldeano);

        ResultadoAccion resultado = operacion.Ejecutar(
            partida,
            new SolicitudAtaque(
                aldeano.Id,
                objetivo.Id));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(resultado.Mensaje, Does.Contain("militar"));
    }

    [Test]
    public void AtacanteNoDisponible_Falla()
    {
        atacante.MarcarNoDisponible();

        ResultadoAccion resultado = operacion.Ejecutar(
            partida,
            new SolicitudAtaque(
                atacante.Id,
                objetivo.Id));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(resultado.Mensaje, Does.Contain("disponible"));
    }

    [Test]
    public void ObjetivoPropio_FallaYConservaMensaje()
    {
        var aliado = new Arquero(new Coordenada(2, 2));
        partida.JugadorHumano.AgregarUnidad(aliado);

        ResultadoAccion resultado = operacion.Ejecutar(
            partida,
            new SolicitudAtaque(
                atacante.Id,
                aliado.Id));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(
            resultado.Mensaje,
            Is.EqualTo("El objetivo pertenece al jugador humano."));
    }

    [Test]
    public void AtacanteInexistente_Falla()
    {
        ResultadoAccion resultado = operacion.Ejecutar(
            partida,
            new SolicitudAtaque(
                Guid.NewGuid(),
                objetivo.Id));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(resultado.Mensaje, Does.Contain("atacante"));
    }

    [Test]
    public void ObjetivoInexistente_Falla()
    {
        ResultadoAccion resultado = operacion.Ejecutar(
            partida,
            new SolicitudAtaque(
                atacante.Id,
                Guid.NewGuid()));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(resultado.Mensaje, Does.Contain("objetivo"));
    }

    [Test]
    public void Servicio_AtaqueValido()
    {
        var servicio = CrearServicio();

        ResultadoAccion resultado = servicio.Atacar(
            new AtacarRequest
            {
                AtacanteId = atacante.Id.ToString("D"),
                ObjetivoId = objetivo.Id.ToString("D")
            });

        Assert.That(resultado.Exito, Is.True);
        Assert.That(servicio.ObtenerEstado(), Is.Not.Null);
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
            operacion.Ejecutar(
                null,
                new SolicitudAtaque(atacante.Id, objetivo.Id)).Exito,
            Is.False);

        Assert.That(
            operacion.Ejecutar(partida, null).Exito,
            Is.False);

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
