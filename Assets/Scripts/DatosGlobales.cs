using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cinemachine;

public class DatosGlobales : NetworkBehaviour
{
    public static DatosGlobales Instance { get; private set; }
    public string EscenaActual  { get; private set; }
    public List<string> EscenasJugadas = new List<string>();
    private Transform miCanvas;
    public Button IrInicioButton;
    public Button SalirButton;
    private string disconnectSceneName = "MenuInicio"; // Nombre de la escena de desconexión
    public bool pausado;
    private CinemachineFreeLook freeLookCamera;
    private Camera camPpal;
  
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
    }

    private bool IsMouseInsideGameWindow()
    {
        Vector3 mousePos = Input.mousePosition;
        return mousePos.x >= 0 && mousePos.x <= Screen.width &&
               mousePos.y >= 0 && mousePos.y <= Screen.height;
    }

    private bool IsMouseInsideGameView()
    {
        return new Rect(0, 0, Screen.width, Screen.height).Contains(Input.mousePosition);
    }

    private void Update()
    {
        //return;
        // cursor activo dentro de ventana:
        if (freeLookCamera != null)
        {
            if (IsMouseInsideGameWindow())
            {
                freeLookCamera.m_XAxis.m_InputAxisName = "Mouse X";
                freeLookCamera.m_YAxis.m_InputAxisName = "Mouse Y";
                //
                pausado = false;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                // Desactivar input de cámara
                freeLookCamera.m_XAxis.m_InputAxisName = "";
                freeLookCamera.m_YAxis.m_InputAxisName = "";
                // desactivar giro camara:
                freeLookCamera.m_XAxis.m_InputAxisValue = 0f;
                freeLookCamera.m_YAxis.m_InputAxisValue = 0f;
                //
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                pausado = true;
                Debug.Log("fuera de ventana");
            }
        }
        else if (camPpal != null)
        {
            bool inside = IsMouseInsideGameView();
            Cursor.visible = inside;
            Cursor.lockState = inside ? CursorLockMode.None : CursorLockMode.Locked;
        }
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

    public override void OnDestroy()
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
        EscenaActual = scene.name;
        OrganizarBotones();
        if (freeLookCamera == null)
        {
            freeLookCamera = FindObjectOfType<CinemachineFreeLook>();
        }
        if (camPpal == null)
        {
            camPpal = Camera.main;
        }
        pausado = false;
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
    // 
    private void EsperarDesconexionCliente(ulong clientId)
    {
        Debug.Log($"Cliente {clientId} desconectado (callback)");
        NetworkManager.Singleton.OnClientDisconnectCallback -= EsperarDesconexionCliente;
        LoadDisconnectScene();
    }
    public void IrAInicio()
    {
        EscenasJugadas = new List<string>();
        // Verificar si el NetworkManager está activo
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            if (NetworkManager.Singleton.IsHost)
            {
                Debug.Log("Host cerrando red...");
                NetworkManager.Singleton.Shutdown();
                Destroy(NetworkManager.Singleton.gameObject);
                LoadDisconnectScene(); // Host puede hacerlo directamente
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                Debug.Log("Cliente cerrando red...");
                NetworkManager.Singleton.OnClientDisconnectCallback += EsperarDesconexionCliente;
                NetworkManager.Singleton.Shutdown(); // Esto desconecta al cliente
                Destroy(NetworkManager.Singleton.gameObject);
            }
        }
        else
        {
            LoadDisconnectScene(); // No había red activa
        }
    }

    public void SalirAplicacion()
    {
        Application.Quit();
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
        OrganizarBotones();
        SceneManager.LoadScene(disconnectSceneName, LoadSceneMode.Single);
    }
}
