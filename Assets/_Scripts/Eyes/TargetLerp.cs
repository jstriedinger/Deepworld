using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class TargetLerp : MonoBehaviour
{

    [SerializeField] EnemyMonster _enemyMonster;
    public Transform idealTarget;
    public float smoothTime;
    private Vector3 velocity = Vector3.zero;

    //TargetLerp is supposed to be attached to the arms' target objects. It allows us to smooth out transitions between different splines and animations by following an ideal target.

    void Update()
    {
        if(_enemyMonster && (_enemyMonster.CurrentState == MonsterState.Frustrated || _enemyMonster.CurrentState == MonsterState.Follow )  )
        {
            transform.position = idealTarget.position;
            //transform.position = Vector3.SmoothDamp(transform.position, idealTarget.position, ref velocity, smoothTime);
        }

    }

}

