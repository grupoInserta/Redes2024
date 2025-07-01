using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ControladorMenu : MonoBehaviour
{
    [SerializeField] private Button RelayButton;
    private cargaEscenasMulti miCargaEscenasMulti;
    // Start is called before the first frame update
    void Start()
    {
        RelayButton.onClick.AddListener(IrARelay);
        miCargaEscenasMulti = gameObject.GetComponent<cargaEscenasMulti>();
        //Screen.SetResolution(1920, 1080, false);
        Screen.SetResolution(1230, 768, false);
        //Screen.SetResolution(900, 600, false);
    }

    private void IrARelay()
    {
        miCargaEscenasMulti.CargarEscenaNombre("InicioRelay");
    }

 

    // Update is called once per frame
    void Update()
    {
        
    }
}
