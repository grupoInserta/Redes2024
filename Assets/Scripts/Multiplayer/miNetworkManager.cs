using UnityEngine;
using Unity.Netcode;

public class miNetworkManager : NetworkBehaviour
{    
    public GameObject[] JugadorPrefabs;
    [SerializeField]


    public void SpawnPlayer(ulong clientId)
    {        
        GameObject playerPrefab = JugadorPrefabs[clientId];/* :: :: error al reiniciar*/
        GameObject playerInstance = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }

}