using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName="MonsterSO",menuName="ScriptableObjects/MonsterSO")]
public class MonsterSO : ScriptableObject
{
    [field: Header("movement")]
    [field: SerializeField] public bool IsReactive { get; private set; }
    [field: SerializeField] public float PatrolSpeed { get; private set; }
    [field: SerializeField] public float FollowRange { get; private set; }
    [field: SerializeField] public float FollowSpeed { get; private set; }
    [field: SerializeField] public float ChasingRange { get; private set; }
    [field: SerializeField] public float ChasingSpeed { get; private set; }
    
    [field: Header("sfx")]
    [field: SerializeField] public EventReference SfxMonsterReact { get; private set; }


    [field:Header("Eye behavior")]
    [field: SerializeField] public Sprite SpriteBall { get; private set; }
    [field: SerializeField] public Sprite SpriteMaw { get; private set; }
    [field: SerializeField] public Sprite SpritePupil { get; private set; }
    [field: SerializeField] public float PupilDropSpeed { get; private set; } = 0.1f;
    [field: SerializeField] public float EyeBallShutSpeed { get; private set; } = 0.05f;
    
    [field:Header("State colors")]
    [field: SerializeField] public Gradient DefaultColorGradient { get; private set; }
    [field: SerializeField] public Color DefaultLightColor { get; private set; }
    [field: SerializeField] public Gradient FollowColorGradient { get; private set; }
    [field: SerializeField] public Gradient ChaseColorGradient { get; private set; }
    [field: SerializeField] public Color ChaseLightColor { get; private set; }
    
    [field: Header("Misc")]
    [field: SerializeField] public int DistanceToShowOnCamera { get; private set; } = 0;
}
