using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MultiplayerManager : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button unirseButton;
    [SerializeField] private Button jugarButton;
    [SerializeField] private TextMeshProUGUI LobbyTexto;
    [SerializeField] private TextMeshProUGUI TextoCargandoNivel;
    [SerializeField] private TextMeshProUGUI textoUnion;
    [SerializeField] private TMP_InputField CodUnionIntro;
    [SerializeField] private GameObject[] playerPrefabs;
    private PlayerSpawner playerSpawner;
    private CargarEscenasMulti cargarEscenasMulti;

    private void Start()
    {
        Debug.Log("INICIO HOST");   
        playerSpawner = gameObject.GetComponent<PlayerSpawner>();
        cargarEscenasMulti = gameObject.GetComponent<CargarEscenasMulti>();        
        // Asigna los eventos de los botones
        hostButton.onClick.AddListener(StartHost);
        clientButton.onClick.AddListener(StartClient);
        unirseButton.onClick.AddListener(Unirse);
        jugarButton.onClick.AddListener(Jugar);
    }

    private async void StartHost()
    {
        try
        {
            Debug.Log("Inicializando Relay para Host...");
            // Configurar Relay
            //string relaySetupSuccess = await RelayManager.Instance.CreateRelayHost(4);
            await RelayManager.Instance.InitializeUnityServices(false);
            hostButton.gameObject.SetActive(false);
            clientButton.gameObject.SetActive(false);
            CodUnionIntro.gameObject.SetActive(false);
            unirseButton.gameObject.SetActive(true);
            
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error al iniciar Host: {ex.Message}");
        }
    }

    public async void StartClient()
    {       
        hostButton.gameObject.SetActive(false);
        clientButton.gameObject.SetActive(false);
        CodUnionIntro.gameObject.SetActive(false);
        string joinCode = CodUnionIntro.text;
        Debug.Log("INICIO CLIENTE: " + joinCode);
        await RelayManager.Instance.JoinRelay(joinCode);
        textoUnion.gameObject.SetActive(false);
        unirseButton.gameObject.SetActive(true);        
    }

    public void mostrarJugar()
    {
        jugarButton.gameObject.SetActive(true);
    }

    private void Jugar()
    {
        cargarEscenasMulti.CargarEscenaAleat();        
        TextoCargandoNivel.gameObject.SetActive(true);
        LobbyTexto.gameObject.SetActive(false);
        jugarButton.gameObject.SetActive(false);
    }

    private void Unirse()
    {
        if (NetworkManager.Singleton.IsClient)
        {
            LobbyTexto.gameObject.SetActive(true);
            playerSpawner.SpawnPlayer();
            unirseButton.gameObject.SetActive(false);
            textoUnion.gameObject.SetActive(false);           
        }        
    }
    
}
