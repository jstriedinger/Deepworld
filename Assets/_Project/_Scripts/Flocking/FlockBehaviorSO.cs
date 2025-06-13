using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName="MonsterSO",menuName="ScriptableObjects/FlockBehavior")]
public class FlockBehaviorSO : ScriptableObject
{
    [field: Header("Flocking properties")]
    [field: SerializeField] public LayerMask AgentLayer { get; private set; } = 1;
    [SerializeField, Range(0.1f, 2f)]
    private float alignmentWeight = 1f;
    public float AlignmentWeight => alignmentWeight;
    [SerializeField, Range(0.1f, 2f)]
    private float cohesionWeight = 1f;
    public float CohesionWeight => cohesionWeight;
    [SerializeField, Range(0.1f, 2f)]
    private float separationWeight = 1f;
    [Tooltip("Separation weight between flock agents")]
    public float SeparationWeight => separationWeight;
    [field: Header("Avoidance")]
    [SerializeField, Range(0.1f, 3f)]
    private float softAvoidanceWeight = 1f;
    [Range(1, 10)][field: SerializeField] public float NeighborRadius { get; private set; } = 5;
    [Tooltip("Avoidance weight for soft obstacles, like obstacles in the middle of the map")]
    public float SoftAvoidanceWeight => softAvoidanceWeight;
    [field: SerializeField] public LayerMask SoftAvoidLayers { get; private set; } = 1;
    [field: SerializeField] public bool avoidPlayer { get; private set; } = false;
    [SerializeField, Range(0.1f, 3f)]
    private float avoidPlayerWeight = 1f;
    public float AvoidPlayerWeight => avoidPlayerWeight;
    [Range(1, 2)] [field: SerializeField] public int avoidPlayerDistanceSqrd { get; private set; } = 1;
    [Range(1, 2)] [field: SerializeField] public float avoidPlayerSpeedMultiplier { get; private set; } = 1;
    [Range(1, 3)] [field: SerializeField] public int avoidPlayerSpeedDuration { get; private set; } = 1;
    [field: Header("Following")]
    [field: SerializeField] public bool followPlayer { get; private set; } = false;
    [SerializeField, Range(0.1f, 1.5f)]
    private float followPlayerWeight = 1f;
    public float FollowPlayerWeight => followPlayerWeight;
    [Tooltip("Less than this distance it will try to follow the player")]
    [field: SerializeField] public int FollowPlayerDistance { get; private set; } = 50;
    [field: Header("Other")]
    
    
    
    public float agentSmoothTime = 0.5f;
    
}
