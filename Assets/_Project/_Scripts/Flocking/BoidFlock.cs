using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using UnityEngine.Serialization;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;


public class BoidFlock : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private FlockBehaviorSO flockBehavior;
    [SerializeField] private FlockAgent agentPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Collider2D _collider2D;
    private Transform _player;
    List<FlockAgent> _agents = new List<FlockAgent>();
    private Transform[] _agentTransforms;
    
    [Header("Properties")]
    [SerializeField] private bool isActive = true;
    [SerializeField] private bool isContained = true;
    [Range(2, 250)]
    public int numAgents = 250;
    [Range(1f, 100f)]
    public float acceleration = 10f;
    [Range(0f, 3f)]
    public float avoidanceRadiusMultiplier = 0.5f;
    
    [Header("Follow behavior")]
    [SerializeField] private bool isFollowing;
    [SerializeField] private Transform followObj;
    [SerializeField, Range(.1f, 3f)] private float followWeight = 1;
    [SerializeField, Tooltip("Will follow within distance, 0 if always follow regardless")]
    private float followDistance = 0;
    [SerializeField]
    private float followKeepDistance = 0;
    
    [Header("Patrol behavior")]
    [SerializeField] bool canAvoidPlayer;
    [SerializeField] bool canAvoidBlue;
    [SerializeField] private bool isPatroling;
    [SerializeField] private GameObject patrolObj;
    [SerializeField, Range(.1f, 3f)] private float patrolWeight = 1; 
    private Transform[] _patrolPoints;
    private int _currentPatrolIndex = 0;
    private Vector2 _currentFlockAvgPos;
    
   
    [Header("Containment")]
    [SerializeField] bool isContainedInCircle;
    [SerializeField] float containCircleRadius;
    [SerializeField] private Transform containMiddlePoint;
    [SerializeField, Range(1,4)] private float containRandomnessWeight;

    float _squareMaxSpeed, _squareNeighborRadius, _squareAvoidanceRadius, _squareMaxSpeedAvoidingPlayer;
    
    //every frame this variables are updated
    private Transform[] _agentNeighbors = new Transform[10];
    private Collider2D[] _agentObstacles = new Collider2D[10];
    private int _agentNumNeighbors, _agentNumObstacles = 0;
    private ContactFilter2D _contactFilter2DAgents, _contactFilter2DObstacles;
    
    //Keep tracking
    private Dictionary<Collider2D, FlockAgent> _agentColliderMap = new();
    private Vector2 _alignmentDir, _cohesionDir, _separationDir;
    private Transform _agentTransform;
    private Vector3 _currentAgentPos;
    private Vector2 _currentAgentMove;
    private float _currentSqrdDistanceFromPlayer;
    
    

    // Start is called before the first frame update
    void Start()
    {
        //cache the transforms
        _agentTransforms = _agents.Select(a => a.transform).ToArray();
        
        _squareNeighborRadius = flockBehavior.NeighborRadius * flockBehavior.NeighborRadius;
        _squareAvoidanceRadius = _squareNeighborRadius * avoidanceRadiusMultiplier * avoidanceRadiusMultiplier;

        for (int i = 0; i < numAgents; i++)
        {
            FlockAgent newAgent = Instantiate(
                agentPrefab,
                (Vector2)spawnPoint.position + (Random.insideUnitCircle * numAgents * 0.08f),
                Quaternion.Euler(Vector3.forward * Random.Range(0f, 360f)),
                transform
                );
            newAgent.name = agentPrefab.name + i;
            newAgent.Initialize(this);
            newAgent.orbitBias = Random.Range(-1f, 1f);
            _agents.Add(newAgent);
            _agentColliderMap[newAgent.GetComponent<Collider2D>()] = newAgent;
        }
        _agentTransforms = _agents.Select(a => a.transform).ToArray();

        _player = GameManager.Instance.playerRef.transform;
        
        //for bounds and obstacles
        _contactFilter2DObstacles = new ContactFilter2D();
        _contactFilter2DObstacles.SetLayerMask(flockBehavior.SoftAvoidLayers);
        //get all possible obstacles in our bounds
        _agentNumObstacles = _collider2D.Overlap(_contactFilter2DObstacles, _agentObstacles);
        
        //patrol info
        if (patrolObj)
        {
            _patrolPoints = patrolObj.GetComponentsInChildren<Transform>()
                .Where(t => t != patrolObj.transform)
                .ToArray();;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActive)
            return;
        
        //opti, only get the average every 2 frames
        
        for (int i = 0; i < numAgents; i++)
        {
            FlockAgent agent = _agents[i]; 
            agent.UpdateBoostTimer(Time.deltaTime);

            _agentTransform = agent.transform;
            _currentAgentPos = _agentTransform.position;
            GetNeighborsAndObstacles(agent);
            CalculateAgentDirection(agent);

            
            if (agent.avoidingPlayer)
            {
                //the closer the fish is to the plaeyr the stronger the boos multiplier
                float dynamicWeight = Mathf.InverseLerp(flockBehavior.hardAvoidanceDistanceSqrd , 4f, _currentSqrdDistanceFromPlayer); // closer = 1.0
                float boostMultiplier = Mathf.Lerp(1f, flockBehavior.hardAvoidanceSpeedMultiplier, dynamicWeight);
                _currentAgentMove *= acceleration * boostMultiplier;
            }
            else
            {
                _currentAgentMove *= acceleration;
            }

            agent.Move(_currentAgentMove);

        }
        if (Time.frameCount % 2 == 0 && isPatroling && patrolWeight > 0)
        {
            UpdateCurrentPatrolPoint();
        }
        
    }

    private void CalculatePatrolingDir(FlockAgent agent)
    {
        if (isPatroling)
        {
            Vector2 toTarget = (_patrolPoints[_currentPatrolIndex].position - _currentAgentPos).normalized;
            Vector2 agentDir = _agentTransform.up.normalized; // or desiredMove.normalized if smoother

            float alignment = Vector2.Dot(agentDir, toTarget);
            if (alignment < 0.8f)
            {
                _currentAgentMove += toTarget * patrolWeight;

            }
            
        }


    }
    

    /**
     * Calculate move of an agent by taking into consideration Alignemtn, Cohesion and Avoidance.
     */
    public void CalculateAgentDirection(FlockAgent agent)
    {
        _currentAgentMove = _agentTransform.up;
        //alignment, cohesion and separation
        GetFlockDirection(agent);

        //avoid soft obstacles
        GetSoftAvoidanceDir(agent);
        
        //Hard avoidance (player, blue)
        CalculateHardAvoidanceDir(agent);
        //Following object
        CalculateFollowObjectDir(agent);
        CalculateSwimAroundObjectDir(agent);
        //containment
        CalculateDirInsideBounds(agent);
        //patroling
        CalculatePatrolingDir(agent);
        //If inside circle
        CalculateRandomInsideCircleDir(agent);
      
        _currentAgentMove.Normalize();
    }

    //Get all neighbors and transforms to avoid
    private void GetNeighborsAndObstacles(FlockAgent agent)
    {
        //this is not gonna change much, lets do it every 2 frames
        if(Time.frameCount % 2 == 0) 
            return;
        
        _agentNumNeighbors = 0;
        Array.Clear(_agentNeighbors, 0, _agentNeighbors.Length);

        int j = 0;
        for (int i = 0; i < numAgents; i++)
        {
            Transform other = _agentTransforms[i];
            if (other == _agentTransform) continue;

            float sqrDist = (other.transform.position - _currentAgentPos).sqrMagnitude;
            if (sqrDist <= flockBehavior.NeighborRadius * flockBehavior.NeighborRadius)
            {
                _agentNeighbors[j] = other;
                j++;
            }
            //no more than 10 neighbors
            if (j >= 10)
                break;
        }
        _agentNumNeighbors = j;
        
    }
    
    #region Behaviors
    private void GetFlockDirection(FlockAgent agent)
    {
        //if no neighbors, maintain current alignment
        if (_agentNumNeighbors < 1)
            return;

        int nAvoid = 0;
        _alignmentDir = Vector2.zero;
        _cohesionDir = Vector2.zero;
        _separationDir = Vector2.zero;
        for (int i = 0; i < _agentNumNeighbors; i++)
        {
            if (_agentNeighbors[i] != null)
            {
                //alignment
                Transform neighbor = _agentNeighbors[i];
                Vector3 neighborPos = neighbor.position;
                Vector3 toNeighbor = neighborPos - _currentAgentPos;
                _alignmentDir += (Vector2)neighbor.up;
                _cohesionDir += (Vector2)neighborPos;
                
                if (toNeighbor.sqrMagnitude < _squareAvoidanceRadius)
                {
                    nAvoid++;
                    _separationDir += (Vector2)(_currentAgentPos - neighborPos);
                    
                }
                
            }
            
        }
        //alignemtn
        _alignmentDir /= _agentNumNeighbors;
        _alignmentDir.Normalize();
        //cohesion
        _cohesionDir /= _agentNumNeighbors;
        _cohesionDir -= (Vector2)_currentAgentPos;
        _cohesionDir.Normalize();
        //separation
        if (nAvoid > 0)
        {
            _separationDir /= nAvoid;
            _separationDir.Normalize();
        }
        
        //Alignment. Half if containment inside circle
        _alignmentDir *= flockBehavior.AlignmentWeight;
                
        //reduce cohesion if is patroling. Temp fix 
        if(isPatroling || isContainedInCircle)
            _cohesionDir *= (flockBehavior.CohesionWeight*2/3);
        else
            _cohesionDir *= flockBehavior.CohesionWeight;
        
        _separationDir *= flockBehavior.SeparationWeight;

        _currentAgentMove += _alignmentDir + _cohesionDir + _separationDir;
    }

    //Get the direction if the agent needs to move inside the bounds
    private void CalculateDirInsideBounds(FlockAgent agent)
    {
        if (isContained && _collider2D && !agent.avoidingPlayer)
        {
            //if inside the collider, no adjustment
            if (_collider2D.OverlapPoint(_currentAgentPos))
                return;

            Vector2 closestPoint = _collider2D.ClosestPoint(_currentAgentPos);
            Vector2 dir = closestPoint - (Vector2)_currentAgentPos;
            float mag = dir.magnitude;
            float percent = mag / 10;
            if (percent > 0.01f)
            {
                _currentAgentMove += dir * percent;
            }
            
        }
    }
    //calculate direction soft avoiding obstacles.
    private void GetSoftAvoidanceDir(FlockAgent agent)
    {
        if (_agentNumObstacles < 1 || flockBehavior.SoftAvoidanceWeight <= 0)
            return;
        
        Vector2 softAvoidance = Vector2.zero;
        Vector2 forward = _agentTransform.up;
        int numSoftObstacles = 0;
        
        for (int i = 0; i < _agentNumObstacles; i++)
        {
            Vector2 toObstacle = (_agentObstacles[i].ClosestPoint(_currentAgentPos) - (Vector2)_currentAgentPos);
            //now check sqr distance to see if that obstacle is near
            if (toObstacle.sqrMagnitude < _squareAvoidanceRadius)
            {
                numSoftObstacles++;
                //close osbtacle!
                toObstacle.Normalize();
                // Compute both perpendicular options
                Vector2 perp = new Vector2(-toObstacle.y, toObstacle.x); // left
                float dot = Vector2.Dot(forward, perp);

                // If dot is negative, right is better → invert perp
                if (dot < 0f)
                    perp = -perp;

                softAvoidance += perp;
            }
        }

        if (numSoftObstacles > 0)
        {
            softAvoidance = (softAvoidance / numSoftObstacles);
            softAvoidance.Normalize();
            _currentAgentMove += softAvoidance * flockBehavior.SoftAvoidanceWeight;
        }
    }
    
    //Hard avoidance is used to escape player and/or blue
    //It pushes the flock agent to the opposite direction and gives it a speed boost
    private void CalculateHardAvoidanceDir(FlockAgent agent)
    {
       
        Vector2 hardAvoidDir = Vector2.zero;
        int num = 0;
        if (canAvoidPlayer)
        {
            num++;
            Vector2 toPlayer = _player.position - _currentAgentPos;
            //_currentSqrdDistanceFromPlayer = toPlayer.sqrMagnitude;
            if (toPlayer.sqrMagnitude < flockBehavior.hardAvoidanceDistanceSqrd)
            {
                agent.TriggerSpeedBoost(flockBehavior.hardAvoidanceSpeedDuration);
                hardAvoidDir = (Vector2)(_currentAgentPos - _player.position);
                hardAvoidDir.Normalize();
            }
            
        }

        if (canAvoidBlue)
        {
            num++;
            Vector2 toBlue = GameManager.Instance.blueNpcRef.transform.position - _currentAgentPos;
            //_currentSqrdDistanceFromPlayer = toPlayer.sqrMagnitude;
            if (toBlue.sqrMagnitude < flockBehavior.hardAvoidanceDistanceSqrd)
            {
                agent.TriggerSpeedBoost(flockBehavior.hardAvoidanceSpeedDuration);
                hardAvoidDir = (Vector2)(_currentAgentPos - GameManager.Instance.blueNpcRef.transform.position);
                hardAvoidDir.Normalize();
            }
        }

        if (num > 0)
        {
            hardAvoidDir /= num;
            _currentAgentMove += hardAvoidDir * flockBehavior.HardAvoidanceWeight;
            
        }

    }

    //THe the direction if this flock should follow a specific object
    //followDistance 0 means it should always be following with the same strength
    private void CalculateFollowObjectDir(FlockAgent agent)
    {
        if (isFollowing && followKeepDistance < 1)
        {
            Vector2 toTarget = (Vector2)followObj.position - (Vector2)_currentAgentPos;
            float SqrDistance = toTarget.sqrMagnitude;
            float dynamicWeight = 1;
            if(followDistance > 0 )
                dynamicWeight = Mathf.InverseLerp(1f,followDistance * followDistance, SqrDistance); // closer = 1.0
            
            _currentAgentMove += (toTarget.normalized * dynamicWeight) * followWeight;
            
        }
    }

    private void CalculateSwimAroundObjectDir(FlockAgent agent)
    {
        if (isFollowing && followKeepDistance > 0)
        {
            Vector2 toTarget = (Vector2)followObj.position - (Vector2)_currentAgentPos;
            float sqrDistance = toTarget.sqrMagnitude;

            // Maintain distance
            float distance = Mathf.Sqrt(sqrDistance);
            float distanceError = distance - followKeepDistance;

            // Get normalized direction to the target
            Vector2 dirToTarget = toTarget.normalized;

            // Get perpendicular direction for orbiting
            Vector2 perpendicular = new Vector2(-dirToTarget.y, dirToTarget.x);

            // Combine the perpendicular "orbit" direction and a push toward/away from the follow object
            Vector2 maintainDistanceForce = dirToTarget * Mathf.Clamp(distanceError, -1f, 1f); // push in or out
            Vector2 orbitForce = perpendicular * (1f + agent.orbitBias * 0.2f); // ±20% variation;

            // Final movement vector
            _currentAgentMove += (maintainDistanceForce + orbitForce) * followWeight;
            
        }
        
    }

    private void CalculateRandomInsideCircleDir(FlockAgent agent)
    {
        if (isContainedInCircle)
        {
            Vector3 toCenter = containMiddlePoint.position - _currentAgentPos;
            float distance = toCenter.sqrMagnitude;
            
            Vector2 randomSteer = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f)
            ).normalized * containRandomnessWeight;
            Vector2 steer = toCenter.normalized;
            if (distance > containCircleRadius * containCircleRadius)
            {
                steer *= (distance - containCircleRadius);
            }
            else
            {
                steer += randomSteer;
            }
            _currentAgentMove += steer;
        }
    }
    private void UpdateCurrentPatrolPoint()
    {
        //update our average flock point
        _currentFlockAvgPos = Vector2.zero;
        foreach (Transform t in _agentTransforms)
        {
            Vector3 pos = t.position;
            _currentFlockAvgPos += new Vector2(pos.x, pos.y);
        }
        _currentFlockAvgPos /= numAgents;
        
        //now compare with our current patrol point
        if (Vector2.Distance(_currentFlockAvgPos, _patrolPoints[_currentPatrolIndex].position) < 8f)
        {
            _currentPatrolIndex = (_currentPatrolIndex + 1) % _patrolPoints.Length;
            
        }
    }
    
    
    #endregion endregion


    //to toggle the whole acitvity on and off
    public void ToggleActivity(bool newActive)
    {
        this.isActive = newActive;
    }

    public void TogglePatrol(bool newPatroling)
    {
        isPatroling = newPatroling;
    }

    public void ToggleFollowing(bool newFollowing)
    {
        isFollowing = newFollowing;
    }

    public void ToggleAvoidPlayer(bool newAvoid)
    {
        canAvoidPlayer = newAvoid;
    }
    
    public void ToggleAvoidBlue(bool newAvoid)
    {
        canAvoidBlue = newAvoid;
    }

    public void ToggleContainment(bool newContainment)
    {
        isContainedInCircle = newContainment;
    }


    private void OnDrawGizmosSelected()
    {
        if (isPatroling)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(_currentFlockAvgPos, 2);
        }

        if (isContainedInCircle)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(containMiddlePoint.position, containCircleRadius);
        }
    }
}
