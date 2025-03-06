using UnityEngine;

public class GateTentacles : MonoBehaviour
{
    [SerializeField] private float smoothSpeed;
    [SerializeField] private float wiggleMagnitude;
    
    private TentacleInfo[] _tentacles;
    private Vector3[][] _tentaclesSegmentPoses;
    private Vector3[][] _tentaclesSegmentV;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _tentacles = GetComponentsInChildren<TentacleInfo>();
        _tentaclesSegmentPoses = new Vector3[_tentacles.Length][];
        _tentaclesSegmentV = new Vector3[_tentacles.Length][];
        
        SetupTentacles();
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < _tentacles.Length; i++)
        {
            TentacleInfo t = _tentacles[i];
            t.wiggleDir.localRotation = Quaternion.Euler(0,0,Mathf.Sin(Time.time * t.minWiggleSpeed) * wiggleMagnitude);
            _tentaclesSegmentPoses[i][0] = t.targetDir.position;
            for(int j = 1; j < t.tentacleLength; j++)
            {
                _tentaclesSegmentPoses[i][j] = Vector3.SmoothDamp(_tentaclesSegmentPoses[i][j], _tentaclesSegmentPoses[i][j-1] + 
                    (t.targetDir.right * t.currentPointGap), ref _tentaclesSegmentV[i][j], 
                    smoothSpeed);
            }
            t.lineRenderer.SetPositions(_tentaclesSegmentPoses[i]);
        }
    }
    
    public void SetupTentacles()
    {
        for (int i = 0; i < _tentacles.Length; i++)
        {
            TentacleInfo t = _tentacles[i];
            
            t.lineRenderer.positionCount = t.tentacleLength;
            _tentaclesSegmentPoses[i] = new Vector3[t.tentacleLength];
            _tentaclesSegmentV[i] = new Vector3[t.tentacleLength];
            _tentaclesSegmentPoses[i][0] = t.targetDir.position;

            for(int j = 1; j < t.tentacleLength; j++)
            {
                _tentaclesSegmentPoses[i][j] = _tentaclesSegmentPoses[i][j-1] + t.targetDir.right * t.currentPointGap;
                //segmentPoses[i] = Vector3.SmoothDamp(segmentPoses[i], segmentPoses[i-1] + targetDir.right * targetDist, ref segmentV[i], 
                //  resettingFromPlayer? 0.15f : smoothSpeed);
            }
            t.lineRenderer.SetPositions(_tentaclesSegmentPoses[i]);
        }
        
    }

    public void ToggleSwitchTentacles(bool openGate)
    {
        if (openGate)
        {
            //make tentacles bigger
            for (int i = 0; i < _tentacles.Length; i++)
            {
                TentacleInfo t = _tentacles[i];
                t.currentPointGap = t.minPointGap + 0.15f;
            }
        }
        else
        {
            for (int i = 0; i < _tentacles.Length; i++)
            {
                TentacleInfo t = _tentacles[i];
                t.currentPointGap = t.minPointGap;
            }
        }
    }

    public void ToggleGateTentacles(bool openGate)
    {
        if (openGate)
        {
            //make tentacles bigger
            for (int i = 0; i < _tentacles.Length; i++)
            {
                TentacleInfo t = _tentacles[i];
                t.currentPointGap = 0.075f;
            }
        }
        else
        {
            for (int i = 0; i < _tentacles.Length; i++)
            {
                TentacleInfo t = _tentacles[i];
                t.currentPointGap = t.minPointGap;
            }
        }
    }
    
    
}
