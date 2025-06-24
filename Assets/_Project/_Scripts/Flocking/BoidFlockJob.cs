using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using Random = UnityEngine.Random;
using Unity.Jobs;
using UnityEngine.Jobs;

using Unity.Mathematics;
public struct BoidData
{
    public float2 position;
    public float2 forward;
}
[BurstCompile]
public struct FlockDirectionJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<BoidData> Boids;
    public NativeArray<float2> Output;

    //alignment, cohesion and separation variables
    public float NeighborRadius, AvoidanceRadius;
    public float AlignmentWeight, CohesionWeight, SeparationWeight;
    
    //for hard avoidance
    [ReadOnly] public float2 PlayerPos;
    [ReadOnly] public float2 BluePos;
    public bool AvoidPlayer;
    public bool AvoidBlue;
    public float HardAvoidanceDistanceSqr;
    public float HardAvoidanceWeight;
    public NativeArray<float> BoostMultipliers;
    public float HardAvoidanceSpeedMultiplier;
    
    //for following object
    [ReadOnly] public float2 FollowTargetPos;
    public bool IsFollowing;
    public float FollowWeight;
    public float FollowDistance;
    public float FollowKeepDistance;
    [ReadOnly] public NativeArray<float> OrbitBiasArray; // per agent
    
    //for containment inside circle
    public bool IsContainedInCircle;
    public float2 ContainmentCenter;
    public float ContainmentRadius;
    public float RandomnessWeight;
    public float DeltaTime;
    [ReadOnly] public NativeArray<uint> randomSeeds;
    
    //For patroling
    public bool isPatroling;
    public float patrolWeight;
    public float2 currentPatrolTarget;
    public float alignmentThreshold;

    public void Execute(int index)
    {
        float2 selfPos = Boids[index].position;
        float2 selfForward = Boids[index].forward;

        float2 alignment = float2.zero;
        float2 cohesion = float2.zero;
        float2 separation = float2.zero;

        int numNeighbors = 0;
        int numAvoiding = 0;

        for (int i = 0; i < Boids.Length; i++)
        {
            if (i == index) continue;

            float2 otherPos = Boids[i].position;
            float2 toOther = otherPos - selfPos;
            float distSqr = math.lengthsq(toOther);

            if (distSqr <= NeighborRadius * NeighborRadius)
            {
                alignment += Boids[i].forward;
                cohesion += otherPos;

                if (distSqr < AvoidanceRadius * AvoidanceRadius)
                {
                    separation += (selfPos - otherPos);
                    numAvoiding++;
                }

                numNeighbors++;
            }

            if (numNeighbors > 9)
                break;
        }

        if (numNeighbors > 0)
        {
            alignment = math.normalize(alignment / numNeighbors) * AlignmentWeight;
            cohesion = math.normalize((cohesion / numNeighbors - selfPos)) * CohesionWeight;
        }

        if (numAvoiding > 0)
        {
            separation = math.normalize(separation / numAvoiding) * SeparationWeight;
        }
        
        //Calculate hard avoidance
        float2 hardAvoid = float2.zero;
        int hardAvoidCount = 0;

        //player avoidance
        BoostMultipliers[index] = 1f;
        if (AvoidPlayer)
        {
            float2 toPlayer = PlayerPos - selfPos;
            float distToPlayerSqr = math.lengthsq(toPlayer);
            if (distToPlayerSqr < HardAvoidanceDistanceSqr)
            {
                hardAvoid += math.normalize(selfPos - PlayerPos);
                hardAvoidCount++;
                BoostMultipliers[index] = HardAvoidanceSpeedMultiplier;
            }
        }
        if (AvoidBlue)
        {
            float2 toBlue = BluePos - selfPos;
            float distToBlueSqr = math.lengthsq(toBlue);
            if (AvoidBlue && distToBlueSqr < HardAvoidanceDistanceSqr)
            {
                hardAvoid += math.normalize(selfPos - BluePos);
                hardAvoidCount++;
                BoostMultipliers[index] = HardAvoidanceSpeedMultiplier;
            }
        }

        if (hardAvoidCount > 0)
        {
            hardAvoid = math.normalize(hardAvoid / hardAvoidCount) * HardAvoidanceWeight;
        }
        
        //for following
        float2 followDir = float2.zero;

        if (IsFollowing)
        {
            float2 toFollow = FollowTargetPos - selfPos;
            float sqrDist = math.lengthsq(toFollow);

            // Keep distance logic (swim around)
            if (FollowKeepDistance > 0)
            {
                float dist = math.sqrt(sqrDist);
                float error = dist - FollowKeepDistance;

                float2 toTarget = math.normalize(toFollow);
                float2 perp = new float2(-toTarget.y, toTarget.x);
                float orbit = 1f + OrbitBiasArray[index] * 0.2f;

                float2 maintain = toTarget * math.clamp(error, -1f, 1f);
                float2 orbiting = perp * orbit;

                followDir = (maintain + orbiting) * FollowWeight;
            }
            else
            {
                // Pure follow (at distance)
                if (FollowDistance > 0)
                {
                    float dynamicWeight = 0f;
                    if (sqrDist <= FollowDistance * FollowDistance)
                    {
                        // Optional: stronger follow when closer, softer when farther
                        dynamicWeight = math.saturate(math.unlerp(FollowDistance * FollowDistance, 1f, sqrDist));
                    }
                    followDir = math.normalize(toFollow) * dynamicWeight * FollowWeight;
                    
                }
                else
                {
                    followDir = math.normalize(toFollow) * FollowWeight;
                }

                
            }
        }
        
        //circle containment
        float2 containmentDir = float2.zero;
        if (IsContainedInCircle)
        {
            //wehn contained in circle, we do not care about alignment, cohesion or separation
            alignment = cohesion = separation = 0;
            float2 toCenter = ContainmentCenter - selfPos;
            float distSqr = math.lengthsq(toCenter);

            Unity.Mathematics.Random random = new Unity.Mathematics.Random(randomSeeds[index]); // Use Unity.Mathematics.Random
            
            float2 randomSteer = math.normalize(random.NextFloat2Direction()) * RandomnessWeight;

            if (distSqr > ContainmentRadius * ContainmentRadius)
            {
                float2 containSteer = math.normalize(toCenter) * (math.sqrt(distSqr) - ContainmentRadius);
                containmentDir += containSteer *2;
            }
            else
            {
                containmentDir += randomSteer *2;
            }
        }
        
        //for patroling
        float2 patrolingDir = float2.zero;
        if (isPatroling && patrolWeight > 0f)
        {
            float2 toTarget = math.normalize(currentPatrolTarget - selfPos);
            float align = math.dot(math.normalize(selfForward), toTarget);

            if (align < alignmentThreshold) // e.g., 0.8f
            {
                patrolingDir = toTarget * patrolWeight;
            }
        }

        float2 finalDir = selfForward + alignment + cohesion + separation 
                          + hardAvoid + followDir + containmentDir + patrolingDir;
        finalDir = math.normalize(finalDir);
        Output[index] = finalDir;
    }

}

[BurstCompile]
public class BoidFlockJob : MonoBehaviour
{
    [Header("Job")]
    private NativeArray<BoidData> _boidDataArray;
    private NativeArray<float2> _steeringOutputArray;
    private NativeArray<float> _orbitBiasArray;
    private NativeArray<float> _boostMultipliers;
    private NativeArray<uint> _randomSeeds;
    
    
    [Header("Config")]
    [SerializeField] private FlockBehaviorSO flockBehavior;
    [SerializeField] private FlockAgentJob agentPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Collider2D _collider2D;
    private Transform _player, _blueNpc;
    List<FlockAgentJob> _agents = new List<FlockAgentJob>();
    private Transform[] _agentTransforms;
    
    [Header("Properties")]
    [SerializeField] private bool isActive = true;
    [SerializeField] private bool isContained = true;
    [SerializeField] bool canAvoidPlayer;
    [SerializeField] bool canAvoidBlue;
    [Range(2, 500)]
    public int numAgents = 250;
    [Range(1f, 100f)]
    public float acceleration = 10f;
    [Range(0f, 3f)]
    public float avoidanceRadiusMultiplier = 0.5f;
    
    [Header("Follow behavior")]
    [SerializeField] private bool isFollowing;
    [SerializeField] private Transform followObj;
    [SerializeField, Range(.1f, 4f)] private float followWeight = 1;
    [SerializeField, Tooltip("Will follow within distance, 0 if always follow regardless")]
    private float followDistance = 0;
    [SerializeField]
    private float followKeepDistance = 0;
    
    [Header("Patrol behavior")]
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
    private Dictionary<Collider2D, FlockAgentJob> _agentColliderMap = new();
    private Vector2 _alignmentDir, _cohesionDir, _separationDir;
    private Transform _agentTransform;
    private Vector3 _currentAgentPos;
    private Vector2 _currentAgentMove;
    private float _currentSqrdDistanceFromPlayer;
    
    

    // Start is called before the first frame update
    void Start()
    {
        //job stuff
        _boidDataArray = new NativeArray<BoidData>(numAgents, Allocator.Persistent);
        _steeringOutputArray = new NativeArray<float2>(numAgents, Allocator.Persistent);
        _orbitBiasArray = new NativeArray<float>(numAgents, Allocator.Persistent);
        _boostMultipliers = new NativeArray<float>(numAgents, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        _randomSeeds = new NativeArray<uint>(numAgents, Allocator.Persistent);
        
        //cache the transforms
        _agentTransforms = _agents.Select(a => a.transform).ToArray();
        
        _squareNeighborRadius = flockBehavior.NeighborRadius * flockBehavior.NeighborRadius;
        _squareAvoidanceRadius = _squareNeighborRadius * avoidanceRadiusMultiplier * avoidanceRadiusMultiplier;

        for (int i = 0; i < numAgents; i++)
        {
            FlockAgentJob newAgent = Instantiate(
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
        _blueNpc = GameManager.Instance.blueNpcRef.transform;
        
        //for bounds and obstacles
        _contactFilter2DObstacles = new ContactFilter2D();
        _contactFilter2DObstacles.SetLayerMask(flockBehavior.SoftAvoidLayers);
        //get all possible obstacles in our bounds
        if(_collider2D)
            _agentNumObstacles = _collider2D.Overlap(_contactFilter2DObstacles, _agentObstacles);
        
        //patrol info
        if (patrolObj)
        {
            _patrolPoints = patrolObj.GetComponentsInChildren<Transform>()
                .Where(t => t != patrolObj.transform)
                .ToArray();;
        }
        
        //saving orbits info
        for (int i = 0; i < numAgents; i++)
        {
            _orbitBiasArray[i] = _agents[i].orbitBias;
            _randomSeeds[i] = (uint)UnityEngine.Random.Range(1, int.MaxValue);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActive)
            return;
        
        //opti, only get the average every 2 frames
        
        //filling the job data
        for (int i = 0; i < numAgents; i++)
        {
            Transform tf = _agentTransforms[i];
            _boidDataArray[i] = new BoidData
            {
                position = new float2( tf.position.x,tf.position.y ),
                forward = new float2( tf.up.x,tf.up.y )
            };
        }
        
        if (Time.frameCount % 2 == 0 && isPatroling && patrolWeight > 0)
        {
            UpdateCurrentPatrolPoint();
        }
        
        //create the job
        var job = new FlockDirectionJob
        {
            Boids = _boidDataArray,
            Output = _steeringOutputArray,
            NeighborRadius = flockBehavior.NeighborRadius,
            AvoidanceRadius = flockBehavior.NeighborRadius * avoidanceRadiusMultiplier,
            AlignmentWeight = flockBehavior.AlignmentWeight,
            CohesionWeight = flockBehavior.CohesionWeight,
            SeparationWeight = flockBehavior.SeparationWeight,
            
            //hard avoidance
            PlayerPos = new float2(_player.position.x, _player.position.y),
            BluePos = new float2(_blueNpc.position.x,_blueNpc.position.y),
            AvoidPlayer = canAvoidPlayer,
            AvoidBlue = canAvoidBlue,
            HardAvoidanceDistanceSqr = flockBehavior.hardAvoidanceDistanceSqrd,
            HardAvoidanceWeight = flockBehavior.HardAvoidanceWeight,
            BoostMultipliers = _boostMultipliers,
            HardAvoidanceSpeedMultiplier = flockBehavior.hardAvoidanceSpeedMultiplier,
            
            //for following
            IsFollowing = isFollowing,
            FollowTargetPos = followObj ? new float2(followObj.position.x, followObj.position.y) : float2.zero,
            FollowWeight = followWeight,
            FollowDistance = followDistance,
            FollowKeepDistance = followKeepDistance,
            OrbitBiasArray = _orbitBiasArray,
            
            //for containment inside circle
            ContainmentCenter = containMiddlePoint ? new float2(containMiddlePoint.position.x, containMiddlePoint.position.y) : float2.zero,
            ContainmentRadius = containCircleRadius,
            IsContainedInCircle = isContainedInCircle,
            RandomnessWeight = containRandomnessWeight,
            randomSeeds = _randomSeeds,
            DeltaTime = Time.deltaTime,
            
            //for patroling
            isPatroling = isPatroling,
            patrolWeight = patrolWeight,
            alignmentThreshold = 0.8f, // match your original behavior
            currentPatrolTarget = new float2(
            _patrolPoints[_currentPatrolIndex].position.x,
            _patrolPoints[_currentPatrolIndex].position.y)
        };

        JobHandle handle = job.Schedule(numAgents, 64);
        handle.Complete();

        
        //apply result back to agents
        for (int i = 0; i < numAgents; i++)
        {
            float2 dir = _steeringOutputArray[i];
            float speed = acceleration * _boostMultipliers[i];
            //containment
            if (Time.frameCount % 2 == 0 && isContained)
            {
                FlockAgentJob agent = _agents[i];
                _agentTransform = agent.transform;
                _currentAgentPos = _agentTransform.position;
                //extra every 3 frame to save memory
                Vector2 dirInsideBounds = CalculateDirInsideBounds(agent);
                //Debug.Log("Agent : "+agent.name+" dir inside:"+dirInsideBounds.ToString());
                if (dirInsideBounds.sqrMagnitude > 0)
                {
                    dir += new float2(dirInsideBounds.x, dirInsideBounds.y);
                    dir = math.normalize(dir);
                    
                }
            }
            _agents[i].Move(dir * speed);
        }
        
        /*for (int i = 0; i < numAgents; i++)
        {
            FlockAgent agent = _agents[i]; 
            agent.UpdateBoostTimer(Time.deltaTime);

            _agentTransform = agent.transform;
            _currentAgentPos = _agentTransform.position;
            GetNeighborsAndObstacles(agent);
            CalculateAgentDirection(agent);

            
            if (agent.AvoidingPlayer)
            {
                //the closer the fish is to the plaeyr the stronger the boos multiplier
                float dynamicWeight = Mathf.InverseLerp(flockBehavior.hardAvoidanceDistanceSqrd , 1f, _currentSqrdDistanceFromPlayer); // closer = 1.0
                float boostMultiplier = Mathf.Lerp(1f, flockBehavior.hardAvoidanceSpeedMultiplier, dynamicWeight);
                float finalAcceleration = acceleration * boostMultiplier;
                _currentAgentMove *= finalAcceleration;
            }
            else
            {
                _currentAgentMove *= acceleration;
            }

            agent.Move(_currentAgentMove);

        }*/
        
        
    }

    private void OnDestroy()
    {
        _boidDataArray.Dispose();
        _steeringOutputArray.Dispose();
        _orbitBiasArray.Dispose();
        _boostMultipliers.Dispose();
        _randomSeeds.Dispose();
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
        
        //Following object
        //CalculateFollowObjectDir(agent);
        CalculateSwimAroundObjectDir(agent);
        //containment
        //CalculateDirInsideBounds(agent);
        //patroling
        CalculatePatrolingDir(agent);
        //If inside circle
        CalculateRandomInsideCircleDir(agent);
        //Hard avoidance (player, blue)
        CalculateHardAvoidanceDir(agent);
      
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
    private Vector2 CalculateDirInsideBounds(FlockAgentJob agent)
    {
        if (isContained && _collider2D && !agent.AvoidingPlayer)
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
            
        }
        return Vector2.zero;
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
            //closest point is very expensive
            //Vector2 toObstacle = (_agentObstacles[i].ClosestPoint(_currentAgentPos) - (Vector2)_currentAgentPos);
            //for now lets just check distance to the object itself. Way less accuarate for level
            Vector2 toObstacle = (_agentObstacles[i].transform.position - _currentAgentPos);
            //check sqr distance to see if that obstacle is near
            if (toObstacle.sqrMagnitude < _squareAvoidanceRadius)
            {
                //close osbtacle!
                numSoftObstacles++;
                //get direction
                toObstacle.Normalize();
                // dot product to get both perpendicular directions
                Vector2 perp = new Vector2(-toObstacle.y, toObstacle.x); // left
                float dot = Vector2.Dot(forward, perp);

                // If dot is negative, then is better to go to the right
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
            _currentSqrdDistanceFromPlayer = toPlayer.sqrMagnitude;
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
        float sqrDistance = (_currentFlockAvgPos - (Vector2)_patrolPoints[_currentPatrolIndex].position).sqrMagnitude;
        if (sqrDistance < 49f)
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
        //if it is contained inside circle, it does not need normal containment
        isContained = !newContainment;
    }


    private void OnDrawGizmosSelected()
    {
        if (isPatroling)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_currentFlockAvgPos, 3);
        }

        if (isContainedInCircle)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(containMiddlePoint.position, containCircleRadius);
        }
    }
}
