using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimedScript : MonoBehaviour
{
    Scrollbar bar;
    void Start()
    {
        MetricsEvents.OnDataCollect += this.CollectData;
        bar = GetComponentInChildren<Scrollbar>();
    }
    public void CollectData()
    {
        //Debug.Log(bar.value);
        if (MetricManagerScript.instance != null)
        {
            // You can change the bar.name as you want e.g "Scrollbar Value"
            MetricManagerScript.instance.LogString(bar.name, bar.value.ToString());
        }
    }
}
