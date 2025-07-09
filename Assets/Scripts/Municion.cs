using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Municion : MonoBehaviour
{
    int maximoUnidades = 1000;
    int unidadesActuales;
    // Start is called before the first frame update
    void Start()
    {
        unidadesActuales = maximoUnidades;
    }

    public void sumimistarMunicion()
    {
        if (maximoUnidades - unidadesActuales < 0)
        {
            unidadesActuales--;
        }
    }
}
