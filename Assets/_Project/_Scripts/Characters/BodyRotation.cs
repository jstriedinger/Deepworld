using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyRotation : MonoBehaviour
{

    public float speed;
    private Vector2 direction;
    public GameObject target;

    void Start(){
        Rotate();
    }

    void Update()
    {
        Rotate();
    }

    void Rotate(){
        
        //Our direction is the difference between the target's transform and our own
        direction = target.transform.position - transform.position;
        //Determine the angle of our direction Vector2
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        //transform.rotation = rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, speed * Time.deltaTime);
    }
}
