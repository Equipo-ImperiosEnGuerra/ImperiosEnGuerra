using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Recursos;

namespace ImperiosEnGuerra.Servicios
{
    public class ServicioArchivos
    {
        private const string ArchivoConfiguracion = "configuracion.txt";
        private const string ArchivoLogPartida = "log_partida.txt";
        private const string ArchivoResultadoFinal = "resultado_final.txt";

        private readonly string directorioBase;

        public ServicioArchivos(string directorioBase)
        {
            if (string.IsNullOrWhiteSpace(directorioBase))
            {
                throw new ArgumentException(
                    "El directorio base no puede estar vacío.", nameof(directorioBase));
            }

            this.directorioBase = directorioBase;
            Directory.CreateDirectory(directorioBase);
        }

        public void GuardarConfiguracion(string contenido)
        {
            if (contenido == null)
            {
                throw new ArgumentNullException(nameof(contenido));
            }

            File.WriteAllText(Path.Combine(directorioBase, ArchivoConfiguracion), contenido);
        }

        public void GuardarConfiguracionInicial(Partida partida)
        {
            if (partida == null)
            {
                throw new ArgumentNullException(nameof(partida));
            }

            StringBuilder texto = new StringBuilder();
            texto.Append("PARTIDA\n");
            AgregarJugador(texto, "JUGADOR_HUMANO", partida.JugadorHumano);
            texto.Append('\n');
            AgregarJugador(texto, "JUGADOR_MAQUINA", partida.JugadorMaquina);

            GuardarConfiguracion(texto.ToString());
        }

        private void AgregarJugador(StringBuilder texto, string seccion, Jugador jugador)
        {
            texto.Append('[').Append(seccion).Append("]\n");
            texto.Append("Nombre=").Append(jugador.Nombre).Append('\n');
            texto.Append("Tipo=").Append(jugador.Tipo).Append('\n');
            texto.AppendFormat(CultureInfo.InvariantCulture,
                "Mapa={0}x{1}\n", jugador.Mapa.Ancho, jugador.Mapa.Alto);

            AgregarRecursoAlmacenado(texto, jugador, TipoRecurso.Oro);
            AgregarRecursoAlmacenado(texto, jugador, TipoRecurso.Madera);
            AgregarRecursoAlmacenado(texto, jugador, TipoRecurso.Comida);

            texto.Append("Edificios:\n");
            foreach (var edificio in jugador.Edificios
                .OrderBy(edificio => edificio.GetType().Name, StringComparer.Ordinal)
                .ThenBy(edificio => edificio.Coordenada.X)
                .ThenBy(edificio => edificio.Coordenada.Y))
            {
                texto.AppendFormat(CultureInfo.InvariantCulture,
                    "{0}=({1},{2})\n", edificio.GetType().Name,
                    edificio.Coordenada.X, edificio.Coordenada.Y);
            }

            texto.Append("RecursosMapa:\n");
            foreach (var recurso in jugador.Mapa.Recursos
                .OrderBy(recurso => recurso.Tipo)
                .ThenBy(recurso => recurso.Coordenada.X)
                .ThenBy(recurso => recurso.Coordenada.Y))
            {
                texto.AppendFormat(CultureInfo.InvariantCulture,
                    "{0}=({1},{2})\n", recurso.Tipo,
                    recurso.Coordenada.X, recurso.Coordenada.Y);
            }
        }

        private void AgregarRecursoAlmacenado(
            StringBuilder texto, Jugador jugador, TipoRecurso tipo)
        {
            texto.AppendFormat(CultureInfo.InvariantCulture,
                "{0}={1}\n", tipo, jugador.Recursos.ObtenerCantidad(tipo));
        }

        public void RegistrarEvento(string contenido)
        {
            if (contenido == null)
            {
                throw new ArgumentNullException(nameof(contenido));
            }

            File.AppendAllText(
                Path.Combine(directorioBase, ArchivoLogPartida), contenido + Environment.NewLine);
        }

        public void GuardarResultadoFinal(string contenido)
        {
            if (contenido == null)
            {
                throw new ArgumentNullException(nameof(contenido));
            }

            File.WriteAllText(Path.Combine(directorioBase, ArchivoResultadoFinal), contenido);
        }
    }
}
