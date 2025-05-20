using Unity.Netcode;
using UnityEngine;


public class ConnectionManager : MonoBehaviour
{
    // Referencia a un Text UI en la escena
    CargarEscenasMulti cargaEscenas;

    private void Start()
    {       
        // Suscribirse al evento de desconexión o fallo de conexión
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        NetworkManager.Singleton.OnTransportFailure += OnTransportFailure;
        cargaEscenas = gameObject.GetComponent<CargarEscenasMulti>();// es el script de caragr esecnas individual
    }

    private void OnDestroy()
    {
        // Desuscribirse de los eventos para evitar problemas de memoria
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            NetworkManager.Singleton.OnTransportFailure -= OnTransportFailure;
        }
    }

    // Método que se llama cuando se pierde la conexión con el servidor
    private void OnClientDisconnected(ulong clientId)
    {
        DisplayErrorMessage("Disconnected from server.");
        NetworkManager.Singleton.Shutdown();
        cargaEscenas.CargarEscenaNombre("Error");
        
    }

    // Método que se llama cuando hay un fallo de transporte (problemas de conexión)
    private void OnTransportFailure()
    {
        DisplayErrorMessage("Connection failed. Please check your network and try again.");
    }

    // Método para mostrar el mensaje de error
    private void DisplayErrorMessage(string message)
    {      
            Debug.Log(message);        
    }
}


