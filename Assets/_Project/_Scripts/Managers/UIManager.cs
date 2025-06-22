using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private void Awake() 
    { 
        // If there is an instance, and it's not me, delete myself.
    
        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            Instance = this; 
        } 
    }
    
    [Header("In-world UI")]
    public SpriteRenderer logoUsc;
    public SpriteRenderer logoBerklee;
    public SpriteRenderer logoTitle;
    public ParticleSystem swimTutorialFeedback;
    //1. move 2.dash 3.call
    [SerializeField] private SpriteRenderer[] uiKeyboardIcons;
    [SerializeField] private SpriteRenderer[] uiGamepadIcons;
    [SerializeField] private SpriteRenderer[] uiXboxIcons;
    [SerializeField] private SpriteRenderer[] uiPlayerPrompts;
    private Sequence _uiPlayerPromptSeq;
    [SerializeField] private RectTransform topCinematicBar;
    [SerializeField] private RectTransform bottomCinematicBar;

    [Header("Pause Menu")]
    public CanvasGroup blackout;
    [SerializeField] private CanvasGroup pauseGroup;
    [Header("Main Menu")]
    [SerializeField] private SpriteRenderer logoSprite;
    [SerializeField] private SpriteRenderer creditsSprite;
    [SerializeField] private GameObject mainMenuGroup;
    [SerializeField] private Button startGameBtn;
    [SerializeField] private CanvasGroup mainMenu;
    [SerializeField] private CanvasGroup mainMenuBack;
    [SerializeField] private BoidFlock menuFlock;
    [SerializeField] private Transform menuFlockFollowPivot;
    private Button _pauseContinueBtn;
    private Button _mainMenuBackBtn;
    private Button _mainMenuStartBtn;
    [HideInInspector]
    public bool isPauseFading = false;

    private void OnEnable()
    {
        PlayerCharacter.PlayerOnControlsChanged += OnControlsChanged;
    }


    private void OnDisable()
    {
        PlayerCharacter.PlayerOnControlsChanged -= OnControlsChanged;
    }

    // Start is called before the first frame update
    void Start()
    {
        blackout.gameObject.SetActive(true);
        pauseGroup.gameObject.SetActive(false);
        //disable main menu
        mainMenuGroup.SetActive(true);
        mainMenuBack.gameObject.SetActive(false);
        mainMenu.gameObject.SetActive(true);
        pauseGroup.alpha = mainMenu.alpha = mainMenuBack.alpha = 0;
        _mainMenuBackBtn = mainMenuBack.GetComponentInChildren<Button>();
        _mainMenuStartBtn = mainMenu.GetComponentInChildren<Button>();
        _pauseContinueBtn = pauseGroup.GetComponentInChildren<Button>();
        //regardless of what happens, we start with a smooth fadeout
        blackout.DOFade(0, 3).SetEase(Ease.InQuad);
        
        Color transparent = new Color(255, 255, 255, 0);
        for (int i = 0; i < uiPlayerPrompts.Length; i++)
        {
            uiPlayerPrompts[i].color =  transparent;
        }
    }
    
    public IEnumerator HideMainMenu()
    {
        //do cinematic
        mainMenuGroup.gameObject.SetActive(false);
        menuFlockFollowPivot.position -= new Vector3(0,30,0);
        yield return new WaitForSeconds(15);
        Destroy(menuFlock);

    }

    public void PauseGame(bool toggle)
    {
        if (toggle)
        {
            Debug.Log("Pausing game");
            isPauseFading = true;
            pauseGroup.gameObject.SetActive(true);
            Gamepad.current?.PauseHaptics();
            _pauseContinueBtn.Select();
            pauseGroup.DOFade(1, 0.25f).OnComplete(()=>{
                isPauseFading = false;
                Time.timeScale = 0;
            });
        }
        else
        {
            isPauseFading = true;
            Time.timeScale = 1;
            pauseGroup.DOFade(0, 0.2f).OnComplete(() =>
            {
                _pauseContinueBtn.Select();
                pauseGroup.gameObject.SetActive(false);
                isPauseFading = false;
            });
        }
    }

    public void ShowMainMenu()
    {
        menuFlock.ToggleActivity(true);
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

    public void SelectStartButtonMainMenu()
    {
        startGameBtn.Select();
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

    //Show and hide cinematic bars
    public void ToggleCinematicBars(bool show)
    {
        Sequence barsSeq = DOTween.Sequence()
            .Append(topCinematicBar.DOSizeDelta(new Vector2(0, show? 100 : 0), 1))
            .Join(bottomCinematicBar.DOSizeDelta(new Vector2(0, show ? 100 : 0), 1));
        
    }

    
    public void SetupWorldUIForTitles()
    {
        Color transparent = new Color(255, 255, 255, 0);
        for (int i = 0; i < uiKeyboardIcons.Length; i++)
        {
            uiKeyboardIcons[i].color =  uiGamepadIcons[i].color = uiXboxIcons[i].color = transparent;
        }
    }
    
    //After Intro cinematic
    public void TutorialShowControlsUI()
    {
        OnControlsChanged();
        Sequence seq = DOTween.Sequence()
            .Append(uiKeyboardIcons[0].DOFade(1, 0.75f))
            .Join(uiGamepadIcons[0].DOFade(1, 0.75f))
            .Join(uiXboxIcons[0].DOFade(1, 0.75f));
        //Add the UI feedback

    }

    public void TutorialShowSwimUI()
    {
        Sequence seq = DOTween.Sequence()
        .Join(uiKeyboardIcons[1].DOFade(1, 0.75f))
        .Join(uiGamepadIcons[1].DOFade(1, 0.75f))
        .Join(uiXboxIcons[1].DOFade(1, 0.75f))
        .OnComplete(() =>
        {
            ToggleOnSwimUIFeedback(true);
        });

        //Add the UI feedback
    }

    public void PrepareBlueMeetupCinematic()
    {
        ToggleCinematicBars(true);
        TogglePlayerUIPrompt(true);
    }
    
    
    
    //callback to control showing the world UI
    public void OnControlsChanged()
    {
        if (GameManager.Instance.playerRef.playerInput.currentControlScheme == "Gamepad")
        {
            //nnow to see if xbox or another
            //Debug.Log(playerCharacterRef.playerInput.devices[0].name);
            if (GameManager.Instance.playerRef.playerInput.devices[0].name.Contains("windows",StringComparison.OrdinalIgnoreCase))
            {
                //xbox gamepad
                for (int i = 0; i < uiGamepadIcons.Length; i++)
                {
                    uiKeyboardIcons[i].gameObject.SetActive(false);
                    uiGamepadIcons[i].gameObject.SetActive(false);
                    uiXboxIcons[i].gameObject.SetActive(true);
                }
                uiPlayerPrompts[0].gameObject.SetActive(true);
                uiPlayerPrompts[1].gameObject.SetActive(false);
                uiPlayerPrompts[2].gameObject.SetActive(false);
            }
            else
            {
                Debug.Log("Default gamepad");
                //default gamepad
                for (int i = 0; i < uiGamepadIcons.Length; i++)
                {
                    uiKeyboardIcons[i].gameObject.SetActive(false);
                    uiXboxIcons[i].gameObject.SetActive(false);
                    uiGamepadIcons[i].gameObject.SetActive(true);
                }
                uiPlayerPrompts[0].gameObject.SetActive(false);
                uiPlayerPrompts[1].gameObject.SetActive(true);
                uiPlayerPrompts[2].gameObject.SetActive(false);
            }
           
        }
        else if(GameManager.Instance.playerRef.playerInput.currentControlScheme.Contains("Keyboard"))
        {
            for (int i = 0; i < uiGamepadIcons.Length; i++)
            {
                uiKeyboardIcons[i].gameObject.SetActive(true);
                uiGamepadIcons[i].gameObject.SetActive(false);
                uiXboxIcons[i].gameObject.SetActive(false);
            }
            uiPlayerPrompts[0].gameObject.SetActive(false);
            uiPlayerPrompts[1].gameObject.SetActive(false);
            uiPlayerPrompts[2].gameObject.SetActive(true);
        }
        
    }
    
    
    //Fadeout title logo + move icons after playerCharacter moves in a little to the right
    public void TutorialFadeoutMoveSwimUI()
    {
        ToggleOnSwimUIFeedback(false);
        Sequence seq = DOTween.Sequence();
        seq.SetEase(Ease.OutCubic);
        seq.Append(uiKeyboardIcons[0].DOFade(0, 4f))
            .Join(uiKeyboardIcons[1].DOFade(0, 4f))
            .Join(uiGamepadIcons[0].DOFade(0, 4f))
            .Join(uiGamepadIcons[1].DOFade(0, 4f))
            .Join(uiXboxIcons[0].DOFade(0, 4f))
            .Join(uiXboxIcons[1].DOFade(0, 4f));

    }
    
    public void ToggleOnSwimUIFeedback(bool toggle)
    {
        if(toggle)
            PlayerCharacter.OnPlayerSwim += UIOnSwimFeedback;
        else
            PlayerCharacter.OnPlayerSwim -= UIOnSwimFeedback;
    }

    public void UIOnSwimFeedback()
    {
        swimTutorialFeedback.Play();
        Sequence seq = DOTween.Sequence();
        seq.SetEase(Ease.OutCubic);
        seq.Append(uiKeyboardIcons[1].transform.DOPunchScale(new Vector3(-1,-1,0) * .35f, 1f, 1))
            .Join(uiGamepadIcons[1].transform.DOPunchScale(new Vector3(-1,-1,0) * .35f, 1f, 1))
            .Join(uiXboxIcons[1].transform.DOPunchScale(new Vector3(-1,-1,0) * .35f, 1f, 1));
    }

    //Toggling player UI prompts
    public void TogglePlayerUIPrompt(bool toggle)
    {
        int x = toggle ? 1 : 0;
        
        _uiPlayerPromptSeq.Kill();
        _uiPlayerPromptSeq = DOTween.Sequence();
        _uiPlayerPromptSeq.Append(uiPlayerPrompts[0].DOFade(x, .5f))
            .Join(uiPlayerPrompts[1].DOFade(x, .5f))
            .Join(uiPlayerPrompts[2].DOFade(x, .5f));
        
        if(toggle)
            GameManager.Instance?.playerRef.ToggleUIPromptPositioning(true);
        else
        {
            _uiPlayerPromptSeq.OnComplete(() =>
            {
                GameManager.Instance?.playerRef.ToggleUIPromptPositioning(false);
            });
        }
    }


    //Change the flock tracking pos
    public void ChangeMenuFlockPosition(Vector3 newPos)
    {
        menuFlockFollowPivot.position = newPos;
    }
    
    
    
    
   
}
