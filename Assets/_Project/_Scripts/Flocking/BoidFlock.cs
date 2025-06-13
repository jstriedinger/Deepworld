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
    private Transform _player;
    private PolygonCollider2D _collider2D;
    List<FlockAgent> _agents = new List<FlockAgent>();
    private Transform[] _agentTransforms;
    
    [Header("Properties")]
    [SerializeField] private bool isActive = true;
    [Tooltip("A exclusive flock means it doesnt mix with other flocks")]
    [SerializeField] private bool isExclusive = true;
    [SerializeField] private bool isContained = true;
    [Range(2, 250)]
    public int numAgents = 250;
    [Range(1f, 100f)]
    public float acceleration = 10f;
    [Range(0f, 2f)]
    public float avoidanceRadiusMultiplier = 0.5f;
    
    [Header("Patrol info")]
    [SerializeField] private bool isPatroling;
    [SerializeField] private GameObject patrolObj;
    [SerializeField, Range(.1f, 3f)] private float patrolWeight = 1; 
    private Transform[] _patrolPoints;
    private int _currentPatrolIndex = 0;
    private Vector2 _currentFlockAvgPos;

    float _squareMaxSpeed, _squareNeighborRadius, _squareAvoidanceRadius, _squareMaxSpeedAvoidingPlayer;
    public float SquareAvoidanceRadius { get { return _squareAvoidanceRadius; } }
    
    public float agentSmoothTime = 0.5f;
    //every frame this get connstantly update to valid neighbors of an agent
    private Collider2D[] _neighborsResults = new Collider2D[20];
    private Collider2D[] _obstacleResults = new Collider2D[10];
    private int _numNeighbors = 0, _numObstacles = 0;
    private ContactFilter2D _contactFilter2DAgents, _contactFilter2DObstacles;
    
    //Keep tracking
    private Dictionary<Collider2D, FlockAgent> _agentColliderMap = new();
    private Vector2 _alignmentDir, _cohesionDir, _separationDir;
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
                (Vector2)transform.position + (Random.insideUnitCircle * numAgents * 0.08f),
                Quaternion.Euler(Vector3.forward * Random.Range(0f, 360f)),
                transform
                );
            newAgent.name = agentPrefab.name + i;
            newAgent.Initialize(this);
            _agents.Add(newAgent);
            _agentColliderMap[newAgent.GetComponent<Collider2D>()] = newAgent;
        }
        _agentTransforms = _agents.Select(a => a.transform).ToArray();

        _player = GameManager.Instance.playerRef.transform;
        _collider2D = GetComponent<PolygonCollider2D>();
        
        _contactFilter2DAgents = new ContactFilter2D();
        _contactFilter2DObstacles = new ContactFilter2D();
        _contactFilter2DAgents.SetLayerMask(flockBehavior.AgentLayer);
        _contactFilter2DObstacles.SetLayerMask(flockBehavior.SoftAvoidLayers);
        _contactFilter2DAgents.useTriggers = _contactFilter2DObstacles.useTriggers = true;
        
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
        
        for (int i = 0; i < _agents.Count; i++)
        {
            FlockAgent agent = _agents[i]; 
            agent.UpdateBoostTimer(Time.deltaTime);
            
            _currentAgentPos = agent.transform.position;
            GetNeighborsAndObstacles(agent);
            CalculateAgentDirection(agent);

            

            if (agent.avoidingPlayer)
            {
                //the closer the fish is to the plaeyr the stronger the boos multiplier
                float dynamicWeight = Mathf.InverseLerp(flockBehavior.avoidPlayerDistanceSqrd , 4f, _currentSqrdDistanceFromPlayer); // closer = 1.0
                float boostMultiplier = Mathf.Lerp(1f, flockBehavior.avoidPlayerSpeedMultiplier, dynamicWeight);
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

    private Vector2 CalculatePatrolingDir(FlockAgent agent)
    {
        Vector2 toTarget = (_patrolPoints[_currentPatrolIndex].position - _currentAgentPos).normalized;
        Vector2 agentDir = agent.transform.up.normalized; // or desiredMove.normalized if smoother

        float alignment = Vector2.Dot(agentDir, toTarget);
        if (alignment < 0.8f)
        {
            return toTarget * patrolWeight;
        }

        return Vector2.zero;


    }

    private void LateUpdate()
    {
        
    }

    //Get all neighbors and transforms to avoid
    private void GetNeighborsAndObstacles(FlockAgent agent)
    {
        
        _numNeighbors = Physics2D.OverlapCircle(_currentAgentPos, flockBehavior.NeighborRadius, _contactFilter2DAgents, _neighborsResults);
        if (isExclusive)
        {
            //filter only our flock
            int filteredCount = 0;
            for (int i = 0; i < _numNeighbors; i++)
            {
                var neighborCollider = _neighborsResults[i];
                if (neighborCollider == null)
                    continue;

                //check if its agent
                if (_agentColliderMap.TryGetValue(neighborCollider, out var itemAgent))
                {
                    if (itemAgent.AgentFlock == agent.AgentFlock)
                    {
                        _neighborsResults[filteredCount++] = neighborCollider;
                    }
                }
                if (filteredCount >= 20) break;
            }
            _numNeighbors = filteredCount;
        }
        
        //Find obstacles
        _numObstacles = Physics2D.OverlapCircle(_currentAgentPos, flockBehavior.NeighborRadius, _contactFilter2DObstacles, _obstacleResults);
       
    }
    #region Behaviors
    /**
     * Calculate move of an agent by taking into consideration Alignemtn, Cohesion and Avoidance.
     */
    public void CalculateAgentDirection(FlockAgent agent)
    {
        _currentAgentMove = agent.transform.up;
        GetFlockDirection(agent);

        //avoid soft obstacles
        if(flockBehavior.SoftAvoidanceWeight > 0 && _numObstacles > 0)
            _currentAgentMove += GetSoftAvoidanceDir(agent) * flockBehavior.SoftAvoidanceWeight;
        
        //avoid player
        if (flockBehavior.avoidPlayer)
            _currentAgentMove += GetAvoidPlayerDir(agent) * flockBehavior.AvoidPlayerWeight;
        else if (flockBehavior.followPlayer)
            _currentAgentMove += GetFollowPlayerDir(agent) * flockBehavior.FollowPlayerWeight;
        //contain inside bounds
        if (isContained && _collider2D && !agent.avoidingPlayer)
            _currentAgentMove += GetDirInsideBounds(agent);
        
        //if patroling go there
        if (isPatroling)
            _currentAgentMove += CalculatePatrolingDir(agent);
      
        //Debug.DrawRay(_currentAgentPos, dir.normalized * 3, Color.green);
        _currentAgentMove.Normalize();
    }

    private Vector2 GetFollowPlayerDir(FlockAgent agent)
    {
        Vector2 toTarget = (Vector2)_player.position - (Vector2)_currentAgentPos;
        float distance = Vector2.Distance(_currentAgentPos, _player.position);
        float dynamicWeight = Mathf.InverseLerp(flockBehavior.FollowPlayerDistance, 3f, distance); // closer = 1.0
        return toTarget.normalized * dynamicWeight;
    }


    //Get the direction if the agent needs to move inside the bounds
    private Vector2 GetDirInsideBounds(FlockAgent agent)
    {
        //if inside the collider, no adjustment
        if (_collider2D.OverlapPoint(_currentAgentPos))
            return Vector2.zero;

        Vector2 closestPoint = _collider2D.ClosestPoint(_currentAgentPos);
        Vector2 dir = closestPoint - (Vector2)_currentAgentPos;
        float mag = dir.magnitude;
        float percent = mag / 10;
        if (percent > 0.01f)
        {
            return dir * percent;
        }

        return Vector2.zero;
    }

    private void GetFlockDirection(FlockAgent agent)
    {
        //if no neighbors, maintain current alignment
        if (_numNeighbors < 1)
            return;

        int nAvoid = 0;
        _alignmentDir = Vector2.zero;
        _cohesionDir = Vector2.zero;
        _separationDir = Vector2.zero;
        for (int i = 0; i < _numNeighbors; i++)
        {
            //alignment
            Transform neighbor = _neighborsResults[i].transform;
            Vector3 neighborPos = neighbor.position;
            Vector3 toNeighbor = neighborPos - _currentAgentPos;
            _alignmentDir += (Vector2)neighbor.up;
            _cohesionDir += (Vector2)neighborPos;
            
            if (toNeighbor.sqrMagnitude < SquareAvoidanceRadius)
            {
                nAvoid++;
                _separationDir += (Vector2)(_currentAgentPos - neighborPos);
                
            }
            
        }
        //alignemtn
        _alignmentDir /= _numNeighbors;
        _alignmentDir.Normalize();
        //cohesion
        _cohesionDir /= _numNeighbors;
        _cohesionDir -= (Vector2)_currentAgentPos;
        _cohesionDir.Normalize();
        //separation
        if (nAvoid > 0)
        {
            _separationDir /= nAvoid;
            _separationDir.Normalize();
        }
        
        //now we use the weights
        _alignmentDir *= flockBehavior.AlignmentWeight;
        //reduce cohesion if is patroling. Temp fix 
        if(isPatroling)
            _cohesionDir *= (flockBehavior.CohesionWeight*2/3);
        else
            _cohesionDir *= flockBehavior.CohesionWeight;
        
        _separationDir *= flockBehavior.SeparationWeight;

        _currentAgentMove += _alignmentDir + _cohesionDir + _separationDir;
    }

    //calculate direction soft avoiding obstacles.
    private Vector2 GetSoftAvoidanceDir(FlockAgent agent)
    {
        if (_numObstacles < 1)
            return Vector2.zero;
        
        Vector2 softAvoidance = Vector2.zero;
        Vector2 forward = agent.transform.up;
        
        for (int i = 0; i < _numObstacles; i++)
        {
            
            Vector2 toObstacle = (_obstacleResults[i].ClosestPoint(_currentAgentPos) - (Vector2)_currentAgentPos);
            toObstacle.Normalize();

            // Compute both perpendicular options
            Vector2 perp = new Vector2(-toObstacle.y, toObstacle.x); // left
            float dot = Vector2.Dot(forward, perp);

            // If dot is negative, right is better → invert perp
            if (dot < 0f)
                perp = -perp;

            softAvoidance += perp;

        }

        softAvoidance = (softAvoidance / _numObstacles);
        softAvoidance.Normalize();
        return softAvoidance;
    }
    
    private Vector2 GetAvoidPlayerDir(FlockAgent agent)
    {
        
        Vector2 avoidPlayerDir = Vector2.zero;
        Vector2 toPlayer = _player.position - _currentAgentPos;
        _currentSqrdDistanceFromPlayer = toPlayer.sqrMagnitude;
        //avoideplayeridstance must be squared already. 25 if 5 distance e.g
        if (_currentSqrdDistanceFromPlayer < flockBehavior.avoidPlayerDistanceSqrd)
        {
            //Debug.Log("Agent % close: "+percent);
            agent.TriggerSpeedBoost(flockBehavior.avoidPlayerSpeedDuration);
            avoidPlayerDir = (Vector2)(_currentAgentPos - _player.position);
            avoidPlayerDir.Normalize();
            //Debug.Log("Avoidance player move: "+avoidancePlayerMove);
        }

        return avoidPlayerDir;
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
        //Debug.Log(averageFlockPosition);
        
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


    private void OnDrawGizmosSelected()
    {
        if (isPatroling)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(_currentFlockAvgPos, 2);
        }
    }
}
