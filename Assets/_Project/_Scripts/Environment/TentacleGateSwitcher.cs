using System;
using System.Collections;
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
    [SerializeField] private float activeFeedbackAlpha;
    [SerializeField] private float defaultFeedbackAlpha;
    [FormerlySerializedAs("gateSwitchCorals")] [FormerlySerializedAs("gateSwitchTentacles")] [SerializeField] private CoralsController switchCoralsController;
    
    [Header("UX circle")]
    [SerializeField] private SpriteRenderer circleSprite;
    [SerializeField] private float circleRotateDuration;
    [SerializeField] private float tweenDuration;

    public bool playerInRange;
    
    private Tween _spriteScaleTween;
    private Tween _spriteRotateTween;
    private Tween _lightTween;
    private Sequence _playerFeedbackTween;
    private TentacleGate _tentacleGate;

    private bool _feedbackActive;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _tentacleGate = transform.parent.GetComponent<TentacleGate>();
    }

    private void Init()
    {
        circleSprite.transform.rotation = quaternion.identity;
        Color tmp = circleSprite.color;
        tmp.a = defaultFeedbackAlpha;
        circleSprite.color = tmp;

        _playerFeedbackTween = DOTween.Sequence();
        /*_playerFeedbackTween.Append(circleSprite.transform.DOScale(
            1.5f,
            tweenDuration));*/
        _playerFeedbackTween.Join(DOTween.To(() => circleSprite.color.a, x =>
                {
                    Color tmp = circleSprite.color;
                    tmp.a = x;
                    circleSprite.color = tmp;
                },
                activeFeedbackAlpha, tweenDuration));
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
        //puts tentacles on top
        //gateSwitchCorals.ToggleCorals(true);
        //stop regular player feedback
        //_playerFeedbackTween.Rewind();
        _feedbackActive = false;

        //
        // bulbLight.intensity = defaultLightIntensity;
        // Color tmp = circleSprite.color;
        // tmp.a = defaultFeedbackAlpha;
        // circleSprite.color = tmp;
        
        Debug.Log("Disabling switcher");
        //Now make bulb small and turn off light
    }

    private void OnEnable()
    {
        Init();
    }

    public void EnableSwitcher()
    {
        //gateSwitchCorals.ToggleCorals(false);
        _playerFeedbackTween.Play();
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerInRange = true;
            _tentacleGate.canOpenGate = true;
            StartCoroutine(TogglePlayerInRangeFeedback());

        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && playerInRange && !_feedbackActive && _tentacleGate.canOpenGate)
        {
            _feedbackActive = true;
            StartCoroutine(TogglePlayerInRangeFeedback());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerInRange = false;
            _tentacleGate.canOpenGate = false;
            _feedbackActive = false;

            _playerFeedbackTween.Pause();
            Sequence seq = DOTween.Sequence();
            seq.Append(DOTween.To(() => bulbLight.intensity, x => bulbLight.intensity = x,
                defaultLightIntensity,
                tweenDuration/3));

            seq.Join(DOTween.To(() => circleSprite.color.a, x =>
                {
                    Color tmp = circleSprite.color;
                    tmp.a = x;
                    circleSprite.color = tmp;
                },
                defaultFeedbackAlpha, tweenDuration / 3));
            seq.OnComplete(() => { _playerFeedbackTween.Restart(); });

        }
    }

    //When player in range we stop regular feedback and just put bulb and ux lit
    IEnumerator TogglePlayerInRangeFeedback()
    {
        yield return new WaitForSeconds(0.1f);
        if (playerInRange && _tentacleGate.canOpenGate)
        {
            _playerFeedbackTween.Pause();
            DOTween.To(() => bulbLight.intensity, x => bulbLight.intensity = x,
                activeLightIntensity,
                tweenDuration/3);

            DOTween.To(() => circleSprite.color.a, x =>
                {
                    Color tmp = circleSprite.color;
                    tmp.a = x;
                    circleSprite.color = tmp;
                },
                activeFeedbackAlpha, tweenDuration / 3);
            
            _feedbackActive = true;

        }
    }
}
