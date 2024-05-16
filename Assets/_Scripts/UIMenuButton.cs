using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIMenuButton : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    private Button _btn;
    private TextMeshProUGUI _btnText;
    // Start is called before the first frame updat
    // Update is called once per frame
    void Update()
    {
        
    }

    private void Awake()
    {
        _btnText = GetComponentInChildren<TextMeshProUGUI>();
        Debug.Log(_btnText.ToString());
    }

    public void OnSelect(BaseEventData eventData)
    {
        _btnText.color = Color.black;
    }
    public void OnDeselect(BaseEventData eventData)
    {
        _btnText.color = Color.white;
    }
}
