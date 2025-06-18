using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName="MonsterSO",menuName="ScriptableObjects/FlockBehavior")]
public class FlockBehaviorSO : ScriptableObject
{
    [field: Header("Flocking properties")]
    [field: SerializeField] public LayerMask AgentLayer { get; private set; } = 1;
    [SerializeField, Range(0.1f, 5f)]
    private float alignmentWeight = 1f;
    public float AlignmentWeight => alignmentWeight;
    [SerializeField, Range(0.1f, 5f)]
    private float cohesionWeight = 1f;
    public float CohesionWeight => cohesionWeight;
    [SerializeField, Range(0.1f, 5f)]
    private float separationWeight = 1f;
    [Tooltip("Separation weight between flock agents")]
    public float SeparationWeight => separationWeight;
    [Range(1, 10)][field: SerializeField] public float NeighborRadius { get; private set; } = 5;
    
    [field: Header("Soft Avoidance")]
    [SerializeField, Range(0f, 3f)]
    private float softAvoidanceWeight = 1f;
    [Tooltip("Avoidance weight for soft obstacles, like obstacles in the middle of the map")]
    public float SoftAvoidanceWeight => softAvoidanceWeight;
    [field: SerializeField] public LayerMask SoftAvoidLayers { get; private set; } = 1;
    [SerializeField, Range(0.1f, 3f)]
    
    [field: Header("Hard Avoidance")]
    private float hardAvoidanceWeight = 1f;
    public float HardAvoidanceWeight => hardAvoidanceWeight;
    [Range(1, 2)] [field: SerializeField] public int hardAvoidanceDistanceSqrd { get; private set; } = 1;
    [Range(1, 2)] [field: SerializeField] public float hardAvoidanceSpeedMultiplier { get; private set; } = 1;
    [Range(1, 3)] [field: SerializeField] public int hardAvoidanceSpeedDuration { get; private set; } = 1;
    
    [field: Header("Other")]
    public float agentSmoothTime = 0.5f;
    
}
