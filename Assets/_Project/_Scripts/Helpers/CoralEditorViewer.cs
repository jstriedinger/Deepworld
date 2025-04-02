using UnityEngine;

[ExecuteInEditMode]
public class CoralEditorViewer : MonoBehaviour
{
    private CoralLimb _coralLimb;
    LineRenderer _lineRenderer;
    int length;
    Vector3[] segmentPoses;
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

        segmentPoses[0] = transform.position;
        for (int i = 1; i < length; i++)
        {
            segmentPoses[i] = segmentPoses[i - 1] + transform.up * targetDist;
        }
        _lineRenderer.SetPositions(segmentPoses);

    }

    private void Setup()
    {
        if (Application.isEditor && !Application.isPlaying) {
            _lineRenderer = GetComponent<LineRenderer>();
            _coralLimb = GetComponent<CoralLimb>();
           if (_coralLimb)
            {
                length = _coralLimb.length;
                targetDist = _coralLimb.closingDistBetweenPoints;
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
