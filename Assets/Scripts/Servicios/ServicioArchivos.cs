using System;
using System.IO;

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
