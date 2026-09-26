using ImperiosEnGuerra.Api.Servicios;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;

namespace ImperiosEnGuerra.Tests;

public class RegeneracionRecursosTests
{
    [Test]
    public void EjecutarPaso_RegeneraMismoTipoEnOtraCasilla()
    {
        Partida partida =
            CrearPartida(
                out Recurso oro);

        oro.Extraer(
            oro.CantidadRestante);

        var estado =
            new EstadoPartidaService();

        estado.EstablecerPartida(
            partida);

        using var servicio =
            new ServicioRegeneracionRecursos(
                estado,
                TimeSpan.FromMilliseconds(20),
                TimeSpan.FromMilliseconds(5),
                17);

        Assert.That(
            servicio.EjecutarPaso(),
            Is.Zero);

        Assert.That(
            servicio.Pendientes,
            Is.EqualTo(1));

        Thread.Sleep(35);

        Assert.That(
            servicio.EjecutarPaso(),
            Is.EqualTo(1));

        Recurso[] activos =
            partida.JugadorHumano.Mapa
                .Recursos
                .Where(
                    recurso =>
                        !recurso.Agotado)
                .ToArray();

        Assert.That(
            activos,
            Has.Length.EqualTo(1));

        Assert.That(
            activos[0].Tipo,
            Is.EqualTo(
                TipoRecurso.Oro));

        Assert.That(
            activos[0].Coordenada.X ==
                oro.Coordenada.X &&
            activos[0].Coordenada.Y ==
                oro.Coordenada.Y,
            Is.False,
            "El nodo debe reaparecer en una casilla libre aleatoria.");
    }

    [Test]
    public void Iniciar_EjecutaRegeneracionEnTaskConcurrente()
    {
        Partida partida =
            CrearPartida(
                out Recurso madera);

        madera.Extraer(
            madera.CantidadRestante);

        var estado =
            new EstadoPartidaService();

        estado.EstablecerPartida(
            partida);

        using var servicio =
            new ServicioRegeneracionRecursos(
                estado,
                TimeSpan.FromMilliseconds(30),
                TimeSpan.FromMilliseconds(10),
                23);

        Assert.That(
            servicio.Iniciar(),
            Is.True);

        Assert.That(
            SpinWait.SpinUntil(
                () =>
                    partida.JugadorHumano.Mapa
                        .Recursos
                        .Any(
                            recurso =>
                                recurso.Tipo ==
                                    TipoRecurso.Madera &&
                                !recurso.Agotado),
                TimeSpan.FromSeconds(2)),
            Is.True);

        Assert.That(
            servicio.Activo,
            Is.True);

        Assert.That(
            servicio.Detener(),
            Is.True);
    }

    private static Partida CrearPartida(
        out Recurso recurso)
    {
        var mapa =
            new Mapa(
                8,
                8);

        var humano =
            new Jugador(
                "Humano",
                TipoJugador.Humano,
                mapa,
                new RecursosJugador());

        var maquina =
            new Jugador(
                "CPU",
                TipoJugador.Maquina,
                mapa,
                new RecursosJugador());

        humano.AgregarEdificio(
            new CentroUrbano(
                new Coordenada(
                    0,
                    0)));

        maquina.AgregarEdificio(
            new CentroUrbano(
                new Coordenada(
                    7,
                    7)));

        mapa.ObtenerCasilla(
                0,
                0)
            .Ocupar();

        mapa.ObtenerCasilla(
                7,
                7)
            .Ocupar();

        recurso =
            new Recurso(
                TipoRecurso.Madera,
                new Coordenada(
                    3,
                    3));

        mapa.ColocarRecurso(
            recurso);

        return new Partida(
            humano,
            maquina);
    }
}
