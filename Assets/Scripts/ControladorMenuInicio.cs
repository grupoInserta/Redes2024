using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ControladorMenuInicio : MonoBehaviour
{

    [SerializeField]
    private GameObject InterfazP2P;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void mostrarPanelP2P()
    {
        InterfazP2P.SetActive(true);
        
    }

    public void mostrarPanelServidor()
    {
        InterfazP2P.SetActive(false); 
        SceneManager.LoadScene("InicioRelay");        
        gameObject.SetActive(false); // se esconden los elementos visuales del inicio
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
