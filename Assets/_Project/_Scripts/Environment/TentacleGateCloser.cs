using System;
using UnityEngine;

public class TentacleGateCloser : MonoBehaviour
{
    [SerializeField] private TentacleGate tentacleGate;

    private bool _triggered;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _triggered = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !_triggered)
        {
            _triggered = true;
            StartCoroutine(tentacleGate.Close());
        }
    }
}
