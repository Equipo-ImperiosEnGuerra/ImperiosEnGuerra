using System;
using UnityEngine;
using UnityEngine.UI;

namespace ImperiosEnGuerra.Vistas
{
    /// <summary>Muestra datos y opciones de interfaz; no consulta la API ni ejecuta acciones.</summary>
    public class VistaHud : MonoBehaviour
    {
        [SerializeField] private Text recursos;
        [SerializeField] private Text seleccion;
        [SerializeField] private Text mensaje;
        [SerializeField] private Button mover;
        [SerializeField] private Button recolectar;
        [SerializeField] private Button construir;
        [SerializeField] private Button entrenar;
        [SerializeField] private Button atacar;

        public event Action<string> AccionSolicitada;

        private void OnEnable()
        {
            if (mover != null) mover.onClick.AddListener(SolicitarMover);
            if (recolectar != null) recolectar.onClick.AddListener(SolicitarRecolectar);
            if (construir != null) construir.onClick.AddListener(SolicitarConstruir);
            if (entrenar != null) entrenar.onClick.AddListener(SolicitarEntrenar);
            if (atacar != null) atacar.onClick.AddListener(SolicitarAtacar);
        }

        private void OnDisable()
        {
            if (mover != null) mover.onClick.RemoveListener(SolicitarMover);
            if (recolectar != null) recolectar.onClick.RemoveListener(SolicitarRecolectar);
            if (construir != null) construir.onClick.RemoveListener(SolicitarConstruir);
            if (entrenar != null) entrenar.onClick.RemoveListener(SolicitarEntrenar);
            if (atacar != null) atacar.onClick.RemoveListener(SolicitarAtacar);
        }

        private void SolicitarMover() => AccionSolicitada?.Invoke("Mover");
        private void SolicitarRecolectar() => AccionSolicitada?.Invoke("Recolectar");
        private void SolicitarConstruir() => AccionSolicitada?.Invoke("Construir");
        private void SolicitarEntrenar() => AccionSolicitada?.Invoke("Entrenar");
        private void SolicitarAtacar() => AccionSolicitada?.Invoke("Atacar");

        public void MostrarRecursos(int oro, int madera, int comida)
        {
            if (recursos != null)
                recursos.text = $"Oro: {oro} | Madera: {madera} | Comida: {comida}";
        }

        public void MostrarSeleccion(EntidadSeleccionableVista entidad)
        {
            if (seleccion == null) return;
            seleccion.text = entidad == null ? "Sin selección" :
                $"{entidad.TipoLogico}\nPropietario: {entidad.Propietario}\nCoordenada: ({entidad.X},{entidad.Y})" +
                (entidad.Propietario == "Maquina" ? " — Enemigo" : "");
        }

        public void MostrarOpciones(bool puedeMover, bool puedeRecolectar, bool puedeConstruir,
            bool puedeEntrenar, bool puedeAtacar)
        {
            if (mover != null) mover.gameObject.SetActive(puedeMover);
            if (recolectar != null) recolectar.gameObject.SetActive(puedeRecolectar);
            if (construir != null) construir.gameObject.SetActive(puedeConstruir);
            if (entrenar != null) entrenar.gameObject.SetActive(puedeEntrenar);
            if (atacar != null) atacar.gameObject.SetActive(puedeAtacar);
        }

        public void MostrarMensaje(string texto, bool error = false)
        {
            if (mensaje == null) return;
            mensaje.text = texto;
            mensaje.color = error ? new Color(1f, 0.55f, 0.55f) : Color.white;
        }
    }
}
