using System;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

public class TentacleGateSwitcher : MonoBehaviour
{
    [Header("Bulb")]
    [SerializeField] private Transform bulb;
    [SerializeField] private Light2D bulbLight;
    [SerializeField] private float activeLightIntensity;
    [SerializeField] private float defaultLightIntensity;
    
    [Header("UX circle")]
    [SerializeField] private SpriteRenderer circleSprite;
    [SerializeField] private float circleRotateDuration;
    [SerializeField] private float circleScaleDuration;
    [FormerlySerializedAs("spriteHighAlpha")] [SerializeField] private float circleHighAlpha;

    private Tween _spriteScaleTween;
    private Tween _spriteRotateTween;
    private Tween _lightTween;


    private Vector3 _spriteScaleHigh;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    private void Init()
    {
        _spriteScaleHigh = circleSprite.transform.localScale;
        circleSprite.transform.rotation = quaternion.identity;
        circleSprite.transform.localScale = _spriteScaleHigh;
        
        _spriteScaleTween = circleSprite.transform.DOScale(
                _spriteScaleHigh - (Vector3.one * .2f),
                circleScaleDuration)
            .SetAutoKill(false).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        
        _spriteRotateTween = circleSprite.transform.DORotate(
                new Vector3(0, 0, 360),
                circleRotateDuration,
                RotateMode.FastBeyond360)
            .SetRelative(true)
            .SetEase(Ease.Linear)
            .SetLoops(-1);

        
        
    }

    public void DisableSwitcher()
    {
        
        //Now make bulb small and turn off light
        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(.75f);
        seq.Append(bulb.DOScaleY(0.5f, 1f));
        seq.Join(DOTween.To(() => bulbLight.intensity, x => bulbLight.intensity = x, 0.25f,
            .75f));

    }

    private void OnDisable()
    {
        _spriteScaleTween.Kill();
        _spriteRotateTween.Kill();
    }

    private void OnEnable()
    {
        Init();
    }

    public void EnableSwitcher()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(bulb.DOScaleY(0.9f, .75f));
        seq.Join(DOTween.To(() => bulbLight.intensity, x => bulbLight.intensity = x, defaultLightIntensity,
            .75f));
        seq.Join(circleSprite.transform.DOScale(
            _spriteScaleHigh,
            .75f));  
        
        
    }

    public void PlayerCloseFeedback(bool toggle)
    {
        if (toggle)
        {
            _lightTween.Kill();
            _lightTween = DOTween.To(() => bulbLight.intensity, x => bulbLight.intensity = x, activeLightIntensity,
                .5f).SetAutoKill(false);
            
        }
        else
        {
            _lightTween.Kill();
            _lightTween = DOTween.To(() => bulbLight.intensity, x => bulbLight.intensity = x, defaultLightIntensity,
                .5f).SetAutoKill(false);
        }
    }
}
