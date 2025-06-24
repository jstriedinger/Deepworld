using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gilzoide.UpdateManager;


public class MonsterReactive : MonsterBase, IUpdatable, IFixedUpdatable
{
    [HideInInspector]
    public bool inCamera = false;
    [SerializeField] private int playerCallReactRadius = 85;
    
    protected override void Awake()
    {
        base.Awake();
    }
    
    private void OnEnable()
    {
        PlayerCharacter.OnPlayerCall += OnPlayerCallReact;
        PlayerCharacter.OnPlayerHide += OnPlayerHides;
    }

    private void OnDisable()
    {
        PlayerCharacter.OnPlayerCall -= OnPlayerCallReact;
        PlayerCharacter.OnPlayerHide -= OnPlayerHides;
    }

    private void OnPlayerCallReact()
    {
        PlayerCharacter player = GameManager.Instance.playerRef;
        //player not hidding?
        if (!player.isHidden)
        {
            //able to react?
            if (canReactToPlayer && CurrentState != MonsterState.Chasing && CurrentState != MonsterState.Follow)
            {
                //is close enough?
                float sqrDistance = (player.transform.position - transform.position).sqrMagnitude;
                if (sqrDistance <= playerCallReactRadius * playerCallReactRadius)
                {
                    StartCoroutine(ReactToPlayerCall());
                }
            }
            
        }

    }
    
    public IEnumerator ReactToPlayerCall()
    {
        yield return new WaitForSeconds(1f);
        UpdateMonsterState(MonsterState.Investigate);
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
    


    //Fire when entering chase mode
    protected void EnterChaseMode()
    {
        Debug.Log("chase mode sfx");
        _monsterChaseMusicEmitter.Play();
    }

    public void OnPlayerHides()
    {
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

    public void ManagedUpdate()
    {
        //eye manager update
        MonsterEyeManager.UpdateBlink();
    }

    public void ManagedFixedUpdate()
    {
        if(canReactToCamera && !GameManager.Instance.isPlayerDead)
            UpdateEnemyInCamera();
    }
}

