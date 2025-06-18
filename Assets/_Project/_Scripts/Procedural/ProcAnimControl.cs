using System;
using UnityEngine;
using UnityEngine.Serialization;

public class ProcAnimControl : MonoBehaviour
{
    [Header("Body")]
    [SerializeField] private BodyTentacleInfo bodyTentacleInfo;
    [SerializeField] private float bodyWiggleMagnitude;
    [SerializeField] private float bodyWiggleSpeed;
    [SerializeField] private float bodySmoothSpeed;
    [SerializeField] private bool randomizeBody = false;
    [SerializeField] private float bodyRandomPointGap;
    //forbody tentacle
    private Vector3[] _bodyTentaclesSegmentPoses;
    private Vector2[] _bodyTentaclesSegmentV;
    
    [Header("Limbs")]
    [SerializeField] private float limbWiggleMagnitude;
    [SerializeField] private float limbSmoothSpeed;
    [SerializeField] private bool randomizeLimbs = false;
    [SerializeField] private float extraPointGap;
    [SerializeField] private float extraWiggleSpeed;
    [SerializeField] private int extraLength;
    
    //Wiggle magnitude reduce by 5 and 10 in Follow and Chase
    
    private TentacleInfo[] _tentacles;
    private Vector3[][] _tentaclesSegmentPoses;
    private Vector3[][] _tentaclesSegmentV;

    private void Awake()
    {
        _tentacles = GetComponentsInChildren<TentacleInfo>();
        _tentaclesSegmentPoses = new Vector3[_tentacles.Length][];
        _tentaclesSegmentV = new Vector3[_tentacles.Length][];
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //start body tentacle
        SetupBodyTentacle();
        SetupTentacles();
        
        
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < _tentacles.Length; i++)
        {
            TentacleInfo t = _tentacles[i];
            t.wiggleDir.localRotation = Quaternion.Euler(0,0,Mathf.Sin(Time.time * t.currentWiggleSpeed) * limbWiggleMagnitude);
            _tentaclesSegmentPoses[i][0] = t.targetDir.position;
            for(int j = 1; j < t.tentacleLength; j++)
            {
                _tentaclesSegmentPoses[i][j] = Vector3.SmoothDamp(_tentaclesSegmentPoses[i][j], _tentaclesSegmentPoses[i][j-1] + 
                    t.targetDir.right * t.minPointGap  , ref _tentaclesSegmentV[i][j], 
                    limbSmoothSpeed);
            }
            t.lineRenderer.SetPositions(_tentaclesSegmentPoses[i]);
        }
        
        //body tentacle
        _bodyTentaclesSegmentPoses[0] = bodyTentacleInfo.targetDir.position;
        
        bodyTentacleInfo.wiggleDir.localRotation = Quaternion.Euler(0,0,Mathf.Sin(Time.time * bodyWiggleSpeed) * bodyWiggleMagnitude);
        _bodyTentaclesSegmentPoses[0] = bodyTentacleInfo.targetDir.position;
        for(int i = 1; i<_bodyTentaclesSegmentPoses.Length; i++){
            
            _bodyTentaclesSegmentPoses[i] = Vector2.SmoothDamp(_bodyTentaclesSegmentPoses[i], _bodyTentaclesSegmentPoses[i-1] + 
                bodyTentacleInfo.targetDir.right * bodyTentacleInfo.pointGap  , ref _bodyTentaclesSegmentV[i], 
                bodySmoothSpeed);
            
            
        }
        bodyTentacleInfo.lineRenderer.SetPositions(_bodyTentaclesSegmentPoses);
    }

    private void LateUpdate()
    {
       PositionBodyTentacleParts();
    }

    private void PositionBodyTentacleParts()
    {
        foreach (TentacleBodyPart tentacleBodyPart in bodyTentacleInfo.bodyParts)
        {
            tentacleBodyPart.bodyPart.transform.position = _bodyTentaclesSegmentPoses[tentacleBodyPart.bodyPos];
            Vector2 dir = _bodyTentaclesSegmentPoses[tentacleBodyPart.bodyPos - 1] - _bodyTentaclesSegmentPoses[tentacleBodyPart.bodyPos];
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg -90f;
            // Apply rotation (Z-axis rotation)
            Quaternion toRotate = Quaternion.Euler(0, 0, angle);
            tentacleBodyPart.bodyPart.transform.rotation = Quaternion.Lerp(tentacleBodyPart.bodyPart.transform.rotation, toRotate, 10 * Time.deltaTime);

        }
    }

    //Setup the body tentacle
    private void SetupBodyTentacle()
    {
        bodyTentacleInfo.lineRenderer.positionCount = bodyTentacleInfo.tentacleLength;
        _bodyTentaclesSegmentPoses = new Vector3[bodyTentacleInfo.tentacleLength];
        _bodyTentaclesSegmentPoses[0] = bodyTentacleInfo.targetDir.position;
        _bodyTentaclesSegmentV = new Vector2[bodyTentacleInfo.tentacleLength];
        
        if(randomizeBody)
            bodyTentacleInfo.SetupRandomValues(bodyRandomPointGap);

        for(int j = 1; j < bodyTentacleInfo.tentacleLength; j++)
        {
            _bodyTentaclesSegmentPoses[j] = _bodyTentaclesSegmentPoses[j-1] + bodyTentacleInfo.targetDir.right * bodyTentacleInfo.pointGap;
        }
        //limbs
        foreach (TentacleBodyPart tentacleBodyPart in bodyTentacleInfo.bodyParts)
        {
            tentacleBodyPart.bodyPart.transform.position = _bodyTentaclesSegmentPoses[tentacleBodyPart.bodyPos];
        }
        bodyTentacleInfo.lineRenderer.SetPositions(_bodyTentaclesSegmentPoses);
    }
    
    //Setup all tentacles that it founds
    public void SetupTentacles()
    {
        for (int i = 0; i < _tentacles.Length; i++)
        {
            TentacleInfo t = _tentacles[i];
            
            if (randomizeLimbs)
            {
                t.SetupMonsterTentacle(extraPointGap, extraWiggleSpeed, extraLength);
            }
            t.lineRenderer.positionCount = t.tentacleLength;
            _tentaclesSegmentPoses[i] = new Vector3[t.tentacleLength];
            _tentaclesSegmentV[i] = new Vector3[t.tentacleLength];
            _tentaclesSegmentPoses[i][0] = t.targetDir.position;

            for(int j = 1; j < t.tentacleLength; j++)
            {
                _tentaclesSegmentPoses[i][j] = _tentaclesSegmentPoses[i][j-1] + t.targetDir.right * t.minPointGap;
            }
            t.lineRenderer.SetPositions(_tentaclesSegmentPoses[i]);
        }
        
    }
    
    public void ResetPositions(){
        _bodyTentaclesSegmentPoses[0] = bodyTentacleInfo.targetDir.position;
        for(int i=1; i<bodyTentacleInfo.tentacleLength; i++){
            _bodyTentaclesSegmentPoses[i] = _bodyTentaclesSegmentPoses[i-1] + bodyTentacleInfo.targetDir.right * bodyTentacleInfo.pointGap;
        }
        //limbs
        foreach (TentacleBodyPart tentacleBodyPart in bodyTentacleInfo.bodyParts)
        {
            tentacleBodyPart.bodyPart.transform.position = _bodyTentaclesSegmentPoses[tentacleBodyPart.bodyPos];
        }
        bodyTentacleInfo.lineRenderer.SetPositions(_bodyTentaclesSegmentPoses);
        //   Debug.Log("finished resetting position");
    }
}
