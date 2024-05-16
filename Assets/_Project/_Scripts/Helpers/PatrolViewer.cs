using System;
using System.Collections;

using System.Collections.Generic;

using UnityEngine;



[ExecuteAlways]
public class PatrolViewer : MonoBehaviour
{
    private Transform[] patrolPoints = new Transform[1];
    [SerializeField] private Color _color = Color.red;

    // Start is called before the first frame update
    void Awake()
    {
        patrolPoints = GetComponentsInChildren<Transform>();
    }

    // Update is called once per frame

    

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = _color;
        for (int i = 1; i < patrolPoints.Length - 1; i++)
        {
            Transform patrolPoint = patrolPoints[i];
            Transform nextPoint = patrolPoints[i+1];
            Gizmos.DrawLine(patrolPoint.position, nextPoint.position);
        }
    }

    private void OnEnable()
    {
        patrolPoints = GetComponentsInChildren<Transform>();
    }
}

