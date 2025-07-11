using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyTentacle : MonoBehaviour
{

    public int length;
    LineRenderer lineRend;

    public Vector3[] segmentPoses;
    private Vector3[] segmentV;

    public Transform targetDir;
    public float targetDist;
    public float smoothSpeed;


    public float wiggleSpeed;
    public float wiggleMagnitude;
    public Transform wiggleDir;

    public Transform[] bodyParts;
    
    [SerializeField] private int upperBody;
    [SerializeField] private int lowerBody;


    void Awake()
    {
        lineRend = GetComponent<LineRenderer>();
        lineRend.positionCount = length;
        segmentPoses = new Vector3[length];
        segmentV = new Vector3[length];

        ResetPositions();
    }

    
    void Update()
    {
        
        wiggleDir.localRotation = Quaternion.Euler(0,0, Mathf.Sin(Time.time*wiggleSpeed)*wiggleMagnitude);
        segmentPoses[0] = targetDir.position;

        for(int i = 1; i<segmentPoses.Length; i++){
            
            Vector3 targetPos = segmentPoses[i-1] + (segmentPoses[i] - segmentPoses[i-1]).normalized * targetDist;
            segmentPoses[i] = Vector3.SmoothDamp(segmentPoses[i], targetPos, ref segmentV[i], smoothSpeed);
            
        }
        lineRend.SetPositions(segmentPoses);
        
        
    }

    private void FixedUpdate()
    {
        
    }

    private void LateUpdate()
    {
        //Arms and legs
        bodyParts[0].transform.position = segmentPoses[upperBody];
        bodyParts[1].transform.position = segmentPoses[lowerBody];
        
        Vector2 dir = segmentPoses[upperBody - 1] - segmentPoses[upperBody];
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg -90f;
        // Apply rotation (Z-axis rotation)
        Quaternion toRotate = Quaternion.Euler(0, 0, angle);
        //bodyParts[0].rotation = toRotate;
        bodyParts[0].rotation = Quaternion.Lerp(bodyParts[0].transform.rotation, toRotate, 10 * Time.deltaTime);
        //Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        //bodyParts[0].transform.rotation = Quaternion.Slerp(bodyParts[0].transform.rotation, rotation, 10 * Time.deltaTime);
        
        
        //lowerbody rotations
        dir = segmentPoses[lowerBody - 1] - segmentPoses[lowerBody];
        angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg -90f;
        // Apply rotation (Z-axis rotation)
        toRotate = Quaternion.Euler(0, 0, angle);
        //bodyParts[1].rotation = toRotate;
        bodyParts[1].rotation = Quaternion.Lerp(bodyParts[1].transform.rotation, toRotate, 100 * Time.deltaTime);
        //bodyParts[1].rotation = Quaternion.Euler(0, 0, angle);
        //rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        //bodyParts[1].transform.rotation = Quaternion.Slerp(bodyParts[1].transform.rotation, rotation, 10 * Time.deltaTime);

    }

    public void ResetPositions(){
        segmentPoses[0] = targetDir.position;
        for(int i=1; i<length; i++){
            segmentPoses[i] = segmentPoses[i-1] + targetDir.right * targetDist;
            
            //This condition works well for creatures with many repeated features, like armored plates or  many pairs of legs.
            /*if(i%10 == 0){
                bodyParts[i/10 - 1].transform.position = segmentPoses[i];
            }*/

            //These conditions hardcode for body part positions on the playerCharacter's model
            if(i == upperBody){
                bodyParts[0].transform.position = segmentPoses[i];
            }
            else if(i == lowerBody){
                bodyParts[1].transform.position = segmentPoses[i];
            }

        }
        lineRend.SetPositions(segmentPoses);
     //   Debug.Log("finished resetting position");
    }
}
