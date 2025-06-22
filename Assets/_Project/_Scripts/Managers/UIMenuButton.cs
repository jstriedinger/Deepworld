using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIMenuButton : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerClickHandler
{
    private Button _btn;
    private TextMeshProUGUI _btnText;
    private Camera _mainCamera;
    // Start is called before the first frame updat
    
    private void Awake()
    {
        _btnText = GetComponentInChildren<TextMeshProUGUI>();
        _btn = GetComponent<Button>();
    }

    private void Start()
    {
        _mainCamera = Camera.main;
        _btn.onClick.AddListener(OnClickedBtn);
    }

    private void OnClickedBtn()
    {
        AudioManager.Instance?.PlayUIButtonSfx();
    }

    public void OnSelect(BaseEventData eventData)
    {
        _btnText.fontWeight = FontWeight.Bold; 
    }
    public void OnDeselect(BaseEventData eventData)
    {
        _btnText.fontWeight = FontWeight.Regular;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        
    }
}
