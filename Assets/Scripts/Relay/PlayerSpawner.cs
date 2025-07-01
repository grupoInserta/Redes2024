using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject[] playerPrefabs;
    [SerializeField] private Transform[] spawnPoints;
    private List <GameObject> jugadoresInstanciados;
    //private ulong contCliente = 0;
    public NetworkVariable<int> contCliente = new NetworkVariable<int>();
    private MultiplayerManager multiplayerManager;


    void Awake()
    {
        contCliente.Value = 0;
        multiplayerManager = gameObject.GetComponent<MultiplayerManager>();
        jugadoresInstanciados = new List<GameObject>();
    }

    public void SpawnPlayer() //
                              // DE UNO EN UNO CADA VEZ CLIC EN BOTON UNIRSE
    {
        // if (!IsOwner) return; // Asegúrate de que solo el cliente propietario pueda ejecutar esto

        // Verifica si el jugador ya está instanciado
        if (playerPrefabs[contCliente.Value] != null)
        {
            SpawnPlayerServerRpc(); // Llama al servidor para gestionar el spawn
        }
        else
        {
            Debug.LogWarning("El jugador ya ha sido spawneado.");
        }
    }

   

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayerServerRpc(ServerRpcParams serverRpcParams = default)
    {
        Debug.Log("ESPAWNEO EN EL SERVIDOR");
        ulong clientId = serverRpcParams.Receive.SenderClientId;

        GameObject playerObject = Instantiate(playerPrefabs[clientId], spawnPoints[clientId].position, Quaternion.identity);
        jugadoresInstanciados.Add(playerObject);
        // Asegúrate de que el objeto tenga un NetworkObject
        NetworkObject networkObject = playerObject.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            // Spawnear el jugador como PlayerObject
            networkObject.SpawnAsPlayerObject(clientId);
            Debug.Log($"Jugador spawneado para el cliente {clientId}");
        }
        playerObject.GetComponent<PlayerManager>().miClientId = clientId;

        //DatosGlobales.Instance.crearListaIdClientes(clientId);
        //playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        //playerInstance.GetComponent<NetworkObject>().SpawnWithOwnership(serverRpcParams.Receive.SenderClientId);
        Debug.Log("Jugador instanciado y spawneado por el servidor.ID:" + clientId);
        contCliente.Value++;
    }

   
    private void OnDestroy()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            // NetworkManager.Singleton.OnClientConnectedCallback -= SpawnPlayer;
        }
    }

    private void Update()
    {
        if (contCliente.Value > 1 && IsServer)
        {
            multiplayerManager.mostrarJugar();
        }
    }

}
