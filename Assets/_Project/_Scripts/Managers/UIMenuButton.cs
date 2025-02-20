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
    // Update is called once per frame
    void Update()
    {
        
    }

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
        if (Time.timeScale == 0)
        {
            Time.timeScale = 1;
            AudioSource.PlayClipAtPoint(AudioManager.Instance.uiButtonSelected, _mainCamera.transform.position, 0.05f);
            Time.timeScale = 0;
        }
        else
            AudioSource.PlayClipAtPoint(AudioManager.Instance.uiButtonSelected, _mainCamera.transform.position, 0.05f);
    }

    public void OnSelect(BaseEventData eventData)
    {
        _btnText.color = Color.black;
        if (Time.timeScale == 0)
        {
            Time.timeScale = 1;
            AudioSource.PlayClipAtPoint(AudioManager.Instance.uiButtonFocus, _mainCamera.transform.position, 0.02f);
            Time.timeScale = 0;
        }
        else
            AudioSource.PlayClipAtPoint(AudioManager.Instance.uiButtonFocus, _mainCamera.transform.position, 0.02f);
    }
    public void OnDeselect(BaseEventData eventData)
    {
        _btnText.color = Color.white;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        
    }
}
