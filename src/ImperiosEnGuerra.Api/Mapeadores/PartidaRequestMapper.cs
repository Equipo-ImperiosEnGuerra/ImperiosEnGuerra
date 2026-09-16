using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;

namespace ImperiosEnGuerra.Api.Mapeadores;

public static class PartidaRequestMapper
{
    public static Coordenada ConvertirCoordenada(CoordenadaRequest? request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        return new Coordenada(request.X, request.Y);
    }

    public static List<Recurso> ConvertirRecursos(
        List<RecursoInicialRequest>? recursos)
    {
        if (recursos == null)
        {
            throw new ArgumentNullException(nameof(recursos));
        }

        var resultado = new List<Recurso>();

        foreach (RecursoInicialRequest recurso in recursos)
        {
            if (recurso == null)
            {
                throw new ArgumentException(
                    "La lista no puede contener recursos nulos.",
                    nameof(recursos));
            }

            if (!Enum.TryParse(
                    recurso.Tipo,
                    true,
                    out TipoRecurso tipo))
            {
                throw new ArgumentException(
                    $"Tipo de recurso inválido: {recurso.Tipo}",
                    nameof(recursos));
            }

            resultado.Add(
                new Recurso(
                    tipo,
                    new Coordenada(recurso.X, recurso.Y)));
        }

        return resultado;
    }
}