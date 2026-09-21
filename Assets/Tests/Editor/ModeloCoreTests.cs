using System;
using System.Collections.Generic;
using NUnit.Framework;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Tests.Editor
{
    public class ModeloCoreTests
    {
        [TestCase(TipoJugador.Humano)]
        [TestCase(TipoJugador.Maquina)]
        public void Jugador_ConstructorValido_ConservaDatos(TipoJugador tipo)
        {
            Mapa mapa = new Mapa(5, 4);
            RecursosJugador recursos = new RecursosJugador();
            Jugador jugador = new Jugador("Participante", tipo, mapa, recursos);

            Assert.That(jugador.Nombre, Is.EqualTo("Participante"));
            Assert.That(jugador.Tipo, Is.EqualTo(tipo));
            Assert.That(jugador.Mapa, Is.SameAs(mapa));
            Assert.That(jugador.Recursos, Is.SameAs(recursos));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Jugador_RechazaNombreInvalido(string nombre)
        {
            Assert.Throws<ArgumentException>(() => new Jugador(
                nombre, TipoJugador.Humano, new Mapa(5, 4), new RecursosJugador()));
        }

        [Test]
        public void Jugador_RechazaMapaNull()
        {
            Assert.Throws<ArgumentNullException>(() => new Jugador(
                "Humano", TipoJugador.Humano, null, new RecursosJugador()));
        }

        [Test]
        public void Jugador_RechazaRecursosNull()
        {
            Assert.Throws<ArgumentNullException>(() => new Jugador(
                "Humano", TipoJugador.Humano, new Mapa(5, 4), null));
        }

        [Test]
        public void Jugador_AgregaYEliminaEdificio()
        {
            Jugador jugador = CrearJugador(TipoJugador.Humano);
            CentroUrbano centro = new CentroUrbano(new Coordenada(0, 0));
            Assert.That(jugador.Edificios, Is.Empty);

            jugador.AgregarEdificio(centro);

            Assert.That(jugador.Edificios.Count, Is.EqualTo(1));
            Assert.That(jugador.Edificios[0], Is.SameAs(centro));
            Assert.That(jugador.EliminarEdificio(centro), Is.True);
            Assert.That(jugador.Edificios, Is.Empty);
        }

        [Test]
        public void Jugador_EdificioNull_NoModificaColeccion()
        {
            Jugador jugador = CrearJugador(TipoJugador.Humano);
            CentroUrbano centro = new CentroUrbano(new Coordenada(0, 0));
            jugador.AgregarEdificio(centro);

            Assert.Throws<ArgumentNullException>(() => jugador.AgregarEdificio(null));
            Assert.That(jugador.EliminarEdificio(null), Is.False);
            Assert.That(jugador.Edificios.Count, Is.EqualTo(1));
            Assert.That(jugador.Edificios[0], Is.SameAs(centro));
        }

        [Test]
        public void Jugador_AgregaYEliminaAldeano()
        {
            Jugador jugador = CrearJugador(TipoJugador.Humano);
            Aldeano aldeano = new Aldeano(new Coordenada(1, 1));
            Assert.That(jugador.Unidades, Is.Empty);

            jugador.AgregarUnidad(aldeano);

            Assert.That(jugador.Unidades.Count, Is.EqualTo(1));
            Assert.That(jugador.Unidades[0], Is.SameAs(aldeano));
            Assert.That(jugador.EliminarUnidad(aldeano), Is.True);
            Assert.That(jugador.Unidades, Is.Empty);
        }

        [Test]
        public void Jugador_UnidadNull_NoModificaColeccion()
        {
            Jugador jugador = CrearJugador(TipoJugador.Humano);
            Aldeano aldeano = new Aldeano(new Coordenada(1, 1));
            jugador.AgregarUnidad(aldeano);

            Assert.Throws<ArgumentNullException>(() => jugador.AgregarUnidad(null));
            Assert.That(jugador.EliminarUnidad(null), Is.False);
            Assert.That(jugador.Unidades.Count, Is.EqualTo(1));
            Assert.That(jugador.Unidades[0], Is.SameAs(aldeano));
        }

        [Test]
        public void Partida_AceptaHumanoYMaquina()
        {
            Jugador humano = CrearJugador(TipoJugador.Humano);
            Jugador maquina = CrearJugador(TipoJugador.Maquina);
            Partida partida = new Partida(humano, maquina);

            Assert.That(partida.JugadorHumano, Is.SameAs(humano));
            Assert.That(partida.JugadorMaquina, Is.SameAs(maquina));
        }

        [Test]
        public void Partida_RechazaHumanoNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new Partida(null, CrearJugador(TipoJugador.Maquina)));
        }

        [Test]
        public void Partida_RechazaMaquinaNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new Partida(CrearJugador(TipoJugador.Humano), null));
        }

        [TestCase(TipoJugador.Maquina, TipoJugador.Maquina)]
        [TestCase(TipoJugador.Humano, TipoJugador.Humano)]
        public void Partida_RechazaTiposIncorrectos(TipoJugador primero, TipoJugador segundo)
        {
            Assert.Throws<ArgumentException>(() =>
                new Partida(CrearJugador(primero), CrearJugador(segundo)));
        }

        [Test]
        public void Inicializador_CreaEstadoInicialDeAmbosJugadores()
        {
            Mapa mapaHumano = new Mapa(5, 4);
            Mapa mapaMaquina = new Mapa(6, 5);
            Coordenada centroHumano = new Coordenada(0, 0);
            Coordenada centroMaquina = new Coordenada(5, 4);
            List<Recurso> recursosHumano = CrearRecursos();
            List<Recurso> recursosMaquina = CrearRecursos();

            Partida partida = new InicializadorPartida().Crear(
                "Humano", mapaHumano, centroHumano, recursosHumano,
                "Maquina", mapaMaquina, centroMaquina, recursosMaquina);

            Assert.That(partida, Is.Not.Null);
            VerificarJugadorInicial(partida.JugadorHumano, TipoJugador.Humano,
                mapaHumano, centroHumano, recursosHumano);
            VerificarJugadorInicial(partida.JugadorMaquina, TipoJugador.Maquina,
                mapaMaquina, centroMaquina, recursosMaquina);
            Assert.That(partida.JugadorHumano.Recursos,
                Is.Not.SameAs(partida.JugadorMaquina.Recursos));
        }

        [TestCase("mapa null")]
        [TestCase("centro null")]
        [TestCase("lista null")]
        [TestCase("centro fuera")]
        [TestCase("recurso null")]
        [TestCase("recurso fuera")]
        [TestCase("posicion repetida")]
        [TestCase("recurso sobre centro")]
        [TestCase("falta Oro")]
        [TestCase("falta Madera")]
        [TestCase("falta Comida")]
        public void Inicializador_RechazaDatosInvalidosDeCualquierJugador(string caso)
        {
            foreach (bool invalidarHumano in new[] { true, false })
            {
                Mapa mapa = new Mapa(5, 4);
                Coordenada centro = new Coordenada(0, 0);
                List<Recurso> recursos = CrearRecursos();
                Type excepcion = typeof(ArgumentException);

                switch (caso)
                {
                    case "mapa null":
                        mapa = null;
                        excepcion = typeof(ArgumentNullException);
                        break;
                    case "centro null":
                        centro = null;
                        excepcion = typeof(ArgumentNullException);
                        break;
                    case "lista null":
                        recursos = null;
                        excepcion = typeof(ArgumentNullException);
                        break;
                    case "centro fuera":
                        centro = new Coordenada(5, 0);
                        break;
                    case "recurso null":
                        recursos.Add(null);
                        break;
                    case "recurso fuera":
                        recursos.Add(new Recurso(TipoRecurso.Oro, new Coordenada(0, 4)));
                        break;
                    case "posicion repetida":
                        recursos.Add(new Recurso(TipoRecurso.Comida, new Coordenada(1, 0)));
                        break;
                    case "recurso sobre centro":
                        recursos.Add(new Recurso(TipoRecurso.Oro, new Coordenada(0, 0)));
                        break;
                    case "falta Oro":
                        recursos.RemoveAll(recurso => recurso.Tipo == TipoRecurso.Oro);
                        break;
                    case "falta Madera":
                        recursos.RemoveAll(recurso => recurso.Tipo == TipoRecurso.Madera);
                        break;
                    case "falta Comida":
                        recursos.RemoveAll(recurso => recurso.Tipo == TipoRecurso.Comida);
                        break;
                    default:
                        throw new ArgumentException("Caso de prueba desconocido.", nameof(caso));
                }

                Mapa mapaValido = new Mapa(5, 4);
                Coordenada centroValido = new Coordenada(0, 0);
                List<Recurso> recursosValidos = CrearRecursos();
                InicializadorPartida inicializador = new InicializadorPartida();

                Assert.Throws(excepcion, () => inicializador.Crear(
                    "Humano", invalidarHumano ? mapa : mapaValido,
                    invalidarHumano ? centro : centroValido,
                    invalidarHumano ? recursos : recursosValidos,
                    "Maquina", invalidarHumano ? mapaValido : mapa,
                    invalidarHumano ? centroValido : centro,
                    invalidarHumano ? recursosValidos : recursos),
                    caso + (invalidarHumano ? " en Humano" : " en Maquina"));
            }
        }

        [Test]
        public void Inicializador_ErrorEnMaquina_NoModificaMapaHumano()
        {
            Mapa mapaHumano = new Mapa(5, 4);
            Recurso existente = new Recurso(TipoRecurso.Oro, new Coordenada(4, 3));
            Assert.That(mapaHumano.ColocarRecurso(existente), Is.True);
            mapaHumano.ObtenerCasilla(4, 2).Ocupar();
            mapaHumano.ObtenerCasilla(3, 3).CambiarTransitabilidad(false);
            bool[,] ocupacion = new bool[5, 4];
            bool[,] transitabilidad = new bool[5, 4];
            for (int x = 0; x < mapaHumano.Ancho; x++)
            {
                for (int y = 0; y < mapaHumano.Alto; y++)
                {
                    ocupacion[x, y] = mapaHumano.ObtenerCasilla(x, y).EstaOcupada;
                    transitabilidad[x, y] = mapaHumano.ObtenerCasilla(x, y).EsTransitable;
                }
            }

            List<Recurso> recursosMaquina = CrearRecursos();
            recursosMaquina.RemoveAll(recurso => recurso.Tipo == TipoRecurso.Comida);

            Assert.Throws<ArgumentException>(() => new InicializadorPartida().Crear(
                "Humano", mapaHumano, new Coordenada(0, 0), CrearRecursos(),
                "Maquina", new Mapa(5, 4), new Coordenada(0, 0), recursosMaquina));

            Assert.That(mapaHumano.Recursos.Count, Is.EqualTo(1));
            Assert.That(mapaHumano.Recursos[0], Is.SameAs(existente));
            for (int x = 0; x < mapaHumano.Ancho; x++)
            {
                for (int y = 0; y < mapaHumano.Alto; y++)
                {
                    Assert.That(mapaHumano.ObtenerCasilla(x, y).EstaOcupada,
                        Is.EqualTo(ocupacion[x, y]));
                    Assert.That(mapaHumano.ObtenerCasilla(x, y).EsTransitable,
                        Is.EqualTo(transitabilidad[x, y]));
                }
            }
        }

        private Jugador CrearJugador(TipoJugador tipo)
        {
            return new Jugador("Participante", tipo, new Mapa(5, 4), new RecursosJugador());
        }

        private List<Recurso> CrearRecursos()
        {
            return new List<Recurso>
            {
                new Recurso(TipoRecurso.Oro, new Coordenada(1, 0)),
                new Recurso(TipoRecurso.Madera, new Coordenada(2, 0)),
                new Recurso(TipoRecurso.Comida, new Coordenada(3, 0))
            };
        }

        private void VerificarJugadorInicial(
            Jugador jugador, TipoJugador tipo, Mapa mapa,
            Coordenada centro, IReadOnlyList<Recurso> recursos)
        {
            Assert.That(jugador.Tipo, Is.EqualTo(tipo));
            Assert.That(jugador.Mapa, Is.SameAs(mapa));
            Assert.That(jugador.Edificios.Count, Is.EqualTo(1));
            Assert.That(jugador.Edificios[0], Is.TypeOf<CentroUrbano>());
            Assert.That(jugador.Edificios[0].Coordenada.X, Is.EqualTo(centro.X));
            Assert.That(jugador.Edificios[0].Coordenada.Y, Is.EqualTo(centro.Y));
            Assert.That(mapa.ObtenerCasilla(centro.X, centro.Y).EstaOcupada, Is.True);
            Assert.That(mapa.Recursos.Count, Is.EqualTo(recursos.Count));
            foreach (Recurso recurso in recursos)
            {
                Assert.That(mapa.ObtenerRecursoEn(
                    new Coordenada(recurso.Coordenada.X, recurso.Coordenada.Y)), Is.SameAs(recurso));
                Assert.That(jugador.Recursos.ObtenerCantidad(recurso.Tipo), Is.Zero);
            }
        }
    }
}
