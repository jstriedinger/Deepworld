using System.Collections;

using System.Collections.Generic;

using UnityEngine;



public class TentacleDynamic : MonoBehaviour

{



    public int length;

    public Rigidbody2D head;
    public LineRenderer lineRend;

    public Vector3[] segmentPoses;

    private Vector3[] segmentV;

    public Transform targetDir;

    public float targetDist;

    public float smoothSpeed;



    public float wiggleSpeed;

    public float wiggleMagnitude;

    [SerializeField] private float magMin;
    [SerializeField] private float magMax;


    [SerializeField] private float speedMin;
    [SerializeField] private float speedMax;

    [SerializeField] private bool isRight = false;


    public Transform wiggleDir;



    private void Start(){

        lineRend.positionCount = length;

        segmentPoses = new Vector3[length];

        segmentV = new Vector3[length];

        
    }



    private void Update(){

        //This script causes the wiggleMagnitude to range between two clamped values depending on the velocity of a character's head
        //This is what differentiates this script from "Tentacle"
        float wiggleTarget = Mathf.Clamp(head.velocity.magnitude * 5f, magMin, magMax);
        //wiggleMagnitude = Mathf.Lerp(wiggleMagnitude, wiggleTarget, Mathf.Abs(wiggleMagnitude - wiggleTarget)/wiggleMagnitude * 10f);

        wiggleMagnitude = wiggleTarget;

        //Here we do the same thing for the wiggleSpeed
        //float speedTarget = Mathf.Clamp(head.velocity.magnitude, speedMin, speedMax);
        //Since speed can be negative for right-side tentacles, we need the absolute value here
        //commented out for debug purposes
        /*wiggleSpeed = Mathf.Abs(wiggleSpeed);
        wiggleSpeed = Mathf.Lerp(wiggleSpeed, speedTarget, Mathf.Abs(wiggleSpeed - speedTarget)/wiggleSpeed * 10f);*/
        
        if(head.velocity.magnitude >= speedMax)
            wiggleSpeed = speedMax;
        else
            wiggleSpeed = speedMin;
        if(isRight){
            wiggleSpeed = wiggleSpeed * -1;
        }


        //This script determines the sway of our tentacle by wiggling from a pivot position
        wiggleDir.localRotation = Quaternion.Euler(0,0,Mathf.Sin(Time.time * wiggleSpeed) * wiggleMagnitude);



        segmentPoses[0] = targetDir.position;



        for(int i = 1; i < segmentPoses.Length; i++){

            segmentPoses[i] = Vector3.SmoothDamp(segmentPoses[i], segmentPoses[i-1] + targetDir.right * targetDist, ref segmentV[i], smoothSpeed);

        }

        lineRend.SetPositions(segmentPoses);

    }


    public void ResetPositions()
    {
        segmentPoses[0] = targetDir.position;
        for(int i = 1; i < length; i++){
            segmentPoses[i] = segmentPoses[i-1] + targetDir.right * targetDist;
        }
        lineRend.SetPositions(segmentPoses);
    }

}

