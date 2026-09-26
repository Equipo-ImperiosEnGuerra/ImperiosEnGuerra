using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Api.Mapeadores;
using ImperiosEnGuerra.Api.Servicios;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Tests;

public class MovimientoTests
{
    private Partida partida;
    private Mapa mapa;
    private Unidad unidad;
    private OperacionMovimiento operacion;

    [SetUp]
    public void Preparar()
    {
        mapa = new Mapa(6, 6);
        partida = new Partida(
            new Jugador("Humano", TipoJugador.Humano, mapa, new RecursosJugador()),
            new Jugador("Máquina", TipoJugador.Maquina, mapa, new RecursosJugador()));
        unidad = new Aldeano(new Coordenada(1, 1));
        partida.JugadorHumano.AgregarUnidad(unidad);
        operacion = new OperacionMovimiento();
    }

    [TestCase(4, 5)]
    [TestCase(0, 0)]
    [TestCase(5, 5)]
    [TestCase(1, 1)]
    public void MovimientoValido_ActualizaCoordenadaConservaIdentidadYDisponibilidad(int x, int y)
    {
        Guid id = unidad.Id;
        var solicitud = new SolicitudMovimiento(id, new Coordenada(x, y));
        var resultado = operacion.Ejecutar(partida, solicitud);

        Assert.That(resultado.Exito, Is.True);
        Assert.That(solicitud.Tipo, Is.EqualTo(TipoAccionJuego.Mover));
        Assert.That(unidad.Coordenada.X, Is.EqualTo(x));
        Assert.That(unidad.Coordenada.Y, Is.EqualTo(y));
        Assert.That(unidad.Id, Is.EqualTo(id));
        Assert.That(unidad.Disponible, Is.True);
        Assert.That(mapa.ObtenerCasilla(1, 1).EstaOcupada, Is.False);
        Assert.That(mapa.ObtenerCasilla(x, y).EstaOcupada, Is.False);
    }

    [TestCase("inexistente")]
    [TestCase("no disponible")]
    [TestCase("nulo")]
    [TestCase("ocupada")]
    [TestCase("recurso")]
    [TestCase("no transitable")]
    [TestCase("unidad humana")]
    [TestCase("unidad enemiga")]
    [TestCase("edificio humano")]
    [TestCase("edificio enemigo")]
    public void MovimientoInvalido_NoModificaCoordenadaNiMapa(string caso)
    {
        Guid id = unidad.Id;
        Coordenada destino = new Coordenada(4, 5);
        switch (caso)
        {
            case "inexistente": id = Guid.NewGuid(); break;
            case "maquina":
                var enemiga = new Guerrero(new Coordenada(2, 2));
                partida.JugadorMaquina.AgregarUnidad(enemiga);
                id = enemiga.Id;
                break;
            case "no disponible": unidad.MarcarNoDisponible(); break;
            case "nulo": destino = null; break;
            case "ocupada": mapa.ObtenerCasilla(4, 5).Ocupar(); break;
            case "recurso": mapa.ColocarRecurso(new Recurso(TipoRecurso.Oro, destino)); break;
            case "no transitable": mapa.ObtenerCasilla(4, 5).CambiarTransitabilidad(false); break;
            case "unidad humana": partida.JugadorHumano.AgregarUnidad(new Aldeano(destino)); break;
            case "unidad enemiga": partida.JugadorMaquina.AgregarUnidad(new Aldeano(destino)); break;
            case "edificio humano": partida.JugadorHumano.AgregarEdificio(new CentroUrbano(destino)); break;
            case "edificio enemigo": partida.JugadorMaquina.AgregarEdificio(new CentroUrbano(destino)); break;
        }

        var posiciones = partida.JugadorHumano.Unidades.Concat(partida.JugadorMaquina.Unidades)
            .Select(u => (Unidad: u, Posicion: u.Coordenada)).ToArray();
        var ocupacion = CapturarOcupacion();
        var resultado = operacion.Ejecutar(partida, new SolicitudMovimiento(id, destino));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(resultado.Mensaje, Is.Not.Empty);
        foreach (var anterior in posiciones)
            Assert.That(anterior.Unidad.Coordenada, Is.SameAs(anterior.Posicion));
        Assert.That(CapturarOcupacion(), Is.EqualTo(ocupacion));
    }

    [Test]
    public void UnidadMaquina_MovimientoValido_UsaSuPropioMapa()
    {
        var mapaMaquina =
            new Mapa(6, 6);

        var maquina =
            new Jugador(
                "Máquina",
                TipoJugador.Maquina,
                mapaMaquina,
                new RecursosJugador());

        var unidadMaquina =
            new Guerrero(
                new Coordenada(1, 1));

        maquina.AgregarUnidad(
            unidadMaquina);

        partida =
            new Partida(
                partida.JugadorHumano,
                maquina);

        ResultadoAccion resultado =
            operacion.Ejecutar(
                partida,
                new SolicitudMovimiento(
                    unidadMaquina.Id,
                    new Coordenada(4, 5)));

        Assert.That(
            resultado.Exito,
            Is.True,
            resultado.Mensaje);

        Assert.That(
            unidadMaquina.Coordenada.X,
            Is.EqualTo(4));

        Assert.That(
            unidadMaquina.Coordenada.Y,
            Is.EqualTo(5));

        Assert.That(
            unidad.Coordenada.X,
            Is.EqualTo(1));

        Assert.That(
            unidad.Coordenada.Y,
            Is.EqualTo(1));
    }

    [TestCase(-1, 0)]
    [TestCase(0, -1)]
    [TestCase(6, 0)]
    [TestCase(0, 6)]
    [TestCase(int.MaxValue, int.MinValue)]
    public void FueraDeLimites_NoMueve(int x, int y)
    {
        Coordenada origen = unidad.Coordenada;
        Assert.That(operacion.Ejecutar(partida,
            new SolicitudMovimiento(unidad.Id, new Coordenada(x, y))).Exito, Is.False);
        Assert.That(unidad.Coordenada, Is.SameAs(origen));
    }

    [Test]
    public void SinPartidaOSolicitud_DevuelveFallo()
    {
        Assert.That(operacion.Ejecutar(null, new SolicitudMovimiento(unidad.Id, new Coordenada(2, 2))).Exito, Is.False);
        Assert.That(operacion.Ejecutar(partida, null).Exito, Is.False);
        Assert.That(new EstadoPartidaService().MoverUnidad(CrearRequest()).Mensaje,
            Is.EqualTo("No hay una partida activa."));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("no-es-guid")]
    public void Transporte_IdInvalido_NoMueve(string id)
    {
        var servicio = CrearServicio();
        var request = CrearRequest();
        request.UnidadId = id;
        Coordenada origen = unidad.Coordenada;
        Assert.That(servicio.MoverUnidad(request).Mensaje, Does.Contain("Guid"));
        Assert.That(unidad.Coordenada, Is.SameAs(origen));
    }

    [Test]
    public void Transporte_DestinoOSolicitudNulos_NoMueve()
    {
        var servicio = CrearServicio();
        var request = CrearRequest();
        request.Destino = null;
        Coordenada origen = unidad.Coordenada;
        Assert.That(servicio.MoverUnidad(request).Exito, Is.False);
        Assert.That(servicio.MoverUnidad(null).Exito, Is.False);
        Assert.That(unidad.Coordenada, Is.SameAs(origen));
    }

    [Test]
    public void ServicioYMapper_ReflejanNuevaCoordenadaEIdentidad()
    {
        var servicio = CrearServicio();
        var antes = servicio.ObtenerEstado();
        Assert.That(servicio.MoverUnidad(CrearRequest()).Exito, Is.True);
        var despues = servicio.ObtenerEstado();
        var mapeada = PartidaEstadoMapper.Convertir(partida).JugadorHumano.Unidades[0];
        Assert.That(antes.JugadorHumano.Unidades[0].Coordenada.X, Is.EqualTo(1));
        Assert.That(despues.JugadorHumano.Unidades[0].Coordenada.X, Is.EqualTo(4));
        Assert.That(despues.JugadorHumano.Unidades[0].Coordenada.Y, Is.EqualTo(5));
        Assert.That(mapeada.Coordenada.X, Is.EqualTo(4));
        Assert.That(mapeada.Coordenada.Y, Is.EqualTo(5));
        Assert.That(mapeada.Id, Is.EqualTo(unidad.Id.ToString("D")));
    }

    [Test]
    public void SegundoMovimiento_LiberaPosicionLogicaAnterior()
    {
        var otra = new Aldeano(new Coordenada(0, 0));
        partida.JugadorHumano.AgregarUnidad(otra);
        Assert.That(operacion.Ejecutar(partida, new SolicitudMovimiento(unidad.Id, new Coordenada(4, 5))).Exito, Is.True);
        Assert.That(operacion.Ejecutar(partida, new SolicitudMovimiento(otra.Id, new Coordenada(4, 5))).Exito, Is.False);
        Assert.That(operacion.Ejecutar(partida, new SolicitudMovimiento(otra.Id, new Coordenada(1, 1))).Exito, Is.True);
    }

    [Test]
    public void OrigenMarcado_NoLiberaOcupacionAjena()
    {
        mapa.ObtenerCasilla(1, 1).Ocupar();
        Assert.That(operacion.Ejecutar(partida, new SolicitudMovimiento(unidad.Id, new Coordenada(4, 5))).Exito, Is.True);
        Assert.That(mapa.ObtenerCasilla(1, 1).EstaOcupada, Is.True);
    }

    [Test]
    public void EntidadesEnOtroMapa_NoBloqueanDestino()
    {
        var maquina = new Jugador("Máquina", TipoJugador.Maquina, new Mapa(6, 6), new RecursosJugador());
        maquina.AgregarUnidad(new Aldeano(new Coordenada(4, 5)));
        partida = new Partida(partida.JugadorHumano, maquina);
        Assert.That(operacion.Ejecutar(partida, new SolicitudMovimiento(unidad.Id, new Coordenada(4, 5))).Exito, Is.True);
    }

    private bool[] CapturarOcupacion() => Enumerable.Range(0, 36)
        .Select(i => mapa.ObtenerCasilla(i % 6, i / 6).EstaOcupada).ToArray();

    private EstadoPartidaService CrearServicio()
    {
        var servicio = new EstadoPartidaService();
        servicio.EstablecerPartida(partida);
        return servicio;
    }

    private MoverUnidadRequest CrearRequest() => new MoverUnidadRequest
    {
        UnidadId = unidad.Id.ToString("D"),
        Destino = new CoordenadaRequest { X = 4, Y = 5 }
    };
}
