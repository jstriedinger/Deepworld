using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class DataTrackingMonsterCount : MonoBehaviour
{
    private CinemachineTargetGroup _targetGroup;
    // Start is called before the first frame update
    void Start()
    {
        _targetGroup = GameObject.Find("Target Group").GetComponent<CinemachineTargetGroup>();
    }

    // Update is called once per frame
    void Update()
    {
        int count = 0;
        for(int i = 1; i <= _targetGroup.Targets.Count; i++){
            if(_targetGroup.Targets[i].Weight > 0){
                count++;
            }
        }
        MetricManagerScript.instance?.LogString("MonsterCount",count.ToString());
    }
}
