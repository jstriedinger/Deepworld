using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;
using FMODUnity;
using UnityEngine.Rendering.Universal;

public class TentacleGate : MonoBehaviour
{
    [SerializeField] private GateSO config;
    [SerializeField] private GameObject tentaclesObj;
    [SerializeField] private Collider2D gateCollider;
    [SerializeField] private Light2D gateLight;
    private TentacleToggler[] _tentacleTogglers;
    [SerializeField] private TentacleGateSwitcher tentacleGateSwitcher;
    [FormerlySerializedAs("gateCorals")] [SerializeField] private CoralsController coralsController;
    [SerializeField] private Transform coralSprite;
    
    private float _nextCloseTime;
    private bool _canCloseGate;
    public bool canOpenGate;

    private bool _isOpen;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _nextCloseTime = 0;
        _canCloseGate = true;
        canOpenGate = true;

        _tentacleTogglers = tentaclesObj.GetComponentsInChildren<TentacleToggler>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_isOpen && Time.time >= _nextCloseTime && _canCloseGate)
        {
            StartCoroutine(Close());
        }
    }

    private void OnEnable()
    {
        PlayerCharacter.OnPlayerCall += Open;
    }

    private void OnDisable()
    {
        PlayerCharacter.OnPlayerCall -= Open;
    }

    public void Open()
    {
        if (!_isOpen && canOpenGate && tentacleGateSwitcher.playerInRange)
        {
            coralSprite.DOScaleX(0, 1).SetEase(Ease.OutSine);
            coralSprite.DOScaleY(0, 0.9f).SetEase(Ease.OutSine);
            
            tentacleGateSwitcher.DisableSwitcher();
            canOpenGate = false;
            _isOpen = true;
            _nextCloseTime = Time.time + config.GateOpenTime;
            Sequence seq = DOTween.Sequence();
            seq.SetEase(Ease.InOutSine);
            //every two tentacles
            seq.AppendCallback(() =>
            {
                // var instance = RuntimeManager.CreateInstance(config.SfxActivate);
                // instance.set3DAttributes(RuntimeUtils.To3DAttributes(tentacleGateSwitcher.transform.position));
                // instance.setVolume(.9f);
                // instance.start();
                // instance.release();
            });
            seq.AppendInterval(.2f);
            seq.AppendCallback(
                () =>
                {
                    coralsController.ToggleCorals(true);
                    RuntimeManager.PlayOneShot(config.SfxActivate);

                });
            seq.Append(DOTween.To(() => gateLight.intensity, x => gateLight.intensity = x, 2,
                1.5f));
            seq.Join(DOTween.To(() => gateCollider.offset.y, x => gateCollider.offset = new Vector2(1, x), -2,
                1));

        }
       
    }

    public IEnumerator Close()
    {
        if (_isOpen)
        {
            _isOpen = false;
            Debug.Log("Closing gate");

            coralsController.ToggleCorals(false);

            Sequence seq = DOTween.Sequence();
            seq.Append(DOTween.To(() => gateLight.intensity, x => gateLight.intensity = x, 0,
                1.5f));
            seq.Join(DOTween.To(() => gateCollider.offset.y, x => gateCollider.offset = new Vector2(1, x), 1,
                1));
            
            yield return new WaitForSeconds(config.SwitchResetTime);
            canOpenGate = true;
            tentacleGateSwitcher.EnableSwitcher();


        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _canCloseGate = false;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _canCloseGate = true;
        }
    }

}
