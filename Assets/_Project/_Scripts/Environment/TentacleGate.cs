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
    [FormerlySerializedAs("tentacleGateSwitcher")] [SerializeField] private CoralGateSwitcher coralGateSwitcher;
    [FormerlySerializedAs("gateCorals")] [SerializeField] private CoralsController coralsController;
    
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

    //Open the coral gate
    public void Open()
    {
        if (!_isOpen && canOpenGate && coralGateSwitcher.playerInRange)
        {
            
            _isOpen = true;
            _nextCloseTime = Time.time + config.GateOpenTime;
            canOpenGate = false;
            Sequence seq = DOTween.Sequence();
            seq.SetEase(Ease.InOutSine);
            seq.PrependInterval(0.25f);
            //every two tentacles
            seq.AppendCallback(() =>
            {
                coralGateSwitcher.DisableSwitcher();
                RuntimeManager.PlayOneShot(config.SfxSwitch);
            });
            seq.AppendInterval(1.75f);
            seq.AppendCallback(
                () =>
                {
                    coralsController.ToggleCorals(true);
                    RuntimeManager.PlayOneShot(config.SfxOpen);

                });
            seq.Append(DOTween.To(() => gateCollider.offset.y, x => gateCollider.offset = new Vector2(1, x), -4,
                1));

        }
       
    }

    //Closes the coral gate
    public IEnumerator Close()
    {
        if (_isOpen)
        {
            _isOpen = false;
            coralsController.ToggleCorals(false);

            Sequence seq = DOTween.Sequence();
            seq.Append(DOTween.To(() => gateLight.intensity, x => gateLight.intensity = x, 0,
                1.5f));
            seq.Join(DOTween.To(() => gateCollider.offset.y, x => gateCollider.offset = new Vector2(1, x), 1,
                1));
            
            yield return new WaitForSeconds(config.SwitchResetTime);
            canOpenGate = true;
            coralGateSwitcher.EnableSwitcher();


        }
    }

    //Collider events to know if coral gate can be closed or not
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
