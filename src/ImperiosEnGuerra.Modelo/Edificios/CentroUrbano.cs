using System;
using System.Collections.Generic;
using System.Linq;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Edificios
{
    public class CentroUrbano : Edificio
    {
        private readonly object sincronizacion =
            new object();

        private readonly List<EntrenamientoPendiente>
            colaEntrenamiento =
                new List<EntrenamientoPendiente>();

        public bool EstaEntrenando
        {
            get
            {
                lock (sincronizacion)
                {
                    return colaEntrenamiento.Count > 0;
                }
            }
        }

        public string TipoUnidadEntrenando
        {
            get
            {
                lock (sincronizacion)
                {
                    return colaEntrenamiento.Count == 0
                        ? null
                        : colaEntrenamiento[0].TipoUnidad;
                }
            }
        }

        public IReadOnlyList<EntrenamientoPendiente> ColaEntrenamiento
        {
            get
            {
                lock (sincronizacion)
                {
                    return colaEntrenamiento
                        .ToArray();
                }
            }
        }

        public CentroUrbano(
            Coordenada coordenada)
            : base(coordenada)
        {
        }

        public EntrenamientoPendiente EncolarEntrenamiento(
            string tipoUnidad,
            Coordenada puntoReunion = null)
        {
            var pendiente =
                new EntrenamientoPendiente(
                    tipoUnidad,
                    puntoReunion);

            lock (sincronizacion)
            {
                colaEntrenamiento.Add(
                    pendiente);
            }

            return pendiente;
        }

        public bool EsPrimero(
            Guid entrenamientoId)
        {
            lock (sincronizacion)
            {
                return colaEntrenamiento.Count > 0 &&
                       colaEntrenamiento[0].Id ==
                       entrenamientoId;
            }
        }

        public ResultadoProgresoEntrenamiento AvanzarEntrenamiento(
            Guid entrenamientoId,
            int incremento)
        {
            lock (sincronizacion)
            {
                if (colaEntrenamiento.Count == 0 ||
                    colaEntrenamiento[0].Id != entrenamientoId)
                {
                    return ResultadoProgresoEntrenamiento.Fallido(
                        "La orden no está al frente de la cola.");
                }

                int progreso =
                    colaEntrenamiento[0]
                        .Avanzar(
                            incremento);

                return ResultadoProgresoEntrenamiento.Exitoso(
                    progreso);
            }
        }

        public bool CompletarEntrenamiento(
            Guid entrenamientoId)
        {
            lock (sincronizacion)
            {
                if (colaEntrenamiento.Count == 0 ||
                    colaEntrenamiento[0].Id != entrenamientoId ||
                    colaEntrenamiento[0].Progreso < 100)
                {
                    return false;
                }

                colaEntrenamiento.RemoveAt(0);
                return true;
            }
        }

        public bool CancelarEntrenamiento(
            Guid entrenamientoId)
        {
            lock (sincronizacion)
            {
                EntrenamientoPendiente pendiente =
                    colaEntrenamiento.FirstOrDefault(
                        p => p.Id == entrenamientoId);

                return pendiente != null &&
                       colaEntrenamiento.Remove(
                           pendiente);
            }
        }

        // Compatibilidad con llamadas históricas.
        public bool IniciarEntrenamiento(
            string tipoUnidad)
        {
            lock (sincronizacion)
            {
                if (colaEntrenamiento.Count > 0 ||
                    string.IsNullOrWhiteSpace(tipoUnidad))
                {
                    return false;
                }

                colaEntrenamiento.Add(
                    new EntrenamientoPendiente(
                        tipoUnidad,
                        null));

                return true;
            }
        }

        public void CompletarEntrenamiento()
        {
            lock (sincronizacion)
            {
                if (colaEntrenamiento.Count > 0)
                {
                    colaEntrenamiento.RemoveAt(0);
                }
            }
        }
    }
}
