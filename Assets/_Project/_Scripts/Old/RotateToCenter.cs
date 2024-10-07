using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class RotateToCenter : MonoBehaviour
{
    public float rotationSpeed;
    private Vector2 direction;

    //public float moveSpeed;
    [FormerlySerializedAs("Center")] public Transform center;
    private MonsterBase monsterBase;

    private void Awake()
    {
        monsterBase = GetComponentInParent<MonsterBase>();
    }

    // Update is called once per frame
    void Update()
    {
        direction = center.transform.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);      

    }
}
