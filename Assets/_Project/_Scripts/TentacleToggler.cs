using System;
using UnityEngine;

public class TentacleToggler : MonoBehaviour
{
    private Tentacle _tentacle;
    private MeshCollider _meshCollider;
    private BoxCollider2D _boxCollider2D;

    private float _targetDistDefault;

    private float _initialColliderHeight;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
    }

    private void Awake()
    {
        _tentacle = GetComponent<Tentacle>();
        _meshCollider = GetComponent<MeshCollider>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _targetDistDefault = _tentacle.targetDist;
        _initialColliderHeight = _boxCollider2D.size.y;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void Close()
    {
        _tentacle.targetDist = _targetDistDefault;
        _boxCollider2D.size = new Vector2(_boxCollider2D.size.x, _initialColliderHeight);
    }

    public void Open()
    {
        _tentacle.targetDist = 0.1f;
        _boxCollider2D.size = new Vector2(_boxCollider2D.size.x, 0);
    }
}
