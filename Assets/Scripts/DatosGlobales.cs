using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DatosGlobales : NetworkBehaviour
{   public static DatosGlobales Instance { get; private set; }
    public List<string> EscenasJugadas = new List<string>();
    private Transform miCanvas; 
    public Button IrInicioButton;
    private string disconnectSceneName = "MenuInicio"; // Nombre de la escena de desconexión
   // private NetworkList<ulong> ListaIdsClientes;

    // Start is called before the first frame update
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Otra instancia de SingletonExample ya existe. Destruyendo esta.");
            Destroy(gameObject); // Destruir esta instancia duplicada
            return;
        }
        // Asignar esta instancia como la instancia única
        Instance = this;
        // Opcional: Si deseas que el objeto persista entre escenas
        DontDestroyOnLoad(gameObject);
        EscenasJugadas = new List<string>();
        miCanvas = transform.GetChild(0);
        IrInicioButton.onClick.AddListener(IrAInicio);
      //  ListaIdsClientes  = new NetworkList<ulong>();
    }
  
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Suscribirse a los eventos de conexión/desconexión
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;           
            // Rellenar la lista con los clientes ya conectados            
        }
    }
   

    private void OnClientConnected(ulong clientId)
    {
        // Añadir al cliente conectado a la lista sincronizada
        //ListaIdsClientes.Add(clientId);
        Debug.Log($"Cliente conectado: {clientId}");
    }

    private void OnClientDisconnected(ulong clientId)
    {
        // Quitar al cliente desconectado de la lista sincronizada
        //ListaIdsClientes.Remove(clientId);
        Debug.Log($"Cliente desconectado: {clientId}");
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null) return;

        if (IsServer)
        {
            // Desuscribirse de los eventos al destruirse
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;            
        }         
    }


    private void OnEnable()
    {
        // Suscribirse al evento de carga de escenas
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        OrganizarBotones();
    }

    private void OrganizarBotones()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName != "MenuInicio")
        {
            miCanvas.transform.GetChild(0).gameObject.SetActive(false);
            miCanvas.transform.GetChild(1).gameObject.SetActive(false);
            IrInicioButton.gameObject.SetActive(true);
        }
        else
        {
            IrInicioButton.gameObject.SetActive(false);
            miCanvas.transform.GetChild(0).gameObject.SetActive(true);
            miCanvas.transform.GetChild(1).gameObject.SetActive(true);
        }

    }

    private void Start()
    {
        OrganizarBotones();
    }

    public short AdjuntarEscena(string _escena) // NO SE REFIERE A LA CARGA, Solo  mete la escena  en la lista de las ya jugadas
    {
        short proximaEscenaVisitada;
        if (EscenasJugadas.Count == 3)
        {
            proximaEscenaVisitada = 2;
        }
        else
        {
            proximaEscenaVisitada = 1;
            if (!EscenasJugadas.Contains(_escena))
            {
                EscenasJugadas.Add(_escena);
                proximaEscenaVisitada = 0;
            }
        }        
        return proximaEscenaVisitada;
    }

    // zona de desconexión: 
    public void IrAInicio()
    {
        // Verificar si el NetworkManager está activo
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            // Suscribirse al evento de desconexión (opcional)
            NetworkManager.Singleton.OnClientDisconnectCallback += OnDisconnected;

            // Cerrar la conexión de red
            NetworkManager.Singleton.Shutdown();

            Debug.Log("Conexión cerrada. Cargando escena de desconexión...");
        }
        else
        {
            // Si no hay red activa, cargar directamente la escena
            LoadDisconnectScene();
        }
    }

    private void OnDisconnected(ulong clientId)
    {
        Debug.Log($"Cliente desconectado: {clientId}");

        // Desuscribirse del evento para evitar problemas
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnDisconnected;
        //ListaIdsClientes.Dispose();

        // Cargar la escena de desconexión
        LoadDisconnectScene();
    }

    private void LoadDisconnectScene()
    {

        if (!Application.CanStreamedLevelBeLoaded("MenuInicio"))
        {
            Debug.LogError($"La escena MenuInicio no existe o no está en la lista de escenas en el Build Settings.");
            return;
        }

        // Cargar la escena sin NetworkManager       
        SceneManager.LoadScene(disconnectSceneName, LoadSceneMode.Single);
    }
}
