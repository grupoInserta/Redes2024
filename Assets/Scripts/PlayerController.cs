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

    private float speed;

    Vector3 inputValues = Vector3.zero;


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


        Vector3 movement = new Vector3(horizontal, 0, vertical);

        if(movement.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg;
            currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref currentAngleVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0, targetAngle, 0);
            player.Move(movement * speed * Time.deltaTime);
        }


        HandleMovement();

        //mouseVertical = Input.GetAxis("Mouse X");
        // mouseHorizontal = Input.GetAxis("Mouse Y");
        //Vector2 MouseY += mouseVertical;
        //Vector2 MouseX += mouseHorizontal;

    }

    
    private void HandleMovement()
    {


        float targetAngle = Mathf.Atan2(inputValues.x, inputValues.z) * Mathf.Rad2Deg + cam.transform.eulerAngles.y;
        currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref currentAngleVelocity, rotationSmoothTime);
        transform.rotation = Quaternion.Euler(0, currentAngle, 0);


        //float rotation = Mathf.Atan2(inputValues.x, inputValues.z) * Mathf.Rad2Deg + cam.transform.eulerAngles.y;
        //transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, rotation, ref currentAngleVelocity, rotationSmoothTime);

        
        
        
        
        //Debug.Log(transform.localEulerAngles = new Vector3 (player.transform.rotation.x, cam.transform.rotation.y));
    }
    
}

