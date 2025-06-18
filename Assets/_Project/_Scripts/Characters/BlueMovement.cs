using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class BlueMovement : MonoBehaviour
{
    public static event Action OnBlueReachedDestination;
    
    public bool canMove;
    public Transform target;
    [SerializeField] private bool fireDestinationEvent;
    [SerializeField] private bool canSwim = true;
    [SerializeField] private  float minDistance = 121;
    private float _minDistancePow, _previousMinDistance;
    [SerializeField] private  float moveSpeed = 500;
    [SerializeField] private  float maxSpeed = 650;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float nextWaypointDistance = 3;
    [SerializeField] private float repathRate = 0.5f;

    
    //if we are calculating a new path we dont want to swim, we might get issues
    private bool _repathing = false;

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
        _previousMinDistance = minDistance;
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
            _repathing = false;
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
        while (true)
        {
            distanceToWaypoint = (transform.position - _path.vectorPath[_currentWaypoint]).sqrMagnitude;
            //distanceToWaypoint = Vector3 .Distance(transform.position, _path.vectorPath[_currentWaypoint]);
            if (distanceToWaypoint < nextWaypointDistance * nextWaypointDistance) {
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

        float toTargetSqrMag = (target.position - transform.position).sqrMagnitude;
        if (!_repathing)
        {
            if (toTargetSqrMag > _minDistancePow )
            {
                //Is not close enough, should move
                if (_currentWaypoint+1 < _path.vectorPath.Count)
                {
                    // Direction to the next waypoint
                    movDirection = (_path.vectorPath[_currentWaypoint+1] - transform.position).normalized;
                }
                
                //if far away then swim
                if (toTargetSqrMag >= _distanceToSwimPow && Time.time >= _nextSwim && canSwim)
                {
                    Debug.DrawRay(transform.position,movDirection * 5, Color.blue,3);
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

                    // Vector2 dir = (target.position - transform.position).normalized;
                    // float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    // Quaternion rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
                    // transform.rotation = rotation;
                    _rigidBody.AddForce(_finalMovement);
                    
                }
                //Afte reverything
                
                //always rotate to look at direction
                /*Vector3 dir = ( (target.position - transform.position).normalized);
                Quaternion tempRotation = Quaternion.LookRotation(Vector3.forward, dir);
                float angles = Quaternion.Angle(transform.rotation, tempRotation);
                    
                //Reduce head wobble a little
                if (angles > 15)
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, tempRotation, rotationSpeed * Time.deltaTime);
                    //transform.rotation = Quaternion.RotateTowards(transform.rotation, tempRotation, rotationSpeed * Time.deltaTime);
                }*/
                Vector3 dir = ( (target.position - transform.position).normalized);
                Vector3 newUp = Vector3.Slerp(transform.up, dir, rotationSpeed * Time.deltaTime);
                transform.up = newUp;
                
                _rigidBody.linearVelocity = Vector3.ClampMagnitude(_rigidBody.linearVelocity, maxSpeed);
            }
            else
            {
                //close enough, fire reached event
                if (fireDestinationEvent)
                {
                    OnBlueReachedDestination?.Invoke();
                    fireDestinationEvent = false;

                }
            }
            
        }
        
        //always at least X units away to even move
        //_finalMovement = Vector3.zero;
    }
    
    //Update Blue stats

    public void ChangeBlueStats(Transform newTarget)
    {
        target = newTarget;
        movDirection = Vector3.zero;
        //nextWaypointDistance = 1;
        minDistance -= 6;
        distanceToSwim -= 5;
        moveSpeed += 200;
        maxSpeed += 750;
        swimForce += 3.5f;
        _minDistancePow = minDistance * minDistance;
        _distanceToSwimPow = distanceToSwim * distanceToSwim;
    }

    public void ChangeFollowTarget(Transform newTarget, float newMinDistance = -1, bool newCanSwim = true)
    {
        canMove = true;
        canSwim = newCanSwim;
        target = newTarget;
        _repathing = true;
        _seeker.StartPath(transform.position, target.position, OnPathComplete);

        if (newMinDistance >= 0)
        {
            _previousMinDistance = minDistance;
            minDistance = newMinDistance;
        }
        else
            minDistance = _previousMinDistance;
        _minDistancePow = minDistance * minDistance;
        
    }

    public void ToggleFireReachedDestinationEvent(bool toggle)
    {
        fireDestinationEvent = toggle;
    }
}
