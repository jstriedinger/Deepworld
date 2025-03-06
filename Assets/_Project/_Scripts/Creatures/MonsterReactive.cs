using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using BehaviorDesigner.Runtime;
using DG.Tweening;
using FMODUnity;
using TMPro;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using LineRenderer = UnityEngine.LineRenderer;
using Random = UnityEngine.Random;


public class MonsterReactive : MonsterBase
{
    [HideInInspector]
    public bool inCamera = false;
    
    protected override void Awake()
    {
        base.Awake();
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        if (canReactToPlayer )
        {
            _behaviorTree.SetVariableValue("PatrolSpeed",monsterStats.PatrolSpeed);
            _behaviorTree.SetVariableValue("FollowSpeed",monsterStats.FollowSpeed);
            _behaviorTree.SetVariableValue("FollowRange",monsterStats.FollowRange);
            _behaviorTree.SetVariableValue("HiddenFollowRange",monsterStats.FollowRange / 2);
            _behaviorTree.SetVariableValue("ChasingSpeed",monsterStats.ChasingSpeed);
            _behaviorTree.SetVariableValue("ChasingRange",monsterStats.ChasingRange);
            _behaviorTree.SetVariableValue("HiddenChasingRange",monsterStats.ChasingRange / 2);
            _behaviorTree.SetVariableValue("isPatrolType",true);
            
            _behaviorTree.SetVariableValue("playerRef",GameManager.Instance.playerRef.gameObject);
            _behaviorTree.SetVariableValue("playerLastPosition",GameManager.Instance.playerLastPosition);
        }
    }

    
    private void FixedUpdate()
    {
        if(canReactToCamera && !GameManager.Instance.isPlayerDead)
            UpdateEnemyInCamera();
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            UpdateMonsterState(MonsterState.Default);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            UpdateMonsterState(MonsterState.Follow);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            UpdateMonsterState(MonsterState.Chasing);
        }
    }

    /*public void UpdatePatrolInfo(Transform newPatrolInfo)
    {
        List<GameObject> patrolPoints = new List<GameObject>();
        foreach (Transform child in newPatrolInfo)
        {
            patrolPoints.Add(child.gameObject);
        }
        _behaviorTree.SetVariableValue("PatrolInfo",patrolPoints);
    }*/

    public bool CanReactToPlayer()
    {
        return canReactToPlayer;
    }
    

    //Always check if its near playerCharacter to visualize on vcam target group
    private void UpdateEnemyInCamera()
    {
        Collider2D playerCheckCamera = Physics2D.OverlapCircle(transform.position, monsterStats.DistanceToShowOnCamera, LayerMask.GetMask("Player"));

        if(playerCheckCamera)
        {
            if(!inCamera)
            {
                //add to the
                CameraManager.Instance.AddObjectToCameraView(transform, true, false);
                inCamera = true;

            }
        }
        else if(inCamera)
        {
            CameraManager.Instance.RemoveObjectFromCameraView(transform, true);
            inCamera = false;
        }


    }


    //react to playerCharacter call, go investigate a position using a lastpos object since btree needs an object
    public IEnumerator ReactToPlayerCall()
    {
        if (canReactToPlayer && CurrentState != MonsterState.Chasing && CurrentState != MonsterState.Follow)
        {
           // if (_canReactToCall || CurrentState == MonsterState.Investigate) 
            //{
                //if mosnter is not chasing or following, it can react to playerCharacter call
                yield return new WaitForSeconds(.75f);
                UpdateMonsterState(MonsterState.Investigate);
            //}
        }
    }


    //Fire when entering chase mode
    protected void EnterChaseMode()
    {
        Debug.Log("chase mode sfx");
        _monsterChaseMusicEmitter.Play();
    }

    public void OnPlayerHides()
    {
        Debug.Log("Player hiding1");
        Debug.Log("State: "+CurrentState);
        if (CurrentState == MonsterState.Chasing ||
            CurrentState == MonsterState.Follow)
        {
            Debug.Log("Player hiding2");
            //player hides when we were chasing or following. Stop that right away
            _behaviorTree.SetVariableValue("isChasing",false);
            _behaviorTree.SetVariableValue("isFollowing",false);
            _behaviorTree.SendEvent("PlayerHidesDuringChase");
            UpdateMonsterState(MonsterState.Frustrated);
        }
    }
    
    #region BehaviourTreeEvents
    public void OnAIReactToCall()
    {
        //only if we are not already chasing or following a playerCharacter
        UpdateMonsterState(MonsterState.Investigate);
    }
    //AI tree begins following playerCharacter
    public void OnAINoticePlayer()
    {
        UpdateMonsterState(MonsterState.Follow);
    }
    
    //AI tree begins chasing playerCharacter
    public void OnAIChasePlayer()
    {
        UpdateMonsterState(MonsterState.Chasing);
    }

    //AI tree goes back to patrol
    public void OnAIBackToPatrol()
    {
        _behaviorTree.SetVariableValue("isFollowing",false);
        _behaviorTree.SetVariableValue("isChasing",false);
        UpdateMonsterState(MonsterState.Default);
    }
    
    #endregion


    

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player") && canReactToPlayer)
        {
            //biting game feel
            AudioManager.Instance.StopChaseMusic();
            EatPlayerAnimation();
            //signal to the tree that we killed the playerCharacter
            //_behaviorTree.SendEvent("PlayerKilled");
        }
    }

    //show distance on camera
    private void OnDrawGizmosSelected()
    {
        if(canReactToPlayer)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, monsterStats.FollowRange);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, monsterStats.FollowRange / 2);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, monsterStats.ChasingRange);
            
        }
    }
    
    
}

