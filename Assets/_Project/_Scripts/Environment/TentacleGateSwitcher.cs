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
    [SerializeField] private GateTentacles gateSwitchTentacles;
    
    [Header("UX circle")]
    [SerializeField] private SpriteRenderer circleSprite;
    [SerializeField] private float circleRotateDuration;
    [SerializeField] private float tweenDuration;

    private Tween _spriteScaleTween;
    private Tween _spriteRotateTween;
    private Tween _lightTween;
    private Sequence _playerFeedbackTween;


    private Vector3 _spriteScaleDefault;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    private void Init()
    {
        _spriteScaleDefault = circleSprite.transform.localScale;
        circleSprite.transform.rotation = quaternion.identity;
        circleSprite.transform.localScale = _spriteScaleDefault;

        circleSprite.color = new Color(circleSprite.color.r, circleSprite.color.g, circleSprite.color.b, 0);
        _playerFeedbackTween = DOTween.Sequence();
        _playerFeedbackTween.Append(circleSprite.transform.DOScale(
            1.5f,
            tweenDuration));
        _playerFeedbackTween.Join(DOTween.To(() => circleSprite.color.a, x =>
                {
                    Color tmp = circleSprite.color;
                    tmp.a = x;
                    circleSprite.color = tmp;
                },
                0.075f, tweenDuration));
        _playerFeedbackTween.Join(DOTween.To(() => bulbLight.intensity, x => bulbLight.intensity = x,
            activeLightIntensity,
            tweenDuration));
        _playerFeedbackTween.SetAutoKill(false).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        
        // _spriteScaleTween = circleSprite.transform.DOScale(
        //         _spriteScaleHigh - (Vector3.one * .4f),
        //         circleScaleDuration)
        //     .SetAutoKill(false).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        // _lightTween = DOTween.To(() => bulbLight.intensity, x => bulbLight.intensity = x, activeLightIntensity,
        //     circleScaleDuration).SetAutoKill(false).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        //
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
        gateSwitchTentacles.ToggleSwitchTentacles(true);
        _playerFeedbackTween.Rewind();
        //Now make bulb small and turn off light
    }

    private void OnDisable()
    {
        //_spriteScaleTween.Kill();
        //_spriteRotateTween.Kill();
    }

    private void OnEnable()
    {
        Init();
    }

    public void EnableSwitcher()
    {
        gateSwitchTentacles.ToggleSwitchTentacles(false);
        _playerFeedbackTween.Play();
        
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
