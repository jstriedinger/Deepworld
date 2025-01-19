using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TentacleDoorSwitcher : MonoBehaviour
{
    [SerializeField] private Light2D tentacleLight;
    [SerializeField] private float tentacleLightIntensity;

    private float _defaultLightIntensity;

    private float _defaultPosY;

    private Sequence _lightSeq;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tentacleLight.intensity = 0.5f;
        _defaultPosY = transform.position.y;
        
        _lightSeq = DOTween.Sequence();
        _lightSeq.SetEase(Ease.InOutSine);
        _lightSeq.Append(DOTween.To(() => tentacleLight.intensity, x => tentacleLight.intensity = x, tentacleLightIntensity,
            1f));
        _lightSeq.SetAutoKill(false).SetLoops(-1,LoopType.Yoyo).Pause();

        _lightSeq.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
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
        _lightSeq = DOTween.Sequence();
        _lightSeq.Append(DOTween.To(() => tentacleLight.intensity, x => tentacleLight.intensity = x, tentacleLightIntensity,
            1f));
        _lightSeq.SetAutoKill(false).SetLoops(-1,LoopType.Yoyo).Pause();

        _lightSeq.Play();
    }
}
