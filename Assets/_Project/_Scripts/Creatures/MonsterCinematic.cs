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


    public void ToggleTrackTarget(GameObject obj = null)
    {
        _eyeTracker.ToggleTrackTarget(obj ? obj :target);
    }

    /**
     * Toggle manual pursuit
     */
    public void TogglePursuit(bool pursue, float timeOffset = 0, bool withSound = false)
    {
        Sequence seq = DOTween.Sequence();
        if (pursue)
        {
            seq.AppendCallback(() =>
            {
                UpdateMonsterState(MonsterState.Follow);
                _aiPath.canMove = true;
                _behaviorTree.EnableBehavior();
                _behaviorTree.Start();
            });
            seq.AppendInterval(timeOffset);
            seq.AppendCallback(() =>
            {
                UpdateMonsterState(MonsterState.Chasing, withSound);
            });
            //_aiPath.maxSpeed = monsterStats.ChasingSpeed;
        }
        else
        {
            UpdateMonsterState(MonsterState.Default);
            _aiPath.canMove = false;
            _behaviorTree.DisableBehavior();
        }
    }

    public void GoToPosition(Vector3 d)
    {
        _behaviorTree.DisableBehavior();
        _aiPath.canMove = true;
        //_aiDestinationSetter.enabled = false;
        _aiPath.destination = d;
    }
    
}
