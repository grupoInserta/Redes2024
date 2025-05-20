using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;


[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
{
    //Creamos la variable del player donde le asignamos el player que vamos a usar
    private CharacterController player;
    //Accedemos al player mediante el transform del mismo para poder mover al personaje
    [SerializeField] private float velGiro = 0;

    [Header("Gravity")]
    [SerializeField] float gravity = 9.8f;
    [SerializeField] float gravityMultiplier = 2;
    [SerializeField] float groundedGravity = -0.5f;
    [SerializeField] float jumpHeight = 5f;

    private string movimientoLateral;
    private float velocityY;
    private float velocidadLateral;
    private float velocidad;
    private float VelMaxima = 8f;
    private float velInicioCorriendo = 3f;
    private float velAvanceInicial = 2f;
    private float aceleracionIncrementoIni = 0.09f;
    private float aceleracionIncremento = 0;
    private float desaceleracionIncremento;
    private float desaceleracionIncrementoIni = -0.35f;

    [SerializeField] float rotationSmoothTime = 0.2f;
    private int contadorBalas;
    private float timeElapsed = 0f;
    //Camera cam;
    private CinemachineFreeLook cam;
    private Camera camPpal;
    public bool soyLocal;
    public bool pausado;
    public Animator Animacion;
    public string Estado { get; set; }
    private string TransicionActual;
    public int transicionNumero { get; set; }
    private AnimatorStateInfo stateInfo;
    private string stateName;
    public int numTransicionRed { get; set; }

    // disparos
    [SerializeField] private GameObject Bala;
    private GameObject[] ArrayBalas;
    [SerializeField] Transform bocaCannon;
    public float fuerzaDisparo = 20f;
    private PlayerManager miPlayerManager;
    private int MaximoBalas;
    private float velocidadLateralIni;
    private float velocidadAtras;
    //
    private SceneChangeHandler miSceneChangeHandler;


    private void Awake()/* va amtes de start pero solo se ejecuta en el primer spawneo
                         * por lo que utilizamos Iniciar, que también se llama con el eventa ecena descargada desde playerManager*/
    {
        Iniciar();
    }
    public void Iniciar()
    {
        velocidadLateral = 0f;
        velocidadLateralIni = 1.5f;
        velocidad = velAvanceInicial;
        velocidadAtras = -1f;
        desaceleracionIncremento = desaceleracionIncrementoIni;
        player = GetComponent<CharacterController>();
        Animacion = gameObject.GetComponent<Animator>();
        miPlayerManager = gameObject.GetComponent<PlayerManager>();
        miSceneChangeHandler = gameObject.GetComponent<SceneChangeHandler>();
        MaximoBalas = miPlayerManager.obtenerMaxBalas();
        ArrayBalas = new GameObject[MaximoBalas];
        contadorBalas = 0;
        velocidad = 0;
        movimientoLateral = "NO";
        pausado = true;
    }

    public class PlayerShooting : NetworkBehaviour
    {
        public GameObject projectilePrefab; // Prefab del proyectil
        public Transform firePoint; // Punto desde donde se dispara el proyectil
        public float projectileSpeed = 15f;
    }

    [ClientRpc]
    private void ShootProjectileClientRpc(Vector3 position, Vector3 direction)
    {
        ArrayBalas[contadorBalas] = Instantiate(Bala) as GameObject;
        ArrayBalas[contadorBalas].transform.position = new Vector3(bocaCannon.position.x, bocaCannon.position.y, bocaCannon.position.z);
        ArrayBalas[contadorBalas].GetComponent<Bala>().ConfigurarVelocidad(fuerzaDisparo, direction);
        contadorBalas++;
        miPlayerManager.quitarBala();
    }

    /*****error al disparar bala *****/
    [ServerRpc]
    private void RequestShootServerRpc(ServerRpcParams serverRpcParams = default)
    {
        // Validación adicional si es necesario (por ejemplo, cooldown o munición)
        // Instanciar y sincronizar el proyectil en todos los clientes
        ulong shooterId = serverRpcParams.Receive.SenderClientId;
        float angleTolerance = 0.98f;
             if(miPlayerManager.controladorNivel == null)
        {
            Debug.Log("miPlayerManager.controladorNivel  no valido");
        }
        Debug.Log("Tamaño del diccionario: " + miPlayerManager.controladorNivel.jugadores.Count);
        foreach (var player in miPlayerManager.controladorNivel.jugadores.Values)
        {
            if (player.OwnerClientId == shooterId) continue;

            Vector3 toPlayer = (player.transform.position - transform.position).normalized; // Vector hacia el jugador
            float alignment = Vector3.Dot(transform.forward, toPlayer);

            if (alignment >= angleTolerance) // Si el jugador está alineado con el disparo
            {
                miPlayerManager.controladorNivel.ApplyDamageServerRpc(player.OwnerClientId); // mandamos su ID
                Debug.Log($"Jugador {shooterId} impactó a {player.OwnerClientId} por dirección precisa.");
            }
        }

        ShootProjectileClientRpc(bocaCannon.position, transform.forward);
    }



    private void disparar()
    {
        if (contadorBalas >= MaximoBalas) // vaciar todo de balas
        {
            ArrayBalas = new GameObject[MaximoBalas];
            contadorBalas = 0;
        }

        if (miPlayerManager.numBalas > 0)
        {
            RequestShootServerRpc();
            // IMPLEMENTAR SONIDO??
        }
    }

    private void HandleMovement()
    {
        /*
        The CharacterController.Move motion moves the GameObject in the 
        given direction. The given direction requires absolute movement delta 
        values. A collision constrains the Move from taking place.
        The return, CollisionFlags, indicates the direction of a collision:
        None, Sides, Above, and Below. CharacterController.Move does not
        use gravity.
          */
        // EVENTOS TECLADO


        if (Input.GetKey(KeyCode.W) && Estado != "Parando" && Estado != "Entransicion" && velocidadLateral ==0)
        {            ///////////////////////            
       
            aceleracionIncremento += aceleracionIncrementoIni;
            velocidad = velAvanceInicial + aceleracionIncremento;

            if (Estado == "Parado")
            {
                Estado = "Andando";
                TransicionActual = "ParadoAndando";
            }
            else if (Estado == "Andando" && velocidad >= velInicioCorriendo)
            {
                Estado = "Corriendo";
                TransicionActual = "AndandoCorriendo";

            }
            else if (Estado == "Corriendo" && velocidad >= VelMaxima)
            {
                velocidad = VelMaxima;
                aceleracionIncremento = aceleracionIncrementoIni;
            }

            if (Estado == "Corriendo" && stateName == "IDL")
            {
                Debug.Log("RESBALO CON ESTADO: " + Estado + " statename: " + stateName);
                TransicionActual = "ParadoCorriendo";
                Estado = "Andando";
            }
        }
        else if (Input.GetKeyUp(KeyCode.W))
        {
            if (Estado == "Corriendo")
            {
                TransicionActual = "CorriendoParado";
            }
            else if (Estado == "Andando")
            {
                TransicionActual = "AndandoParado";
            }
            Estado = "Parando";
        }
        // MOVIMIENTO LATERAL
        else if (Input.GetKey(KeyCode.A) && Estado != "Corriendo" && Estado != "Parando" && Estado != "Entransicion")
        {
            TransicionActual = "ParadoAndandoI";
            velocidad = 0;
            Estado = "AndandoI";
            velocidadLateral = velocidadLateralIni;
            movimientoLateral = "IZQ";
        }
        else if (Input.GetKey(KeyCode.D) && Estado != "Corriendo" && Estado != "Parando" && Estado != "Entransicion")
        {
            TransicionActual = "ParadoAndandoD";
            velocidad = 0;
            Estado = "AndandoD";
            velocidadLateral = -velocidadLateralIni;
            movimientoLateral = "DER";
        }
        else if (Input.GetKeyUp(KeyCode.D))
        {
            TransicionActual = "LadoParadoD";
            Estado = "Parado";
            velocidadLateral = 0;
            movimientoLateral = "NO";
        }
        else if (Input.GetKeyUp(KeyCode.A))
        {
            TransicionActual = "LadoParadoI";
            Estado = "Parado";
            velocidadLateral = 0;
            movimientoLateral = "NO";
        }
        else if (Input.GetKey(KeyCode.S))
        {
            Estado = "Atras";
            TransicionActual = "Atras";
            velocidad = velocidadAtras;
        }
        else if (Input.GetKeyUp(KeyCode.S))
        {
            Estado = "Parado";
            TransicionActual = "ParandoAtras";
            velocidad = 0;
        }

        else if (Input.GetKey(KeyCode.C) && Estado != "Entransicion")
        {
            Debug.Log("agachar");
            TransicionActual = "Abajo";
            Estado = "Entransicion";
            velocidad = 0;
            transform.GetChild(3).transform.Translate(new Vector3(0, -0.5f, 0));
            transform.GetChild(1).transform.Translate(new Vector3(0, -0.5f, 0));
        }
        else if (Input.GetKeyUp(KeyCode.C) && Estado == "Entransicion")
        {
            Debug.Log("Levantarse");
            TransicionActual = "Levantarse";
            Estado = "Parado";
            transform.GetChild(3).transform.Translate(new Vector3(0, 0.5f, 0));
            transform.GetChild(1).transform.Translate(new Vector3(0, 0.5f, 0));
        }

        // zona de cambios de Estados debido a diferentes velocidades y cambios de estados
        if (Estado == "Parando")
        {
            desaceleracionIncremento += desaceleracionIncrementoIni;
            velocidad += desaceleracionIncremento;
            if (velocidad < velAvanceInicial)
            {
                velocidad = 0;
                Estado = "Parado";
                desaceleracionIncremento = desaceleracionIncrementoIni;
                TransicionActual = "AndandoParado";
            }

        }
        else if (movimientoLateral == "DER" || movimientoLateral == "IZQ")
        {
            if (Estado == "Corriendo" || Estado == "Andando")
            {
                aceleracionIncremento = desaceleracionIncremento;//que se pare
            }
            else if (Estado == "Parado" && movimientoLateral == "IZQ")
            {
                TransicionActual = "ParadoAndandoI";
            }
            else if (Estado == "Parado" && movimientoLateral == "DER")
            {
                TransicionActual = "ParadoAndandoD";
            }
        }

        // TRANSICIONES ANIMACION:
        HandleAnimation(667, TransicionActual);

        // MOVIMIENTO REAL:
        if (movimientoLateral == "NO")
        {// movemos de frente hacia delante o atrás
            player.Move(transform.forward * velocidad * Time.deltaTime);
        }
        else
        { // movemos de lado
            player.Move(-transform.right * velocidadLateral * Time.deltaTime);
        }
    }

    private void HandleRotation()
    {
          float rotationY = camPpal.transform.eulerAngles.y;
          float smoothedY = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationY, ref velGiro, rotationSmoothTime);
          transform.rotation = Quaternion.Euler(0f, smoothedY, 0f);
    }


    private void HandleGravityandJump()
    {
        if (!player.isGrounded)//ES LA GRAVEDAD, QUE ACTUA EN CUALQUIER CIRCUNSTANCIA MENOS CUANDO SE TOCA EL SUELO
        {
            velocityY -= gravity * gravityMultiplier * Time.deltaTime;
            player.Move(Vector3.up * velocityY * Time.deltaTime);
        }

        if (Estado == "Aterrizando" && player.isGrounded)//termina de saltar y se queda parado
        {
            Estado = "Parado";
            TransicionActual = "SaltandoParado";
            velocidad = 0;
            Debug.Log("TERMINO DE SALTAR");
        }
        else if (player.isGrounded && velocityY < 0f)
        {
            velocityY = groundedGravity;/*QUE EL JUGADOR ESTE PEGADO AL SUELO,
                 es el estado habitual en el jueo*/
            // groundedGravity es negativo
        }
        // saltar y aterrizar
        if (player.isGrounded && Input.GetKeyDown(KeyCode.Space) && Estado != "Aterrizando")
        {
            velocityY = Mathf.Sqrt(jumpHeight * 2f * gravity);
            Estado = "Salto";
            TransicionActual = "CualquieraSaltando";

        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            Estado = "Aterrizando";
            TransicionActual = "Aterrizando";
        }
    }

    public void HandleAnimation(int Num, string TransicionActual)
    { // el primer parametro sirve para distinguir si la animacion es propia o viene de la red de otro jugagor

        if (Num == 667) // esto significa que la animacion provienen del owner
        {
            switch (TransicionActual)
            {
                case "ParadoAndando":
                    transicionNumero = 1;
                    break;
                case "AndandoCorriendo":
                    transicionNumero = 3;
                    break;
                case "AndandoParado":
                    transicionNumero = 99;
                    break;
                case "ParadoCorriendo":
                    transicionNumero = 57;
                    break;
                case "ParadoAndandoI":
                    transicionNumero = 5;
                    break;
                case "ParadoAndandoD":
                    transicionNumero = 4;
                    break;
                case "CualquieraSaltando":
                    transicionNumero = 6;
                    break;
                case "CualquieraParado":
                    transicionNumero = 197;
                    break;
                case "CorriendoParado":
                    transicionNumero = 94;
                    break;
                case "SaltandoParado":
                    transicionNumero = 95;
                    break;
                case "LadoParadoD":
                    transicionNumero = 96;
                    break;
                case "LadoParadoI":
                    transicionNumero = 97;
                    break;
                case "Atras":
                    transicionNumero = 2;
                    break;
                case "ParandoAtras":
                    transicionNumero = 0;
                    break;
                case "Herido":
                    transicionNumero = 10;
                    break;
                case "RecuperadoHerido":
                    transicionNumero = 37;
                    break;
                case "Desactivado":
                    transicionNumero = 9;
                    break;
                case "Abajo":
                    transicionNumero = 92;
                    break;
                case "Levantarse":
                    transicionNumero = 93;
                    break;
                //  Animacion.SetInteger("Velocidad", 9) se utiliza para la salida a abatido, quedaria en posicion horizontal
                default:
                    // code block
                    break;
            }
            Animacion.SetInteger("Velocidad", transicionNumero);
        }
        else
        {
            numTransicionRed = Num;
            Animacion.SetInteger("Velocidad", numTransicionRed);
        }
    }

    private bool IsMouseInsideGameWindow()
    {
        Vector3 mousePos = Input.mousePosition;
        return mousePos.x >= 0 && mousePos.x <= Screen.width &&
               mousePos.y >= 0 && mousePos.y <= Screen.height;
    }


    private void Update()
    {
        stateName = Animacion.GetCurrentAnimatorClipInfo(0)[0].clip.name;
        if (IsOwner)
        {            
            // cursor activo dentro de ventana:
            if (cam != null)
            {           
                if (IsMouseInsideGameWindow())
                {                 
                    cam.m_XAxis.m_InputAxisName = "Mouse X";
                    cam.m_YAxis.m_InputAxisName = "Mouse Y";
                    //
                    pausado = false;
                    /*
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                    */
                }
                else
                {
                    // Desactivar input de cámara
                    cam.m_XAxis.m_InputAxisName = "";
                    cam.m_YAxis.m_InputAxisName = "";
                    // desactivar giro camara:
                    cam.m_XAxis.m_InputAxisValue = 0f;
                    cam.m_YAxis.m_InputAxisValue = 0f;
                    //
                    /*
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                    */
                    pausado = true;
                    Debug.Log("fuera de ventana");
                }
            }

            //
            if (miSceneChangeHandler.ListoCamara == true && miPlayerManager.Desactivado == true)
            {
                Estado = "Parado";
                miPlayerManager.Desactivado = false;
                cam = miSceneChangeHandler.freeLookCamera;
                camPpal = Camera.main;
                pausado = false;               
             
            }
           
            if (miPlayerManager.Desactivado == true) return; // solo se maneja movimiento y animacion cuando
            // este activado el manager
            if (miPlayerManager.Herido)
            {
                Estado = "Herido";
            }
            else if (miPlayerManager.RecuperadoHerido)
            {
                Estado = "RecuperadoHerido";
            }
            else if (miPlayerManager.Desactivado)
            {
                Estado = "Desactivado";
            }
           if(cam == null)
            {
                Debug.Log("Camara NO **** encontrada");
            }           
            
            if (Estado != "Desactivado" && pausado == false)
            {
                HandleGravityandJump();
                HandleRotation();
                HandleMovement();
                ResetearSeguridad();

                if (Input.GetMouseButtonDown(0))
                {
                    disparar();                   
                }
            }
        } // fin is Owner
        else
        {
            if (velocidad == 0 && stateName != "IDL")
            {
                restablecerAnimacionClient();
            }
        }
    }


    private void restablecerAnimacionClient()
    {
        TransicionActual = "CualquieraParado";
        transicionNumero = 197;
    }

    private void ResetearSeguridad() // Por si la animacion o movimiento estan trabados y no responden a acciones de teclado
    {
        stateInfo = Animacion.GetCurrentAnimatorStateInfo(0);
        bool isMoving = velocidad != 0 || velocidadLateral != 0;
        if (isMoving && !Input.anyKey && !Input.anyKeyDown || !isMoving && !stateInfo.IsName("Rifle Idle") && !Input.anyKey && !Input.anyKeyDown)
        {
            if (Mathf.Floor(timeElapsed) % 1 == 0)
            {
                if (Mathf.Floor(timeElapsed) > 1)
                { // un segundo de espera por si se traba la animacion o movimiento
                    velocidad = 0;
                    velocidadLateral = 0;
                    TransicionActual = "CualquieraParado";
                    Estado = "Parado";
                    timeElapsed = 0;
                    transicionNumero = 197;
                }
                else
                {
                    timeElapsed += Time.deltaTime;
                }
            }
        }
    }

}
