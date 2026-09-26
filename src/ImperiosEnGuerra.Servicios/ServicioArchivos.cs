using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Recursos;

namespace ImperiosEnGuerra.Servicios
{
    /// <summary>
    /// Centraliza la persistencia de configuración, eventos y resultado de partida mediante System.IO.
    /// </summary>
    public class ServicioArchivos
    {
        private const string ArchivoConfiguracion = "configuracion.txt";
        private const string ArchivoLogPartida = "log_partida.txt";
        private const string ArchivoResultadoFinal = "resultado_final.txt";

        private readonly string directorioBase;

        /// <summary>
        /// Conserva la ruta recibida y asegura que exista el directorio, sin crear los archivos de partida.
        /// </summary>
        /// <param name="directorioBase">Ruta del directorio donde se guardarán los archivos.</param>
        /// <exception cref="ArgumentException">La ruta es nula, vacía o solo contiene espacios.</exception>
        /// <exception cref="IOException">El directorio no se puede crear por un error de entrada o salida.</exception>
        /// <exception cref="UnauthorizedAccessException">No se dispone de acceso para crear el directorio.</exception>
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

        /// <summary>
        /// Escribe el contenido en configuracion.txt, creando el archivo o reemplazando todo su contenido.
        /// </summary>
        /// <param name="contenido">Texto que se guarda; puede estar vacío.</param>
        /// <exception cref="ArgumentNullException">El contenido es nulo.</exception>
        /// <exception cref="IOException">La escritura falla por un error de entrada o salida.</exception>
        /// <exception cref="UnauthorizedAccessException">No se dispone de acceso para escribir el archivo.</exception>
        public void GuardarConfiguracion(string contenido)
        {
            if (contenido == null)
            {
                throw new ArgumentNullException(nameof(contenido));
            }

            File.WriteAllText(Path.Combine(directorioBase, ArchivoConfiguracion), contenido);
        }

        /// <summary>
        /// Genera una descripción determinista del estado recibido de ambos jugadores y la guarda mediante GuardarConfiguracion.
        /// Incluye nombres, tipos, dimensiones, saldos, edificios, unidades y recursos físicos; usa números con cultura invariable y saltos de línea LF.
        /// </summary>
        /// <param name="partida">Partida cuyo estado actual se registra como configuración inicial.</param>
        /// <exception cref="ArgumentNullException">La partida es nula.</exception>
        /// <exception cref="IOException">La escritura falla por un error de entrada o salida.</exception>
        /// <exception cref="UnauthorizedAccessException">No se dispone de acceso para escribir el archivo.</exception>
        public void GuardarConfiguracionInicial(Partida partida)
        {
            if (partida == null)
            {
                throw new ArgumentNullException(nameof(partida));
            }

            StringBuilder texto = new StringBuilder();
            texto.Append("PARTIDA\n");

            for (int i = 0;
                 i < partida.Jugadores.Count;
                 i++)
            {
                Jugador jugador =
                    partida.Jugadores[i];

                string seccion;

                if (ReferenceEquals(
                        jugador,
                        partida.JugadorHumano))
                {
                    seccion =
                        "JUGADOR_HUMANO";
                }
                else
                {
                    int indiceMaquina =
                        partida.JugadoresMaquina
                            .ToList()
                            .IndexOf(
                                jugador);

                    seccion =
                        indiceMaquina <= 0
                            ? "JUGADOR_MAQUINA"
                            : $"JUGADOR_MAQUINA_{indiceMaquina + 1}";
                }

                AgregarJugador(
                    texto,
                    seccion,
                    jugador);

                if (i <
                    partida.Jugadores.Count - 1)
                {
                    texto.Append('\n');
                }
            }

            GuardarConfiguracion(texto.ToString());
        }

        /// <summary>
        /// Añade una sección de jugador; ordena los edificios por nombre de tipo y coordenadas, y los recursos por tipo y coordenadas.
        /// </summary>
        /// <param name="texto">Texto al que se agrega la sección.</param>
        /// <param name="seccion">Identificador de la sección.</param>
        /// <param name="jugador">Jugador cuyos datos se describen.</param>
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

            texto.Append("Unidades:\n");
            foreach (var unidad in jugador.Unidades
                .OrderBy(unidad => unidad.GetType().Name, StringComparer.Ordinal)
                .ThenBy(unidad => unidad.Coordenada?.X ?? int.MinValue)
                .ThenBy(unidad => unidad.Coordenada?.Y ?? int.MinValue))
            {
                if (unidad.Coordenada == null)
                {
                    texto.Append(unidad.GetType().Name)
                        .Append("=(sin_posicion)\n");
                    continue;
                }

                texto.AppendFormat(CultureInfo.InvariantCulture,
                    "{0}=({1},{2})\n", unidad.GetType().Name,
                    unidad.Coordenada.X, unidad.Coordenada.Y);
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

        /// <summary>
        /// Añade el saldo de un recurso con representación numérica de cultura invariable.
        /// </summary>
        /// <param name="texto">Texto al que se agrega el saldo.</param>
        /// <param name="jugador">Jugador que posee los saldos.</param>
        /// <param name="tipo">Tipo de recurso almacenado que se consulta.</param>
        private void AgregarRecursoAlmacenado(
            StringBuilder texto, Jugador jugador, TipoRecurso tipo)
        {
            texto.AppendFormat(CultureInfo.InvariantCulture,
                "{0}={1}\n", tipo, jugador.Recursos.ObtenerCantidad(tipo));
        }

        /// <summary>
        /// Añade el contenido al final de log_partida.txt seguido de Environment.NewLine; crea el archivo si no existe.
        /// </summary>
        /// <param name="contenido">Texto que se guarda; puede estar vacío.</param>
        /// <exception cref="ArgumentNullException">El contenido es nulo.</exception>
        /// <exception cref="IOException">La escritura falla por un error de entrada o salida.</exception>
        /// <exception cref="UnauthorizedAccessException">No se dispone de acceso para escribir el archivo.</exception>
        public void RegistrarEvento(string contenido)
        {
            if (contenido == null)
            {
                throw new ArgumentNullException(nameof(contenido));
            }

            File.AppendAllText(
                Path.Combine(directorioBase, ArchivoLogPartida), contenido + Environment.NewLine);
        }

        /// <summary>
        /// Escribe el contenido en resultado_final.txt, creando el archivo o reemplazando todo su contenido.
        /// </summary>
        /// <param name="contenido">Texto que se guarda; puede estar vacío.</param>
        /// <exception cref="ArgumentNullException">El contenido es nulo.</exception>
        /// <exception cref="IOException">La escritura falla por un error de entrada o salida.</exception>
        /// <exception cref="UnauthorizedAccessException">No se dispone de acceso para escribir el archivo.</exception>
        public void GuardarResultadoFinal(string contenido)
        {
            if (contenido == null)
            {
                throw new ArgumentNullException(nameof(contenido));
            }

            File.WriteAllText(Path.Combine(directorioBase, ArchivoResultadoFinal), contenido);
        }

        /// <summary>
        /// Guarda el resultado estructurado de una partida ya finalizada.
        /// </summary>
        public void GuardarResultadoPartidaFinalizada(Partida partida)
        {
            if (partida == null)
                throw new ArgumentNullException(nameof(partida));

            if (!partida.Finalizada ||
                partida.Ganador == null)
            {
                throw new InvalidOperationException(
                    "La partida debe estar finalizada y tener un ganador antes de guardar el resultado.");
            }

            Jugador[] perdedores =
                partida.Jugadores
                    .Where(
                        jugador =>
                            !ReferenceEquals(
                                jugador,
                                partida.Ganador))
                    .ToArray();

            StringBuilder texto =
                new StringBuilder();

            texto.Append("RESULTADO_FINAL\n");
            texto.Append("Estado=Finalizada\n");
            texto.Append("ReglaVictoria=AND\n");
            texto.Append("GanadorTipo=")
                .Append(partida.Ganador.Tipo)
                .Append('\n');
            texto.Append("GanadorNombre=")
                .Append(partida.Ganador.Nombre)
                .Append('\n');

            texto.Append("Perdedores=")
                .Append(perdedores.Length)
                .Append('\n');

            for (int i = 0;
                 i < perdedores.Length;
                 i++)
            {
                texto.Append("Perdedor")
                    .Append(i + 1)
                    .Append("Tipo=")
                    .Append(perdedores[i].Tipo)
                    .Append('\n');

                texto.Append("Perdedor")
                    .Append(i + 1)
                    .Append("Nombre=")
                    .Append(perdedores[i].Nombre)
                    .Append('\n');
            }

            texto.Append("Motivo=")
                .Append(partida.MotivoFinalizacion)
                .Append('\n');

            GuardarResultadoFinal(
                texto.ToString());
        }
    }
}
