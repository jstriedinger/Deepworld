using System;
using System.Collections;

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Serialization;


public class TentacleDynamic : MonoBehaviour

{

    [FormerlySerializedAs("monsterPlayer")] public PlayerCharacter playerCharacter;

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

    [SerializeField]
    private float changeSpeed;


    public Transform wiggleDir;


    private float _prevWiggleSpeed;
    private float _phase;



    private void Start(){

        lineRend.positionCount = length;

        segmentPoses = new Vector3[length];

        segmentV = new Vector3[length];

        _prevWiggleSpeed = wiggleSpeed;
        _phase = 0;


    }




    private void Update()
    {

        //resetting to positive for calculations
        wiggleSpeed = Mathf.Abs(wiggleSpeed);
        //This script causes the wiggleMagnitude to range between two clamped values depending on the velocity of a character's head
        //This is what differentiates this script from "Tentacle"
        //two stages, either we are in  swim mode or not 
        if (playerCharacter && playerCharacter.swimStage)
        {
            //Debug.Log("Speed before: " + wiggleSpeed);
            //we are swimming, we need to change everything
            //wiggleMagnitude = Mathf.Lerp(wiggleMagnitude, magMax, Time.deltaTime);
            wiggleSpeed = Mathf.Lerp(wiggleSpeed, speedMax, changeSpeed * Time.deltaTime);
            wiggleSpeed = Mathf.Clamp(wiggleSpeed, speedMin, speedMax);
            //Debug.Log("Speed after: " + wiggleSpeed);

        }
        else
        {
            //float wiggleTarget = Mathf.Clamp(head.velocity.magnitude * 3f, magMin, magMax);
            //wiggleMagnitude = Mathf.Lerp(wiggleMagnitude, wiggleTarget, Mathf.Abs(wiggleMagnitude - wiggleTarget)/wiggleMagnitude * 10f);
            //wiggleMagnitude = wiggleTarget;

            //Here we do the same thing for the wiggleSpeed
            //Since speed can be negative for right-side tentacles, we need the absolute value here
            //commented out for debug purposes
            /*wiggleSpeed = Mathf.Abs(wiggleSpeed);
            wiggleSpeed = Mathf.Lerp(wiggleSpeed, speedTarget, Mathf.Abs(wiggleSpeed - speedTarget)/wiggleSpeed * 10f);*/
            float speedTarget = Mathf.Clamp(head.linearVelocity.magnitude / 1.5f, speedMin, speedMax);
            wiggleSpeed = speedTarget;

        }


        if (isRight)
        {
            wiggleSpeed = wiggleSpeed * -1;
        }

        //wiggleSpeed = Mathf.Clamp(wiggleSpeed, speedMin, speedMax);
        //wiggleMagnitude = Mathf.Clamp(wiggleMagnitude, magMin, magMax);

        //This script determines the sway of our tentacle by wiggling from a pivot position
        if(_prevWiggleSpeed != wiggleSpeed)
            CalculatehaseForNewFrequency();
        wiggleDir.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * _prevWiggleSpeed + _phase) * wiggleMagnitude);

        segmentPoses[0] = targetDir.position;

        for (int i = 1; i < segmentPoses.Length; i++)
        {

            segmentPoses[i] = Vector3.SmoothDamp(segmentPoses[i], segmentPoses[i - 1] + targetDir.right * targetDist,
                ref segmentV[i], smoothSpeed);

        }

        lineRend.SetPositions(segmentPoses);

    }
    
    void CalculatehaseForNewFrequency() {
        float curr = (Time.time * _prevWiggleSpeed + _phase) % (2.0f * Mathf.PI);
        float next = (Time.time * wiggleSpeed) % (2.0f * Mathf.PI);
        //Debug.Log("Current phase: "+curr);
        //Debug.Log("Next phase: "+next);
        _phase = curr - next;
        _prevWiggleSpeed = wiggleSpeed;
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

