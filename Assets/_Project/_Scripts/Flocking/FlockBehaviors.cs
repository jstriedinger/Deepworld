using System;
using System.Collections.Generic;
using UnityEngine;

public class FlockBehaviors : MonoBehaviour
{
    [SerializeField] private bool sameFlock = true;
    [Range(0,50)] 
    [SerializeField] private float alignmentWeight = 1;
    [SerializeField] private float cohesionWeight = 1;
    
    [SerializeField] private float avoidanceWeight = 1;
    [SerializeField] private float radius = 0;
    
    
    //Filter our context for only our same flock
    private List<Transform> FilterSameFlock(FlockAgent agent, List<Transform> original)
    {
        List<Transform> filtered = new List<Transform>();
        foreach (Transform item in original)
        {
            FlockAgent itemAgent = item.GetComponent<FlockAgent>();
            if (itemAgent != null && itemAgent.AgentFlock == agent.AgentFlock)
            {
                filtered.Add(item);
            }
        }
        return filtered;
    }


 
}
