using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Gilzoide.UpdateManager;

public class TargetLerp : AManagedBehaviour, IUpdatable
{

    [SerializeField] MonsterBase monsterBase;
    [SerializeField] private bool followOverride;
    //this becomes a ref to the player in runtime
    public Transform idealTarget;
    public float smoothTime;
    private Vector3 velocity = Vector3.zero;

    private void Start()
    {
        idealTarget = GameManager.Instance.playerRef.transform;
    }

    //TargetLerp is supposed to be attached to the arms' target objects. It allows us to smooth out transitions between different splines and animations by following an ideal target.
    public void ManagedUpdate()
    {
        if(monsterBase && (monsterBase.CurrentState == MonsterState.Frustrated || monsterBase.CurrentState == MonsterState.Follow )  )
        {
            transform.position = Vector3.SmoothDamp(transform.position, idealTarget.position, ref velocity, smoothTime);
        }

    }
}

