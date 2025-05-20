
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerManager : NetworkBehaviour
{
    public ulong miClientId { get; set; }
    private ulong localClientId;
    public Vector3 PosicionInicial { get; set; }
    public string Lugar { get; set; }
    public ControladorNivel controladorNivel;
    private int impactosRecibidos;
    private int maximoImpactos;
    private bool TengoElDocumento;
    private bool DocumentoTransmitido;
    private int contadorCarga;
    private int contadorHerido;
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


    private void Awake()
    {
        Desactivado = true;
        soyElLocal = false;
        Lugar = "NADA";
    }
    void Start()
    {
        Iniciar();
    }

    public void establecerTipoEscena(string sceneName)
    {
        if(sceneName !=  "Nivel1" && sceneName != "Nivel2")
        {
            Desactivado = true;
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
    public void colocarInicio()
    {   // va antes de encontrar la cámara  y solo se ejecuta en las ESCENAS DE NIVEL   
        controladorNivel = GameObject.FindGameObjectWithTag("ControladorEscena").GetComponent<ControladorNivel>();
        // para saber la zona en el servidor de todos hace falta ser Host en el condicional
        // Obtener el ClientId del jugador local
        localClientId = NetworkManager.Singleton.LocalClientId;
        PosicionInicial = controladorNivel.obtenerPosicionEnEscena(localClientId);
        transform.position = PosicionInicial;

        NetworkObject networkObject = GetComponent<NetworkObject>();
        ulong clientId = networkObject.OwnerClientId;
        Debug.Log($"Este objeto pertenece al cliente: {clientId}");
        if (IsOwner)
        {
            Lugar = controladorNivel.obtenerZonaEnEscena(clientId);
        };

        Debug.Log($"Mi ClientId es: {clientId} y el Lugar es: {Lugar}");
        SetPlayerVisible(true);
    }
    /*
    Utilizamos Iniciar porque Start solo se ejecuta en cuanto se spawnea el jugador
        al principio pero si se cambia de escena no, por lo que hay que poner los datos de nuevo */

    public void Iniciar()
    {
        TiempoTerminado = false;
        impactosRecibidos = 0;
        maximoImpactos = 2;
        TengoElDocumento = false;
        DocumentoTransmitido = false;
        contadorCarga = 0;
        contadorHerido = 0;
        numBalas = MaximoBalas;
        Herido = false;
        RecuperadoHerido = false;
        Desactivado = true;
        JuegoGanado = false;
        miPayerController = gameObject.GetComponent<PlayerController>();
        numeroMaximoTransmisiones = 2;//!!!!!!!!!!!!!!!! CAMBIAR A CINCO PARA EL JUEGO REAL
        MunicionTransforms = new List<Transform>();
        GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag("Municion");

        // Recorrer cada objeto encontrado y agregar su Transform a la lista
        foreach (GameObject obj in objectsWithTag)
        {
            MunicionTransforms.Add(obj.transform);
        }
    }
    public void ObtenerInventario() // se llama cuando de acaba de cargar toda la escena
    {
        Iniciar();
        miPayerController.Iniciar();
        if (IsOwner)
        {
            soyElLocal = true;
            InventarioScript = GameObject.FindGameObjectWithTag("CanvasInventario").GetComponent<Inventario>();

            if (Lugar == "Posicion1")
            {
                targetTransmisor = GameObject.FindGameObjectWithTag("Transmisor1").transform;
            }
            else if (Lugar == "Posicion2")
            {
                targetTransmisor = GameObject.FindGameObjectWithTag("Transmisor2").transform;
            }

        }

        targetDocumento = GameObject.FindGameObjectWithTag("Documento").transform;
        miCargarEscenasMulti = GameObject.FindGameObjectWithTag("CargarEscenasMulti").GetComponent<CargarEscenasMulti>();

    }

    private void respawnear()
    {
        impactosRecibidos = 0;
        Herido = false;
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


    private void ImpactoBala()
    {
        Herido = true;
        impactosRecibidos++;
        if (impactosRecibidos == maximoImpactos)
        {
            Herido = false;
            respawnear();
        }
    }


    [ClientRpc]
    public void HerirClientRpc()
    {
        StartCoroutine(ImpactoBalaRetar());        
    }

    IEnumerator ImpactoBalaRetar()
    {
        ImpactoBala();
        yield return new WaitForSeconds(0.5f);
    }

        private void resetearInventario()
    {
        Debug.Log("RESETEO INVENTARIO");
        InventarioScript.mostrarDocumento(0, totalCantDoc);
        InventarioScript.mostrarTransmision(0, cantTotalTransmision);
        InventarioScript.textoLlevoDocumento.text = "";
        if (IsOwner)
        {
            ActualizarScoreServerRpc(Lugar, localClientId);
        }
    }

   

    [ServerRpc]
    private void ActualizarScoreServerRpc(string zona, ulong _localClientId)
    {        
        ActualizarScoreClientRpc(zona, _localClientId);        
    }

    [ClientRpc]
    public void ActualizarScoreClientRpc(string zona, ulong _localClientId)
    {
        controladorNivel.IncrementarContadorScore(zona);
        comprobarGanarPartida(_localClientId);
    }
    
    private void comprobarGanarPartida(ulong _localClientId)
    {
   
        if (controladorNivel.contadorLocalScore >= numeroMaximoTransmisiones)
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
        if (controladorNivel.miCountdownTimer.tiempoterminado)
        {
            TiempoTerminado = true;

            if (controladorNivel.contadorLocalScore == controladorNivel.contadorContrarioScore)
            {
                if (IsOwner)
                {
                    empatarPartidaServerRpc();
                }
            }  else if(controladorNivel.contadorLocalScore > controladorNivel.contadorContrarioScore)
            {
                if (IsOwner)
                {
                    ActualizarScoreServerRpc(Lugar, localClientId);
                }
            } 
        }
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
                InventarioScript.mostrarImpactos(impactosRecibidos);
                InventarioScript.mostrarDocumento(cantidaDocumentoObtenido, totalCantDoc);
                InventarioScript.mostrarTransmision(cantTransmisionEmitida, cantTotalTransmision);
            }

        }

        if (Herido == true)
        {
            contadorHerido++;
            if (contadorHerido % 8 != 0) return;
            if (contadorHerido > 10)
            {
                Herido = false;
                RecuperadoHerido = true;
            }
        }
        else if (RecuperadoHerido == true)
        {
            RecuperadoHerido = false;
            contadorHerido = 0;
        }


        // Datos en el Canvas Inventario        
        contadorCarga++;
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

        if (targetDocumento != null)
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




