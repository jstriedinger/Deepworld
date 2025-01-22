using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TentacleGateSwitcher : MonoBehaviour
{
    [SerializeField] private Light2D tentacleLight;
    [SerializeField] private float tentacleLightIntensity;

    private float _defaultLightIntensity;
    private float _defaultPosY;
    private Sequence _lightSeq;

    private TentacleGate _tentacleGate;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _tentacleGate = transform.parent.GetComponent<TentacleGate>();
        tentacleLight.intensity = 1f;
        _defaultPosY = transform.position.y;
        
    }

    public void Open()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOMoveY(_defaultPosY +3, 1));
        _lightSeq.Kill();
        DOTween.To(() => tentacleLight.intensity, x => tentacleLight.intensity = x, 1,
            .5f);
    }

    public void Close()
    {
        transform.DOMoveY(_defaultPosY, 1);
    }

    public void ToggleLightAnim(bool toggle)
    {
        if (toggle)
        {
            _lightSeq.Kill();
            _lightSeq = DOTween.Sequence();
            _lightSeq.Append(DOTween.To(() => tentacleLight.intensity, x => tentacleLight.intensity = x, tentacleLightIntensity,
                1f));
            _lightSeq.SetAutoKill(false).SetLoops(-1,LoopType.Yoyo).Pause();

            _lightSeq.Play();
        }
        else
        {
            _lightSeq.Kill();
            DOTween.To(() => tentacleLight.intensity, x => tentacleLight.intensity = x, 1,
                .5f);
        }
    }
}
