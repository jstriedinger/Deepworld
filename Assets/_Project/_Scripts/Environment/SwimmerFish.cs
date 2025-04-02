using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using UnityEngine;

public class SwimmerFish : MonoBehaviour
{
    [SerializeField] private Transform patrolObject;
    private BehaviorTree _behaviorTree;

    private void Awake()
    {
        _behaviorTree = GetComponent<BehaviorTree>();
    }

    // Start is called before the first frame update
    void Start()
    {
        List<GameObject> patrolPoints = new List<GameObject>();
        foreach (Transform child in patrolObject)
        {
            patrolPoints.Add(child.gameObject);
        }
        _behaviorTree.SetVariableValue("PatrolInfo",patrolPoints);
    }

   
}
