using System;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class TentacleInfo : MonoBehaviour
{
    public bool randomize = false;
    public int tentacleLength = 45;
    public LineRenderer lineRenderer;
    public Transform targetDir;
    public Transform wiggleDir;
    [FormerlySerializedAs("minDistBetweenPoints")] [FormerlySerializedAs("targetDist")] public float minPointGap;
    [FormerlySerializedAs("idleWiggleSpeed")] public float minWiggleSpeed;
    
    [HideInInspector]
    public float currentPointGap, currentWiggleSpeed;

    private void Awake()
    {
        currentWiggleSpeed = minWiggleSpeed;
        currentPointGap = minPointGap;
    }

    private float GetRandomStepFloat(float min, float max, float step)
    {
        int steps = Mathf.RoundToInt((max - min) / step); // Total possible steps
        int randomStep = Random.Range(0, steps + 1); // Random step index
        return min + (randomStep * step); // Convert step index to float
    }
    private int GetRandomStepInt(int min, int max, int step)
    {
        int steps = Mathf.RoundToInt((max - min) / step); // Total possible steps
        int randomStep = Random.Range(0, steps + 1); // Random step index
        return min + (randomStep * step); // Convert step index to float
    }

    public void SetupMonsterTentacle(float randomDistStep = 0, float randomWiggleStep = 0, int randomLengthStep = 0)
    {
        tentacleLength = GetRandomStepInt(tentacleLength, tentacleLength + randomLengthStep, 1);
        minPointGap = GetRandomStepFloat(minPointGap, minPointGap + randomDistStep, 0.01f);
        minWiggleSpeed = GetRandomStepFloat(minWiggleSpeed, minWiggleSpeed + randomWiggleStep, 0.1f);

    }
}
