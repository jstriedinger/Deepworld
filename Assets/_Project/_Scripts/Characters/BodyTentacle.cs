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

        ResetPos();
    }

    
    void Update(){

        wiggleDir.localRotation = Quaternion.Euler(0,0, Mathf.Sin(Time.time*wiggleSpeed)*wiggleMagnitude);

        segmentPoses[0] = targetDir.position;

        for(int i = 1; i<segmentPoses.Length; i++){
            Vector3 targetPos = segmentPoses[i-1] + (segmentPoses[i] - segmentPoses[i-1]).normalized * targetDist;
            segmentPoses[i] = Vector3.SmoothDamp(segmentPoses[i], targetPos, ref segmentV[i], smoothSpeed);
            
            //This condition works well for creatures with many repeated features, like armored plates or  many pairs of legs.
            /*
            if(i%10 == 0){
                bodyParts[i/10 - 1].transform.position = segmentPoses[i];
            }*/

            //These conditions hardcode for body part positions on the player's model
            if(i == upperBody){
                bodyParts[0].transform.position = segmentPoses[i];
            }
            else if(i == lowerBody){
                bodyParts[1].transform.position = segmentPoses[i];
            }

            }
        lineRend.SetPositions(segmentPoses);
     //   Debug.Log("first update--body parts are in position");
    }

    private void ResetPos(){
        segmentPoses[0] = targetDir.position;
        for(int i=1; i<length; i++){
            segmentPoses[i] = segmentPoses[i-1] + targetDir.right * targetDist;
            
            //This condition works well for creatures with many repeated features, like armored plates or  many pairs of legs.
            /*if(i%10 == 0){
                bodyParts[i/10 - 1].transform.position = segmentPoses[i];
            }*/

            //These conditions hardcode for body part positions on the player's model
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