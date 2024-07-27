using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Godray : MonoBehaviour
{
    private Light2D _light2D;

    private float _initIntensity;
    private float _initFalloff;
    private float _initPosX;
    private Sequence _lightSequence;
    private Tween _moverTween;
    private Tween _intensityTween;
    [SerializeField] private bool starMovingRight = false;
    [SerializeField] private float moveOffsetX = 1;
    [SerializeField] private float moveOffsetTime = 2.5f;
    [SerializeField] private float intensityOffsetTime = 2.5f;

    private void Awake()
    {
        _light2D = GetComponent<Light2D>();
        _initIntensity = _light2D.intensity;
        _initFalloff = _light2D.falloffIntensity;
        _initPosX = transform.position.x;
        _lightSequence = DOTween.Sequence();
        _lightSequence.Append(DOTween.To(() => _light2D.intensity, x => _light2D.intensity = x, _initIntensity + 0.2f,
            intensityOffsetTime));
        _lightSequence.Join(DOTween.To(() => _light2D.falloffIntensity, x => _light2D.falloffIntensity = x, _initFalloff - 0.01f,
            intensityOffsetTime));
        _lightSequence.SetAutoKill(false).SetLoops(-1,LoopType.Yoyo).Pause();

        if (starMovingRight)
        {
            _moverTween = transform.DOMoveX(_initPosX + moveOffsetX, moveOffsetTime).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetAutoKill(false).Pause();
        }
        else
        {
            _moverTween = transform.DOMoveX(_initPosX - moveOffsetX, moveOffsetTime).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetAutoKill(false).Pause();

        }
    }

    // Start is called before the first frame update
    void Start()
    {
        ToggleTweens(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ToggleTweens(bool toggle)
    {
        if (toggle)
        {
            _moverTween.Rewind();
            _moverTween.Play();
            
            _lightSequence.Rewind();
            _lightSequence.Play();
        }
        else
        {
            _moverTween.Rewind();
            _lightSequence.Rewind();
        }
    }

    private void OnEnable()
    {
        ToggleTweens(true);
    }

    private void OnDisable()
    {
        ToggleTweens(false);
    }
}
