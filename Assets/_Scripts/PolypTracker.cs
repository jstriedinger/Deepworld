using System;
using System.Collections.Generic;
using UnityEngine;
 
[RequireComponent(typeof(LineRenderer))]
public class PolypTracker : MonoBehaviour
{
    public Vector3 targetPosition;
    public bool isFixed;
    public GameObject anchoredStart;
    public int segmentAmount = 10; 
    public float segmentsLength = 1;
    Segment[] segments;
    LineRenderer lineRenderer;
    public GameObject targetSearchLocation;
    private GameObject player;
    public float distTrigger;
    public float smoothFactor;
    private Tentacle tentacle;
 
    void Awake()
    {
        InitializeSegments();
    }
 
    void InitializeSegments()
    {
        player = GameObject.Find("Player");
        lineRenderer = this.GetComponent<LineRenderer>();
        segments = new Segment[segmentAmount];
        for(int i = 0; i<segmentAmount;i++)
        {
            Segment segment = new Segment();
            //segment.startingPosition = Vector3.zero+(Vector3.up*segmentsLength*(i));
            segment.startingPosition = anchoredStart.transform.position+(anchoredStart.transform.up*segmentsLength*(i));
            segment.endingPosition = segment.startingPosition+(anchoredStart.transform.up*segmentsLength*(i));
            segment.length = segmentsLength;
            segments[i] = segment;
        }
    }
 
    void Update()
    {
        tentacle = GetComponent<Tentacle>();



        //Check if the player is within our target distance
        //If not, skip eveything else in this script
        if(distTrigger < Math.Abs(Vector3.Distance(targetSearchLocation.transform.position, player.transform.position))){
            if(tentacle.enabled == false){
                tentacle.RecalcPos(segments);
            }
            tentacle.enabled = true;
            return;
        }

        if(segmentAmount!=segments.Length)
        {
            InitializeSegments(); //reinitialize if amount changed
        }
        
        /*if(distTrigger >= Math.Abs(Vector3.Distance(this.transform.position, player)))
        {
          //targetPosition = GetWorldPositionFromMouse();
          
        }*/

        if(tentacle.enabled == true){
            //Call RecalcSegments with tentacle.segmentposes
            RecalcSegments(tentacle.segmentPoses);
        }

        tentacle.enabled = false;
        targetPosition = player.transform.position;
        Follow();        
        DrawSegments(segments);
    }
 
    void Follow()
    {
        segments[segmentAmount-1].Follow(targetPosition, smoothFactor);
        for(int i = segmentAmount-2; i>=0;i--)
        {
            segments[i].Follow(segments[i+1], smoothFactor);
        }
 
        if(isFixed)
        {
            segments[0].AnchorStartAt(anchoredStart.transform.position);
            for(int i = 1 ; i<segmentAmount;i++)
            {
                segments[i].AnchorStartAt(segments[i-1].endingPosition);
            }
        }
    }
 
    void DrawSegments(Segment[] segments)
    {
        lineRenderer.positionCount = segmentAmount+1;
        List<Vector3> points = new List<Vector3>();
        for(int i = 0; i < segmentAmount;i++)
        {
            points.Add(segments[i].startingPosition);
        }
        points.Add(segments[segmentAmount-1].endingPosition);
        lineRenderer.SetPositions(points.ToArray());
    }
 
/*    Vector3 GetWorldPositionFromMouse()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        return mousePos;
    }*/

    private void RecalcSegments (Vector3[] poses){
        for(int i = 0; i < poses.Length - 1; i++){
            segments[i].startingPosition = poses[i];
            segments[i].endingPosition = poses[i+1];
        }
    }

}
