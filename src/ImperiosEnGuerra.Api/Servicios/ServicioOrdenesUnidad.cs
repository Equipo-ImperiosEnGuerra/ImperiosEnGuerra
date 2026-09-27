using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Api.Servicios;

/// <summary>
/// Encapsula el inicio, finalización y cancelación de la orden activa de una unidad.
/// </summary>
public sealed class ServicioOrdenesUnidad
{
    //Delega en la unidad la validación para impedir órdenes incompatibles al mismo tiempo.
    public bool Iniciar(
        Unidad unidad,
        TipoAccionJuego tipo)
        {
            if(unidad == null)
            return false;
            
            return unidad.IntentarIniciarOrden(tipo);
        }

        //Libera la unidad cuando la acción termina normalmente.
        public void Completar(
            Unidad unidad)
    {
        if (unidad == null)
        return;

        unidad.CompletarOrden();
    }

    //Restablece la unidad cuando una acción se interrumpe antes de terminar.
    public void Cancelar(
        Unidad unidad)
    {
        if(unidad == null)
        return;

        unidad.CancelarOrden();
    }
    }
