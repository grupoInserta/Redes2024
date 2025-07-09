
using Unity.Netcode;
using UnityEngine;
using Cinemachine;
using System.Collections;
using System.Collections.Generic;

/*
    This file has a commented version with details about how each line works. 
    The commented version contains code that is easier and simpler to read. This file is minified.
*/


/// <summary>
/// Main script for third-person movement of the character in the game.
/// Make sure that the object that will receive this script (the player) 
/// has the Player tag and the Character Controller component.
/// </summary>
public class ThirdPersonController : NetworkBehaviour
{

    [Tooltip("Speed ​​at which the character moves. It is not affected by gravity or jumping.")]
    public float velocity = 5f;
    [Tooltip("This value is added to the speed value while the character is sprinting.")]
    public float sprintAdittion = 3.5f;
    [Tooltip("The higher the value, the higher the character will jump.")]
    public float jumpForce = 9f;
    [Tooltip("Stay in the air. The higher the value, the longer the character floats before falling.")]
    public float jumpTime = 0.85f;
    [Space]
    [Tooltip("Force that pulls the player down. Changing this value causes all movement, jumping and falling to be changed as well.")]
    public float gravity = 9.8f;
    float jumpElapsedTime = 0;

    // Player states
    bool isJumping = false;
    bool isSprinting = false;
    bool isCrouching = false;
    bool apuntando = false;
    private Vector3 moveDirection;

    // Inputs
    float inputHorizontal;
    float inputVertical;
    bool inputJump;
    bool inputCrouch;
    bool inputSprint;

    Animator animator;
    CharacterController cc;

    // anterior PlayerController:
    public string Estado { get; set; }
    public int numTransicionRed { get; set; }
    // disparos
    [SerializeField] private GameObject Bala;
    private GameObject[] ArrayBalas;
    [SerializeField] public Transform bocaCannon;
    private float fuerzaDisparo = 40f; //20
    private PlayerManager miPlayerManager;
    private int MaximoBalas;
    private int contadorBalas; // DISPARADAS!!!
    public bool soyLocal;
    public bool pausado;
    private CinemachineFreeLook cam;
    private Camera camPpal;
    public AudioSource audioSource;
    public AudioClip disparo;
    private float rotationSpeed = 5f;

    private void Awake()
    {
        Iniciar();
        audioSource = GetComponent<AudioSource>();
        audioSource.enabled = true;
    }

    public void pararSonido()
    {
        audioSource.Pause();
    }  
    public void Iniciar()
    {
        miPlayerManager = gameObject.GetComponent<PlayerManager>();
        MaximoBalas = miPlayerManager.obtenerMaxBalas();
        ArrayBalas = new GameObject[100];
        contadorBalas = 0;
        pausado = true;
    }

    /// <summary>
    /// DISPAROS
    /// </summary>
  [ClientRpc]
    private void ShootProjectileClientRpc(Vector3 posicionCannon, Quaternion rotacion, Vector3 direccion)
    {
        ArrayBalas[contadorBalas] = Instantiate(Bala, posicionCannon, rotacion);
        ArrayBalas[contadorBalas].GetComponent<Bala>().ConfigurarVelocidad(fuerzaDisparo, direccion, rotacion, gameObject);
        contadorBalas++;
        miPlayerManager.quitarBala();
    }


    [ServerRpc]
    private void RequestShootServerRpc(ServerRpcParams serverRpcParams = default)
    {
        // Validación adicional si es necesario (por ejemplo, cooldown o munición)
        // Instanciar y sincronizar el proyectil en todos los clientes
        ulong shooterId = serverRpcParams.Receive.SenderClientId;

        if (miPlayerManager.controladorNivel == null)
        {
            Debug.Log("miPlayerManager.controladorNivel  no valido");
        }

        float maxHitDistance = 1.5f;     // radio del cilindro (lateral)
        float maxRange = 25.0f;
        foreach (var clientPair in NetworkManager.Singleton.ConnectedClients)
        {
            ulong clientId = clientPair.Key;
            NetworkClient client = clientPair.Value;
            NetworkObject playerObject = client.PlayerObject;

            if (playerObject != null && playerObject.OwnerClientId != shooterId)
            {
                GameObject playerGO = playerObject.gameObject;// es el gameobject del player alcanzado!!
                Vector3 origin = transform.position;
                Vector3 dir = transform.forward;
                Vector3 toPlayer = playerGO.transform.position - origin;

                float projectionLength = Vector3.Dot(toPlayer, dir); // distancia hacia delante
                float distanceToRay = Vector3.Cross(dir, toPlayer).magnitude; // distancia lateral

                if (projectionLength >= 0f && projectionLength <= maxRange && distanceToRay <= maxHitDistance)
                {
                    playerGO.GetComponent<PlayerManager>().HerirCliente();
                    Debug.Log($"Jugador {shooterId} impactó a {clientId} dentro de zona cilíndrica.");
                }
            }
        }

        ShootProjectileClientRpc(bocaCannon.transform.position , bocaCannon.transform.rotation, bocaCannon.transform.forward);
    }

    [ServerRpc]
    void comunicarSonidoDisparoServerRpc(Vector3 _posicion)
    {
        comunicarSonidoDisparoClientRpc(_posicion);
    }

    [ClientRpc]
    void comunicarSonidoDisparoClientRpc(Vector3 _posicion)
    {
        if (audioSource != null && disparo != null )
        {
            float distancia = Vector3.Distance(transform.position, _posicion);
            if (distancia < 0.5f) return;
            float volumen = Mathf.Clamp01(1f - (distancia));
            audioSource.volume = volumen;
            audioSource.clip = disparo;
            audioSource.Play();
        }
    }
    private void disparar()
    {
        if (!IsOwner) return;
        if (contadorBalas >= MaximoBalas) // vaciar todo de balas
        {
            ArrayBalas = new GameObject[100];
            contadorBalas = 0;
        }
        if(contadorBalas < MaximoBalas)
        {
            audioSource.clip = disparo;
            audioSource.Play();
            comunicarSonidoDisparoServerRpc(transform.position);
        }              

        if (miPlayerManager.numBalas > 0)
        {
            RequestShootServerRpc();
        }
    }
   

    [ServerRpc]
    private void UpdateAnimacionServerRpc(string tipoAnimacion, bool _booleano)
    {
        // Actualizar la variable de red en el servidor
        // y seguidamente en los clientes
        UpdateAnimacionClientRpc(tipoAnimacion, _booleano);
    }

    [ClientRpc]
    private void UpdateAnimacionClientRpc(string tipoAnimacion, bool _booleano)
    {
        HandleAnimation(tipoAnimacion, _booleano); // solo enviamos el mumero de la variable del componente animator
    }


    public void HandleAnimation(string tipoAnimacion, bool _booleano)
    { 
        if(animator != null)
        {
            animator.SetBool(tipoAnimacion, _booleano);
        }        
    }

    // ANIMACIONES Y MOVIMIENTO
    void Start()
    {
       // cc = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();        
        //cc.center = new Vector3(0, cc.height / 2f, 0); AJUSTAR ALTURA


        // Message informing the user that they forgot to add an animator
        if (animator == null)
            Debug.LogWarning("Hey buddy, you don't have the Animator component in your player. Without it, the animations won't work.");
    }


    // Update is only being used here to identify keys and trigger animations
    void Update()
    {
        if (miPlayerManager.ListoCamara)
        {
            cc = GetComponent<CharacterController>();
            cc.center = new Vector3(0, cc.height / 2f, 0); //AJUSTAR ALTURA
        }
        if (!IsOwner || cc == null)
        { return; }
        // menejar el estado del player para que se pueda mover o no dependiendo de la escena, esta herido etc.
        if (DatosGlobales.Instance.pausado == false)
        {
            pausado = false;
        }
        else
        {
            pausado = true;
        }
        //
        if (miPlayerManager.ListoCamara == true && miPlayerManager.Desactivado == true)
        {
            Estado = "Parado";
            miPlayerManager.Desactivado = false;
            cam = miPlayerManager.freeLookCamera;
            camPpal = Camera.main;
            pausado = false;
        }

        if (miPlayerManager.Desactivado == true) return; // solo se maneja movimiento y animacion cuando
                                                         // este activado el manager
        
       if (miPlayerManager.Desactivado)
        {
            Estado = "Desactivado";
        }
        if (cam == null)
        {
            Debug.Log("Camara NO **** encontrada");
        }

        if (Estado != "Desactivado" && pausado == false && Input.GetMouseButtonDown(0))
        {
            disparar();            
        }
        // Input checkers, PARTE del Paquete de Animacion Estandar
        inputHorizontal = Input.GetAxis("Horizontal");
        inputVertical = Input.GetAxis("Vertical");
        inputJump = Input.GetAxis("Jump") == 1f;
        inputSprint = Input.GetAxis("Fire3") == 1f; // QUE ES? 
        // Unfortunately GetAxis does not work with GetKeyDown, so inputs must be taken individually
        inputCrouch = Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.JoystickButton1);
        // tecla Ctrl, de la izquierda 
        // Check if you pressed the crouch input key and change the player's state
        if ( inputCrouch )
            isCrouching = !isCrouching;

        if (isCrouching || isSprinting || isJumping || cc.velocity.magnitude >= 0.1f)
        {
            apuntando = false;
        }
        //GetMouseButtonDown(1) una pulsacion
        if (Input.GetMouseButton(1) && !isJumping && !isSprinting && !isCrouching && cc.velocity.magnitude <= 0.1f) // 1 = botón derecho del ratón
        {
            apuntando = true;            
        }
        if (Input.GetMouseButtonUp(1) && apuntando) // 1 = botón derecho del ratón
        {
            apuntando = false;
        }

        // Run and Crouch animation
        // If dont have animator component, this block wont run
        if ( cc.isGrounded && animator != null )
        {            
            // Crouch
            // Note: The crouch animation does not shrink the character's collider
            animator.SetBool("crouch", isCrouching);
            UpdateAnimacionServerRpc("crouch", isCrouching);
            // Run
            float minimumSpeed = 0.9f;
            bool mibooleano = cc.velocity.magnitude > minimumSpeed;
            animator.SetBool("run", mibooleano);
            UpdateAnimacionServerRpc("run", mibooleano);
            // Sprint

            isSprinting = cc.velocity.magnitude > minimumSpeed && inputSprint;           
            animator.SetBool("sprint", isSprinting );
            UpdateAnimacionServerRpc("sprint", isSprinting);
        }

        // Jump animation
        if( animator != null && cc != null)
            animator.SetBool("air", cc.isGrounded == false );        
            bool booleano = cc.isGrounded == false;
            UpdateAnimacionServerRpc("air", booleano);       

        // Handle can jump or not
        if ( inputJump && cc.isGrounded )
        {
            isJumping = true;
            // Disable crounching when jumping
            //isCrouching = false; 
        }

        if (apuntando)
        {      
            animator.SetBool("apuntar", true);
            UpdateAnimacionServerRpc("apuntar", true);
        }
        else
        {
            animator.SetBool("apuntar", false);
            UpdateAnimacionServerRpc("apuntar", false);
        }
        HeadHittingDetect();
    }


    // With the inputs and animations defined, FixedUpdate is responsible for applying movements and actions to the player
    private void FixedUpdate()
    {
        if (cc == null) return;
            // Sprinting velocity boost or crounching desacelerate
            float velocityAdittion = 0;
            if ( isSprinting )
                velocityAdittion = sprintAdittion;
            if (isCrouching)
                velocityAdittion =  - (velocity * 0.50f); // -50% velocity

            // Direction movement
            float directionX = inputHorizontal * (velocity + velocityAdittion) * Time.deltaTime;
            float directionZ = inputVertical * (velocity + velocityAdittion) * Time.deltaTime;
            float directionY = 0;

            // Jump handler
            if ( isJumping )
            {
                // Apply inertia and smoothness when climbing the jump
                // It is not necessary when descending, as gravity itself will gradually pulls
                directionY = Mathf.SmoothStep(jumpForce, jumpForce * 0.30f, jumpElapsedTime / jumpTime) * Time.deltaTime;

                // Jump timer
                jumpElapsedTime += Time.deltaTime;
                if (jumpElapsedTime >= jumpTime)
                {
                    isJumping = false;
                    jumpElapsedTime = 0;
                }
            }

            // Add gravity to Y axis
            directionY = directionY - gravity * Time.deltaTime;        
            // --- Character rotation --- 
            Vector3 forward = Camera.main.transform.forward;
            Vector3 right = Camera.main.transform.right;

            forward.y = 0;
            right.y = 0;

            forward.Normalize();
            right.Normalize();

            // Relate the front with the Z direction (depth) and right with X (lateral movement)
            forward = forward * directionZ;
            right = right * directionX;

            if (directionX != 0 || directionZ != 0)
            {
                float angle = Mathf.Atan2(forward.x + right.x, forward.z + right.z) * Mathf.Rad2Deg;
                if (!apuntando) {
                    Quaternion rotation = Quaternion.Euler(0, angle, 0);
                    transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.15f);
                }                                   
            }

        if (apuntando)
        {
           // GIRO SOBRE SU EJE DIRECCION OPUESTA A CAMARA CUANDO ESTA APUNTADO
            Vector3 cameraForward = camPpal.transform.forward;
            cameraForward.y = 0; // Ignorar la componente Y para que el movimiento sea en el plano horizontal
            cameraForward.Normalize();
            moveDirection = cameraForward; // La direccion es siempre forward 
            if (moveDirection != Vector3.zero)
            {
                moveDirection.x *= 2f; // Amplifica el bajandoeje X para más sensibilidad lateral
                moveDirection = moveDirection.normalized;
                //transform.rotation = Quaternion.LookRotation(moveDirection);
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }

            // --- End rotation ---        
            Vector3 verticalDirection = Vector3.up * directionY;
            Vector3 horizontalDirection = forward + right;

            Vector3 moviment = verticalDirection + horizontalDirection;
            cc.Move(moviment);
    }


    //This function makes the character end his jump if he hits his head on something
    void HeadHittingDetect()
    {
        float headHitDistance = 1.1f;
        Vector3 ccCenter = transform.TransformPoint(cc.center);
        float hitCalc = cc.height / 2f * headHitDistance;

        // Uncomment this line to see the Ray drawed in your characters head
        // Debug.DrawRay(ccCenter, Vector3.up * headHeight, Color.red);

        if (Physics.Raycast(ccCenter, Vector3.up, hitCalc))
        {
            jumpElapsedTime = 0;
            isJumping = false;
        }
    }

}
