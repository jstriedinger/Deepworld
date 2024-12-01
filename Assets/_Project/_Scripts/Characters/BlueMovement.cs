using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class BlueMovement : MonoBehaviour
{
    public bool canMove;
    public Transform target;
    [SerializeField] private  float minDistance = 121;
    private float _minDistancePow;
    [SerializeField] private  float moveSpeed = 500;
    [SerializeField] private  float maxSpeed = 650;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float nextWaypointDistance = 3;
    [SerializeField] private float repathRate = 0.5f;

    [Header("Swim")] 
    [SerializeField] private float distanceToSwim;
    [SerializeField] private float swimForce;
    [SerializeField] private float timeBetweenSwim;
    private float _distanceToSwimPow;
    private float _nextSwim = 0;
    
    
    private BlueNPC _blueNpc;

    [HideInInspector]
    private Path _path;
    private bool _reachedEndOfPath;

    private float _lastRepath = float.NegativeInfinity;
    private int _currentWaypoint = 0;
    private Seeker _seeker;
    private Vector3 _finalMovement;
    private Rigidbody2D _rigidBody;
    private Vector3 movDirection;

    private void Awake()
    {
        
        _rigidBody = GetComponent<Rigidbody2D>();
        _blueNpc = GetComponent<BlueNPC>();
        movDirection = Vector3.zero;
        
        //we do this bc we are comparing for sqrmagnitude to save a little performance
        _minDistancePow = minDistance * minDistance;
        _distanceToSwimPow = distanceToSwim * distanceToSwim;
    }

    public void Start () {
        // Get a reference to the Seeker component we added earlier
        _seeker = GetComponent<Seeker>();

        // Start to calculate a new path to the targetPosition object, return the result to the OnPathComplete method.
        // Path requests are asynchronous, so when the OnPathComplete method is called depends on how long it
        // takes to calculate the path. Usually it is called the next frame.
        _seeker.StartPath(transform.position, target.position, OnPathComplete);
        
    }

    public void OnPathComplete (Path p) {
        
        p.Claim(this);
        if (!p.error) {
            if (_path != null) _path.Release(this);
            _path = p;
            // Reset the waypoint counter so that we start to move towards the first point in the path
            _currentWaypoint = 0;
        } else {
            Debug.Log("AI Error: " + p.errorLog);
            p.Release(this);
        }
    }

    private void Update()
    {
        if (!canMove)
            return;
        
        if (Time.time > _lastRepath + repathRate && _seeker.IsDone()) {
            _lastRepath = Time.time;

            // Start a new path to the targetPosition, call the the OnPathComplete function
            // when the path has been calculated (which may take a few frames depending on the complexity)
            _seeker.StartPath(transform.position, target.position, OnPathComplete);
        }
        if (_path == null) {
            // We have no path to follow yet, so don't do anything
            return;
        }
        
        _reachedEndOfPath = false;
        
        // The distance to the next waypoint in the path
        float distanceToWaypoint;
        while (true) {
            distanceToWaypoint = Vector3 .Distance(transform.position, _path.vectorPath[_currentWaypoint]);
            if (distanceToWaypoint < nextWaypointDistance) {
                // Check if there is another waypoint or if we have reached the end of the path
                if (_currentWaypoint + 1 < _path.vectorPath.Count) {
                    _currentWaypoint++;
                } else {
                    _reachedEndOfPath = true;
                    break;
                }
            } else {
                break;
            }
        }
        if ((target.position - transform.position).sqrMagnitude > _minDistancePow)
        {
            if (_currentWaypoint+1 < _path.vectorPath.Count)
            {
                // Direction to the next waypoint
                movDirection = (_path.vectorPath[_currentWaypoint+1] - transform.position).normalized;
                
            }
            
            //if far away then swim
            if ((target.position - transform.position).sqrMagnitude >= _distanceToSwimPow && Time.time >= _nextSwim)
            {
                _rigidBody.AddForce((movDirection * swimForce ), ForceMode2D.Impulse);
                _blueNpc.SwimEffect();
                _nextSwim = Time.time + timeBetweenSwim;
            }
            else
            {
                // Slow down smoothly upon approaching the end of the path
                // This value will smoothly go from 1 to 0 as the agent approaches the last waypoint in the path.
                var speedFactor = _reachedEndOfPath ? Mathf.Sqrt(distanceToWaypoint/nextWaypointDistance) : 1f;
                _finalMovement = ( moveSpeed * speedFactor * transform.up * Time.deltaTime);
                _rigidBody.AddForce(_finalMovement);
                
            }
            
        }
        
        //Afte reverything
        
        //always rotate to look at direction
        Vector3 rotatedTemp = Quaternion.Euler(0, 0, 0) * movDirection;
        Quaternion tempRotation = Quaternion.LookRotation(Vector3.forward, rotatedTemp);
        float angles = Quaternion.Angle(transform.rotation, tempRotation);
            
        //Reduce head wobble a little
        if (angles > 15)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, tempRotation, rotationSpeed * Time.deltaTime);
        }
        //always at least X units away to even move
        _rigidBody.linearVelocity = Vector3.ClampMagnitude(_rigidBody.linearVelocity, maxSpeed);
        _finalMovement = Vector3.zero;
    }

    public void MakeMovementFaster()
    {
        movDirection = Vector3.zero;
        nextWaypointDistance = 2;
        minDistance -= 7;
        distanceToSwim -= 7;
        moveSpeed += 250;
        maxSpeed += 550;
        swimForce += 4f;
        _minDistancePow = minDistance * minDistance;
        _distanceToSwimPow = distanceToSwim * distanceToSwim;
    }
    
}
