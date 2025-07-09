using Unity.Netcode;
using UnityEngine;

/* MIRAR DE QUITARLO*/

public class SceneChangeHandler : NetworkBehaviour
{
    private void OnEnable()
    {       
        NetworkManager.Singleton.SceneManager.OnSceneEvent += HandleSceneEvent;// para cuando se abandona una escena
    
    }

    private void OnDisable()
    {
        NetworkManager.Singleton.SceneManager.OnSceneEvent -= HandleSceneEvent;
    }
    private void HandleSceneEvent(SceneEvent sceneEvent)
    {
        if (sceneEvent.SceneEventType == SceneEventType.Unload)
        {
            Debug.Log("Se inició la descarga de la escena: " + sceneEvent.SceneName);
            if (IsServer)
            {
                foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
                {
                    var playerObject = client.PlayerObject;
                    if (playerObject != null && playerObject.IsSpawned)
                    {
                        playerObject.Despawn(true); // true = destruye el GameObject
                    }
                }

            }

        }
    }


   
    

}





