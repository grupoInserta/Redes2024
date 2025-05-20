using Unity.Netcode;
using UnityEngine;
using Cinemachine;

public class SceneChangeHandler : NetworkBehaviour
{
    private string sceneName;
    private PlayerController miPlayerController;
    private PlayerManager miPlayerManager;
    public bool ListoCamara { get; set; }// se refiere a que la camara ya ha sido encontrada y adjudicada

    //public CinemachineVirtualCamera virtualCamera;
    public CinemachineFreeLook freeLookCamera;
    //public Camera cam { get; set; }

    private void OnEnable()
    {
        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnSceneLoaded;
        miPlayerController = gameObject.GetComponent<PlayerController>();
        miPlayerManager = gameObject.GetComponent<PlayerManager>();
        ListoCamara = false;
    }

    private void Start()
    {
        // eventualment cambiar el contenido de OnSceneLoaded quí.
    }

    private void OnDisable()
    {
        NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnSceneLoaded;
    }

    [ClientRpc]
    void UpdatePositionClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            miPlayerManager.colocarInicio();
        }
    }

    [ServerRpc]
    private void RequestPositionUpdateServerRpc(ulong clientId, string sceneName)
    {
        // Enviar nueva posición al cliente
        UpdatePositionClientRpc(clientId);
    }   

    private void OnSceneLoaded(ulong clientId, string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode)
    {
        sceneName = gameObject.scene.name;
        miPlayerManager.establecerTipoEscena(sceneName);

            if (sceneName == "MenuInicio")
            {// desactivar playerController
                miPlayerController.Estado = "Desactivado";
            }  else if(sceneName == "Nivel1" || sceneName == "Nivel2")
            {             
                miPlayerManager.ObtenerInventario();
                if (IsOwner)
                {
                    // encontrar camara:
                    // Encuentra la FreeLook Camera en la escena si no está asignada
                    if (freeLookCamera == null)
                    {
                        freeLookCamera = FindObjectOfType<CinemachineFreeLook>();                       
                    }

                    if (freeLookCamera != null)
                    {
                        // Asigna el jugador local como el objetivo de la FreeLook Camera
                        Debug.Log("camara encontrada***");
                        freeLookCamera.Follow = transform.GetChild(0).transform;               
                        freeLookCamera.LookAt = transform.GetChild(0).transform;
                        freeLookCamera.m_YAxis.Value = 0.7f;
                        ListoCamara = true;
                    
                        Debug.Log($"Escena {sceneName} completamente cargada para el cliente local.");
                        RequestPositionUpdateServerRpc(NetworkManager.Singleton.LocalClientId, sceneName);
                    }
                }            
            } 
            else if(sceneName == "Victoria" || sceneName == "Derrota")
            {
            Debug.Log("FINAL");
            }
            
    }

}





