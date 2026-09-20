using System;
using System.Linq;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Acciones
{
    public sealed class OperacionRecoleccion
    {
        public ResultadoAccion Ejecutar(
            Partida partida,
            SolicitudRecoleccion solicitud)
        {
            if (partida == null)
                return ResultadoAccion.Fallido("No hay una partida activa.");

            if (solicitud == null)
                return ResultadoAccion.Fallido(
                    "La solicitud de recolección es obligatoria.");

            if (partida.JugadorMaquina.Unidades.Any(
                u => u.Id == solicitud.AldeanoId))
            {
                return ResultadoAccion.Fallido(
                    "No se puede recolectar con una unidad de la máquina.");
            }

            Unidad unidad = partida.JugadorHumano.Unidades
                .FirstOrDefault(u => u.Id == solicitud.AldeanoId);

            if (unidad == null)
                return ResultadoAccion.Fallido(
                    "No existe una unidad humana con ese ID.");

            if (!(unidad is Aldeano))
                return ResultadoAccion.Fallido(
                     "La unidad seleccionada no es un Aldeano.");

            if (!unidad.Disponible)
                return ResultadoAccion.Fallido(
                    "El Aldeano no está disponible.");

            Coordenada objetivo = solicitud.Objetivo;

            if (objetivo == null)
                return ResultadoAccion.Fallido(
                    "El objetivo de recolección es obligatorio.");

            Mapa mapa = partida.JugadorHumano.Mapa;

            if (!mapa.EstaDentroDeLimites(objetivo))
                return ResultadoAccion.Fallido(
                    "El objetivo está fuera del mapa.");

            Recurso recurso = mapa.ObtenerRecursoEn(objetivo);

            if (recurso == null)
                return ResultadoAccion.Fallido(
                    "No existe un recurso en la posición indicada.");

            if (!Enum.IsDefined(typeof(TipoRecurso), recurso.Tipo))
                return ResultadoAccion.Fallido(
                    "El objetivo no contiene un tipo de recurso válido.");

            return ResultadoAccion.Exitoso(
                $"Recolección de {recurso.Tipo} preparada.");
        }
    }
}