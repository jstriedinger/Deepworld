using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateToCenter : MonoBehaviour
{
    public float rotationSpeed;
    private Vector2 direction;

    //public float moveSpeed;
    public Transform Center;

    // Update is called once per frame
    void Update()
    {
        
        direction = Center.transform.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);      


    }
}
