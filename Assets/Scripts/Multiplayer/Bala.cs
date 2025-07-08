using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Bala : MonoBehaviour
{
    public Vector3 direccion;
    private float Velocidad;
    public float tiempoVida = 5f;
    public GameObject explosionPrefab;
    private GameObject Explosion;
    private GameObject Jugador;

    private void Awake()
    {        
             
    }
    void Start()
    {        
        Explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(Explosion, 3f);       
        Explosion.transform.position = transform.position;
        Destroy(gameObject, tiempoVida);
    }
    

    public void ConfigurarVelocidad(float velocidad, Vector3 _direccion, Quaternion rotacion, GameObject _jugador)
    {
        Velocidad = velocidad;
        direccion = _direccion;  // Configuramos la dirección hacia adelante
        transform.rotation = Quaternion.LookRotation(_direccion);
        Jugador = _jugador;
        // transform.rotation = rotacion;
    }

    
    // Update is called once per frame
    void Update()
    {
        transform.position += direccion * Velocidad * Time.deltaTime;
        if (Explosion != null)
        {
            Explosion.transform.position = Jugador.GetComponent<ThirdPersonController>().bocaCannon.transform.position;
        }
    }
}
