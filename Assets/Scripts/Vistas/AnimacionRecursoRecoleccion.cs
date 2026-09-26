using UnityEngine;

namespace ImperiosEnGuerra.Vistas
{
    /// <summary>
    /// Feedback visual puramente gráfico para un nodo que está siendo
    /// recolectado. No modifica cantidades ni reglas del Modelo.
    /// </summary>
    public sealed class AnimacionRecursoRecoleccion : MonoBehaviour
    {
        [SerializeField, Min(0f)]
        private float amplitudSacudida = 0.07f;

        [SerializeField, Min(0f)]
        private float amplitudPulso = 0.045f;

        [SerializeField, Min(0.1f)]
        private float frecuencia = 9f;

        private Vector3 posicionBase;
        private Vector3 escalaBase;
        private bool inicializada;
        private bool recolectando;
        private bool pausada;

        public bool Recolectando =>
            recolectando;

        private void Awake()
        {
            Inicializar();
        }

        private void OnEnable()
        {
            Inicializar();
        }

        public void EstablecerRecolectando(
            bool activo)
        {
            Inicializar();

            if (recolectando == activo)
                return;

            recolectando =
                activo;

            if (!recolectando)
            {
                Restaurar();
            }
        }

        public void EstablecerPausada(
            bool valor)
        {
            pausada =
                valor;
        }

        private void Update()
        {
            if (!recolectando ||
                pausada)
            {
                return;
            }

            Inicializar();

            float fase =
                Time.unscaledTime *
                frecuencia;

            float sacudidaX =
                Mathf.Sin(
                    fase) *
                amplitudSacudida;

            float sacudidaY =
                Mathf.Sin(
                    fase * 1.73f) *
                amplitudSacudida *
                0.45f;

            float pulso =
                1f +
                Mathf.Sin(
                    fase * 0.82f) *
                amplitudPulso;

            transform.localPosition =
                posicionBase +
                new Vector3(
                    sacudidaX,
                    sacudidaY,
                    0f);

            transform.localScale =
                escalaBase *
                pulso;
        }

        private void OnDisable()
        {
            Restaurar();
        }

        private void Inicializar()
        {
            if (inicializada)
                return;

            posicionBase =
                transform.localPosition;

            escalaBase =
                transform.localScale;

            inicializada =
                true;
        }

        private void Restaurar()
        {
            if (!inicializada)
                return;

            transform.localPosition =
                posicionBase;

            transform.localScale =
                escalaBase;
        }
    }
}
