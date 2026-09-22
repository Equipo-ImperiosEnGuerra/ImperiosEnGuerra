using System;
using System.Collections.Generic;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Core
{
    /// <summary>
    /// Agrupa los datos del participante y sus colecciones de unidades y edificios.
    /// </summary>
    public class Jugador
    {
        private readonly List<Unidad> unidades;
        private readonly List<Edificio> edificios;
        private readonly List<ObraConstruccion> obrasConstruccion;

        /// <summary>
        /// Nombre del participante dentro de la partida.
        /// </summary>
        public string Nombre { get; }
        /// <summary>
        /// Modalidad del participante: humano o máquina.
        /// </summary>
        public TipoJugador Tipo { get; }
        /// <summary>
        /// Mapa lógico asociado al participante.
        /// </summary>
        public Mapa Mapa { get; }
        /// <summary>
        /// Saldos económicos del participante, independientes de los recursos físicos del mapa.
        /// </summary>
        public RecursosJugador Recursos { get; }

        /// <summary>
        /// Vista de solo lectura de las unidades; refleja los cambios de la colección interna.
        /// </summary>
        public IReadOnlyList<Unidad> Unidades
        {
            get { return unidades.AsReadOnly(); }
        }

        /// <summary>
        /// Vista de solo lectura de los edificios; refleja los cambios de la colección interna.
        /// </summary>
        public IReadOnlyList<Edificio> Edificios
        {
            get { return edificios.AsReadOnly(); }
        }

        public IReadOnlyList<ObraConstruccion> ObrasConstruccion
        {
            get { return obrasConstruccion.AsReadOnly(); }
        }

        /// <summary>
        /// Crea un participante con las referencias recibidas y colecciones vacías de unidades y edificios.
        /// </summary>
        /// <param name="nombre">Nombre no vacío ni compuesto solo por espacios.</param>
        /// <param name="tipo">Modalidad del participante.</param>
        /// <param name="mapa">Mapa asociado.</param>
        /// <param name="recursos">Saldos económicos asociados.</param>
        /// <exception cref="ArgumentException">El nombre es nulo, vacío o solo contiene espacios.</exception>
        /// <exception cref="ArgumentNullException">El mapa o los recursos son nulos.</exception>
        public Jugador(
            string nombre,
            TipoJugador tipo,
            Mapa mapa,
            RecursosJugador recursos)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                throw new ArgumentException(
                    "El nombre del jugador no puede estar vacío.",
                    nameof(nombre));
            }

            if (mapa == null)
            {
                throw new ArgumentNullException(nameof(mapa));
            }

            if (recursos == null)
            {
                throw new ArgumentNullException(nameof(recursos));
            }

            Nombre = nombre;
            Tipo = tipo;
            Mapa = mapa;
            Recursos = recursos;

            unidades = new List<Unidad>();
            edificios = new List<Edificio>();
            obrasConstruccion = new List<ObraConstruccion>();
        }

        /// <summary>
        /// Añade la referencia a la colección de unidades, permitiendo referencias repetidas.
        /// </summary>
        /// <param name="unidad">Elemento que se añade a la colección.</param>
        /// <exception cref="ArgumentNullException">El parámetro unidad es nulo.</exception>
        public void AgregarUnidad(Unidad unidad)
        {
            if (unidad == null)
            {
                throw new ArgumentNullException(nameof(unidad));
            }

            unidades.Add(unidad);
        }

        /// <summary>
        /// Retira la primera aparición del elemento de la colección del jugador.
        /// </summary>
        /// <param name="unidad">Elemento que se intenta retirar.</param>
        /// <returns>true si se eliminó; false si es nulo o no pertenece a la colección.</returns>
        public bool EliminarUnidad(Unidad unidad)
        {
            if (unidad == null)
            {
                return false;
            }

            return unidades.Remove(unidad);
        }

        /// <summary>
        /// Añade la referencia a la colección de edificios, permitiendo referencias repetidas.
        /// </summary>
        /// <param name="edificio">Elemento que se añade a la colección.</param>
        /// <exception cref="ArgumentNullException">El parámetro edificio es nulo.</exception>
        public void AgregarEdificio(Edificio edificio)
        {
            if (edificio == null)
            {
                throw new ArgumentNullException(nameof(edificio));
            }

            edificios.Add(edificio);
        }

        /// <summary>
        /// Retira la primera aparición del elemento de la colección del jugador.
        /// </summary>
        /// <param name="edificio">Elemento que se intenta retirar.</param>
        /// <returns>true si se eliminó; false si es nulo o no pertenece a la colección.</returns>
        public bool EliminarEdificio(Edificio edificio)
        {
            if (edificio == null)
            {
                return false;
            }

            return edificios.Remove(edificio);
        }
        public void AgregarObraConstruccion(
            ObraConstruccion obra)
        {
            if (obra == null)
            {
                throw new ArgumentNullException(
                    nameof(obra));
            }

            obrasConstruccion.Add(obra);
        }

        public bool EliminarObraConstruccion(
            ObraConstruccion obra)
        {
            if (obra == null)
                return false;

            return obrasConstruccion.Remove(obra);
        }

    }
}