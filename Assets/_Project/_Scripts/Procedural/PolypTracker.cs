using System;
using System.Collections.Generic;
using UnityEngine;
 
[RequireComponent(typeof(LineRenderer))]
public class PolypTracker : MonoBehaviour
{
    [SerializeField] private Transform target;
    public Vector3 targetPosition;
    public bool isFixed;
    public GameObject anchoredStart;
    public int segmentAmount = 10; 
    public float segmentsLength = 1;
    public GameObject targetSearchLocation;
    public float distTrigger;
    public float smoothFactor;
    
    Segment[] _segments;
    LineRenderer _lineRenderer;
    private Tentacle tentacle;
 
    void Start()
    {
        InitializeSegments();
    }
 
    void InitializeSegments()
    {
        if(!target)
            target = GameObject.Find("Player").transform;
        _lineRenderer = this.GetComponent<LineRenderer>();
        _segments = new Segment[segmentAmount];
        for(int i = 0; i<segmentAmount;i++)
        {
            Segment segment = new Segment();
            //segment.startingPosition = Vector3.zero+(Vector3.up*segmentsLength*(i));
            segment.startingPosition = anchoredStart.transform.position+(anchoredStart.transform.up*segmentsLength*(i));
            segment.endingPosition = segment.startingPosition+(anchoredStart.transform.up*segmentsLength*(i));
            segment.length = segmentsLength;
            _segments[i] = segment;
        }
    }
 
    void Update()
    {
        tentacle = GetComponent<Tentacle>();



        //Check if the player is within our target distance
        //If not, skip eveything else in this script
        if(distTrigger < Math.Abs(Vector3.Distance(targetSearchLocation.transform.position, target.transform.position))){
            if(!tentacle.enabled){
                tentacle.enabled = true;
                tentacle.RecalcPos(_segments);
                StartCoroutine(tentacle.resetFromPlayer());
                //tentacle.ResetPos();
            }
            return;
        }

        //player within our reach!
        if(segmentAmount!=_segments.Length)
        {
            InitializeSegments(); //reinitialize if amount changed
        }
        
        if(tentacle.enabled){
            //Call RecalcSegments with tentacle.segmentposes
            RecalcSegments(tentacle.segmentPoses);
        }

        tentacle.enabled = false;
        targetPosition = target.transform.position;
        Follow();        
        DrawSegments(_segments);
    }
 
    void Follow()
    {
        _segments[segmentAmount-1].Follow(targetPosition, smoothFactor);
        for(int i = segmentAmount-2; i>=0;i--)
        {
            _segments[i].Follow(_segments[i+1], smoothFactor);
        }
 
        if(isFixed)
        {
            _segments[0].AnchorStartAt(anchoredStart.transform.position);
            for(int i = 1 ; i<segmentAmount;i++)
            {
                _segments[i].AnchorStartAt(_segments[i-1].endingPosition);
            }
        }
    }
 
    void DrawSegments(Segment[] segments)
    {
        _lineRenderer.positionCount = segmentAmount+1;
        List<Vector3> points = new List<Vector3>();
        for(int i = 0; i < segmentAmount;i++)
        {
            points.Add(segments[i].startingPosition);
        }
        points.Add(segments[segmentAmount-1].endingPosition);
        _lineRenderer.SetPositions(points.ToArray());
    }

    private void RecalcSegments (Vector3[] poses){
        for(int i = 0; i < poses.Length - 1; i++){
            _segments[i].startingPosition = poses[i];
            _segments[i].endingPosition = poses[i+1];
        }
    }

}
