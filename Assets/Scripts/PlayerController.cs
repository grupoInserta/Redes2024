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

    private void Update()
    {
        Vector3 inputValues = Vector3.zero;

        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");

        inputValues.z += vertical;
        inputValues.x += horizontal;

        player.transform.position += inputValues;

        mouseVertical = Input.GetAxis("Mouse X");
        mouseHorizontal = Input.GetAxis("Mouse Y");
        Vector2 MouseY += mouseVertical;
        Vector2 MouseX += mouseHorizontal;
        
    }
}
