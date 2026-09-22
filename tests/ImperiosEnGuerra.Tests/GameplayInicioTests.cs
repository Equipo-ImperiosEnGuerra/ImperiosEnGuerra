using System.Collections.Generic;
using System.Linq;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recoleccion;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;
using NUnit.Framework;

namespace ImperiosEnGuerra.Tests;

public class GameplayInicioTests
{
    [Test]
    public void LayoutInicial_TodosLosRecursosSonAccesiblesParaUnAldeano()
    {
        var mapa =
            new Mapa(10, 10);

        List<Recurso> recursosHumano =
            CrearRecursosHumano();

        List<Recurso> recursosMaquina =
            CrearRecursosMaquina();

        Partida partida =
            new InicializadorPartida()
                .Crear(
                    "Humano",
                    mapa,
                    new Coordenada(1, 1),
                    recursosHumano,
                    "CPU",
                    mapa,
                    new Coordenada(8, 8),
                    recursosMaquina);

        Aldeano aldeano =
            partida.JugadorHumano.Unidades
                .OfType<Aldeano>()
                .First();

        var planificador =
            new PlanificadorAproximacionRecurso();

        Assert.That(
            mapa.Recursos.Count,
            Is.EqualTo(12));

        foreach (Recurso recurso in mapa.Recursos)
        {
            ResultadoAproximacionRecurso resultado =
                planificador.Preparar(
                    partida,
                    new SolicitudRecoleccion(
                        aldeano.Id,
                        recurso.Coordenada));

            Assert.That(
                resultado.Exito,
                Is.True,
                $"{recurso.Tipo} en ({recurso.Coordenada.X},{recurso.Coordenada.Y}) debe conservar al menos una aproximación accesible. {resultado.Mensaje}");
        }
    }

    private static List<Recurso> CrearRecursosHumano()
    {
        return new List<Recurso>
        {
            new Recurso(TipoRecurso.Oro, new Coordenada(3, 1)),
            new Recurso(TipoRecurso.Oro, new Coordenada(4, 3)),
            new Recurso(TipoRecurso.Madera, new Coordenada(1, 4)),
            new Recurso(TipoRecurso.Madera, new Coordenada(3, 5)),
            new Recurso(TipoRecurso.Comida, new Coordenada(4, 1)),
            new Recurso(TipoRecurso.Comida, new Coordenada(1, 5))
        };
    }

    private static List<Recurso> CrearRecursosMaquina()
    {
        return new List<Recurso>
        {
            new Recurso(TipoRecurso.Oro, new Coordenada(6, 8)),
            new Recurso(TipoRecurso.Oro, new Coordenada(5, 6)),
            new Recurso(TipoRecurso.Madera, new Coordenada(8, 5)),
            new Recurso(TipoRecurso.Madera, new Coordenada(6, 4)),
            new Recurso(TipoRecurso.Comida, new Coordenada(5, 8)),
            new Recurso(TipoRecurso.Comida, new Coordenada(8, 4))
        };
    }
}
