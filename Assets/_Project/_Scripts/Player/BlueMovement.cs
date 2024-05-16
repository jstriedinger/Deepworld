using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using Unity.Mathematics;
using UnityEngine;
using FMODUnity;
using DG.Tweening;
using UnityEngine.Serialization;

[RequireComponent(typeof(AIDestinationSetter))]
public class BlueMovement : AIPath
{
    private float distanceToSwimPow = 0;
    private Transform _playerRef;
    private float _nextSwim = 0;
    private BlueNPC _blueNpc;
    private Vector3 dirSwim;
    
    protected override void Update()
    {
        base.Update();
    }

    protected override void Awake()
    {
        base.Awake();
        _playerRef = GetComponent<AIDestinationSetter>().target;
        _blueNpc = GetComponent<BlueNPC>();
        distanceToSwimPow = Mathf.Pow(_blueNpc._distanceToSwim,2);
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void MovementUpdateInternal(float deltaTime, out Vector3 nextPosition, out Quaternion nextRotation)
    {
        //before any internal stuff lets check if we should swim 
        base.MovementUpdateInternal(deltaTime, out nextPosition, out nextRotation);
        if ((_playerRef.position - transform.position).sqrMagnitude >= distanceToSwimPow && Time.time >= _nextSwim)
        {
            //far away, make blue swim if it can
            if (Time.time >= _nextSwim)
            {
                _nextSwim = Time.time + _blueNpc.timeBetweenSwim;
                StartCoroutine(_blueNpc.Swim());
            }
        }
        //detect how far away we are from player
        
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void OnPathComplete(Path newPath)
    {
        base.OnPathComplete(newPath);
    }

    protected override void ClearPath()
    {
        base.ClearPath();
    }
    
}
