using UnityEngine;
using UnityEngine.UI;

public class DamageFlashEffect : MonoBehaviour
{
    [Header("Referencia al Image rojo del Canvas")]
    private Image damageImage;

    [Header("Par�metros del flash")]
    public float flashDuration = 0.2f;  // Tiempo que dura el flash inicial
    public float fadeSpeed = 2f;        // Velocidad de desvanecimiento

    private Color targetColor = new Color(1, 0, 0, 0); // Color final (transparente)
    private bool isFlashing = false;

    private void Start()
    {
        if (damageImage != null)
            damageImage.color = targetColor; // Aseg�rate de que empieza invisible
    }

    private void Update()
    {
        if (isFlashing)
        {
            damageImage.color = Color.Lerp(damageImage.color, targetColor, fadeSpeed * Time.deltaTime);
            if (damageImage.color.a <= 0.01f)
            {
                damageImage.color = targetColor;
                isFlashing = false;
                damageImage.enabled = true;
            }
        }
    }

    /// <summary>
    /// Llama a este m�todo cuando el jugador reciba da�o.
    /// </summary>
    public void MostrarFlash(Image rojo)
    {
        if (rojo == null) return;

        damageImage = rojo;
        // Establece un rojo fuerte con opacidad total
        damageImage.color = new Color(1, 0, 0, 0.6f);
        isFlashing = true;
    }
}
