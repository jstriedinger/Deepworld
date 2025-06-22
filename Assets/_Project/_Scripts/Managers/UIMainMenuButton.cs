using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIMainMenuButton : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerClickHandler
{
    private Button _btn;
    private RectTransform _rectTransform;
    private TextMeshProUGUI _btnText;
    private string _txt;
    private float _txtSize;
    private Camera _mainCamera;

    private Color _deselectColor = new Color(1,1,1,0.2f);
    // Start is called before the first frame updat
    
    private void Awake()
    {
        _btnText = GetComponentInChildren<TextMeshProUGUI>();
        _btn = GetComponent<Button>();
        _rectTransform = GetComponent<RectTransform>();
        _txt = _btnText.text;
        _txtSize = _btnText.fontSize;
    }

    private void Start()
    {
        _mainCamera = Camera.main;
        _btn.onClick.AddListener(OnClickedBtn);
        
    }

    private void OnClickedBtn()
    {
        AudioManager.Instance?.PlayUIButtonSfx();
        //call flock 
        
    }

    public void OnSelect(BaseEventData eventData)
    {
        _btnText.fontStyle = FontStyles.Bold | FontStyles.UpperCase;
        _btnText.fontSize = _txtSize + 16;
        _btnText.color = Color.white; 

        Vector3[] corners = new Vector3[4];
        _rectTransform.GetWorldCorners(corners); // [0]=bottom left, [2]=top right
        // Get world center between bottom-left and top-right corners
        Vector3 centerScreen = (corners[0] + corners[2]) * 0.5f;
        float zDistance = Mathf.Abs(_mainCamera.transform.position.z); // usually 10 in 2D
        Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(centerScreen.x, centerScreen.y, zDistance));
        worldPos.z = 0f;
        UIManager.Instance?.ChangeMenuFlockPosition(worldPos);
    }
    public void OnDeselect(BaseEventData eventData)
    {
        _btnText.fontStyle = FontStyles.Normal | FontStyles.UpperCase;
        _btnText.fontSize = _txtSize;
        _btnText.color = _deselectColor;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        
    }
}
