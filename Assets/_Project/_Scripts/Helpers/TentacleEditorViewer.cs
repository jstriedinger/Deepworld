using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static UnityEngine.Rendering.HableCurve;
using UnityEngine.UIElements;

[ExecuteInEditMode]
public class TentacleEditorViewer : MonoBehaviour
{

    private Tentacle _tentacle;
    private Light2D _light2D;
    private BodyTentacle _bodyTentacle;
    private Dangler _dangler;
    LineRenderer _lineRenderer;
    int length;
    Vector3[] segmentPoses;
    Transform targetDir;
    float targetDist;

#if UNITY_EDITOR

    // Start is called before the first frame update
    void Start()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            ResetPositions();
        }

    }

    private void ResetPositions()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = length;
        segmentPoses = new Vector3[length];

        segmentPoses[0] = targetDir.position;
        for (int i = 1; i < length; i++)
        {
            segmentPoses[i] = segmentPoses[i - 1] + targetDir.right * targetDist;
        }
        _lineRenderer.SetPositions(segmentPoses);

        if (_light2D)
            _light2D.transform.parent.transform.position = segmentPoses[^1];
    }

    private void Setup()
    {
        if (Application.isEditor && !Application.isPlaying) {
            _lineRenderer = GetComponent<LineRenderer>();
            _tentacle = GetComponent<Tentacle>();
            _bodyTentacle = GetComponent<BodyTentacle>();
            _dangler = GetComponent<Dangler>();
            if (_tentacle)
            {
                length = _tentacle.length;
                targetDir = _tentacle.targetDir;
                targetDist = _tentacle.targetDist;
                _light2D = _tentacle.GetInternalLight();


            }
            else if(_bodyTentacle)
            {
                length = _bodyTentacle.length;
                targetDir = _bodyTentacle.targetDir;
                targetDist = _bodyTentacle.targetDist;
                
            }
            else if (_dangler)
            {
                length = _dangler.length;
                targetDir = _dangler.targetDir;
                targetDist = _dangler.targetDist;
            }
        }
    }

    private void OnEnable()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            Setup();
            ResetPositions();
        }

    }
#endif
   
}
