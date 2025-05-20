using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MultiplayerManagerSimple : MonoBehaviour
{
    [SerializeField] private Button jugarButton;
    [SerializeField] private TextMeshProUGUI TextoCargandoNivel;
    private CargarEscenasMulti cargarEscenasMulti;

    private void Start()
    {          
        cargarEscenasMulti = gameObject.GetComponent<CargarEscenasMulti>();
        jugarButton.onClick.AddListener(Jugar);
    }

   

    public void mostrarJugar()
    {
        jugarButton.gameObject.SetActive(true);        
    }

    private void Jugar()
    {        
        cargarEscenasMulti.CargarEscenaAleat();        
        TextoCargandoNivel.gameObject.SetActive(true);
        jugarButton.gameObject.SetActive(false);
    }
    
}
