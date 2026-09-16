using System;
using System.Collections.Generic;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Core
{
    public class Jugador
    {
        private readonly List<Unidad> unidades;
        private readonly List<Edificio> edificios;

        public string Nombre { get; }
        public TipoJugador Tipo { get; }
        public Mapa Mapa { get; }
        public RecursosJugador Recursos { get; }

        public IReadOnlyList<Unidad> Unidades
        {
            get { return unidades.AsReadOnly(); }
        }

        public IReadOnlyList<Edificio> Edificios
        {
            get { return edificios.AsReadOnly(); }
        }

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
        }

        public void AgregarUnidad(Unidad unidad)
        {
            if (unidad == null)
            {
                throw new ArgumentNullException(nameof(unidad));
            }

            unidades.Add(unidad);
        }

        public bool EliminarUnidad(Unidad unidad)
        {
            if (unidad == null)
            {
                return false;
            }

            return unidades.Remove(unidad);
        }

        public void AgregarEdificio(Edificio edificio)
        {
            if (edificio == null)
            {
                throw new ArgumentNullException(nameof(edificio));
            }

            edificios.Add(edificio);
        }

        public bool EliminarEdificio(Edificio edificio)
        {
            if (edificio == null)
            {
                return false;
            }

            return edificios.Remove(edificio);
        }
    }
}