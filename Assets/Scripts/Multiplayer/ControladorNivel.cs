using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.SceneManagement;


public class ControladorNivel : NetworkBehaviour
{
    public string EscenaActual { get; set; }
    public Dictionary<int, PlayerManager> jugadores { get; set; }
    public CountdownTimer miCountdownTimer { get; set; }   
    public int contadorLocalScore;
    public int contadorContrarioScore;
    [SerializeField]
    public Transform[] PosicionesIni;
    [SerializeField]
    private GameObject InventarioObjeto;
    private TextMeshProUGUI scoreLocal;
    private TextMeshProUGUI scoreContrario;
    public bool keepRunning;// DETECTA QUE SE HAN GUARDADO EN DICCIONARIO TODOS LOS JUGADORES
    private string PlayerManagerLocalZona;
    private bool InventarioEncontrado;
    private VictoriaDerrota miVictoriaDerrota;
    private miNetworkManager MiNetworkManager;


    // Start is called before the first frame update
    private void Start()
    {       
        miCountdownTimer = GetComponent<CountdownTimer>();
        miVictoriaDerrota = GetComponent<VictoriaDerrota>();
        MiNetworkManager = GetComponent<miNetworkManager>();
        keepRunning = true;
        if (NetworkManager.Singleton.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.OnLoadComplete += OnSceneLoadComplete;
        }
    }   


    private void OnDisable()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnSceneLoadComplete;
        }
    }

    public Vector3 obtenerPosicionEnEscena(ulong id)
    {
        Vector3 Posicion = new Vector3(0, 0, 0);
        for (int i = 0; i < PosicionesIni.Length; i++)
        {
            if (i == (int)id)
            {
                Posicion = PosicionesIni[i].position;
            }
        }
        return Posicion;
    }

    public string obtenerZonaEnEscena(ulong id)
    {
        string Zona = "Posicion2";
        if (id % 2 == 0)
        {
            Zona = "Posicion1";
        }
        PlayerManagerLocalZona = Zona;

        return Zona;
    }

    
    private string GetLocalPlayerZona() // ES PARA OBTENER EL LUGAR/ONA DEL PLAYER LOCAL
    { // solo se puede hacer en el SERVIDOR
        string Lugar;
        if (IsServer)
        {
            ulong localClientId = NetworkManager.Singleton.LocalClientId;
            PlayerManager LocalPlayerManager = jugadores.ContainsKey((int)localClientId) ? jugadores[(int)localClientId] : null;
            Lugar = LocalPlayerManager.Lugar;
        }
        else
        {
            Lugar = PlayerManagerLocalZona;
        }
        return Lugar;
    }

    public void IncrementarContadorScore(string zona, ulong _id)
    { // viene de un clientRpc...

        string JugadorManagerZona = GetLocalPlayerZona();
        if (JugadorManagerZona == zona)
        {
            IncrementarContadorLocalScore();
        }
        else
        {
            IncrementarContadorContrarioScore();
        }
    }

    private void IncrementarContadorLocalScore()
    {
        Debug.Log("contadorLocalScore: " + contadorLocalScore);
        contadorLocalScore++;
        scoreLocal.text = contadorLocalScore.ToString();
    }

    private void IncrementarContadorContrarioScore()
    {
        Debug.Log("contadorContrarioScore: " + contadorContrarioScore);
        contadorContrarioScore++;
        scoreContrario.text = contadorContrarioScore.ToString();
    }

    private void EspawnearTodosLosJugadores()
    {
        Debug.Log("Espawnear Todos Los Jugadores");
        var connectedClients = NetworkManager.Singleton.ConnectedClientsList;
        foreach (var cliente in connectedClients) // client es un NetworkClient
        {
            ulong clienteId = cliente.ClientId;
            MiNetworkManager.SpawnPlayer(clienteId);
        }
    }

    private void OnSceneLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        EscenaActual = sceneName;
        if (sceneName == "Nivel1" || sceneName == "Nivel2")
        {
            if (IsServer)
            {
                EspawnearTodosLosJugadores();
            }           
            contadorLocalScore = 0;
            contadorContrarioScore = 0;
            InventarioObjeto = GameObject.FindGameObjectWithTag("CanvasInventario");

            if (IsServer)
            {
                scoreLocal = InventarioObjeto.GetComponent<Inventario>().textoScoreLocal;
                scoreContrario = InventarioObjeto.GetComponent<Inventario>().textoScoreContrario;
            }
            else
            {
                InventarioEncontrado = false;
                StartCoroutine(buscarInventarioCadaSegundo());            }
            //-----
            keepRunning = true;
            if (!IsServer) return;
            StartCoroutine(EncontrarJugadores());
        }
    }
    IEnumerator buscarInventarioCadaSegundo()
    {
        while (!InventarioEncontrado) // Se ejecuta hasta que la condición sea verdadera
        {
            InventarioObjeto = GameObject.FindGameObjectWithTag("CanvasInventario");
            scoreLocal = InventarioObjeto.GetComponent<Inventario>().textoScoreLocal;
            scoreContrario = InventarioObjeto.GetComponent<Inventario>().textoScoreContrario;
            if (scoreLocal != null && scoreContrario != null)
            {
                InventarioEncontrado = true;
            }
            yield return new WaitForSeconds(1f); // Espera 1 segundo
        }
    }


    IEnumerator EncontrarJugadores()
    {
        while (keepRunning)
        {
            jugadores = new Dictionary<int, PlayerManager>();
            var connectedClients = NetworkManager.Singleton.ConnectedClientsList;
            foreach (var cliente in connectedClients) // client es un NetworkClient
            {
                ulong clienteId = cliente.ClientId; // ID del cliente
                NetworkObject clientObject = cliente.PlayerObject; // GameObject asociado al cliente (si existe)
                string zona = clientObject.GetComponent<PlayerManager>().Lugar;

                jugadores.Add((int)clienteId, clientObject.GetComponent<PlayerManager>());

                //Debug.Log($"Client ID: {clientId}, GameObject: {(clientObject != null ? clientObject.name : "None")}");
                Debug.Log("connectedClients: " + connectedClients.Count);// vale count para el diccionario de jugadores
            }

            // Aquí puedes cambiar la condición para salir del bucle
            if (jugadores.Count == connectedClients.Count)
            {
                keepRunning = false; // Detiene la corrutina después de 1 segundos
            }
            yield return new WaitForSeconds(1f); // Espera 1 segundo antes de repetir
        }
        Debug.Log("¡Acción ejecutada después de 1 segundo!");
    }
        

    public void PartidaTerminada(ulong idGanador)
    {
         miVictoriaDerrota.EndGame(idGanador);
    }
    

}
