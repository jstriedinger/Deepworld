using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

public class TentacleGate : MonoBehaviour
{
    [SerializeField] private GameObject tentaclesObj;
    [SerializeField] private Collider2D door1;
    [SerializeField] private Collider2D door2;
    private TentacleToggler[] _tentacleTogglers;
    [SerializeField] private TentacleGateSwitcher tentacleGateSwitcher;
    [SerializeField] private float openedTime;
    [SerializeField] private float switchTime;
    
    private float _nextCloseTime;
    private bool _canCloseGate;
    private bool _canOpenGate;

    private bool _isOpen;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _nextCloseTime = 0;
        _canCloseGate = true;
        _canOpenGate = true;

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

    public void Open()
    {
        if (!_isOpen && _canOpenGate )
        {
            _isOpen = true;
            _nextCloseTime = Time.time + openedTime;
            Sequence seq = DOTween.Sequence();
            seq.SetEase(Ease.InOutSine);
            //every two tentacles
            seq.AppendCallback(() =>
            {
                tentacleGateSwitcher.DisableSwitcher();
            });
            seq.AppendInterval(.75f);
            seq.AppendCallback(
                () =>
                {
                    foreach (TentacleToggler toggler in _tentacleTogglers)
                    {
                        toggler.Open();
                    }

                });
            seq.Append(DOTween.To(() => door1.offset.y, x => door1.offset = new Vector2(1, x), -2,
                1));
            seq.Join(DOTween.To(() => door2.offset.y, x => door2.offset = new Vector2(1, x), -2,
                1));

        }
       
    }

    public IEnumerator Close()
    {
        if (_isOpen)
        {
            _canOpenGate = false;
            _isOpen = false;
            Debug.Log("Closing gate");

            foreach (TentacleToggler toggler in _tentacleTogglers)
            {
                toggler.Close();
            }

            Sequence seq = DOTween.Sequence();
            seq.Append(DOTween.To(() => door1.offset.y, x => door1.offset = new Vector2(1, x), 1,
                1));
            seq.Join(DOTween.To(() => door2.offset.y, x => door2.offset = new Vector2(1, x), 1,
                1));
            
            yield return new WaitForSeconds(switchTime);
            _canOpenGate = true;
            tentacleGateSwitcher.EnableSwitcher();


        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _canCloseGate = false;
            Debug.Log("Cnat close gate");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _canCloseGate = true;
            Debug.Log("Can close gate");
        }
    }

    //Feedback when player is close and can activate the Gate
    public void ClosePlayerFeedback(bool toggle)
    {
        if(toggle && _canOpenGate)
            tentacleGateSwitcher.PlayerCloseFeedback(true);
        else
        {
            tentacleGateSwitcher.PlayerCloseFeedback(false);
        }
        
    }
}
