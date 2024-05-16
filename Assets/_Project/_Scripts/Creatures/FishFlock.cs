using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks.Movement.AstarPathfindingProject;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class FishFlock : MonoBehaviour
{
    [SerializeField] private GameObject _playerRef;
    [SerializeField] private float numFishes;
    [SerializeField] private GameObject fishPrefab;
    private List<GameObject> _fishes = new List<GameObject>();
    private BehaviorTree _behaviorTree;
    private Flock _flockTask;
    private void Awake()
    {
        _behaviorTree = GetComponent<BehaviorTree>();
        _flockTask = _behaviorTree.FindTask<Flock>();
        
        
    }

    public void InitializeFlock()
    {
        for (int i = 0; i < numFishes; i++)
        {
            //create a fish
            Vector3 randomPos = Random.insideUnitCircle * 3;
            GameObject fishObj = Instantiate(fishPrefab,transform.position,quaternion.identity,transform);
            fishObj.transform.localPosition = randomPos;
            Fish fish = fishObj.GetComponent<Fish>();
            fish.Initialize(_behaviorTree,_flockTask, _playerRef);
            _fishes.Add(fishObj);
        }
        //setup the fish var of the tree
        _behaviorTree.SetVariable("Fishes",(SharedGameObjectList)_fishes);
        
        //lets run it again
        _behaviorTree.EnableBehavior();
        _flockTask.OnStart();
        
    }
    // Start is called before the first frame update
    void Start()
    {
        InitializeFlock();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
