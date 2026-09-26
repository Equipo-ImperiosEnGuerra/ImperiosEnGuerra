using ImperiosEnGuerra.Controladores.Red;
using ImperiosEnGuerra.Vistas;
using UnityEngine;
using UnityEngine.InputSystem;

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
        private VistaMenuPausa vistaPausa;
        private bool partidaEnCurso;
        private bool cambioPausaEnCurso;

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

            GameObject objetoPausa =
                new GameObject(
                    "MenuPausa",
                    typeof(RectTransform));

            vistaPausa =
                objetoPausa.AddComponent<VistaMenuPausa>();

            controladorSeleccion?.BloquearInteraccion();
            vistaHud?.OcultarResultadoFinal();

            vistaMenu.JugarSolicitado += Jugar;
            vistaMenu.InstruccionesSolicitadas += MostrarInstrucciones;
            vistaMenu.VolverSolicitado += MostrarPrincipal;
            vistaMenu.SalirSolicitado += Salir;

            if (vistaPausa != null)
            {
                vistaPausa.ReanudarSolicitado +=
                    ReanudarPartida;

                vistaPausa.SalirSolicitado +=
                    Salir;
            }

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

            if (vistaPausa != null)
            {
                vistaPausa.ReanudarSolicitado -=
                    ReanudarPartida;

                vistaPausa.SalirSolicitado -=
                    Salir;
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

        private void Update()
        {
            if (!partidaEnCurso ||
                conexionApi == null ||
                conexionApi.PartidaFinalizada ||
                Keyboard.current == null ||
                !Keyboard.current.escapeKey.wasPressedThisFrame ||
                cambioPausaEnCurso)
            {
                return;
            }

            if (vistaPausa != null &&
                vistaPausa.Visible)
            {
                ReanudarPartida();
            }
            else
            {
                PausarPartida();
            }
        }

        private void Jugar()
        {
            if (conexionApi == null)
            {
                vistaMenu.MostrarEstado(
                    "No se pudo preparar la partida.",
                    true);

                return;
            }

            vistaMenu.EstablecerCargando(true);
            vistaMenu.MostrarEstado(
                "Preparando una nueva partida...");

            conexionApi.IniciarPartidaDesdeMenu();
        }

        private void PartidaIniciada()
        {
            partidaEnCurso = true;
            cambioPausaEnCurso = false;

            vistaMenu.EstablecerCargando(false);
            vistaMenu.Ocultar();
            vistaPausa?.Ocultar();

            controladorSeleccion?.DesbloquearInteraccion();
        }

        private void InicioFallido(
            string mensaje)
        {
            partidaEnCurso = false;
            cambioPausaEnCurso = false;

            controladorSeleccion?.BloquearInteraccion();

            vistaMenu.MostrarPrincipal();
            vistaMenu.EstablecerCargando(false);
            vistaMenu.MostrarEstado(
                string.IsNullOrWhiteSpace(mensaje)
                    ? "No fue posible iniciar la partida. Inténtalo de nuevo."
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

        private void PausarPartida()
        {
            if (!partidaEnCurso ||
                conexionApi == null ||
                vistaPausa == null ||
                cambioPausaEnCurso)
            {
                return;
            }

            cambioPausaEnCurso =
                true;

            controladorSeleccion?
                .BloquearInteraccion();

            vistaPausa.Mostrar();

            conexionApi.PausarPartidaDesdeMenu(
                exito =>
                {
                    cambioPausaEnCurso =
                        false;

                    if (!exito)
                    {
                        vistaPausa.Ocultar();

                        controladorSeleccion?
                            .DesbloquearInteraccion();
                    }
                });
        }

        private void ReanudarPartida()
        {
            if (!partidaEnCurso ||
                conexionApi == null ||
                vistaPausa == null ||
                cambioPausaEnCurso)
            {
                return;
            }

            cambioPausaEnCurso =
                true;

            conexionApi.ReanudarPartidaDesdeMenu(
                exito =>
                {
                    cambioPausaEnCurso =
                        false;

                    if (!exito)
                        return;

                    vistaPausa.Ocultar();

                    controladorSeleccion?
                        .DesbloquearInteraccion();
                });
        }

        private void VolverAlMenu()
        {
            partidaEnCurso = false;
            cambioPausaEnCurso = false;
            vistaPausa?.Ocultar();

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

        private void Salir()
        {
            partidaEnCurso = false;
            cambioPausaEnCurso = false;

            // Detiene también la simulación remota antes de cerrar el build.
            conexionApi?.PrepararRegresoAlMenu();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying =
                false;
#else
            Application.Quit();
#endif
        }
    }
}
