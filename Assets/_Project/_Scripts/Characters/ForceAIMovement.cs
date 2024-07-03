using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class ForceAIMovement : MonoBehaviour
{
    public Transform targetPosition;

    private Seeker _seeker;
    public void Start () {
        // Get a reference to the Seeker component we added earlier
        _seeker = GetComponent<Seeker>();

        // Start to calculate a new path to the targetPosition object, return the result to the OnPathComplete method.
        // Path requests are asynchronous, so when the OnPathComplete method is called depends on how long it
        // takes to calculate the path. Usually it is called the next frame.
        
    }

    public void OnPathComplete (Path p) {
        Debug.Log("Yay, we got a path back. Did it have an error? " + p.errorLog);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            _seeker.StartPath(transform.position, targetPosition.position, OnPathComplete);
        }
    }
}
