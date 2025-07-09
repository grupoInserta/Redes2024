using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoriaDerrota : NetworkBehaviour
{
    public void EndGame(ulong winnerClientId)
    {
        Debug.Log("winnerClientId: " + winnerClientId);
        foreach (var client in NetworkManager.Singleton.ConnectedClients)
        {
            ulong clientId = client.Key;
            if (clientId == winnerClientId && winnerClientId != 1000)
            {
                SendPlayerToScene(clientId, "Victoria");
            }
            else if (winnerClientId != 1000)
            {
                SendPlayerToScene(clientId, "Derrota");
            }
            else
            {
                SendPlayerToScene(clientId, "Empate");
            }
        }
    }

    private void SendPlayerToScene(ulong _clientId, string sceneName)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            SendSceneChangeClientRpc(_clientId, sceneName);
        }
    }

    [ClientRpc]
    private void SendSceneChangeClientRpc(ulong targetClientId, string sceneName, ClientRpcParams clientRpcParams = default)
    {
        if (NetworkManager.Singleton.LocalClientId == targetClientId)
        {
            //NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            SceneManager.LoadScene(sceneName);
        }
    }
}
