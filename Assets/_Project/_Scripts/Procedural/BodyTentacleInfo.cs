using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public struct TentacleBodyPart
{
    public Transform bodyPart;
    public int bodyPos;
}
public class BodyTentacleInfo : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public int tentacleLength = 45;
    
    [Header("Dir and positioning")]
    public Transform targetDir;
    public Transform wiggleDir;
    public float pointGap;
    
    [Header("Body parts")]
    public bool randomize = false;
    
    [ListDrawerSettings(Expanded = true)]
    public List<TentacleBodyPart> bodyParts = new();
    
    private int GetRandomStepInt(int min, int max, int step)
    {
        int steps = Mathf.RoundToInt((max - min) / step); // Total possible steps
        int randomStep = Random.Range(0, steps + 1); // Random step index
        return min + (randomStep * step); // Convert step index to float
    }
    
    private float GetRandomStepFloat(float min, float max, float step)
    {
        int steps = Mathf.RoundToInt((max - min) / step); // Total possible steps
        int randomStep = Random.Range(0, steps + 1); // Random step index
        return min + (randomStep * step); // Convert step index to float
    }
    
    public void SetupRandomValues(float randomPointGap = 0, int randomLengthStep = 0)
    {
        //tentacleLength = GetRandomStepInt(tentacleLength, tentacleLength + randomLengthStep, 1);
        pointGap = GetRandomStepFloat(pointGap, pointGap + randomPointGap, 0.01f);

    }
}
