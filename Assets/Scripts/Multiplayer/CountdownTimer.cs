using UnityEngine;
using TMPro;
using UnityEngine;
using Unity.Netcode;

public class CountdownTimer : NetworkBehaviour
{
    public TextMeshProUGUI countdownText; // Referencia al texto de la cuenta atrás
    [SerializeField]
    public int countdownTime; // Tiempo inicial de la cuenta atrás en segundos
    public bool tiempoterminado { get; set; }
    private void Start()
    {
        // Inicia la cuenta atrás
        StartCoroutine(StartCountdown());
        tiempoterminado = false;
    }

    [ClientRpc]
    public void mostrarTiempoClientRpc(string textoReloj, bool _tiempoterminado)
    {
        countdownText.text = textoReloj;
        tiempoterminado = _tiempoterminado;
    }

    [ServerRpc]
    public void mostrarTiempoServerRpc(string tiempo, bool _tiempoterminado)
    {
        mostrarTiempoClientRpc( tiempo, _tiempoterminado);
        tiempoterminado = _tiempoterminado;
    }       

    private System.Collections.IEnumerator StartCountdown()
    {
        int currentTime = countdownTime;

        while (currentTime > 0)
        {
            // Actualiza el texto con el tiempo restante
            string textoReloj = ConvertSecondsToMinutes(currentTime);
            if (IsOwner)
            {
                mostrarTiempoServerRpc(textoReloj, tiempoterminado);
            }          

            // Espera un segundo
            yield return new WaitForSeconds(1);
            // Reduce el tiempo
            currentTime--;
        }

        // Cuando la cuenta atrás llega a cero
        countdownText.text = "¡Tiempo terminado!";
        tiempoterminado = true;
    }

    private string ConvertSecondsToMinutes(int seconds)
    {
        int minutes = seconds / 60; // Calcula los minutos
        int remainingSeconds = seconds % 60; // Calcula los segundos restantes

        // Devuelve el formato MM:SS
        return string.Format("{0:00}:{1:00}", minutes, remainingSeconds);
    }
}

