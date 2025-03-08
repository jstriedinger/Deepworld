using UnityEngine;
using UnityEngine.Serialization;

public class GateCoral : MonoBehaviour
{
    [SerializeField] private int length;
    [SerializeField] private Transform targetDir;
    [FormerlySerializedAs("targetDist")] [SerializeField] private float distBetweenPoints;
    [SerializeField] private float smoothSpeed;
    private Vector3[] segmentPoses;
    private Vector3[] segmentV;
    private LineRenderer _lineRenderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        segmentPoses[0] = targetDir.position;
        for(int i = 1; i < segmentPoses.Length; i++)
        {
            segmentPoses[i] = Vector3.SmoothDamp(segmentPoses[i], segmentPoses[i-1] + targetDir.right * distBetweenPoints, ref segmentV[i], 
                smoothSpeed);
        }

        _lineRenderer.SetPositions(segmentPoses);
    }
    

    
}
