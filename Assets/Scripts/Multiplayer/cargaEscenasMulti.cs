using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class cargaEscenasMulti : NetworkBehaviour
{
    [SerializeField]
    public string escena;

    public void CargarEscenaError()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("Error", LoadSceneMode.Single);
    }

    public void CargarEscenaNombre(string sceneName)
    {
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"La escena '{sceneName}' no existe o no está en la lista de escenas en el Build Settings.");
            return;
        }

        // Cargar la escena sin NetworkManager
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        // single significa que reemplaza totalmente a la escena actual
    }
}
