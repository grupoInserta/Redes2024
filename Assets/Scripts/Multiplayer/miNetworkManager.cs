using UnityEngine;
using Unity.Netcode;

public class miNetworkManager : NetworkBehaviour
{
    public GameObject[] JugadorPrefabs;
    [SerializeField]
    private ControladorNivel miControladorNivel;

    private void Awake()
    {
        miControladorNivel = GetComponent<ControladorNivel>();
    }

    public void SpawnPlayer(ulong clientId)
    {     // SOLO SE EJECUTA EN SERVIDOR   
        GameObject playerPrefab = JugadorPrefabs[clientId];
        GameObject playerInstance = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        //
        ulong localClientId = NetworkManager.Singleton.LocalClientId;
        Vector3 PosicionInicial = miControladorNivel.obtenerPosicionEnEscena(clientId);
        playerInstance.GetComponent<PlayerManager>().colocarInicio(PosicionInicial, clientId);
    }
}
