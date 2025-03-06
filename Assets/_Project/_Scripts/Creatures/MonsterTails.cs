using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class MonsterTails : MonoBehaviour
{
    [SerializeField] private Transform idleCenterPivot;
    [SerializeField] private Transform huntCenterPivot;
    private Transform _currentPivot;
    [SerializeField] private AIPath aiPath;
    [SerializeField] private float rotateToCenterSpeed;
    [FormerlySerializedAs("randomDistStep")] [SerializeField] private float extraDistBetweenPoints;
    [FormerlySerializedAs("randomWiggleStep")] [SerializeField] private float extraWiggleSpeed;
    [FormerlySerializedAs("randomLengthStep")] [SerializeField] private int extraLength;
    
    public float smoothSpeed;
    //Wiggle magnitude reduce by 5 and 10 in Follow and Chase
    [SerializeField] private float idleWiggleMagnitude;
    
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
        SetupTentacles();
    }

    private void FixedUpdate()
    {
        if (aiPath.velocity.magnitude > 0)
        {
            _currentPivot = huntCenterPivot;
        }
        else
        {
            _currentPivot = idleCenterPivot;
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < _tentacles.Length; i++)
        {
            TentacleInfo t = _tentacles[i];
            t.wiggleDir.localRotation = Quaternion.Euler(0,0,Mathf.Sin(Time.time * t.currentWiggleSpeed) * idleWiggleMagnitude);
            _tentaclesSegmentPoses[i][0] = t.targetDir.position;
            for(int j = 1; j < t.tentacleLength; j++)
            {
                _tentaclesSegmentPoses[i][j] = Vector3.SmoothDamp(_tentaclesSegmentPoses[i][j], _tentaclesSegmentPoses[i][j-1] + 
                    (t.targetDir.right * (aiPath.velocity.magnitude > Mathf.Epsilon ? t.minPointGap : t.minPointGap + 0.1f) ), ref _tentaclesSegmentV[i][j], 
                smoothSpeed);
            }
            t.lineRenderer.SetPositions(_tentaclesSegmentPoses[i]);
            //Rotate tail to center
            Vector2 direction = _currentPivot.transform.position - t.transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            t.transform.rotation = Quaternion.Slerp(t.transform.rotation, rotation, rotateToCenterSpeed * Time.deltaTime);      

        }

    }

    public void SetupTentacles()
    {
        for (int i = 0; i < _tentacles.Length; i++)
        {
            TentacleInfo t = _tentacles[i];
            
            if (t.randomize)
            {
                t.SetupMonsterTentacle(extraDistBetweenPoints, extraWiggleSpeed, extraLength);
            }
            t.lineRenderer.positionCount = t.tentacleLength;
            _tentaclesSegmentPoses[i] = new Vector3[t.tentacleLength];
            _tentaclesSegmentV[i] = new Vector3[t.tentacleLength];
            _tentaclesSegmentPoses[i][0] = t.targetDir.position;

            for(int j = 1; j < t.tentacleLength; j++)
            {
                _tentaclesSegmentPoses[i][j] = _tentaclesSegmentPoses[i][j-1] + t.targetDir.right * t.minPointGap;
                //segmentPoses[i] = Vector3.SmoothDamp(segmentPoses[i], segmentPoses[i-1] + targetDir.right * targetDist, ref segmentV[i], 
                //  resettingFromPlayer? 0.15f : smoothSpeed);
            }
            t.lineRenderer.SetPositions(_tentaclesSegmentPoses[i]);
        }
        
    }
    
   
    
    public void OnUpdateMonsterState(MonsterState monsterState)
    {
        switch (monsterState)
        {
            case MonsterState.Chasing:
                for (int i = 0; i < _tentacles.Length; i++)
                {
                    TentacleInfo t = _tentacles[i];
                    t.currentWiggleSpeed = t.minWiggleSpeed + 2;
                }
                break;
            case MonsterState.Frustrated:
                for (int i = 0; i < _tentacles.Length; i++)
                {
                    TentacleInfo t = _tentacles[i];
                    t.currentWiggleSpeed = t.minWiggleSpeed + 2f;
                }
                break;
            case MonsterState.Follow:
                for (int i = 0; i < _tentacles.Length; i++)
                {
                    TentacleInfo t = _tentacles[i];
                    t.currentWiggleSpeed = t.minWiggleSpeed + 1f;
                }
                break;
            case MonsterState.Investigate:
                for (int i = 0; i < _tentacles.Length; i++)
                {
                    TentacleInfo t = _tentacles[i];
                    t.currentWiggleSpeed = t.minWiggleSpeed + 1f;
                }
                break;
            default:
                for (int i = 0; i < _tentacles.Length; i++)
                {
                    TentacleInfo t = _tentacles[i];
                    t.currentWiggleSpeed = t.minWiggleSpeed;
                }

                break;
        }
        
    }
    
    
    
    
}
