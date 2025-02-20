using UnityEngine;
using FMODUnity;

[CreateAssetMenu(fileName="GateSO",menuName="ScriptableObjects/GateSO")]
public class GateSO : ScriptableObject
{
    [field: Header("Sound")]
    [field: SerializeField] public EventReference SfxActivate { get; private set; }
    
    [field: SerializeField] public float GateOpenTime { get; private set; }
    [field: SerializeField] public float SwitchResetTime { get; private set; }
    
}
