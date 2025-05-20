using UnityEngine;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using TMPro;
using Unity.Services.Core;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance { get; private set; }
    private string joinCode;
    [SerializeField]
    private TextMeshProUGUI textoUnion;
    [SerializeField]
    private TMP_InputField CodUnionIntro;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public async Task InitializeUnityServices(bool codigoJoin)
    {
        try
        {
            await Unity.Services.Core.UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            Debug.Log("Unity Services inicializado y jugador autenticado.");
            if(codigoJoin == false) {
                joinCode = await CreateRelayHost(4);
                textoUnion.text = joinCode;
            }
            else
            {
                joinCode = CodUnionIntro.text;
            }
            Debug.Log($"Join Code: {joinCode}");
            
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error inicializando Unity Services: {e.Message}");
        }
    }

    public async Task<string> CreateRelayHost(int maxPlayers)
    {        
        try
        {               
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers); // Máximo de conexiones
            
            if (allocation != null)
             {
                 int port = allocation.RelayServer?.Port ?? 0;
                 joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId); // Extraer el Guid de Allocation
                 Debug.Log($"Relay creado con Código de unión: {joinCode}");
                 var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                 transport.SetRelayServerData(
                 allocation.RelayServer.IpV4,
                 (ushort)port,
                 allocation.AllocationIdBytes,
                 allocation.Key,
                 allocation.ConnectionData
             );
                 NetworkManager.Singleton.StartHost();
                 return joinCode;                
            }
             else
             {
                 Debug.LogError("Error: La asignación de Relay es nula.");
                 return null;
             }            
         }
         catch (RelayServiceException e)
         {
             Debug.LogError($"Error al crear Relay: {e.Message}");
             return null;
         }     
              
    }

    public async Task JoinRelay(string joinCode) // con parametro es distinta de la de abajo
    {
        Debug.Log("el joinCode:" + joinCode);
        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                await InitializeUnityServices(true);
            }
            // Obtener la asignación de Relay basada en el joinCode
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            int port = joinAllocation.RelayServer?.Port ?? 0;
            transport.SetRelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );
            // Iniciar cliente en la red
            NetworkManager.Singleton.StartClient();
            Debug.Log("Cliente conectado al Relay exitosamente.");           
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Error al unirse al Relay: {e.Message}");
        }
    }


    public async Task JoinRelay()
    {        
        string ClientjoinCode = CodUnionIntro.text;
        Debug.Log("intento conectarme como cliente con codigo "+ ClientjoinCode);
        if (!string.IsNullOrEmpty(ClientjoinCode))
        {
            await RelayManager.Instance.JoinRelay(ClientjoinCode);
            Debug.Log($"Conectado al Relay usando el Join Code: {ClientjoinCode}");
        }
        else
        {
            Debug.LogError("El Join Code está vacío. Introduce un código válido.");
        }

    }
}

