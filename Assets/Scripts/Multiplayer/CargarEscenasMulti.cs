using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
public class CargarEscenasMulti : NetworkBehaviour
{
    [SerializeField]
    public string escena;
    public void CargarEscena()
    {
        SceneManager.LoadScene(escena);
    }

    public void CargarEscenaError()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("Error", LoadSceneMode.Single);
    }
        public void CargarEscenaNombre(string txt)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(txt, LoadSceneMode.Single);
            // single significa que reemplaza totalmente a la escena actual
        }
    }
    public void CargarEscenaAleat()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            string[] Escenas = new string[2];
            Escenas[0] = "Nivel1";
            Escenas[1] = "Nivel2";
            
            
            int indiceAleatorio = Random.Range(0, Escenas.Length);
            
            string escenaAleatoria = Escenas[indiceAleatorio];
           
            //escenaAleatoria = "Nivel1"; 
            short escenavisitada = DatosGlobales.Instance.AdjuntarEscena(escenaAleatoria);
            if (escenavisitada == 1)
            {
                CargarEscenaAleat();
            }
            else if(escenavisitada == 0)
            {                
                NetworkManager.Singleton.SceneManager.LoadScene(escenaAleatoria, LoadSceneMode.Single);
            }
            else
            {
                NetworkManager.Singleton.SceneManager.LoadScene("Final", LoadSceneMode.Single);
            }
           
            // single significa que reemplaza totalmente a la escena actual
            NetworkManager.Singleton.SceneManager.LoadScene("Nivel1", LoadSceneMode.Single);

        }
    }

}
