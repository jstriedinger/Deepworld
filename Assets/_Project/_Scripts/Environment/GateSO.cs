using UnityEngine;
using FMODUnity;

[CreateAssetMenu(fileName="GateSO",menuName="ScriptableObjects/GateSO")]
public class GateSO : ScriptableObject
{
    [field: Header("Sound")]
    [field: SerializeField] public EventReference SfxSwitch { get; private set; }
    [field: SerializeField] public EventReference SfxOpen { get; private set; }
    
    [field: SerializeField] public float GateOpenTime { get; private set; }
    [field: SerializeField] public float SwitchResetTime { get; private set; }
    
}
