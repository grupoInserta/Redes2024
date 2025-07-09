using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpcionesControlador : MonoBehaviour
{
    [SerializeField]
    public TMPro.TMP_Dropdown resolutionDropdown;
    [SerializeField]
    private GameObject volver_btn;
    private CargarEscenasMulti miCargarEscenas;

    // Start is called before the first frame update
    void Start()
    {
        miCargarEscenas = GetComponent<CargarEscenasMulti>();
    }

    public void cambiarResolucion(int index)
    {
        string selectedText = resolutionDropdown.options[index].text;
        char delimitador = 'x';
        string[] dimensiones = selectedText.Split(delimitador);
        int x = int.Parse(dimensiones[0]);
        int y = int.Parse(dimensiones[1]);
        Screen.SetResolution(x, y, false);
    }

    public void IrAMenu()
    {
        miCargarEscenas.CargarEscenaNombre("Menu");
    }
}
