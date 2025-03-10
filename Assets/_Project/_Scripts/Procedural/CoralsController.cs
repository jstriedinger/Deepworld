using UnityEngine;

public class CoralsController : MonoBehaviour
{
    [SerializeField] private float smoothSpeed;
    [SerializeField] private float wiggleMagnitude;
    
    private CoralLimb[] _corals;
    private Vector3[][] _coralsSegmentPoses;
    private Vector3[][] _coralsSegmentV;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _corals = GetComponentsInChildren<CoralLimb>();
        _coralsSegmentPoses = new Vector3[_corals.Length][];
        _coralsSegmentV = new Vector3[_corals.Length][];
        
        SetupCorals();
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < _corals.Length; i++)
        {
            CoralLimb t = _corals[i];
            _coralsSegmentPoses[i][0] = t.transform.position;
            for(int j = 1; j < t.length; j++)
            {
                _coralsSegmentPoses[i][j] = Vector3.SmoothDamp(_coralsSegmentPoses[i][j], _coralsSegmentPoses[i][j-1] + 
                    (t.transform.up * t.currentDistBetweenPoints), ref _coralsSegmentV[i][j], 
                    t.currentSpeed);
            }
            t.lineRenderer.SetPositions(_coralsSegmentPoses[i]);
        }
    }
    
    public void SetupCorals()
    {
        for (int i = 0; i < _corals.Length; i++)
        {
            CoralLimb t = _corals[i];
            
            t.lineRenderer.positionCount = t.length;
            _coralsSegmentPoses[i] = new Vector3[t.length];
            _coralsSegmentV[i] = new Vector3[t.length];
            _coralsSegmentPoses[i][0] = t.transform.position;

            for(int j = 1; j < t.length; j++)
            {
                _coralsSegmentPoses[i][j] = _coralsSegmentPoses[i][j-1] + t.transform.up * t.currentDistBetweenPoints;
                //segmentPoses[i] = Vector3.SmoothDamp(segmentPoses[i], segmentPoses[i-1] + targetDir.right * targetDist, ref segmentV[i], 
                //  resettingFromPlayer? 0.15f : smoothSpeed);
            }
            t.lineRenderer.SetPositions(_coralsSegmentPoses[i]);
        }
        
    }

    public void ToggleCorals(bool openGate)
    {
        if (openGate)
        {
            //make tentacles bigger
            foreach (CoralLimb coralLimb in _corals)
            {
                coralLimb.Open();
            }
        }
        else
        {
            //make tentacles bigger
            foreach (CoralLimb coralLimb in _corals)
            {
                coralLimb.Close();
            }
        }
    }
    
}
