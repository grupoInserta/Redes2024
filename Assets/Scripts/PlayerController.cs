using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    //Creamos la variable del player donde le asignamos el player que vamos a usar
    [SerializeField] CharacterController player;
    //Accedemos al player mediante el transform del mismo para poder mover al personaje
    [SerializeField] private float vertical;
    [SerializeField] private float horizontal;

    private float mouseVertical = 0;
    private float mouseHorizontal = 0;

    [SerializeField] private float speed;

    [Header("Gravity")]
    [SerializeField] float gravity = 9.8f;
    [SerializeField] float gravityMultiplier = 2;
    [SerializeField] float groundedGravity = -0.5f;
    [SerializeField] float jumpHeight = 3f;

    private float velocityY;

    public Vector3 inputValues = Vector3.zero;

    [SerializeField] float rotationSmoothTime = 0.2f;
    float currentAngle;
    float currentAngleVelocity;

    PlayerController controller;
    Camera cam;

    private void Awake()
    {
        player = GetComponent<CharacterController>();
        controller = GetComponent<PlayerController>();
        cam = Camera.main;
    }

    private void Update()
    {
        //El personaje tiene que rotar con respecto a la camara
        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");

        inputValues.z = vertical;
        inputValues.x = horizontal;

        player.transform.position += inputValues;

        HandleMovement();

        HandleGravityandJump();
    }
    
    private void HandleMovement()
    {
        Vector3 movement = new Vector3(horizontal, 0, vertical);

        if (movement.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg + cam.transform.eulerAngles.y;
            currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref currentAngleVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0, currentAngle, 0);
            Vector3 rotatedMovement = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            player.Move(rotatedMovement * speed * Time.deltaTime);
        }
    }
    private void HandleGravityandJump()
    {
        if(player.isGrounded && velocityY < 0f)
        {
            velocityY = groundedGravity;
        }

        if(player.isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            velocityY = Mathf.Sqrt(jumpHeight * 2f * gravity);
        }

        velocityY -= gravity * gravityMultiplier * Time.deltaTime;
        player.Move(Vector3.up * velocityY * Time.deltaTime);
    }
}
