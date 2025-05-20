using Unity.Netcode;
using UnityEngine;

public class NetworkManagerHandler : MonoBehaviour
{
    private void Awake()
    {
        // Si ya existe un NetworkManager.Singleton que no sea este, destruye este objeto
        if (NetworkManager.Singleton != null && NetworkManager.Singleton != GetComponent<NetworkManager>())
        {
            Destroy(gameObject); // Elimina el NetworkManager duplicado
            return;
        }

        // Haz que este NetworkManager persista entre escenas
        DontDestroyOnLoad(gameObject);
    }
}
