using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MushGroup : MonoBehaviour
{
    private Light2D _light2D;

    private void Awake()
    {
        _light2D = GetComponent<Light2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Light2D[] lights = GetComponentsInChildren<Light2D>();
        foreach (Light2D l in lights)
        {
            if (l.lightType == Light2D.LightType.Point)
                l.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
