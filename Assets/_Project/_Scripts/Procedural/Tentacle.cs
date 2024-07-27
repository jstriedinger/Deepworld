using System;
using System.Collections;

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering.Universal;


public class Tentacle : MonoBehaviour
{

    private LineRenderer _lineRenderer;
    private Vector3[] segmentV;
    [SerializeField] Light2D _internalLight2D;
    private Transform _internalLightParent;
    
    public int length;
    public Vector3[] segmentPoses;
    public Transform targetDir;
    public float targetDist;
    public float smoothSpeed;

    public float wiggleSpeed;
    public float wiggleMagnitude;
    public Transform wiggleDir;
    [HideInInspector] public bool resettingFromPlayer;
    private Segment[] _segmentsFromFollowPlayer;

    private void Awake()
    {
        resettingFromPlayer = false;
        _lineRenderer = GetComponent<LineRenderer>();
        if (_internalLight2D is not null)
            _internalLightParent = _internalLight2D.transform.parent;
    }

    private void Start()
    {
        _lineRenderer.positionCount = length;

        segmentPoses = new Vector3[length];
        segmentV = new Vector3[length];

        ResetPos();
    }

    private void OnEnable()
    {
        _lineRenderer.positionCount = length;

        segmentPoses = new Vector3[length];
        segmentV = new Vector3[length];

        
        //ResetPos();
    }

    public IEnumerator resetFromPlayer()
    {
        yield return new WaitForSeconds(2);
        resettingFromPlayer = false;
    }


    private void Update()
    {
        wiggleDir.localRotation = Quaternion.Euler(0,0,Mathf.Sin(Time.time * wiggleSpeed) * wiggleMagnitude);
        segmentPoses[0] = targetDir.position;

        for(int i = 1; i < segmentPoses.Length; i++)
        {
            segmentPoses[i] = Vector3.SmoothDamp(segmentPoses[i], segmentPoses[i-1] + targetDir.right * targetDist, ref segmentV[i], 
                resettingFromPlayer? 0.15f : smoothSpeed);
        }

        //light always follows last point in line renderer
        if (_internalLight2D is not null)
            _internalLightParent.position = segmentPoses[^1];

        _lineRenderer.SetPositions(segmentPoses);
    }

    public void ResetPos()
    {
        segmentPoses[0] = targetDir.position;
        for(int i = 1; i < length; i++){
            segmentPoses[i] = segmentPoses[i-1] + targetDir.right * targetDist;
        }
        _lineRenderer.SetPositions(segmentPoses);
    }

    //puts all tentacles back in their place, not smoothly
    public void RecalcPos(Segment[] segments)
    {
        resettingFromPlayer = true;
        segmentPoses[0] = segments[0].startingPosition;
        for(int i = 0; i < segments.Length; i++){
            segmentPoses[i+1] = segments[i].endingPosition;
        }
    }
    
    public Light2D GetInternalLight()
    {
        return _internalLight2D;
    }

}

