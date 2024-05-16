using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;


public class WispGlow : MonoBehaviour
{
   
    [SerializeField] float _glowMin;
    [SerializeField] float _glowMax;
    [SerializeField] float _glowTime;
    
    private float startTime;
    private bool glowUp = true;
    private Light2D _light2D;
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _light2D = GetComponent<Light2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        
        startTime = _glowTime;
    }

    void Update()
    {
        //Basically, invert the direction of change when the intensity of glow hits its limits
        if(_light2D.intensity <= _glowMin || _light2D.intensity >= _glowMax){
            glowUp = !glowUp;
            _glowTime = startTime;
        }

        _glowTime = _glowTime - Time.deltaTime;
        float t = _glowTime / startTime;
        _light2D.intensity = glowUp ? Mathf.SmoothStep(_glowMin, _glowMax, t) : Mathf.SmoothStep(_glowMax, _glowMin, t);
    }
}
