using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Api.Servicios;

public sealed class ServicioOrdenesUnidad
{
    public bool Iniciar(
        Unidad unidad,
        TipoAccionJuego tipo)
        {
            if(unidad == null)
            return false;
            
            return unidad.IntentarIniciarOrden(tipo);
        }

        public void Completar(
            Unidad unidad)
    {
        if (unidad == null)
        return;

        unidad.CompletarOrden();
    }

    public void Cancelar(
        Unidad unidad)
    {
        if(unidad == null)
        return;

        unidad.CancelarOrden();
    }
    }
