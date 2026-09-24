using ImperiosEnGuerra.Controladores.Red;
using ImperiosEnGuerra.Vistas;
using UnityEngine;

namespace ImperiosEnGuerra.Controladores
{
    /// <summary>
    /// Coordina el menú inicial con la API y la navegación de la Vista.
    /// No contiene reglas del juego.
    /// </summary>
    public sealed class ControladorMenuInicial : MonoBehaviour
    {
        private ControladorConexionApi conexionApi;
        private ControladorSeleccion controladorSeleccion;
        private VistaHud vistaHud;
        private VistaMenuInicial vistaMenu;

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CrearBootstrap()
        {
            if (Object.FindFirstObjectByType<ControladorConexionApi>() == null ||
                Object.FindFirstObjectByType<ControladorMenuInicial>() != null)
            {
                return;
            }

            GameObject objeto =
                new GameObject(
                    "ControladorMenuInicial");

            objeto.AddComponent<ControladorMenuInicial>();
        }

        private void Awake()
        {
            conexionApi =
                Object.FindFirstObjectByType<ControladorConexionApi>();

            controladorSeleccion =
                Object.FindFirstObjectByType<ControladorSeleccion>();

            vistaHud =
                Object.FindFirstObjectByType<VistaHud>();

            GameObject objetoVista =
                new GameObject(
                    "MenuInicial",
                    typeof(RectTransform));

            vistaMenu =
                objetoVista.AddComponent<VistaMenuInicial>();

            controladorSeleccion?.BloquearInteraccion();
            vistaHud?.OcultarResultadoFinal();

            vistaMenu.JugarSolicitado += Jugar;
            vistaMenu.InstruccionesSolicitadas += MostrarInstrucciones;
            vistaMenu.VolverSolicitado += MostrarPrincipal;
            vistaMenu.SalirSolicitado += Salir;

            if (vistaHud != null)
            {
                vistaHud.VolverMenuSolicitado +=
                    VolverAlMenu;

                vistaHud.SalirSolicitado +=
                    Salir;
            }

            if (conexionApi != null)
            {
                conexionApi.PartidaIniciadaDesdeMenu +=
                    PartidaIniciada;

                conexionApi.InicioPartidaFallido +=
                    InicioFallido;
            }

            vistaMenu.MostrarPrincipal();
        }

        private void OnDestroy()
        {
            if (vistaMenu != null)
            {
                vistaMenu.JugarSolicitado -= Jugar;
                vistaMenu.InstruccionesSolicitadas -= MostrarInstrucciones;
                vistaMenu.VolverSolicitado -= MostrarPrincipal;
                vistaMenu.SalirSolicitado -= Salir;
            }

            if (vistaHud != null)
            {
                vistaHud.VolverMenuSolicitado -=
                    VolverAlMenu;

                vistaHud.SalirSolicitado -=
                    Salir;
            }

            if (conexionApi != null)
            {
                conexionApi.PartidaIniciadaDesdeMenu -=
                    PartidaIniciada;

                conexionApi.InicioPartidaFallido -=
                    InicioFallido;
            }
        }

        private void Jugar()
        {
            if (conexionApi == null)
            {
                vistaMenu.MostrarEstado(
                    "No se encontró el controlador de conexión con la API.",
                    true);

                return;
            }

            vistaMenu.EstablecerCargando(true);
            vistaMenu.MostrarEstado(
                "Conectando con la API e iniciando partida...");

            conexionApi.IniciarPartidaDesdeMenu();
        }

        private void PartidaIniciada()
        {
            vistaMenu.EstablecerCargando(false);
            vistaMenu.Ocultar();

            controladorSeleccion?.DesbloquearInteraccion();
        }

        private void InicioFallido(
            string mensaje)
        {
            controladorSeleccion?.BloquearInteraccion();

            vistaMenu.MostrarPrincipal();
            vistaMenu.EstablecerCargando(false);
            vistaMenu.MostrarEstado(
                string.IsNullOrWhiteSpace(mensaje)
                    ? "No fue posible iniciar la partida."
                    : mensaje,
                true);
        }

        private void MostrarInstrucciones()
        {
            vistaMenu.MostrarInstrucciones();
        }

        private void MostrarPrincipal()
        {
            vistaMenu.MostrarPrincipal();
        }

        private void VolverAlMenu()
        {
            // No se recarga la escena: en Editor eso podía dejar el flujo de
            // Play Mode en un estado inesperado. Se restablece únicamente la
            // presentación y el controlador de conexión.
            controladorSeleccion?.BloquearInteraccion();
            conexionApi?.PrepararRegresoAlMenu();

            vistaHud?.OcultarResultadoFinal();

            if (vistaMenu != null)
            {
                vistaMenu.EstablecerCargando(false);
                vistaMenu.MostrarPrincipal();
                vistaMenu.MostrarEstado("");
            }
        }

        private static void Salir()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying =
                false;
#else
            Application.Quit();
#endif
        }
    }
}
