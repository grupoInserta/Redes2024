using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Transports.UTP;
using UnityEngine.UI;


public class NetworkManagerUI : NetworkBehaviour {

    [SerializeField]
    public GameObject CanvasPrincipal;
    [SerializeField]
    public GameObject CreacionCanvas;
    [SerializeField]
    public GameObject ConectarCanvas;
    [SerializeField]
    public GameObject LobbyCanvas;
    [SerializeField]
    public GameObject CargandoCanvas;
    public static NetworkManagerUI Instance;
    [SerializeField]
    private InputField IpAdressText;
    [SerializeField]
    private InputField PuertoText;
    // public string ipAddress = "127.0.0.1";// IP Conexiones locales   
    private ushort Puerto;
    [SerializeField]
    public GameObject CargarEscenas; 
    private string ipAdress;

    private void ConvertTextToUShort()
    {
        // Intentamos convertir el texto del InputField a ushort
        if (ushort.TryParse(PuertoText.text, out Puerto))
        {
            Debug.Log("El número convertido es: " + Puerto);            
        }
        else
        {
            Debug.LogWarning("El valor ingresado no es un número válido para ushort.");
        }
    }


    [ClientRpc]
    private void IniPartidaLocalClientRpc()
    {
        LobbyCanvas.SetActive(false);
        CargandoCanvas.SetActive(true);        
    }

    [ServerRpc]
    private void llamarIniPartidaServerRpc()
    {
        IniPartidaLocalClientRpc();
    }

    /* una vez que se han conectado clientes y servidor aparece boton
       iniciar Partida en CanvasInicioPartida que esta en el panel IniciarJuegoCanvas
        que se activa en miNetworkManager.cs cuando el numero de jugadores es mayor que uno*/
    public void IniciarPartida(Button boton)
    {
        CargarEscenas.GetComponent<cargaEscenasMulti>().CargarEscenaNombre("Nivel1");
        boton.gameObject.SetActive(false);
        if (IsOwner)
        {
            llamarIniPartidaServerRpc();                            
        }       
    }

    public void IrAMenu()
    {
        CargarEscenas.GetComponent<cargaEscenasMulti>().CargarEscenaNombre("Menu");
    }

    public void StartHost()
    {      
        NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address = ipAdress;
        NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Port = Puerto;
        NetworkManager.Singleton.StartHost();
        CreacionCanvas.SetActive(false);
        LobbyCanvas.SetActive(true);
        /*** EL ERROR VIENE DE AQUI*/
        //CargarEscenas.GetComponent<cargaEscenasMulti>().CargarEscenaNombre("Lobby");
        CanvasPrincipal.SetActive(false);
    }


    public void Conectar()
    {
        //ipAdress = System.Text.RegularExpressions.Regex.Replace(IpAdressText.text, "[^0-9.]", "");
        ConvertTextToUShort();
        //ipAdress = "127.0.0.1";
        ipAdress = IpAdressText.text.ToString();
        if(ipAdress == "")
        {
            ipAdress = "127.1.0.0";
            Puerto = 7777;
        }
        CreacionCanvas.SetActive(true);
        ConectarCanvas.SetActive(false);

    }

    public void StartClient()
    {        
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.ConnectionData.Address = ipAdress;       
        NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Port = Puerto;
        bool result = NetworkManager.Singleton.StartClient();
        if (!result)
        {
            Debug.LogError("Fallo inicio cliente.");
        }
        CreacionCanvas.SetActive(false);
        LobbyCanvas.SetActive(true);
        CanvasPrincipal.SetActive(false);
    }
   
}
