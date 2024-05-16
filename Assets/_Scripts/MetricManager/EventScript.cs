using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventScript : MonoBehaviour
{
    public void ValueChanged(GameObject obj)
    {
        if (MetricManagerScript.instance != null)
        {
            Toggle toggle = obj.GetComponent<Toggle>();
            Slider slider = obj.GetComponent<Slider>();
            Dropdown dropdown = obj.GetComponent<Dropdown>();
            Button button = obj.GetComponent<Button>();
            InputField input = obj.GetComponent<InputField>();

            // If this event is called by a toggle
            if (toggle)
            {
                // You can change the obj.name as you want e.g "Toggle Value"
                MetricManagerScript.instance.LogString(obj.name, toggle.isOn.ToString());
            }

            // If this event is called by a slider
            if (slider)
            {
                MetricManagerScript.instance.LogString(obj.name, slider.value.ToString());
            }

            // If this event is called by a dropdown
            if (dropdown)
            {
                MetricManagerScript.instance.LogString(obj.name, dropdown.value.ToString());
            }

            // If this event is called by an input
            if (input)
            {
                MetricManagerScript.instance.LogString(obj.name, input.text);
            }

            // If this event is called by a button
            if (button)
            {
                MetricManagerScript.instance.LogString(obj.name, "Clicked");
            }
        }
    }
}
