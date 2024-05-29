using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static event Action OnStartGame;

    [Header("Pause Menu")]
    public CanvasGroup blackout;
    [SerializeField] private CanvasGroup pauseGroup;
    [Header("Main Menu")]
    [SerializeField] private SpriteRenderer logoSprite;
    [SerializeField] private SpriteRenderer creditsSprite;
    [SerializeField] private GameObject mainMenuGroup;
    [SerializeField] private CanvasGroup mainMenu;
    [SerializeField] private CanvasGroup mainMenuBack;
    [SerializeField] private Button pauseContinueBtn;
    private Button _mainMenuBackBtn;
    private Button _mainMenuStartBtn;
    [HideInInspector]
    public bool isPauseFading = false;

    //references to other managers
    private Level1Manager _level1Manager;

    private void Awake()
    {
        //UI stuff
        
        blackout.gameObject.SetActive(true);
        pauseGroup.gameObject.SetActive(false);
        mainMenuBack.gameObject.SetActive(false);
        mainMenu.gameObject.SetActive(true);
        pauseGroup.alpha = mainMenu.alpha = mainMenuBack.alpha = 0;
        _mainMenuBackBtn = mainMenuBack.GetComponentInChildren<Button>();
        _mainMenuStartBtn = mainMenu.GetComponentInChildren<Button>();
        
        

    }

    // Start is called before the first frame update
    void Start()
    {
        //regardless of what happens, we start with a smooth fadeout
        blackout.DOFade(0, 3).SetEase(Ease.InQuad);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PauseGame(bool toggle)
    {
        if (toggle)
        {
            isPauseFading = true;
            pauseGroup.gameObject.SetActive(true);
            pauseGroup.DOFade(1, 0.2f).OnComplete(()=>{
                Time.timeScale = 0;
                isPauseFading = false;
            });
            Gamepad.current?.PauseHaptics();
        }
        else
        {
            isPauseFading = true;
            Time.timeScale = 1;
            pauseGroup.DOFade(0, 0.2f).OnComplete(() =>
            {
                pauseContinueBtn.Select();
                pauseGroup.gameObject.SetActive(false);
                isPauseFading = false;
            });
        }
    }

    public void ShowMainMenu()
    {
        Debug.Log("show menu");
        mainMenu.gameObject.SetActive(true);
        _mainMenuStartBtn.Select();
        Sequence seq = DOTween.Sequence()
            .Append(mainMenuBack.DOFade(0, 0.5f))
            .Join(creditsSprite.DOFade(0, .5f))
            .Append(mainMenu.DOFade(1, 0.5f))
            .Join(logoSprite.DOFade(1, .5f))
            .OnComplete(() =>
            {
                mainMenuBack.gameObject.SetActive(false);
            });

    }

    //show credits from main menu
    public void ShowMenuCredits()
    {
        mainMenuBack.gameObject.SetActive(true);
        _mainMenuBackBtn.Select();
        Sequence seq = DOTween.Sequence()
            .Append(mainMenu.DOFade(0, 0.5f))
            .Join(logoSprite.DOFade(0, .5f))
            .Append(mainMenuBack.DOFade(1, 0.5f))
            .Join(creditsSprite.DOFade(1, .5f))
            .OnComplete(() =>
            {
                mainMenu.gameObject.SetActive(false);
            });
    }

    public void StartGame()
    {
        //do cinematic
        mainMenuGroup.gameObject.SetActive(false);
        OnStartGame?.Invoke();
        
    }
    
    
   
}
