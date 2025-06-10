
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;

public class PlayerManager : NetworkBehaviour
{
    public ulong miClientId { get; set; }
    private ulong localClientId;
    private ulong clientId;
    public Vector3 PosicionInicial { get; set; }
    public string Lugar { get; set; }
    public ControladorNivel controladorNivel;
    private int ImpactosRecibidos;
    private int maximoImpactos;
    private bool TengoElDocumento;
    private bool DocumentoTransmitido;
    private int totalTransmisiones;
    private int contadorCarga;
    public int numBalas;
    [SerializeField]
    private int MaximoBalas = 20;
    public bool Herido { get; set; }
    public bool RecuperadoHerido { get; set; }
    public bool Desactivado { get; set; }
    private PlayerController miPayerController;
    private Inventario InventarioScript;
    private int cantidaDocumentoObtenido = 0;
    private int totalCantDoc = 40;
    private int cantTotalTransmision = 40;
    private int cantTransmisionEmitida = 0;
    private List<Transform> MunicionTransforms;
    private Transform targetDocumento;
    private Transform targetTransmisor;
    private float detectionRange = 2f;
    public int numeroMaximoTransmisiones { get; set; }
    public bool JuegoGanado { get; set; }
    private bool TiempoTerminado;
    public bool soyElLocal { get; set; }
    private CargarEscenasMulti miCargarEscenasMulti;
    private Dictionary<int, PlayerManager> jugadores;
    public bool ListoCamara { get; set; }// se refiere a que la camara ya ha sido encontrada y adjudicada

    //public CinemachineVirtualCamera virtualCamera;
    public CinemachineFreeLook freeLookCamera;

    private void Awake()
    {
        Desactivado = true;
        soyElLocal = false;
        Lugar = "NADA";
    }
    void Start()
    {
        Iniciar();
        if (DatosGlobales.Instance.EscenaActual != "Nivel1" && DatosGlobales.Instance.EscenaActual != "Nivel2")
        {
            Desactivado = true;
        }
        else if(DatosGlobales.Instance.EscenaActual == "MenuInicio")
        {
            miPayerController.Estado = "Desactivado";
        }
    }

   

    public void SetPlayerVisible(bool visible)
    {     
        MeshRenderer[] renderers = transform.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in renderers)
        {
            renderer.enabled = visible;
        }
        SkinnedMeshRenderer[] skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var renderer in skinnedMeshRenderers)
        {
            renderer.enabled = visible;
        }      
    }

    private void ObtenerCamara()
    {
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
        }
    }
    public void colocarInicio()// viene de ScenechangeHandler a traves de un client rpc
    {
        /* :::: */
        if (DatosGlobales.Instance.EscenaActual != "Nivel1" && DatosGlobales.Instance.EscenaActual != "Nivel2") return;
        // va antes de encontrar la cámara  y solo se ejecuta en las ESCENAS DE NIVEL
        controladorNivel = GameObject.FindGameObjectWithTag("ControladorEscena").GetComponent<ControladorNivel>();

        if (controladorNivel == null) return;        

        // para saber la zona en el servidor de todos hace falta ser Host en el condicional
        // Obtener el ClientId del jugador local
        
        if (IsOwner)
        {
            
            localClientId = NetworkManager.Singleton.LocalClientId;
            Lugar = controladorNivel.obtenerZonaEnEscena(localClientId);
            PosicionInicial = controladorNivel.obtenerPosicionEnEscena(localClientId);
            transform.position = PosicionInicial;
            NetworkObject networkObject = GetComponent<NetworkObject>();
            clientId = networkObject.OwnerClientId;
            Debug.Log($"Este objeto pertenece al cliente: {clientId}");
            ComunicarPosicionATodosServerRpc(PosicionInicial);            
            ObtenerCamara();
        }       
        
    }
    /*
    Utilizamos Iniciar porque Start solo se ejecuta en cuanto se spawnea el jugador
        al principio pero si se cambia de escena no, por lo que hay que poner los datos de nuevo */

    public void Iniciar()
    {
        TiempoTerminado = false;
        if (IsServer)
        {
            ImpactosRecibidos = 0;
        }
        maximoImpactos = 3;
        TengoElDocumento = false;
        DocumentoTransmitido = false;
        totalTransmisiones = 0;
        contadorCarga = 0;
        numBalas = MaximoBalas;
        Herido = false;
        RecuperadoHerido = false;
        Desactivado = true;
        JuegoGanado = false;
        miPayerController = gameObject.GetComponent<PlayerController>();
        numeroMaximoTransmisiones = 5;//!!!!!!!!!!!!!!!! CAMBIAR A CINCO PARA EL JUEGO REAL A 5
        MunicionTransforms = new List<Transform>();
        GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag("Municion");
        // Recorrer cada objeto encontrado y agregar su Transform a la lista
        foreach (GameObject obj in objectsWithTag)
        {
            MunicionTransforms.Add(obj.transform);
        }
    }
    private void ObtenerInventarioYzonaTransmis() // se llama cuando de acaba de cargar toda la escena
    {
        Iniciar();
        miPayerController.Iniciar();
        if (IsOwner)
        {
            soyElLocal = true;
            InventarioScript = GameObject.FindGameObjectWithTag("CanvasInventario").GetComponent<Inventario>();
            if (InventarioScript == null) return;
            if (Lugar == "Posicion1")
            {
                targetTransmisor = GameObject.FindGameObjectWithTag("Transmisor1").transform;
            }
            else if (Lugar == "Posicion2")
            {
                targetTransmisor = GameObject.FindGameObjectWithTag("Transmisor2").transform;
            }
        }
        Debug.Log("EL LUGAR ES: " + Lugar);
        targetDocumento = GameObject.FindGameObjectWithTag("Documento").transform;
        miCargarEscenasMulti = GameObject.FindGameObjectWithTag("CargarEscenasMulti").GetComponent<CargarEscenasMulti>();
    }

    private void respawnear()
    {
        gameObject.transform.position = PosicionInicial;
        TengoElDocumento = false;
    }

    public int obtenerMaxBalas()
    {
        return 20;
    }
    public int obtenerBalas()
    {
        return numBalas;
    }

    public void quitarBala()
    {
        numBalas--;
    }


    //  DAÑOS Y DISPAROS

    [ClientRpc]
    void NotifyDamageClientRpc(int _ImpactosRecibidos, ClientRpcParams rpcParams = default)
    {       
        Herido = true;
        ImpactosRecibidos = _ImpactosRecibidos;
        if (ImpactosRecibidos == maximoImpactos)
        {
            Debug.Log("RESPAWNEAR");
            Herido = false;
            ImpactosRecibidos = 0;
            respawnear();
        }
        Debug.Log("IMPACTOS: " + ImpactosRecibidos);
        if (IsOwner)
        {
            if(ImpactosRecibidos == 0)
            {
                NotificarReinicioImpactosServerRpc();
            }
            
            // Actualiza panel de vida del jugador local
            Debug.Log($"[Client] Impactos Recibidos: {ImpactosRecibidos}");
            InventarioScript.mostrarImpactos(ImpactosRecibidos);
        }
    }

    [ServerRpc]
    private void NotificarReinicioImpactosServerRpc()
    {
        NotificarReinicioImpactosClientRpc();
    }

    [ClientRpc]
    private void NotificarReinicioImpactosClientRpc()
    {
        ImpactosRecibidos = 0;
    }

    public void HerirCliente()
    { // se ejecuta solo en el servidor

        ImpactosRecibidos++;
        // para mostrar en el Inventario:
        NotifyDamageClientRpc(ImpactosRecibidos, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { OwnerClientId }
            }
        });       

    }  

    [ClientRpc]
    public void ComunicarPosicionATodosClientRpc(Vector3 PosicionInicial)
    {
      SetPlayerVisible(true);
      transform.position = PosicionInicial;
      ObtenerInventarioYzonaTransmis();
    }

    [ServerRpc]
    public void ComunicarPosicionATodosServerRpc(Vector3 PosicionInicial)
    {
        ComunicarPosicionATodosClientRpc(PosicionInicial);
    }  


    private void resetearInventario()
    {
        Debug.Log("RESETEO INVENTARIO");
        InventarioScript.mostrarDocumento(0, totalCantDoc);
        InventarioScript.mostrarTransmision(0, cantTotalTransmision);
        InventarioScript.textoLlevoDocumento.text = "";
        if (IsOwner)
        {
            ActualizarScoreServerRpc(Lugar, localClientId, totalTransmisiones);
        }
    }
   

    [ServerRpc]
    private void ActualizarScoreServerRpc(string zona, ulong _localClientId, int _transmisiones)
    {
        Debug.Log("ACTUALIZO SERVER EN ZONA: " + zona);     
        ActualizarScoreClientRpc(zona, _localClientId, _transmisiones);        
    }

    [ClientRpc]
    public void ActualizarScoreClientRpc(string zona, ulong _localClientId, int _transmisiones)
    {
        Debug.Log("actualizo el score del que ha ganado en todos los clientes");
        Debug.Log("zona: " +zona+ "localclientId: "+ _localClientId + "Num transmisiones:" + _transmisiones);
 
        if(Lugar == zona)
        {
            InventarioScript.IncrementarContadorScore(_transmisiones, 1000);
            controladorNivel.contadorLocalScore = _transmisiones;             
        }
        else
        {
            Inventario InventarioScript = GameObject.FindGameObjectWithTag("CanvasInventario").GetComponent<Inventario>();
            InventarioScript.IncrementarContadorScore(1000, _transmisiones);
            ControladorNivel controladorNivel = GameObject.FindGameObjectWithTag("ControladorEscena").GetComponent<ControladorNivel>();
            controladorNivel.contadorContrarioScore = _transmisiones;
        }        
        comprobarGanarPartida(_localClientId, _transmisiones);
    }

    private void comprobarGanarPartida(ulong _localClientId, int _transmisiones)
    {
        if (!TiempoTerminado)
        {
            if (_transmisiones >= numeroMaximoTransmisiones)
            {
                if (IsOwner)
                {
                    ganarPartidaServerRpc(_localClientId);
                }
            }

        }
        else
        {
            if (IsOwner)
            {
                ganarPartidaServerRpc(_localClientId);
            }
        }
       
    }

    [ServerRpc]
    private void ganarPartidaServerRpc(ulong _localClientId)
    {
        ganarPartidaClientRpc(); // hacer que no se acualicen todas las acciones del jugador
        ControladorNivel controladorNivel = GameObject.FindGameObjectWithTag("ControladorEscena").GetComponent<ControladorNivel>();
        controladorNivel.PartidaTerminada(_localClientId);             
    }

    [ClientRpc]
    private void ganarPartidaClientRpc()
    {
        Desactivado = true;
    }

    [ClientRpc]
    private void empatarPartidaClientRpc()
    {
        controladorNivel.PartidaTerminada(1000);
    }

    [ServerRpc]
    private void empatarPartidaServerRpc()
    {
        empatarPartidaClientRpc();
    }

    private void DetectarTiempoTerminado()
    {
        if (controladorNivel == null || TiempoTerminado) return;
        if (controladorNivel.miCountdownTimer.tiempoterminado && TiempoTerminado == false)
        {
            TiempoTerminado = true;
            if (!IsOwner) return;
            if (controladorNivel.contadorLocalScore == controladorNivel.contadorContrarioScore)
            {
                empatarPartidaServerRpc();
                Debug.Log("empato por tiempo como host o client...");

            }  else if(controladorNivel.contadorLocalScore > controladorNivel.contadorContrarioScore)
            {
                    ActualizarScoreServerRpc(Lugar, localClientId, totalTransmisiones);
                    Debug.Log("gano por tiempo como host o client...en la zona:" + Lugar);              
            } 
        }
    }

   

    public override void OnNetworkSpawn()
    {
         colocarInicio();
    }



    // Update is called once per frame
    void Update()
    {
        if (Desactivado) return;
        DetectarTiempoTerminado();
        if (IsOwner && (Lugar == "Posicion1" || Lugar == "Posicion2"))
        {
            //SINCRONIZAR
            SincronizarPosicionServerRpc(transform.position);// sincronizar es mantener los objetos en posiciones persisitentes en todas las visualizaciones del juego por parte los jugadores.
            SincronizarRotacionServerRpc(transform.rotation);

            // sincronizacion de animaciones **************
            UpdateAnimacionServerRpc(miPayerController.transicionNumero);
            SincronizarArmaServerRpc(miPayerController.transicionNumero);
            // inventario:
            if (InventarioScript != null)
            {
                InventarioScript.mostrarMunicion(numBalas);                
                InventarioScript.mostrarDocumento(cantidaDocumentoObtenido, totalCantDoc);
                InventarioScript.mostrarTransmision(cantTransmisionEmitida, cantTotalTransmision);
             }

        }
        // Datos en el Canvas Inventario        
        contadorCarga++; // DE MUNICION
        if (contadorCarga % 8 != 0) return;
        if (MunicionTransforms.Count > 0)
        {
            foreach (Transform targetMunicion in MunicionTransforms)
            {
                float distance = Vector3.Distance(transform.position, targetMunicion.position);
                if (distance <= detectionRange && numBalas < MaximoBalas)
                {
                    targetMunicion.gameObject.GetComponent<Municion>().sumimistarMunicion();
                    numBalas++;
                }
            }
        }

        if (targetDocumento != null) // RECOGIDA DOCUMENTO
        {
            float distance = Vector3.Distance(transform.position, targetDocumento.position);
            if (distance <= detectionRange)
            {
                cantidaDocumentoObtenido++;
                if (cantidaDocumentoObtenido >= totalCantDoc)
                {
                    TengoElDocumento = true;
                    // targetDocumento.gameObject.SetActive(false);HAY QUE DESACTIVARLO PARA NO RECOGERLO
                }

            }
        }
        if (targetTransmisor != null)
        {
            float distance = Vector3.Distance(transform.position, targetTransmisor.position);

            if (distance <= detectionRange && TengoElDocumento)
            {
                cantTransmisionEmitida++;
                if (cantTransmisionEmitida >= cantTotalTransmision)
                {
                    totalTransmisiones++; 
                    DocumentoTransmitido = true;
                    resetearInventario();
                    cantidaDocumentoObtenido = 0;
                    cantTransmisionEmitida = 0;
                    TengoElDocumento = false;
                }
            }
        }

    }// cierre Update

    // si el tiempo se ha terminado tiene mayor score o si el tiempo no ha terminado y ha alcanzado el score maximo
   


    // SINCRONIZACIÓN RESTO JUGADORES,ANIMACION
    [ServerRpc]
    private void UpdateAnimacionServerRpc(int newSpeed)
    {
        // Actualizar la variable de red en el servidor
        // y seguidamente en los clientes
        UpdateAnimacionClientRpc(newSpeed);
    }

    [ServerRpc]
    private void SincronizarArmaServerRpc(int newSpeed)
    {
        SincronizarArmaClientRpc(newSpeed);
    }

    [ClientRpc]
    private void SincronizarArmaClientRpc(int newSpeed)
    {
        transform.GetChild(3).transform.position = transform.GetChild(3).transform.position;
        transform.GetChild(1).transform.position = transform.GetChild(1).transform.position;
    }

    [ClientRpc]
    private void UpdateAnimacionClientRpc(int newSpeed)
    {
        miPayerController.HandleAnimation(newSpeed, "nada"); // solo enviamos el mumero de la variable del componente animator
    }


    // SINCRONIZACIÓN RESTO JUGADORES, POSICION Y ROTACION
    [ServerRpc]
    private void SincronizarPosicionServerRpc(Vector3 posic)
    {
        SincronizarPosicionClientRpc(posic);
    }

    [ServerRpc]
    private void SincronizarRotacionServerRpc(Quaternion rot)
    {
        SincronizarRotacionClientRpc(rot);
    }

    [ClientRpc]
    private void SincronizarPosicionClientRpc(Vector3 posic)
    {
        if (!IsOwner)
        {
            transform.position = posic;
        }
    }

    [ClientRpc]
    private void SincronizarRotacionClientRpc(Quaternion rot)
    {
        if (!IsOwner)
        {
            transform.rotation = rot;
        }
    }
}




