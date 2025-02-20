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
    public float segmentsLength = 1;
    public GameObject targetSearchLocation;
    public float distTrigger;
    public float smoothFactor;
    
    Segment[] _segments;
    LineRenderer _lineRenderer;
    private int _segmentAmount = 10; 
    private Tentacle _tentacle;
    private LayerMask _layerMask;
    private bool _onlyCheckPlayer = false;
    
 
    void Start()
    {
        _tentacle = GetComponent<Tentacle>();
        InitializeSegments();
    }

    public void Setup(LayerMask layerMask, Transform targetP = null)
    {
        _layerMask = layerMask;
        if (targetP)
        {
            target = targetP;
            _onlyCheckPlayer = true;
        }
    }
 
    void InitializeSegments()
    {
        
        _lineRenderer = this.GetComponent<LineRenderer>();
        _segmentAmount = _tentacle.length;
        _segments = new Segment[_segmentAmount];
        
        for(int i = 0; i<_segmentAmount;i++)
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
        if (_onlyCheckPlayer && target)
        {
            
            //Check if the playerCharacter is within our target distance
            //If not, skip eveything else in this script
        
            if(distTrigger < Math.Abs(Vector3.Distance(targetSearchLocation.transform.position, target.transform.position))){
                if(!_tentacle.enabled){
                    _tentacle.enabled = true;
                    _tentacle.RecalcPos(_segments);
                    StartCoroutine(_tentacle.resetFromPlayer());
                    //tentacle.ResetPos();
                }
                return;
            }
        }
        else
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position,distTrigger, _layerMask.value);
            if (!hit)
            {
                //Debug.Log("No Hit");
                if(!_tentacle.enabled){
                    _tentacle.enabled = true;
                    _tentacle.RecalcPos(_segments);
                    StartCoroutine(_tentacle.resetFromPlayer());
                    //tentacle.ResetPos();
                }

                return;
            }
            target = hit.transform;
        }
        

        //playerCharacter within our reach!
        if(_segmentAmount!=_segments.Length)
        {
            InitializeSegments(); //reinitialize if amount changed
        }
        
        if(_tentacle.enabled){
            //Call RecalcSegments with tentacle.segmentposes
            RecalcSegments(_tentacle.segmentPoses);
        }

        _tentacle.enabled = false;
        targetPosition = target.transform.position;
        Follow();        
        DrawSegments(_segments);
    }
 
    void Follow()
    {
        _segments[_segmentAmount-1].Follow(targetPosition, smoothFactor);
        for(int i = _segmentAmount-2; i>=0;i--)
        {
            _segments[i].Follow(_segments[i+1], smoothFactor);
        }
 
        if(isFixed)
        {
            _segments[0].AnchorStartAt(anchoredStart.transform.position);
            for(int i = 1 ; i<_segmentAmount;i++)
            {
                _segments[i].AnchorStartAt(_segments[i-1].endingPosition);
            }
        }
    }
 
    void DrawSegments(Segment[] segments)
    {
        _lineRenderer.positionCount = _segmentAmount+1;
        List<Vector3> points = new List<Vector3>();
        for(int i = 0; i < _segmentAmount;i++)
        {
            points.Add(segments[i].startingPosition);
        }
        points.Add(segments[_segmentAmount-1].endingPosition);
        _lineRenderer.SetPositions(points.ToArray());
    }

    private void RecalcSegments (Vector3[] poses){
        for(int i = 0; i < poses.Length - 1; i++){
            _segments[i].startingPosition = poses[i];
            _segments[i].endingPosition = poses[i+1];
        }
    }

    private void OnDrawGizmosSelected()
    {
        //Gizmos.color = Color.red;
        //Gizmos.DrawWireSphere(transform.position,distTrigger);
        
    }
}
