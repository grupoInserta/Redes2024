
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class Inventario : NetworkBehaviour
{
    private Image Rojo;
    private TextMeshProUGUI textoImpactos;
    private TextMeshProUGUI textoMunicion;
    public TextMeshProUGUI textoLlevoDocumento;
    private TextMeshProUGUI textoTransmision;
    public TextMeshProUGUI textoScoreLocal { get; set; }
    public TextMeshProUGUI textoScoreContrario { get; set; }

    // poner barra progreso
    [Header("UI Components")]
    public Image fillBarDoc; // Asignar el componente de relleno
    public Image fillBarTrans; // Asignar el componente de relleno
    [Header("Progress Values")]
    private float currentValueDoc; // Valor actual
    private float currentValueTrans;
    void Awake()
    {
        Rojo = transform.GetChild(0).gameObject.GetComponent<Image>();
        SetPanelOpacity(0f);// ROJO
        // Inventario es:  transform.GetChild(1)
        textoImpactos = transform.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>();        
        textoMunicion = transform.GetChild(1).GetChild(5).GetComponent<TextMeshProUGUI>();        
        textoLlevoDocumento = transform.GetChild(1).GetChild(6).GetComponent<TextMeshProUGUI>();        
        textoTransmision = transform.GetChild(1).GetChild(9).GetComponent<TextMeshProUGUI>();
        textoScoreLocal = transform.GetChild(3).GetChild(2).GetComponent<TextMeshProUGUI>();
        textoScoreContrario = transform.GetChild(3).GetChild(3).GetComponent<TextMeshProUGUI>();
    }

    public void SetPanelOpacity(float alpha)
    {
        if (Rojo != null)
        {
            // Asegúrate de que el alpha esté entre 0 y 1
            alpha = Mathf.Clamp01(alpha);

            // Obtén el color actual del panel
            Color currentColor = Rojo.color;

            // Ajusta el canal alpha
            currentColor.a = alpha;

            // Asigna el nuevo color al panel
            Rojo.color = currentColor;
        }
    }

    public void IncrementarContadorScore(int _local, int _contrario)
    {
        if(_local != 1000)
        {
            textoScoreLocal.text = _local.ToString();
        }
        if(_contrario != 1000)
        {
            textoScoreContrario.text = _contrario.ToString();
        }
        
    }

    // actualizaciones del Inventario:
    public void mostrarMunicion(int numBalas)
    {
        textoMunicion.text = numBalas.ToString();
    }
    public void mostrarImpactos(int impactosRecibidos)
    {
        textoImpactos.text = impactosRecibidos.ToString();
    }
    public void mostrarDocumento(float cantDocObtenido, float total)
    {
        if(cantDocObtenido >= total)
        {
            textoLlevoDocumento.text = "OK ";// cambiar por el num de documentos ?
        }

        UpdateProgressDoc((float)cantDocObtenido, (float)total);
        //IMPLEMENTAR BARRA PROGRESO 
    }
    public void mostrarTransmision(float cantTransmisionEmitida, float TotalTransmision)
    {
        if(cantTransmisionEmitida>= TotalTransmision)
        {
            textoTransmision.text = "OK";
        }       
        //IMPLEMENTAR BARRA PROGRESO 2
        UpdateProgressTrans((float)cantTransmisionEmitida, (float)TotalTransmision);
    }

    // BARRAS DE PROGRESO:
// Método para actualizar la barra de progreso
private void UpdateProgressDoc(float current, float maxValue) { 
    currentValueDoc = Mathf.Clamp(current, 0, maxValue); // Asegura que esté dentro del rango
    fillBarDoc.fillAmount = currentValueDoc / maxValue; // Calcula el porcentaje
}
private void UpdateProgressTrans(float current, float maxValue)
{
    currentValueTrans = Mathf.Clamp(current, 0, maxValue); // Asegura que esté dentro del rango
    fillBarTrans.fillAmount = currentValueTrans / maxValue; // Calcula el porcentaje
}

// Update is called once per frame
void Update()
    {

    }
}
