using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

[CreateAssetMenu(fileName="CoverSO",menuName="ScriptableObjects/CoverSO")]
public class CoverSO : ScriptableObject
{
    [field: Header("Sound")]
    [field: SerializeField] public EventReference SfxEnter { get; private set; }
    [field: SerializeField] public EventReference SfxExit { get; private set; }
    [field: SerializeField] public int CoverForce { get; private set; }
    
}
