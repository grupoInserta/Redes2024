using Unity.Netcode;
using UnityEngine;


public class Desconexion : NetworkBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        
        NetworkManager.Singleton.Shutdown();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
