using System;
using UnityEngine;

public class TentacleToggler : MonoBehaviour
{
    private TentacleInfo _tentacle;

    private float _targetDistDefault;

    private float _initialColliderHeight;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
    }

    private void Awake()
    {
        _tentacle = GetComponent<TentacleInfo>();
        _targetDistDefault = _tentacle.minPointGap;
    }
    

    public void Close()
    {
        _tentacle.currentPointGap = _tentacle.minPointGap;
    }

    public void Open()
    {
        _tentacle.currentPointGap = 0.05f;
    }
    
}
