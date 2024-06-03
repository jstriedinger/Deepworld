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

    [SerializeField] private GameObject level2;
    
    
    [Header("Title and intro UI")]
    [SerializeField] private SpriteRenderer Title;

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
    [SerializeField] private UIManager uiManager;


    [Header("World UI")]
    private bool isUiActive;
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


    [Header("Cinematic - earthquake")] 
    [SerializeField] AudioClip sfxExplosion;
    [SerializeField] GameObject _bluePathEarthquake;
    
    [Header("Cinematic - screams")]
    [SerializeField] EventReference sfxBlueCall;


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
    
    private void OnEnable()
    {
        MonsterPlayer.PlayerOnControlsChanged += OnControlsChanged;
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
            uiManager.ToggleCinematicBars(true);
    }

    private void AfterCinematicEnds(bool bars)
    {
        uiManager.ToggleCinematicBars(false);
        _playerInput.ActivateInput();
        _playerInput.enabled = true;
        _gameManager.ChangeGameState(GameState.Default);

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
        
        uiGamepadIcons[2].DOFade(0, 0.75f);
        uiKeyboardIcons[2].DOFade(0, 0.75f);
            
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
            .AppendCallback(() => { StartCoroutine(_player.PlayCallSFX(false)); })
            .AppendCallback(() => { _cameraManager.ChangePlayerRadius(35);  })
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

    #endregion


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

