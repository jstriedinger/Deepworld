using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class TentacleGate : MonoBehaviour
{
    [SerializeField] private TentacleToggler[] tentacles;
    [SerializeField] private TentacleDoorSwitcher tentacleDoorSwitcher;
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
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Next close: "+_nextCloseTime+" and time is: "+Time.time);
        if (_isOpen && Time.time >= _nextCloseTime && _canCloseGate)
        {
            Debug.Log("Trying to close");
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
                tentacleDoorSwitcher.Open();
            });
            seq.AppendInterval(.5f);
            seq.AppendCallback(
                () =>
                {
                    tentacles[0].Open();
                    tentacles[1].Open();

                });
            seq.AppendInterval(0.5f);
            seq.AppendCallback(
                () =>
                {
                    tentacles[2].Open();
                    tentacles[3].Open();

                });
            seq.AppendInterval(0.5f);
            seq.AppendCallback(
                () =>
                {
                    tentacles[4].Open();
                    tentacles[5].Open();

                });
            
        }
       
    }

    IEnumerator Close()
    {
        if (_isOpen)
        {
            _canOpenGate = false;
            _isOpen = false;
            Debug.Log("Closing gate");
            
            tentacles[0].Close();
            tentacles[1].Close();
            tentacles[2].Close();
            tentacles[3].Close();
            tentacles[4].Close();
            tentacles[5].Close();

            yield return new WaitForSeconds(switchTime);
            _canOpenGate = true;
            tentacleDoorSwitcher.Close();


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
}
