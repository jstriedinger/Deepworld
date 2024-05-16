using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks.Movement.AstarPathfindingProject;
using UnityEngine;
using Random = UnityEngine.Random;

public class Fish : MonoBehaviour
{
    private BehaviorTree _parentFlockTree;
    private BehaviorTree _behaviorTree;
    private bool evadingPlayer;
    private Flock _flockParentTask;
    [SerializeField] private SpriteRenderer _spriteRenderer;


    private void Awake()
    {
        evadingPlayer = false;
        //randome sizing ourselves
        float r1 = Random.Range(0.75f, 1.51f);
        float r2 = Random.Range(0.75f, 1.51f);
        transform.localScale = new Vector3(r1,r2,1);
        _behaviorTree = GetComponent<BehaviorTree>();
    }

    public void Initialize(BehaviorTree parentFlockTree, Flock parentFlockTask,GameObject playerRef)
    {
        _parentFlockTree = parentFlockTree;
        _flockParentTask = parentFlockTask;
        _behaviorTree.SetVariable("playerRef",(SharedGameObject)playerRef);
        _behaviorTree.EnableBehavior();
        
    }

    public void OnDetectPlayer()
    {
        if (!evadingPlayer)
        {
            evadingPlayer = true;
            ToggleFishOnFlock(false);
        }
    }

    public void OnAwayFromPlayer()
    {
        if (evadingPlayer)
        {
            evadingPlayer = false;
            ToggleFishOnFlock(true);
        }
    }

   

    private void ToggleFishOnFlock(bool addToFlock)
    {
        List<GameObject> a = _flockParentTask.agents.Value;

        if (addToFlock)
        {
            a.Add(gameObject);
            Debug.Log("Add fish to flock");
        }
        else
        {
            a.Remove(gameObject);
            Debug.Log("Remove fish of flock");
        }

        _flockParentTask.agents = a;
        _flockParentTask.OnStart();
    }
        
}
