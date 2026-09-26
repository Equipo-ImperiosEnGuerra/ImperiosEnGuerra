using System;
using System.Threading.Tasks;
using NUnit.Framework;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;

namespace ImperiosEnGuerra.Tests.Editor
{
    public class ModeloMapRecursosTests
    {
        [Test]
        public void Casilla_IniciaLibre_YControlaOcupacionYLiberacion()
        {
            Casilla casilla = new Casilla(new Coordenada(0, 0), true);

            Assert.That(casilla.EstaOcupada, Is.False);
            Assert.That(casilla.Ocupar(), Is.True);
            Assert.That(casilla.EstaOcupada, Is.True);
            Assert.That(casilla.Ocupar(), Is.False);
            Assert.That(casilla.EstaOcupada, Is.True);

            casilla.Liberar();

            Assert.That(casilla.EstaOcupada, Is.False);
            Assert.That(casilla.Ocupar(), Is.True);
        }

        [TestCase(true, false)]
        [TestCase(false, true)]
        public void Casilla_CambiarTransitabilidad_ActualizaValor(bool inicial, bool nuevo)
        {
            Casilla casilla = new Casilla(new Coordenada(0, 0), inicial);
            casilla.CambiarTransitabilidad(nuevo);
            Assert.That(casilla.EsTransitable, Is.EqualTo(nuevo));
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(int.MinValue)]
        public void Mapa_Constructor_RechazaAnchoNoPositivo(int ancho)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Mapa(ancho, 3));
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(int.MinValue)]
        public void Mapa_Constructor_RechazaAltoNoPositivo(int alto)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Mapa(4, alto));
        }

        [TestCase(0, 0)]
        [TestCase(3, 2)]
        [TestCase(2, 1)]
        public void Mapa_ObtenerCasilla_DevuelveCasillaDeLaPosicion(int x, int y)
        {
            Mapa mapa = new Mapa(4, 3);
            Casilla casilla = mapa.ObtenerCasilla(x, y);

            Assert.That(casilla, Is.Not.Null);
            Assert.That(casilla.Posicion.X, Is.EqualTo(x));
            Assert.That(casilla.Posicion.Y, Is.EqualTo(y));
            Assert.That(mapa.ObtenerCasilla(x, y), Is.SameAs(casilla));
        }

        [TestCase(-1, 0)]
        [TestCase(0, -1)]
        [TestCase(4, 0)]
        [TestCase(0, 3)]
        public void Mapa_ObtenerCasilla_FueraDeLimitesDevuelveNull(int x, int y)
        {
            Assert.That(new Mapa(4, 3).ObtenerCasilla(x, y), Is.Null);
        }

        [TestCase(0, 0, true)]
        [TestCase(3, 0, true)]
        [TestCase(0, 2, true)]
        [TestCase(3, 2, true)]
        [TestCase(-1, 0, false)]
        [TestCase(0, -1, false)]
        [TestCase(4, 0, false)]
        [TestCase(0, 3, false)]
        public void Mapa_EstaDentroDeLimites_ValidaBordes(int x, int y, bool esperado)
        {
            Mapa mapa = new Mapa(4, 3);
            Assert.That(mapa.EstaDentroDeLimites(new Coordenada(x, y)), Is.EqualTo(esperado));
        }

        [Test]
        public void Mapa_CoordenadaNull_NoEsValida()
        {
            Mapa mapa = new Mapa(4, 3);
            Assert.That(mapa.EstaDentroDeLimites(null), Is.False);
            Assert.That(mapa.PuedeColocar(null), Is.False);
            Assert.That(mapa.ObtenerRecursoEn(null), Is.Null);
        }

        [Test]
        public void Mapa_PuedeColocar_DependeDeLaOcupacion()
        {
            Mapa mapa = new Mapa(4, 3);
            Coordenada coordenada = new Coordenada(1, 1);

            Assert.That(mapa.PuedeColocar(coordenada), Is.True);
            mapa.ObtenerCasilla(1, 1).Ocupar();
            Assert.That(mapa.PuedeColocar(coordenada), Is.False);
            mapa.ObtenerCasilla(1, 1).Liberar();
            Assert.That(mapa.PuedeColocar(coordenada), Is.True);
        }

        [Test]
        public void Mapa_ColocarRecurso_ValidoLoRegistraYBloqueaColocacion()
        {
            Mapa mapa = new Mapa(4, 3);
            Recurso recurso = new Recurso(TipoRecurso.Oro, new Coordenada(1, 1));

            Assert.That(mapa.ColocarRecurso(recurso), Is.True);
            Assert.That(mapa.Recursos.Count, Is.EqualTo(1));
            Assert.That(mapa.Recursos[0], Is.SameAs(recurso));
            Assert.That(mapa.PuedeColocar(new Coordenada(1, 1)), Is.False);
        }

        [Test]
        public void Mapa_ColocarRecurso_RechazaNull()
        {
            Mapa mapa = new Mapa(4, 3);
            Assert.That(mapa.ColocarRecurso(null), Is.False);
            Assert.That(mapa.Recursos, Is.Empty);
        }

        [TestCase(-1, 0)]
        [TestCase(0, -1)]
        [TestCase(4, 0)]
        [TestCase(0, 3)]
        public void Mapa_ColocarRecurso_RechazaFueraDeLimites(int x, int y)
        {
            Mapa mapa = new Mapa(4, 3);
            Coordenada coordenada = new Coordenada(x, y);

            Assert.That(mapa.ColocarRecurso(new Recurso(TipoRecurso.Oro, coordenada)), Is.False);
            Assert.That(mapa.Recursos, Is.Empty);
            Assert.That(mapa.PuedeColocar(coordenada), Is.False);
            Assert.That(mapa.ObtenerRecursoEn(coordenada), Is.Null);
        }

        [Test]
        public void Mapa_ColocarRecurso_RechazaCasillaOcupada()
        {
            Mapa mapa = new Mapa(4, 3);
            mapa.ObtenerCasilla(1, 1).Ocupar();

            Assert.That(mapa.ColocarRecurso(
                new Recurso(TipoRecurso.Oro, new Coordenada(1, 1))), Is.False);
            Assert.That(mapa.Recursos, Is.Empty);
            Assert.That(mapa.ObtenerCasilla(1, 1).EstaOcupada, Is.True);
        }

        [Test]
        public void Mapa_ColocarRecurso_RechazaCoordenadaRepetida()
        {
            Mapa mapa = new Mapa(4, 3);
            Recurso original = new Recurso(TipoRecurso.Oro, new Coordenada(1, 1));
            Assert.That(mapa.ColocarRecurso(original), Is.True);

            Assert.That(mapa.ColocarRecurso(
                new Recurso(TipoRecurso.Madera, new Coordenada(1, 1))), Is.False);
            Assert.That(mapa.Recursos.Count, Is.EqualTo(1));
            Assert.That(mapa.ObtenerRecursoEn(new Coordenada(1, 1)), Is.SameAs(original));
        }

        [Test]
        public void Mapa_ObtenerRecursoEn_BuscaPorValoresDeCoordenada()
        {
            Mapa mapa = new Mapa(4, 3);
            Recurso oro = new Recurso(TipoRecurso.Oro, new Coordenada(1, 1));
            Recurso madera = new Recurso(TipoRecurso.Madera, new Coordenada(1, 2));
            Assert.That(mapa.ColocarRecurso(oro), Is.True);
            Assert.That(mapa.ColocarRecurso(madera), Is.True);

            Assert.That(mapa.ObtenerRecursoEn(new Coordenada(1, 1)), Is.SameAs(oro));
            Assert.That(mapa.ObtenerRecursoEn(new Coordenada(1, 2)), Is.SameAs(madera));
            Assert.That(mapa.ObtenerRecursoEn(new Coordenada(2, 1)), Is.Null);
        }

        [TestCase(TipoRecurso.Oro)]
        [TestCase(TipoRecurso.Madera)]
        [TestCase(TipoRecurso.Comida)]
        public void RecursosJugador_IniciaEnCero_YAgregarAcumula(TipoRecurso tipo)
        {
            RecursosJugador recursos = new RecursosJugador();
            Assert.That(recursos.ObtenerCantidad(tipo), Is.Zero);
            recursos.Agregar(tipo, 10);
            recursos.Agregar(tipo, 5);
            recursos.Agregar(tipo, 0);
            Assert.That(recursos.ObtenerCantidad(tipo), Is.EqualTo(15));
        }

        [Test]
        public void RecursosJugador_AgregarNegativo_LanzaSinModificarSaldo()
        {
            RecursosJugador recursos = new RecursosJugador();
            recursos.Agregar(TipoRecurso.Oro, 10);
            Assert.Throws<ArgumentOutOfRangeException>(() => recursos.Agregar(TipoRecurso.Oro, -1));
            Assert.That(recursos.ObtenerCantidad(TipoRecurso.Oro), Is.EqualTo(10));
        }

        [TestCase(0, true)]
        [TestCase(5, true)]
        [TestCase(10, true)]
        [TestCase(11, false)]
        [TestCase(-1, false)]
        public void RecursosJugador_PuedePagar_ConsultaSinModificarSaldo(int cantidad, bool esperado)
        {
            RecursosJugador recursos = new RecursosJugador();
            recursos.Agregar(TipoRecurso.Oro, 10);

            Assert.That(recursos.PuedePagar(TipoRecurso.Oro, cantidad), Is.EqualTo(esperado));
            Assert.That(recursos.ObtenerCantidad(TipoRecurso.Oro), Is.EqualTo(10));
        }

        [TestCase(0, true, 10)]
        [TestCase(4, true, 6)]
        [TestCase(10, true, 0)]
        [TestCase(11, false, 10)]
        [TestCase(-1, false, 10)]
        public void RecursosJugador_IntentarGastar_ValidaResultadoYSaldo(
            int cantidad, bool esperado, int saldoEsperado)
        {
            RecursosJugador recursos = new RecursosJugador();
            recursos.Agregar(TipoRecurso.Oro, 10);

            Assert.That(recursos.IntentarGastar(TipoRecurso.Oro, cantidad), Is.EqualTo(esperado));
            Assert.That(recursos.ObtenerCantidad(TipoRecurso.Oro), Is.EqualTo(saldoEsperado));
        }

        [Test]
        public async Task RecursosJugador_AgregarConcurrente_NoPierdeActualizaciones()
        {
            RecursosJugador recursos = new RecursosJugador();

            const int cantidadTareas = 8;
            const int incrementosPorTarea = 1000;

            Task[] tareas = new Task[cantidadTareas];

            for (int i = 0; i < cantidadTareas; i++)
            {
                tareas[i] = Task.Run(() =>
                {
                    for (int incremento = 0;
                         incremento < incrementosPorTarea;
                         incremento++)
                    {
                        recursos.Agregar(TipoRecurso.Oro, 1);
                    }
                });
            }

            await Task.WhenAll(tareas);

            Assert.That(
                recursos.ObtenerCantidad(TipoRecurso.Oro),
                Is.EqualTo(cantidadTareas * incrementosPorTarea));
        }

        [Test]
        public async Task RecursosJugador_GastoConcurrente_NoPermiteSaldoNegativo()
        {
            RecursosJugador recursos = new RecursosJugador();
            recursos.Agregar(TipoRecurso.Madera, 1000);

            const int cantidadTareas = 8;
            const int intentosPorTarea = 200;

            Task<int>[] tareas = new Task<int>[cantidadTareas];

            for (int i = 0; i < cantidadTareas; i++)
            {
                tareas[i] = Task.Run(() =>
                {
                    int exitos = 0;

                    for (int intento = 0;
                         intento < intentosPorTarea;
                         intento++)
                    {
                        if (recursos.IntentarGastar(
                            TipoRecurso.Madera,
                            1))
                        {
                            exitos++;
                        }
                    }

                    return exitos;
                });
            }

            int[] exitosPorTarea = await Task.WhenAll(tareas);
            int totalExitos = 0;

            foreach (int exitos in exitosPorTarea)
            {
                totalExitos += exitos;
            }

            Assert.That(totalExitos, Is.EqualTo(1000));
            Assert.That(
                recursos.ObtenerCantidad(TipoRecurso.Madera),
                Is.Zero);
        }

        [TestCase(TipoRecurso.Oro)]
        [TestCase(TipoRecurso.Madera)]
        [TestCase(TipoRecurso.Comida)]
        public void Recurso_ConservaTipoYCoordenada(TipoRecurso tipo)
        {
            Coordenada coordenada = new Coordenada(2, 3);
            Recurso recurso = new Recurso(tipo, coordenada);
            Assert.That(recurso.Tipo, Is.EqualTo(tipo));
            Assert.That(recurso.Coordenada, Is.SameAs(coordenada));
        }

        [Test]
        public void Recurso_IniciaConCantidadPredeterminada()
        {
            Recurso recurso =
                new Recurso(
                    TipoRecurso.Oro,
                    new Coordenada(2, 3));

            Assert.That(
                recurso.CantidadRestante,
                Is.EqualTo(
                    Recurso.CantidadInicialPredeterminada));

            Assert.That(
                recurso.Agotado,
                Is.False);
        }

        [Test]
        public void Recurso_Extraer_NoSuperaCantidadDisponible()
        {
            Recurso recurso =
                new Recurso(
                    TipoRecurso.Madera,
                    new Coordenada(1, 1),
                    5);

            Assert.That(
                recurso.Extraer(3),
                Is.EqualTo(3));

            Assert.That(
                recurso.Extraer(10),
                Is.EqualTo(2));

            Assert.That(
                recurso.CantidadRestante,
                Is.Zero);

            Assert.That(
                recurso.Agotado,
                Is.True);
        }

        [Test]
        public async Task Recurso_ExtraccionConcurrente_NoDuplicaCantidad()
        {
            Recurso recurso =
                new Recurso(
                    TipoRecurso.Comida,
                    new Coordenada(1, 1),
                    1000);

            const int cantidadTareas = 8;
            const int intentosPorTarea = 200;

            Task<int>[] tareas =
                new Task<int>[cantidadTareas];

            for (int i = 0; i < cantidadTareas; i++)
            {
                tareas[i] = Task.Run(() =>
                {
                    int extraido = 0;

                    for (int intento = 0;
                         intento < intentosPorTarea;
                         intento++)
                    {
                        extraido +=
                            recurso.Extraer(1);
                    }

                    return extraido;
                });
            }

            int[] resultados =
                await Task.WhenAll(tareas);

            Assert.That(
                resultados.Sum(),
                Is.EqualTo(1000));

            Assert.That(
                recurso.CantidadRestante,
                Is.Zero);
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Recurso_CantidadInicialNoPositiva_Lanza(
            int cantidad)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                    new Recurso(
                        TipoRecurso.Oro,
                        new Coordenada(1, 1),
                        cantidad));
        }

        [Test]
        public void Recurso_RechazaCoordenadaNull()
        {
            Assert.Throws<ArgumentNullException>(() => new Recurso(TipoRecurso.Oro, null));
        }
    }
}
