using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cinemachine;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;
using TMPro;
using FMODUnity;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Serialization;

public class Level1Manager : MonoBehaviour
{

    private IDisposable _AnyKeyStartEventListener;
    [SerializeField] private GameObject level2;
    
    
    [Header("Title and intro UI")]
    [SerializeField] private SpriteRenderer LogoUSC;
    [SerializeField] private SpriteRenderer LogoBerklee;
    [SerializeField] private SpriteRenderer Title;
    [SerializeField] private CanvasGroup fadeOut;

    [FormerlySerializedAs("player")]
    [Header("Player references")]
    [SerializeField] private MonsterPlayer _player;
    private PlayerInput _playerInput;
    private float _noiseAmplitudeBefore;


    [Header("References")]
    [SerializeField] CinemachineTargetGroup targetGroup;
    [SerializeField] CinemachineVirtualCamera vCam;
    private CameraManager _cameraManager;
    private GameManager _gameManager;
    private AudioManager _audioManager;


    [Header("World UI")]
    private bool isUiActive;
    [SerializeField] private TextMeshPro uiPressAnyKey;
    //1. move 2.dash 3.call
    [SerializeField] private SpriteRenderer[] uiKeyboardIcons;
    [SerializeField] private SpriteRenderer[] uiGamepadIcons;
    [SerializeField] private RectTransform topCinematicBar;
    [SerializeField] private RectTransform bottomCinematicBar;

    [FormerlySerializedAs("blueObject")]
    [Header("Blue behavoir")]
    [SerializeField] private GameObject _blueIntro;

    private Rigidbody2D blueRb;
    private BlueNPC blueNPCIntro;
    [SerializeField] private Transform[] bluePathC1;
    private StudioEventEmitter studioEventEmitter;


    [Header("FMOD audio-music")]

    [Header("Cinematic -Intro")] 
    [SerializeField] private GameObject _blueC0P1;
    [SerializeField] private GameObject _blueC0P2;
    [SerializeField] private GameObject _blueC0P3;
    [SerializeField] private GameObject _playerC0P1;
    [SerializeField] Transform camPivotFollow;
    [SerializeField] Transform mainMenuObject;
    [SerializeField] float blueC0Time;
    [SerializeField] int blueC01Time;

    [Header("Cinematic - Blue encounter")]
    [SerializeField] private GameObject _blueC1P1;




    [Header("Final cinematic")]

    [SerializeField] GameObject newCamFollow;

    [SerializeField] EventReference SFXMonsterScream;

    [SerializeField] Transform enemy1;

    [SerializeField] Transform enemy2;

    [SerializeField] Transform[] Enemy1Path;

    [SerializeField] Transform[] Enemy2Path;
    [SerializeField] Transform[] playerPathC3;
    [SerializeField] Transform[] bluePathC3;
    [SerializeField] Transform[] bluePathC3_2;
    [SerializeField] Transform[] bluePathC3_3;
    [SerializeField] float Enemy1Time;
    [SerializeField] float Enemy2Time;
    [SerializeField] float blueTime;

    private void Awake()
    {
        blueRb = _blueIntro.GetComponent<Rigidbody2D>();
        blueNPCIntro = _blueIntro.GetComponent<BlueNPC>();
        studioEventEmitter = GetComponent<FMODUnity.StudioEventEmitter>();
    }
    
    void Start()
    {
        _playerInput = _player.playerInput;
        _cameraManager = GameObject.FindFirstObjectByType<CameraManager>();
        _gameManager = GameObject.FindFirstObjectByType<GameManager>();
        _audioManager = GameObject.FindFirstObjectByType<AudioManager>();
    }
    
    //Start this level. This is called by the gameManager
    public void StartLevel()
    {
        //music
        fadeOut.gameObject.SetActive(true);
        
        //make all ui invisible by default
        isUiActive = true;
        Color transparent = new Color(255, 255, 255, 0);
        for (int i = 0; i < uiKeyboardIcons.Length; i++)
        {
            uiKeyboardIcons[i].color =  uiGamepadIcons[i].color = transparent;
        }
        
        DoCinematicTitles();
    }
    // Start is called before the first frame update
    

    
    

    //function called when showing the control UI for the first time, when game is starting
    private void ShowMoveControls()
    {
        OnControlsChanged();
        Sequence seq = DOTween.Sequence()
            .Append(uiKeyboardIcons[0].DOFade(1, 0.5f))
            .Join(uiKeyboardIcons[1].DOFade(1, 0.5f))
            .Join(uiGamepadIcons[0].DOFade(1, 0.5f))
            .Join(uiGamepadIcons[1].DOFade(1, 0.5f));
       
    }
    
    //callback when controls are changed. Enabling/disabling according to input
    public void OnControlsChanged()
    {
        if (isUiActive)
        {
            if (_playerInput.currentControlScheme == "Gamepad")
            {
                for (int i = 0; i < uiGamepadIcons.Length; i++)
                {
                    uiKeyboardIcons[i].gameObject.SetActive(false);
                    uiGamepadIcons[i].gameObject.SetActive(true);
                }
               
            }
            else if(_playerInput.currentControlScheme.Contains("Keyboard"))
            {
                for (int i = 0; i < uiGamepadIcons.Length; i++)
                {
                    uiKeyboardIcons[i].gameObject.SetActive(true);
                    uiGamepadIcons[i].gameObject.SetActive(false);
                }
            }
            
        }
    }

    private void OnDisable()
    {
        MonsterPlayer.PlayerOnControlsChanged -= OnControlsChanged;
    }

    #region Cinematics

    private void BeforeCinematicStarts(bool bars = true)
    {
        _player.StopMovement();
        blueNPCIntro.ToggleFollow(false);
        //playerInput.enabled = false;
       _playerInput.DeactivateInput();
        
        if(bars)
            ToggleCinematicBars(true);
    }

    private void AfterCinematicEnds(bool bars)
    {
        ToggleCinematicBars(false);
        _playerInput.ActivateInput();
        _playerInput.enabled = true;
        _gameManager.ChangeGameState(GameState.Default);
       
    }
    /**
     * Introduction cinematic of the game with logos and title
     */
    private void DoCinematicTitles()
    {
        BeforeCinematicStarts(false);
        uiPressAnyKey.alpha =  0;
        _noiseAmplitudeBefore = vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain;
        vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0;

        Transform[] playerPathC0Transforms = _playerC0P1.GetComponentsInChildren<Transform>();

        Transform[] bluePathC0Transforms = _blueC0P1.GetComponentsInChildren<Transform>();
        Vector3[] bluePathC0Pos = new Vector3[bluePathC0Transforms.Length-1];
        for (int i = 1; i < bluePathC0Transforms.Length; i++)
        {
            bluePathC0Pos[i-1] = bluePathC0Transforms[i].position;
        }

        Transform[] bluePathC1Transforms = _blueC0P2.GetComponentsInChildren<Transform>();
        Vector3[] bluePathC1Pos = new Vector3[bluePathC1Transforms.Length-1];
        for (int i = 1; i < bluePathC1Transforms.Length; i++)
        {
            bluePathC1Pos[i-1] = bluePathC1Transforms[i].position;
        }


        Sequence introLogos = DOTween.Sequence()
            .AppendInterval(3f)
            .Append(LogoUSC.DOFade(1, 3).SetEase(Ease.InQuart))
            .Join(blueNPCIntro.transform.DOPath(bluePathC0Pos, blueC0Time, PathType.CatmullRom, PathMode.Sidescroller2D)
                .SetEase(Ease.InOutSine)
                .SetLookAt(0.001f, transform.forward, Vector3.right)
                .OnWaypointChange(
                    (int waypointIndex) =>
                    {
                        if (waypointIndex == 2)
                            StartCoroutine(blueNPCIntro.PlayCallSFX());
                    }))
            .Join(LogoUSC.DOFade(0, 3).SetDelay(4f))
            .Join(LogoBerklee.DOFade(1, 3).SetEase(Ease.InQuart).SetDelay(4f))
            .Join(LogoBerklee.DOFade(0, 3).SetDelay(4f))
            .AppendCallback(() =>
            {
                blueNPCIntro.transform.position = bluePathC1Pos[0];
            })
            .Join(blueNPCIntro.transform
                .DOPath(bluePathC1Pos, blueC01Time, PathType.CatmullRom, PathMode.Sidescroller2D)
                .SetEase(Ease.InOutSine)
                .SetLookAt(0.001f, transform.forward, Vector3.right)
                .SetDelay(2f))
            .Join(Title.transform.DOScale(7, 4).SetDelay(3f ))
            .Join(Title.DOFade(1, 4).SetEase(Ease.InQuart).SetDelay(3f ));

        Sequence introMover = DOTween.Sequence()
        .Append(mainMenuObject.transform.DOMoveY(playerPathC0Transforms[^1].position.y, 28)
            .SetEase(Ease.InOutSine))
        .Join(camPivotFollow.DOMoveY(playerPathC0Transforms[^1].position.y, 28).SetEase(Ease.InOutSine).OnComplete(
            () => {
                mainMenuObject.parent = null;
            }
         ))
        .Join(fadeOut.DOFade(0, 3).SetDelay(0.5f))
        .Join(introLogos)
        .OnComplete(
            () =>
            {

                uiPressAnyKey.DOFade(1, 1f);
                //Wait for player input to start
                _AnyKeyStartEventListener = InputSystem.onAnyButtonPress.Call(DoCinematicStartGame);
            }
            
        );
    }

   
    //prepare everything to ask the player to use the Call function
    //A special case of beforeCinematicStarts
    public void BeforeDoCinematicBlueEncounter()
    {
        //disable move input action
        _playerInput.actions.FindAction("Move").Disable();
        _player.StopMovement();
        ToggleCinematicBars(true);
        
        //show Call UI prompt
        if (_playerInput.currentControlScheme == "Gamepad")
            uiGamepadIcons[2].DOFade(1, 0.75f);
        else
            uiKeyboardIcons[2].DOFade(1, 0.75f);
        
        //register or waitforcall first time callback
        _playerInput.actions.FindAction("Call").Enable();
        _playerInput.actions.FindAction("Call").performed += FirstTimeCallInput;

    }

    private void FirstTimeCallInput(InputAction.CallbackContext ctx)
    {
        //does the call normally
        DoCinematicBlueEncounter();
        _playerInput.actions.FindAction("Call").performed -= FirstTimeCallInput;
        
    }
    
    /* Initial Blue encounter.This is triggered after Player uses the call input */
    public void DoCinematicBlueEncounter()
    {
        
        Transform[] bluePathTransforms = _blueC1P1.GetComponentsInChildren<Transform>();
        Vector3[] bluePathPos = new Vector3[bluePathTransforms.Length-1];
        for (int i = 1; i < bluePathTransforms.Length; i++)
        {
            bluePathPos[i-1] = bluePathTransforms[i].position;
        }
        
        Sequence cinematic = DOTween.Sequence()
            .AppendInterval(2f);
        
        if (_playerInput.currentControlScheme == "Gamepad")
            cinematic.Append(uiGamepadIcons[2].DOFade(0, 0.5f));
        else
            cinematic.Append(uiKeyboardIcons[2].DOFade(0, 0.5f));
            
        cinematic.AppendCallback(() =>
            {
                _audioManager.ChangeBackgroundMusic(2);
                StartCoroutine(blueNPCIntro.PlayCallSFX());
                //change to friend music
            })
            .AppendInterval(1)
            .Append(blueNPCIntro.transform.DOPath(bluePathPos, 4, PathType.CatmullRom, PathMode.Sidescroller2D)
                .SetEase(Ease.InOutSine)
                .SetLookAt(0.001f, transform.forward, Vector3.right))
            .AppendCallback(() => { StartCoroutine(blueNPCIntro.PlayCallSFX()); })
            .AppendInterval(1)
            .OnComplete(
                () =>
                {
                    
                    _playerInput.actions.FindAction("Move").Enable();
                   AfterCinematicEnds(true);
                   blueNPCIntro.ToggleFollow(true);
                   _player.SetBlueReference(blueNPCIntro);
                }
            );

    }
    
    /**
     * Intro level cinematic of Blue getting chased
    */
    public void DoCinematicBlueChase()
    {
        BeforeCinematicStarts();
        Vector3[] playerPathC3V = new Vector3[playerPathC3.Length];
        for (int i = 0; i < playerPathC3.Length; i++)
        {
            playerPathC3V[i] = playerPathC3[i].position;
        }

        Vector3[] bluepPathC3Vector = new Vector3[bluePathC3.Length];
        for (int i = 0; i < bluePathC3.Length; i++)
        {
            bluepPathC3Vector[i] = bluePathC3[i].position;
        }

        Vector3[] bluepPathC3_2V = new Vector3[bluePathC3_2.Length];
        for (int i = 0; i < bluePathC3_2.Length; i++)
        {
            bluepPathC3_2V[i] = bluePathC3_2[i].position;
        }

        Vector3[] bluepPathC3_3V = new Vector3[bluePathC3_3.Length];
        for (int i = 0; i < bluePathC3_3.Length; i++)
        {
            bluepPathC3_3V[i] = bluePathC3_3[i].position;
        }

        Vector3[] enemy1PathVectors = new Vector3[Enemy1Path.Length];
        for (int i = 0; i < Enemy1Path.Length; i++)
        {
            enemy1PathVectors[i] = Enemy1Path[i].position;
        }

        Vector3[] enemy2PathVectors = new Vector3[Enemy2Path.Length];
        for (int i = 0; i < Enemy2Path.Length; i++)
        {
            enemy2PathVectors[i] = Enemy2Path[i].position;
        }

        Sequence blueCinematic3 = DOTween.Sequence()
            .AppendInterval(1)
            .Append(blueNPCIntro.transform.DOPath(bluepPathC3Vector, 3, PathType.CatmullRom, PathMode.Sidescroller2D)
                .SetEase(Ease.InOutSine)
                .SetLookAt(0.001f, transform.forward, Vector3.right))
            .AppendInterval(0.5f)
            .AppendCallback(() => { StartCoroutine(blueNPCIntro.PlayCallSFX()); })
            .AppendInterval(1f)
            .AppendCallback(() => { StartCoroutine(_player.PlayCallSFX()); })
            .AppendCallback(() => { _cameraManager.ChangePlayerRadius(35); /*cameraManager.AddObjectToTargetGroup(newCamFollow.gameObject);*/ })
            .AppendInterval(0.5f)
            .Append(_player.transform.DOPath(playerPathC3V, blueTime, PathType.CatmullRom, PathMode.Sidescroller2D)
                .SetEase(Ease.InOutSine)
                .SetLookAt(0.001f, transform.forward, Vector3.right))
            .Join(blueNPCIntro.transform.DOPath(bluepPathC3_2V, blueTime, PathType.CatmullRom, PathMode.Sidescroller2D)
                .SetEase(Ease.InOutSine)
                .SetLookAt(0.001f, transform.forward, Vector3.right))
            .AppendCallback(() => { StartCoroutine(blueNPCIntro.PlayCallSFX()); })
            .AppendInterval(0.5f)
            .Append(blueNPCIntro.transform.DOPath(bluepPathC3_3V, blueTime, PathType.CatmullRom, PathMode.Sidescroller2D)
                .SetEase(Ease.InOutSine)
                .SetLookAt(0.001f, transform.forward, Vector3.right))
            //enemies start chasing Blue
            .Join(enemy1.DOPath(enemy1PathVectors, Enemy1Time, PathType.CatmullRom, PathMode.Sidescroller2D)
                .SetEase(Ease.InOutSine)
                .SetLookAt(0.001f, transform.forward, Vector3.right)
                .OnWaypointChange(
            (int waypointIndex) =>
            {
                if (waypointIndex == 1 )
                    RuntimeManager.PlayOneShot(SFXMonsterScream, transform.position);
            }))
            .Join(enemy2.DOPath(enemy2PathVectors, Enemy2Time, PathType.CatmullRom, PathMode.Sidescroller2D)
                .SetEase(Ease.InOutSine)
                .SetLookAt(0.001f, transform.forward, Vector3.right)
                .OnWaypointChange(
            (int waypointIndex) =>
            {
                if (waypointIndex == 0)
                    RuntimeManager.PlayOneShot(SFXMonsterScream, transform.position);
            }))
            //Blue screams
            .AppendCallback(() => { blueNPCIntro.PlayScreamSFX(); })
            .OnComplete(
                () =>
                {
                    _audioManager.ChangeBackgroundMusic(1);
                    _cameraManager.RemoveddObjectToTargetGroup(newCamFollow);
                    _cameraManager.ChangePlayerRadius(30);
                    Destroy(blueNPCIntro.gameObject);
                    Destroy(enemy1.gameObject);
                    Destroy(enemy2.gameObject);
                    AfterCinematicEnds(true);
                    //activate level two
                    level2.SetActive(true);
                }

            );

    }

    private void ToggleCinematicBars(bool show)
    {
        Sequence barsSeq = DOTween.Sequence()
            .Append(topCinematicBar.DOSizeDelta(new Vector2(0, show? 100 : 0), 1))
            .Join(bottomCinematicBar.DOSizeDelta(new Vector2(0, show ? 100 : 0), 1));
        
    }
   
    private void DoCinematicStartGame(InputControl _inputControl)
    {
        _AnyKeyStartEventListener.Dispose();
        
        //Blue little swim away
        Transform[] bluePathC2Transforms = _blueC0P3.GetComponentsInChildren<Transform>();
        Vector3[] bluePathC2Pos = new Vector3[bluePathC2Transforms.Length-1];
        for (int i = 1; i < bluePathC2Transforms.Length; i++)
        {
            bluePathC2Pos[i-1] = bluePathC2Transforms[i].position;
        }
        
        //Green appears from the cave
        Transform[] playerPathC0Transforms = _playerC0P1.GetComponentsInChildren<Transform>();
        Vector3[] playerPathC0Pos = new Vector3[playerPathC0Transforms.Length-1];
        for (int i = 1; i < playerPathC0Transforms.Length; i++)
        {
            playerPathC0Pos[i-1] = playerPathC0Transforms[i].position;
        }
        
        //we need the first point of the next Blue cinematic to position her
        Transform[] nextBlueCinematicTransforms = _blueC1P1.GetComponentsInChildren<Transform>();
       

        Sequence startGameCinematic = DOTween.Sequence()
            .Append(uiPressAnyKey.DOFade(0, 1f))
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                StartCoroutine(blueNPCIntro.PlayCallSFX());
                _player.transform.position = playerPathC0Pos[0];
            })
            .Append(blueNPCIntro.transform.DOPath(bluePathC2Pos, 5, PathType.CatmullRom, PathMode.Sidescroller2D)
                .SetEase(Ease.InOutSine)
                .SetLookAt(0.001f, transform.forward, Vector3.right)
                .OnComplete(() =>
                {
                    var blueTransform = blueNPCIntro.transform;
                    blueTransform.position = nextBlueCinematicTransforms[1].position + Vector3.down * 5;
                    blueTransform.rotation = Quaternion.identity;
                    blueNPCIntro.transform.DOMoveY(nextBlueCinematicTransforms[1].position.y, 1);
                }))
            .Join(_player.transform.DOPath(playerPathC0Pos, 6, PathType.CatmullRom, PathMode.Sidescroller2D)
                .SetEase(Ease.InOutSine)
                .SetLookAt(0.001f, transform.forward, Vector3.right)
                .SetDelay(1f)
                .OnComplete(
                    () =>
                    {
                        AfterCinematicEnds(false);
                        //change targetgroup to us
                        targetGroup.transform.position = camPivotFollow.position;
                        vCam.Follow = targetGroup.transform;
                        ShowMoveControls();
                        _playerInput.actions.FindAction("Call").Disable();
                        MonsterPlayer.PlayerOnControlsChanged += OnControlsChanged;
                    }

                ))
            .Append(DOTween.To(() => vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain,
                x => vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = x, _noiseAmplitudeBefore, 6));;
    }
    #endregion


    //Fadeout title logo + move icons after player moves in a little to the right
    public void FadeOutUIPt1()
    {
        Sequence seq = DOTween.Sequence();
        seq.SetEase(Ease.OutCubic);
        seq.Append(Title.transform.DOScale(6.8f, 5))
            .Join(Title.DOFade(0, 4f))
            .Join(uiKeyboardIcons[0].DOFade(0, 4f))
            .Join(uiGamepadIcons[0].DOFade(0, 4f));

        seq.OnComplete(() =>
        {
            Destroy(Title.gameObject);
        });
    }
    
    //fading out the dash + call icons
    public void FadeOutUIPt2()
    {
        Sequence seq = DOTween.Sequence();
        seq.SetEase(Ease.OutCubic);
        seq.Append(uiKeyboardIcons[1].DOFade(0, 4f))
            .Join(uiGamepadIcons[1].DOFade(0, 4f));
    }


    //Activates danger music for first encoutner of the tutorial
    public void TriggerSwarmEncounter(bool trigger)
    {
        if (trigger)
            _cameraManager.ChangePlayerRadius(35);
        else
            _cameraManager.ChangePlayerRadius(28);
        _audioManager.ToggleCloseDangerAndFriendMusic(trigger);
    }









}

