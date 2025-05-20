using UnityEngine;


public class GameManager : MonoBehaviour
{
    
    public RelayManager relayManager;

    private async void Start()
    {
        // Crear un relay host
         
        // Opcional: Compartir el join code con otros jugadores.         
    }

    public async void JoinGame(string joinCode)
    {
        // Unirse a un relay usando el código de unión
        //await relayManager.JoinRelay(joinCode);
    }
       
}
