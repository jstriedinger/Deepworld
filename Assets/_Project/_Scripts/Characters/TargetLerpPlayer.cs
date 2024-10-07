using System.Collections;

using System.Collections.Generic;

using UnityEngine;

using Cinemachine;


public class TargetLerpPlayer : MonoBehaviour

{


        //since players can't blink right now, we only use eyeManager as an anchor point
    public EyeManagerPlayer eyeManager;

    [SerializeField] CinemachineTargetGroup targetGroup;

    public Vector3 idealTarget;

    public float smoothTime;

    private Vector3 velocity = Vector3.zero;

    private int targetNum;



    //TargetLerp is supposed to be attached to the eye's target objects.

    void Update()

    {
        /*
        //NOTE: This method of identifying an ideal target only worked when objects were being removed from the targetGroup array.
            //We only need to know whether there is an ideal target
        if(targetGroup.m_Targets.Length >= 2)
        {
            
            idealTarget = targetGroup.m_Targets[1].target.transform.position;
                //Basically: Smoothdamp this target towards the ideal target
            transform.position = Vector3.SmoothDamp(transform.position, idealTarget, ref velocity, smoothTime);

        }*/
        if (targetGroup)
        {
            for(int i = 0; i < targetGroup.m_Targets.Length; i++){
                if(targetGroup.m_Targets[i].weight > 0){
                    targetNum = i;
                    //Debug.Log("Target Identified!");
                }
            }

            if(targetNum > 0){
                idealTarget = targetGroup.m_Targets[targetNum].target.transform.position;
                transform.position = Vector3.SmoothDamp(transform.position, idealTarget, ref velocity, smoothTime);
            }
            else{
                
                //Debug.Log("No current eyeTarget for playerCharacter.");
                //Debug.Log("eyeManager localPosition = " + eyeManager.transform.localPosition);
                //Basically: Smoothdamp towards the default "forward" position
                transform.position = Vector3.SmoothDamp(transform.position, eyeManager.transform.TransformPoint(eyeManager.transform.localPosition + new Vector3(0, 10, 0)), ref velocity, smoothTime);
                
            }
            //In theory this is redundant, but it's here to make sure nothing blows up.
            targetNum = 0;
            
        }
        

    }

}

