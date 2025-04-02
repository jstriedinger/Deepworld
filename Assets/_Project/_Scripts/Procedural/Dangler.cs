using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

public class Dangler : MonoBehaviour
{
    [SerializeField] Light2D internalLight2D;
    
    public int length;
    public Transform targetDir;
    public float targetDist;

    private Transform _internalLightParent;
    private LineRenderer _lineRenderer;
    private Vector3[] _segmentPoses;
    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _lineRenderer.positionCount = length;
        _segmentPoses = new Vector3[length];
        if (internalLight2D is not null)
            _internalLightParent = internalLight2D.transform.parent;
        ResetPos();
    }
    
    public void ResetPos()
    {
        _segmentPoses[0] = targetDir.position;
        for(int i = 1; i < length; i++){
            _segmentPoses[i] = _segmentPoses[i-1] + targetDir.right * targetDist;
        }
        _lineRenderer.SetPositions(_segmentPoses);
        if (internalLight2D is not null)
            _internalLightParent.position = _segmentPoses[^1];
        
    }
}
