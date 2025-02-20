using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks.Movement.AstarPathfindingProject;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class FishFlock : MonoBehaviour
{
    [SerializeField] private GameObject playerRef;
    [SerializeField] private float numFishes;
    [SerializeField] private GameObject fishPrefab;
    [SerializeField] private bool reactToPlayer = true;
    [SerializeField] private float speed = 3;
    [Tooltip("The greater the alignmentWeight is the more likely it is that the agents will be facing the same direction")]
    [SerializeField] private float alignment = .25f;
    [Tooltip("The greater the cohesionWeight is the more likely it is that the agents will be moving towards a common position")]
    [SerializeField] private float cohesion = .6f;
    [Tooltip("The greater the separationWeight is the more likely it is that the agents will be separated")]
    [SerializeField] private float separation = .3f;
    [SerializeField] private float initialFlockRadius = 3;
    private List<GameObject> _fishes = new List<GameObject>();
    private BehaviorTree _behaviorTree;
    private Flock _flockTask;
    private void Awake()
    {
        _behaviorTree = GetComponent<BehaviorTree>();
        _flockTask = _behaviorTree.FindTask<Flock>();
        InitializeFlock();
        
        
    }

    public void InitializeFlock()
    {
        for (int i = 0; i < numFishes; i++)
        {
            //create a fish
            Vector3 randomPos = Random.insideUnitCircle * initialFlockRadius;
            GameObject fishObj = Instantiate(fishPrefab,transform.position,quaternion.identity,transform);
            fishObj.transform.localPosition = randomPos;
            Fish fish = fishObj.GetComponent<Fish>();
            fish.Initialize(_behaviorTree,_flockTask, reactToPlayer, playerRef);
            _fishes.Add(fishObj);
        }
        //setup the fish var of the tree
        _behaviorTree.SetVariable("Fishes",(SharedGameObjectList)_fishes);
        _behaviorTree.SetVariableValue("FlockSpeed",speed);
        _behaviorTree.SetVariableValue("Cohesion",cohesion);
        _behaviorTree.SetVariableValue("Separation",separation);
        _behaviorTree.SetVariableValue("Alignment",alignment);
        
        //lets run it again
        _behaviorTree.EnableBehavior();
        _flockTask.OnStart();
        
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
