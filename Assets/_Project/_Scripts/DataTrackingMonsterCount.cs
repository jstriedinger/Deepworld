using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class DataTrackingMonsterCount : MonoBehaviour
{
    private CinemachineTargetGroup targetGroup;
    // Start is called before the first frame update
    void Start()
    {
        targetGroup = GameObject.Find("Target Group").GetComponent<CinemachineTargetGroup>();
    }

    // Update is called once per frame
    void Update()
    {
        int count = 0;
        for(int i = 1; i <= targetGroup.m_Targets.Length; i++){
            if(targetGroup.m_Targets[i].weight > 0){
                count++;
            }
        }
        //MetricManagerScript.instance?.LogString("MonsterCount",count.ToString());
    }
}
