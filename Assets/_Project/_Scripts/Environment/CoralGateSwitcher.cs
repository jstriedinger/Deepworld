using System;
using System.Collections;
using System.Numerics;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using Vector3 = UnityEngine.Vector3;

public class CoralGateSwitcher : MonoBehaviour
{
    [Header("Feedbacks")]
    [SerializeField] private ParticleSystem[] bubbles;
    [SerializeField] private float activateAnimDuration;
    [SerializeField] private float resetAnimDuration;
    
    [Header("Bulb")]
    [SerializeField] private Transform bulb;
    private Vector3 _defaultBulbScale;
    [SerializeField] private Light2D bulbLight;
    [SerializeField] private float activeLightIntensity;
    [SerializeField] private float defaultLightIntensity;
    
    [Header("UX circle")]
    [SerializeField] private SpriteRenderer circleSprite;
    [SerializeField] private float circleRotateDuration;
    [SerializeField] private float defaultFeedbackAlpha;
    [SerializeField] private ParticleSystem vfxDust;

    public bool playerInRange;
    
    private Tween _spriteScaleTween;
    private Tween _spriteRotateTween;
    private Tween _lightTween;
    private TentacleGate _tentacleGate;

    private bool _feedbackActive;

    private Sequence _resetAwayFeedbackTween, _resetCloseFeedbackTween, _activateFeedbackTween;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _tentacleGate = transform.parent.GetComponent<TentacleGate>();
    }

    private void Init()
    {
        //setup circle alhpha
        circleSprite.transform.rotation = quaternion.identity;
        Color tmp = circleSprite.color;
        tmp.a = defaultFeedbackAlpha;
        circleSprite.color = tmp;
        
        //setup ligh and default sprite light
        _defaultBulbScale = bulb.localScale;
        bulbLight.intensity = defaultLightIntensity;

        //Anim when player activates the gate
        _activateFeedbackTween = DOTween.Sequence();
        _activateFeedbackTween.Append(
            bulb.DOPunchScale(Vector3.one *.17f, activateAnimDuration, 5, 0));
        _activateFeedbackTween.Join(DOTween.To(() => bulbLight.intensity, x => bulbLight.intensity = x,
            0,
            activateAnimDuration +0.2f));
        _activateFeedbackTween.Join(DOTween.To(() => circleSprite.color.a, x =>
            {
                Color tmp = circleSprite.color;
                tmp.a = x;
                circleSprite.color = tmp;
            },
            defaultFeedbackAlpha, activateAnimDuration + 0.2f));
        _activateFeedbackTween.Join(bulb.DOScale(_defaultBulbScale / 2, .5f).SetDelay(activateAnimDuration/2));
        _activateFeedbackTween.SetAutoKill(false).SetEase(Ease.InOutSine).Pause();
        
        //reset when player is away
        _resetAwayFeedbackTween = DOTween.Sequence();
        _resetAwayFeedbackTween.Append(DOTween.To(() => bulbLight.intensity, x => bulbLight.intensity = x,
            defaultLightIntensity,
            resetAnimDuration));
        _resetAwayFeedbackTween.Join(DOTween.To(() => circleSprite.color.a, x =>
            {
                Color tmp = circleSprite.color;
                tmp.a = x;
                circleSprite.color = tmp;
            },
            defaultFeedbackAlpha, resetAnimDuration));
        _resetAwayFeedbackTween.Join(bulb.DOScale(_defaultBulbScale, .5f));
        _resetAwayFeedbackTween.SetAutoKill(false).SetEase(Ease.InOutSine).Pause();
        
        //Reset tween when player is close
        _resetCloseFeedbackTween = DOTween.Sequence();
        _resetCloseFeedbackTween.Append(DOTween.To(() => bulbLight.intensity, x => bulbLight.intensity = x,
            activeLightIntensity,
            resetAnimDuration));
        _resetCloseFeedbackTween.Join(DOTween.To(() => circleSprite.color.a, x =>
            {
                Color tmp = circleSprite.color;
                tmp.a = x;
                circleSprite.color = tmp;
            },
            defaultFeedbackAlpha, resetAnimDuration));
        _resetCloseFeedbackTween.Join(bulb.DOScale(_defaultBulbScale, .5f));
        _resetCloseFeedbackTween.SetAutoKill(false).SetEase(Ease.InOutSine).Pause();
        
        
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
        _activateFeedbackTween.Restart();
        vfxDust.Stop();
        foreach (ParticleSystem particleSystem in bubbles)
        {
            particleSystem.Play();
        }
        

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
        if (playerInRange)
        {
            _resetCloseFeedbackTween.Restart();
        }
        else
            _resetAwayFeedbackTween.Restart();
        
        vfxDust.Play();
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerInRange = true;
            if (_tentacleGate.canOpenGate)
            {
                StartCoroutine(TogglePlayerInRangeFeedback());
                if (_tentacleGate.showUIHelp)
                {
                    UIManager.Instance?.TogglePlayerUIPrompt(true);
                }
            }

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
            _feedbackActive = false;

            if (_tentacleGate.canOpenGate)
            {
                DOTween.To(() => bulbLight.intensity, x => bulbLight.intensity = x,
                  defaultLightIntensity,
                    activateAnimDuration/2);
                
                if (_tentacleGate.showUIHelp)
                {
                    UIManager.Instance?.TogglePlayerUIPrompt(false);
                }
                
            }

        }
    }

    //When player in range we stop regular feedback and just put bulb and ux lit
    IEnumerator TogglePlayerInRangeFeedback()
    {
        yield return new WaitForSeconds(0.1f);
        if (playerInRange && _tentacleGate.canOpenGate)
        {
            DOTween.To(() => bulbLight.intensity, x => bulbLight.intensity = x,
                activeLightIntensity,
                activateAnimDuration/2);

            _feedbackActive = true;

        }
    }
}
