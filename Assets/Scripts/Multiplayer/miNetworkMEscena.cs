
using UnityEngine;
using Unity.Netcode;

public class miNetworkMEscena : NetworkBehaviour
{


    public void Awake()
    {
        if (NetworkManager.Singleton.IsHost) 
        {
          
        }


    }
     
}
