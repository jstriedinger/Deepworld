using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TentacleTest : MonoBehaviour
{
    [SerializeField] private int numVertices;
    [SerializeField] private Vector3[] vertices;
    [SerializeField] private Transform targetDir;
    [SerializeField] private float vertDistance;
    [SerializeField] private float smoothSpeed;
    [SerializeField] private float trailSpeed;

    private LineRenderer lineRenderer;
    private Vector3[] verticesSpeed;

    // Start is called before the first frame update
    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }
    void Start()
    {
        lineRenderer.positionCount = numVertices;
        vertices = new Vector3[numVertices];
        verticesSpeed = new Vector3[numVertices];
    }

    // Update is called once per frame
    void Update()
    {
        vertices[0] = targetDir.position;

        for (int i = 1; i < numVertices; i++)
        {
            vertices[i] = Vector3.SmoothDamp(vertices[i], vertices[i-1] + (targetDir.right * vertDistance), ref verticesSpeed[i], smoothSpeed + i/trailSpeed);
        }
        lineRenderer.SetPositions(vertices);
    }
}
