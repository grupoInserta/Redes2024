using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bala : MonoBehaviour
{
    public Vector3 direccion;
    private float Velocidad;
    public float tiempoVida = 5f;
    public AudioSource audioSource;
    public AudioClip[] audioClips;

    private void Awake()
    {
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.enabled = true;
            audioSource.Play();
        }         
    }
    void Start()
    {
        Destroy(gameObject, tiempoVida);
        PlaySound(0);// disparo
    }

    public void PlaySound(int index)
    {
        if (index >= 0 && index < audioClips.Length)
        {
            audioSource.clip = audioClips[index];
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("Índice de sonido fuera de rango");
        }
    }

    public void ConfigurarVelocidad(float velocidad, Vector3 _direccion, Quaternion rotacion)
    {
        Velocidad = velocidad;
        direccion = _direccion;  // Configuramos la dirección hacia adelante
        transform.rotation = Quaternion.LookRotation(_direccion);
      // transform.rotation = rotacion;
    }

    private void OnTriggerEnter(Collider other)
    {
        /*
        if (other.gameObject.CompareTag("Obstaculo"))
        {
            PlaySound(1);// impacto
        }  
        */
    }

        // Update is called once per frame
    void Update()
    {
        transform.position += direccion * Velocidad * Time.deltaTime;
    }
}
