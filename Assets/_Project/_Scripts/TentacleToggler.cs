using System;
using UnityEngine;

public class TentacleToggler : MonoBehaviour
{
    private Tentacle _tentacle;

    private float _targetDistDefault;

    private float _initialColliderHeight;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
    }

    private void Awake()
    {
        _tentacle = GetComponent<Tentacle>();
        _targetDistDefault = _tentacle.targetDist;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void Close()
    {
        _tentacle.targetDist = _targetDistDefault;
    }

    public void Open()
    {
        _tentacle.targetDist = 0.1f;
    }
}
