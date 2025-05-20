using UnityEngine;

public class ExplosionGrande : MonoBehaviour
{
    public AudioClip explosion;
    private AudioSource audioSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        audioSource.clip = explosion;
        audioSource.Play();
    }

    // Update is called once per frame

}
