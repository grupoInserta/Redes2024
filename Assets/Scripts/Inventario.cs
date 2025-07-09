
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class Inventario : NetworkBehaviour
{
    public Image Rojo;
    private TextMeshProUGUI textoImpactos;
    private TextMeshProUGUI textoMunicion;
    public TextMeshProUGUI textoLlevoDocumento;
    private TextMeshProUGUI textoTransmision;
    public TextMeshProUGUI textoScoreLocal { get; set; }
    public TextMeshProUGUI textoScoreContrario { get; set; }
    // poner barras progreso
    [Header("UI Components")]
    public Image fillBarDoc; // Asignar el componente de relleno
    public Image fillBarTrans; // Asignar el componente de relleno
    public Image fillBarSalud; // Asignar el componente de relleno
    [Header("Progress Values")]
    private float currentValueDoc; // Valor actual
    private float currentValueTrans;
    private float currentValueSalud;

    // sonidos

    [SerializeField]
    private AudioClip obtDoc;
    [SerializeField]
    private AudioClip transmision;
    [SerializeField]
    private AudioClip ok;
    private AudioSource audioSource;
    public bool transmitiendo { get; set; }
    public bool copiandoDoc { get; set; }
    private bool documentoTransmitido;
    private bool documentoCopiado;
    private DamageFlashEffect miDamageFlashEffect;

    void Awake()
    {
       
        //SetPanelOpacity(0f);// ROJO
        textoImpactos = transform.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>();        
        textoMunicion = transform.GetChild(1).GetChild(5).GetComponent<TextMeshProUGUI>();        
        textoLlevoDocumento = transform.GetChild(1).GetChild(6).GetComponent<TextMeshProUGUI>();        
        textoTransmision = transform.GetChild(1).GetChild(9).GetComponent<TextMeshProUGUI>();
        textoScoreLocal = transform.GetChild(3).GetChild(2).GetComponent<TextMeshProUGUI>();
        textoScoreContrario = transform.GetChild(3).GetChild(3).GetComponent<TextMeshProUGUI>();
        audioSource = GetComponent<AudioSource>();
        audioSource.enabled = true;
        transmitiendo = false;
        copiandoDoc = false;
        documentoTransmitido = false;
        documentoCopiado = false;
        miDamageFlashEffect = GetComponent<DamageFlashEffect>();
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
    public void mostrarImpactos(int _impactosRecibidos, int _maximoImpactos)
    {
        miDamageFlashEffect.MostrarFlash(Rojo);             
        //textoImpactos.text = impactosRecibidos.ToString();
        float maximoImpactos = (float) _maximoImpactos;
        float impactosRecibidos = (float) _impactosRecibidos;
        UpdateProgressSalud(impactosRecibidos, maximoImpactos);
    }

    public void pararSonido()
    {
        audioSource.Pause();       
    }

    public void mostrarDocumento(float cantDocObtenido, float total)//QUE SE RECIBE!!!
    {
        if (cantDocObtenido >= total && copiandoDoc == true)
        {            
            textoLlevoDocumento.text = "OK";//

            if (audioSource != null && ok != null)
            {
                Debug.Log("OIR SONIDO OBTENCIAON DOc");
                audioSource.Pause();
                audioSource.clip = ok;
                audioSource.volume = 1f;
                audioSource.Play();
            }
            documentoCopiado = true;            
            copiandoDoc = false;
        }

        UpdateProgressDoc((float)cantDocObtenido, (float)total);
        if (copiandoDoc == false && cantDocObtenido > 0 && !documentoCopiado)
        {
            copiandoDoc = true;
            if (audioSource != null && obtDoc != null)
            {
                audioSource.volume = 0.2f;
                audioSource.clip = obtDoc;
                audioSource.Play();
            }
        }
    }

    public void resetearDatos()
    {
        transmitiendo = false;
        copiandoDoc = false;
        documentoCopiado = false;
        documentoTransmitido = false;
        documentoCopiado = false;
        textoTransmision.text = "";
        textoLlevoDocumento.text = "";
    }

    public void mostrarTransmision(float cantTransmisionEmitida, float TotalTransmision)
    {
        if (cantTransmisionEmitida >= TotalTransmision-1 && transmitiendo == true)
        {
            textoTransmision.text = "OK";
            if (audioSource != null && ok != null)
            {
                audioSource.Pause();
                audioSource.clip = ok;
                audioSource.volume = 1f;
                audioSource.Play();
            }
            transmitiendo = false;
            documentoTransmitido = true;
        }
        //IMPLEMENTAR BARRA PROGRESO
        UpdateProgressTrans(cantTransmisionEmitida,TotalTransmision);
        if ( transmitiendo == false && cantTransmisionEmitida > 0 && !documentoTransmitido)
        {
            transmitiendo = true;
            if (audioSource != null && transmision != null)
            {
                audioSource.volume = 0.2f;
                audioSource.clip = transmision;
                audioSource.Play();
            }           
        }
    }

    // BARRAS DE PROGRESO:
    // Método para actualizar la barra de progreso

    private void UpdateProgressSalud(float impactosRecibidos, float totalPermitidos)
    {      
        currentValueSalud = Mathf.Clamp(impactosRecibidos, 0, totalPermitidos);
        fillBarSalud.fillAmount = 1f - (currentValueSalud / totalPermitidos);
    }

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

}
