using UnityEngine;

public class FishFlockAgentJob : FlockAgentJob
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Material[] fishMats;
    [SerializeField]
    private bool randomSizes = true;
    [SerializeField] private float maxXSize, maxYSize;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();  
        if (_spriteRenderer)
        {
            //randome sizing ourselves
            if (randomSizes)
            {
                float r1 = GetRandomStepFloat(1, maxXSize, 0.1f);
                float r2 = GetRandomStepFloat(1, maxYSize, 0.1f);
                transform.localScale = new Vector3(r1,r2,1);
            }
            _spriteRenderer.sharedMaterial = fishMats[Random.Range(0, fishMats.Length)];
            
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private float GetRandomStepFloat(float min, float max, float step)
    {
        int steps = Mathf.RoundToInt((max - min) / step); // Total possible steps
        int randomStep = Random.Range(0, steps + 1); // Random step index
        return min + (randomStep * step); // Convert step index to float
    }
}
