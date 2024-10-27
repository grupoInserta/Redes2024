using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //Creamos la variable del player donde le asignamos el player que vamos a usar
    [SerializeField] GameObject player;
    //Accedemos al player mediante el transform del mismo para poder mover al personaje
    private float vertical = 0;
    private float horizontal = 0;

    private float mouseVertical = 0;
    private float mouseHorizontal = 0;

    Vector3 inputValues = Vector3.zero;

    [SerializeField] float rotationSmoothTime;
    float currentAngle;
    float currentAngleVelocity;

    PlayerController controller;
    Camera cam;
    

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        cam = Camera.main;
    }

    private void Update()
    {
       

        mouseVertical = Input.GetAxis("Mouse X");
        mouseHorizontal = Input.GetAxis("Mouse Y");
        //Vector2 MouseY += mouseVertical;
        //Vector2 MouseX += mouseHorizontal;


        //El personaje tiene que rotar con respecto a la camara

        float targetAngel = Mathf.Atan2(inputValues.x, inputValues.z) * Mathf.Rad2Deg + cam.transform.eulerAngles.y;

        
    }

    private void HandleMovement()
    {
        
        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");

        inputValues.z += vertical;
        inputValues.x += horizontal;

        player.transform.position += inputValues;


        if(inputValues.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(inputValues.x, inputValues.z) * Mathf.Rad2Deg + cam.transform.eulerAngles.y;
            currentAngle = Mathf.SmoothDamp(currentAngle, targetAngle, ref currentAngleVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0, currentAngle, 0);

            Vector3 rotatedMovement = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            
        }
    }
}
