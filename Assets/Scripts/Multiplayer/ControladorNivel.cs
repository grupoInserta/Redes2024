using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.SceneManagement;


public class ControladorNivel : NetworkBehaviour
{
    /*
    private TextMeshProUGUI textObjetos;
    private TextMeshProUGUI TextoJuego;
    private cargaEscenasMulti cargaEscenas;
    */

    private AudioSource Derrota;
    public Dictionary<int, PlayerManager> jugadores { get; set; }
    /*
    private bool JugadoresDetectados = false;
     */
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


    // Start is called before the first frame update
    private void Start()
    {
        miCountdownTimer = GetComponent<CountdownTimer>();
        miVictoriaDerrota = GetComponent<VictoriaDerrota>();
        keepRunning = true;
    }

    public void ChangeScene(string sceneName)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }

    private void OnEnable()
    {
        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnSceneLoadComplete;
    }

    private void OnDisable()
    {
        NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnSceneLoadComplete;
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

    [ServerRpc]
    public void ApplyDamageServerRpc(ulong OwnerClientId) // player que ha recibido bala
    {
        NetworkObject JugadorHerido = GetPlayerObjectByClientId(OwnerClientId);
        if (IsOwner)
        {
            JugadorHerido.GetComponent<PlayerManager>().HerirClientRpc();
        }

    }

    // Método para obener players a traves de sus idCliente, no utilizado:
    private NetworkObject GetPlayerObjectByClientId(ulong clientId)
    {
        // Verificar si el cliente está conectado
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            // Retornar el objeto de red del cliente (si existe)
            return client.PlayerObject;
        }

        Debug.LogWarning($"Cliente con ID {clientId} no está conectado o no tiene un objeto asignado.");
        return null;
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

    public void IncrementarContadorScore(string zona)
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
   

    private void OnSceneLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if (sceneName == "Nivel1" || sceneName == "Nivel2")
        {
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

    // PASAR ESTO DESDE AQUI AL SERVIDOR Y LUEGO A LOS DEMAS
    /*
    public void PartidaTerminada(ulong ganadorId)
    {
        Debug.Log("PARTIDA GANADA");
        Debug.Log("Partida terminada GANADORID: " + ganadorId);
        if (IsOwner)
        {
            Debug.Log("PARTIDA GANADA");
            PartidaTerminadaServerRpc(ganadorId);
        }       
        
    }
    */

    public void PartidaTerminada(ulong idGanador)
    {
         miVictoriaDerrota.EndGame(idGanador);
    }

    /*
    [ServerRpc]
    public void PartidaTerminadaServerRpc(ulong ganadorId)
    {   Debug.Log("GANADORID-1: " + ganadorId);
        PartidaTerminadaClientRpc(ganadorId);     
    }

    [ClientRpc]
    private void PartidaTerminadaClientRpc(ulong ganadorId)
    {
        miVictoriaDerrota.EndGame(ganadorId);
        Debug.Log("GANADORID-2: " + ganadorId);
    }
    */

}
