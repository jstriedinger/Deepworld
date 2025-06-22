using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Pathfinding;

public class MonsterCinematic : MonsterBase
{
    //tutorialmonster
    private EyeTracker _eyeTracker;
    //private AIDestinationSetter _aiDestinationSetter;

    [Header("Extra")]
    [SerializeField] private GameObject target;
    [SerializeField] private float speed;
    
    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        _eyeTracker = GetComponentInChildren<EyeTracker>();
        _aiPath.canMove = false;
        //_aiDestinationSetter = GetComponent<AIDestinationSetter>();

        
        _behaviorTree.SetVariableValue("speed",speed);
        if(target != null)
            _behaviorTree.SetVariableValue("target",target);
    }

    protected override void Start()
    {
        base.Start();
    }


    public void ToggleTrackTarget(bool track, GameObject obj = null)
    {
        _eyeTracker.ToggleTrackTarget(track, obj ? obj :target);
    }

    /**
     * Toggle manual pursuit.
     * Type used to know if to change right away to pursue or follow
     */
    public void TogglePursuit(bool pursue, bool type = false, bool withSound = false)
    {
        if (pursue)
        {
            Debug.Log("Toggling persuit");
            _aiPath.canMove = true;
            _behaviorTree.EnableBehavior();
            _behaviorTree.Start();
            if(!type)
                UpdateMonsterState(MonsterState.Follow, withSound);
            else
                UpdateMonsterState(MonsterState.Chasing, withSound);
            
            
                //UpdateMonsterState(MonsterState.Chasing, withSound);
            //_aiPath.maxSpeed = monsterStats.ChasingSpeed;
        }
        else
        {
            UpdateMonsterState(MonsterState.Default);
            _aiPath.canMove = false;
            _behaviorTree.DisableBehavior();
        }
    }
    
    
}
