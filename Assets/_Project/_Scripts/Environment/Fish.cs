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
    [SerializeField] private Material[] fishMats;

    [SerializeField]
    private bool randomSizes = true;


    private void Awake()
    {
        evadingPlayer = false;
        if (_spriteRenderer)
        {
            //randome sizing ourselves
            if (randomSizes)
            {
                float r1 = GetRandomStepFloat(1, 1.5f, 0.2f);
                float r2 = GetRandomStepFloat(1, 1.2f, 0.1f);
                transform.localScale = new Vector3(r1,r2,1);
            }
            _spriteRenderer.sharedMaterial = fishMats[Random.Range(0, fishMats.Length)];
            _behaviorTree = GetComponent<BehaviorTree>();
            
        }
        
    }
    private float GetRandomStepFloat(float min, float max, float step)
    {
        int steps = Mathf.RoundToInt((max - min) / step); // Total possible steps
        int randomStep = Random.Range(0, steps + 1); // Random step index
        return min + (randomStep * step); // Convert step index to float
    }

    public void Initialize(BehaviorTree parentFlockTree, Flock parentFlockTask, bool reactToPlayer, GameObject playerRef)
    {
        _parentFlockTree = parentFlockTree;
        _flockParentTask = parentFlockTask;
        if (reactToPlayer)
        {
            _behaviorTree.SetVariable("playerRef",(SharedGameObject)playerRef);
            _behaviorTree.EnableBehavior();
        }
        
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
            a.Add(gameObject);
        else
            a.Remove(gameObject);

        _flockParentTask.agents = a;
        _flockParentTask.OnStart();
    }
        
}
