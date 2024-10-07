using System.Collections;

using System.Collections.Generic;

using UnityEngine;



public class EyeTrackerPlayer : MonoBehaviour

{
        //Since the playerCharacter currently cannot blink, this isn't necessary.
    //public EyeManagerPlayer eyeManager;

    public GameObject target = null;

    public GameObject pupil;

    public float offMod;

    public float offMax;





    private float offSetFromCenter = 50f;

    private Vector3 initialPos;

    private Vector3 initialRot;



    void Start()

    {

        //Get and save the initial position of the _pupil.

        

        //_pupil.transform.localPosition;





        //Save the _pupil's initial rotation.

        initialRot = pupil.transform.localEulerAngles;

    }





    void FixedUpdate()

    {
            //This condition will never be met if the playerCharacter cannot blink
        /*
        if(eyeManager.CurrentState == EyeManagerPlayer.MonsterState.Blinking){

            return;

        }*/



        Vector3 targetPos = target.transform.position;

        

        SetOffSet();



        //Code from here down moves the _pupil



        //Redefine the Vector3 of targetPos as the difference in x,y coordinates between this and the target object

        targetPos.x = targetPos.x - this.transform.position.x;

        targetPos.y = targetPos.y - this.transform.position.y;



        //Find the angle of the new target line, then rotate the _pupil in that direction

        float angle = Mathf.Atan2(targetPos.y, targetPos.x) * Mathf.Rad2Deg;



        //transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));





        pupil.transform.localPosition = new Vector3(offSetFromCenter, 0, 0);





        //Maintain the initial rotation value of the _pupil (so it doesn't spin while tracking)

        pupil.transform.localRotation = Quaternion.Euler(initialRot);



    }



    //This function exists outside of FixedUpdate just in case we want to do something that changes how this affects eyeballs vs pupils

    //i.e. if we want the eyeballs to be affected by their distance to the center of the head, in addition to the other modifiers

    void SetOffSet(){

        initialPos = this.transform.position;



        //Here we create a function that determines offset distance

        //The goal is for the eyes + pupils to move in the direction of their target, affected by the (distance between the eye and target), (distance from initialPos), and offMod

        float distTar = Vector3.Distance(pupil.transform.position, target.transform.position);

 /*       float distStart = Vector3.Distance(_pupil.transform.position, initialPos);



        Debug.Log("distTar = " + distTar);

        Debug.Log("distStart = " + distStart);



        if(distStart != 0){

            float temp = (distStart < 1) ? (distTar * distStart): (distTar/distStart) * offMod;

            offSetFromCenter = (temp > offMax) ? offMax : temp;

        }*/



        //offSetFromCenter = distTar/(distStart * offMod);



        //Check whether our offset exceeds our maximum

     /*   if(offSetFromCenter > offMax){

            offSetFromCenter = offMax;

        }*/



        offSetFromCenter = EaseInQuad(0f, offMax, distTar * offMod);

        offSetFromCenter = Mathf.Clamp(offSetFromCenter, 0, offMax);





    }



    private static float EaseInQuad(float start, float end, float value){

            end -= start;

            return end * value * value + start;

    }



}

