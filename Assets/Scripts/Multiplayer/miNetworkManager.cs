using UnityEngine;
using Unity.Netcode;

public class miNetworkManager : NetworkBehaviour
{    
    public GameObject[] JugadorPrefabs;
    [SerializeField]
    public GameObject IniciarJuegoCanvas;   

    public void Start()
    {
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
       
    }
    /*
    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
    */

    private void OnServerStarted() // se ejecuta una vez
    {
        if (NetworkManager.Singleton.IsHost)
        {
            
        }
        else
        {
            
        }
    }




    private void OnClientConnected(ulong clientId) // 
    {
    
        if (NetworkManager.Singleton.IsServer )
        {
           
            SpawnPlayer(clientId);
        }
        else
        {
            Debug.Log("Cliente conectado.");// solo el host puede spawnear   

        }
        
    }

    private void SpawnPlayer(ulong clientId)
    {
         if(clientId > 0) {
            IniciarJuegoCanvas.SetActive(true);
        }
        GameObject playerPrefab = JugadorPrefabs[clientId];
        GameObject playerInstance = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        if(clientId == 0)
        {
            playerInstance.transform.transform.Translate(1f, 0, 0);           
        }
        else
        {
            playerInstance.transform.transform.Translate(-1f, 0, 0);
        }
     
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }

}