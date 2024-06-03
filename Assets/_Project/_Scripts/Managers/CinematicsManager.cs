using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using DG.Tweening;
using FMODUnity;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

//everything related to cinematics happening in the game
public class CinematicsManager : MonoBehaviour
{
    private AudioManager _audioManager;
    private UIManager _uiManager;
    private GameManager _gameManager;
    [SerializeField] private CameraManager _cameraManager;
    
    [SerializeField] private BlueNPC _blueNpc;
    [SerializeField] private MonsterPlayer _playerRef;
    
    [Header("Cinematic - Intro & main menu")]
    [SerializeField] private GameObject pathBlueTitles1;
    [SerializeField] private GameObject pathBlueTitles2;
    [SerializeField] private GameObject pathBlueTitles3;
    [SerializeField] private GameObject pathPlayerTitles;
    [SerializeField] Transform logosFollowCamObj;
    [SerializeField] Transform mainMenuObject;
    [SerializeField] float blueC0Time;
    [SerializeField] int blueC01Time;
    
    [Header("Cinematic - Blue meetup")]
    [SerializeField] private GameObject pathBlueMeetup;
    
    [Header("Cinematic - earthquake & tunnel")] 
    [SerializeField] AudioClip sfxExplosion;
    [SerializeField] GameObject pathBlueEarthquake;
    [SerializeField] private GameObject tunnelRocks;

    [Header("Cinematic - Monster encounter")] 
    [SerializeField] private GameObject pathBlueMonster;
    [SerializeField] private GameObject pathMonster1;
    [SerializeField] private GameObject pathMonster2;
    [SerializeField] private GameObject encounterFollowCamObj;
    [SerializeField] private GameObject pathPlayer1;
    [SerializeField] private GameObject pathPlayer2;
    

    private void Awake()
    {
        _gameManager = GetComponent<GameManager>();
        _audioManager = GetComponent<AudioManager>();
        _uiManager = GetComponent<UIManager>();

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    //stuff that happens before starting a blocking cinematic
    private void BeforeCinematicStarts(bool bars = true, bool deactivateInput = true)
    {
        _playerRef.StopMovement();
        _blueNpc.ToggleFollow(false);
        //playerInput.enabled = false;
        if (deactivateInput)
        {
            Debug.Log("deactivating input");
            _playerRef.playerInput.DeactivateInput();
            
        }
        
        if(bars)
            _uiManager.ToggleCinematicBars(true);
    }
    
    private void AfterCinematicEnds()
    {
        _uiManager.ToggleCinematicBars(false);
        _playerRef.playerInput.ActivateInput();
        _gameManager.ChangeGameState(GameState.Default);
       
    }
    
    //Start cinematic of blue encounter and wait for player input
    public void BeforeDoCinematicBlueEncounter()
    {
        PlayerInput pInput = _playerRef.playerInput;
        //disable move input action
        pInput.actions.FindAction("Move").Disable();
        _playerRef.StopMovement();
        _uiManager.PrepareBlueMeetupCinematic();
        
        //register or waitforcall first time callback
        pInput.actions.FindAction("Call").Enable();
        pInput.actions.FindAction("Call").performed += FirstTimeCallInput;

    }
    
    private void FirstTimeCallInput(InputAction.CallbackContext ctx)
    {
        //does the call normally
        DoCinematicBlueEncounter();
        _playerRef.playerInput.actions.FindAction("Call").performed -= FirstTimeCallInput;
        
    }
    
    /* Initial Blue encounter.This is triggered after Player uses the call input */
    public void DoCinematicBlueEncounter()
    {
        
        Transform[] bluePathTransforms = pathBlueMeetup.GetComponentsInChildren<Transform>();
        Vector3[] bluePathPos = new Vector3[bluePathTransforms.Length-1];
        for (int i = 1; i < bluePathTransforms.Length; i++)
        {
            bluePathPos[i-1] = bluePathTransforms[i].position;
        }
        
        Sequence cinematic = DOTween.Sequence()
            .AppendInterval(2f);
        
        _uiManager.ToggleCallIcons(false);
            
        cinematic.AppendCallback(() =>
            {
                _audioManager.ChangeBackgroundMusic(2);
                StartCoroutine(_blueNpc.PlayCallSFX());
                //change to friend music
            })
            .AppendInterval(1)
            .Append(_blueNpc.transform.DOPath(bluePathPos, 4, PathType.CatmullRom, PathMode.Sidescroller2D)
                .SetEase(Ease.InOutSine)
                .SetLookAt(0.001f, transform.forward, Vector3.right))
            .AppendCallback(() => { StartCoroutine(_blueNpc.PlayCallSFX()); })
            .AppendInterval(1)
            .OnComplete(
                () =>
                {
                    
                    _playerRef.playerInput.actions.FindAction("Move").Enable();
                    AfterCinematicEnds();
                    _blueNpc.ToggleFollow(true);
                    _playerRef.SetBlueReference(_blueNpc);
                }
            );

    }


    
    
    //Cinematic of screaming and changing background music
    public void DoCinematicScreams()
    {
        _audioManager.DoCinematicScreams();
    }
    
    //After going inside a little bit of the tunnel

    public void DoCinematicInsideTunnel()
    {
        _cameraManager.ShakeCamera();
        AudioSource.PlayClipAtPoint(sfxExplosion, Camera.main.transform.position);
        RuntimeManager.PlayOneShot(_audioManager.sfxMonsterScream, transform.position);
        tunnelRocks.SetActive(true);
        
    }

    public void DoCinematicMonsterEncounterPt2(InputAction.CallbackContext ctx)
    {
        PlayerInput pInput = _playerRef.playerInput;
        //we disable it here so that player cna not spam it. Consequence is that the normal action is not called, gotta do it manually
        pInput.actions.FindAction("Call").Disable();
        StartCoroutine(_playerRef.PlayCallSFX(false));
        
        
        _uiManager.ToggleCallIcons(false);
        _playerRef.playerInput.actions.FindAction("Call").performed -= DoCinematicMonsterEncounterPt2;
        //disable call again
        
        Transform[] tPathPlayer2 = pathPlayer2.GetComponentsInChildren<Transform>();
        Vector3[] pathPlayerPos = new Vector3[tPathPlayer2.Length-1];
        for (int i = 1; i < tPathPlayer2.Length; i++)
        {
            pathPlayerPos[i-1] = tPathPlayer2[i].position;
        }
        
        Sequence seq = DOTween.Sequence()
            .AppendInterval(1)
            .Append(_playerRef.transform.DOPath(pathPlayerPos, 3, PathType.CatmullRom, PathMode.Sidescroller2D)
                .SetEase(Ease.InOutSine)
                .SetLookAt(0.001f, transform.forward, Vector3.right))
            .OnComplete(() =>
            {
                AfterCinematicEnds();
                //enable everything again
                pInput.actions.FindAction("Call").Enable();
                pInput.actions.FindAction("Move").Enable();
                
            });
    }

    public void DoCinematicMonsterEncounterPt1()
    {
        BeforeCinematicStarts(true, false);
        
        Transform[] tBluePathMonster = pathBlueMonster.GetComponentsInChildren<Transform>();
        Vector3[] bluePathMonsterPos = new Vector3[tBluePathMonster.Length-1];
        for (int i = 1; i < tBluePathMonster.Length; i++)
        {
            bluePathMonsterPos[i-1] = tBluePathMonster[i].position;
        }
        
        Transform[] tPathPlayer1 = pathPlayer1.GetComponentsInChildren<Transform>();
        Vector3[] pathPlayerPos = new Vector3[tPathPlayer1.Length-1];
        for (int i = 1; i < tPathPlayer1.Length; i++)
        {
            pathPlayerPos[i-1] = tPathPlayer1[i].position;
        }
        
        PlayerInput pInput = _playerRef.playerInput;
        //disable move input action
        pInput.actions.FindAction("Move").Disable();
        pInput.actions.FindAction("Call").Disable();
        
        
        Sequence seq = DOTween.Sequence()
            .AppendCallback(() => { _cameraManager.AddTempFollowedObj(encounterFollowCamObj);})
            .Append(_blueNpc.transform.DOPath(bluePathMonsterPos, 6, PathType.CatmullRom, PathMode.Sidescroller2D)
                .SetEase(Ease.InOutSine)
                .SetLookAt(0.001f, transform.forward, Vector3.right)
                .OnWaypointChange(
                    (int waypointIndex) =>
                    {
                        if (waypointIndex == 0)
                            StartCoroutine(_blueNpc.PlayCallSFX());
                        else if (waypointIndex == 1)
                            RuntimeManager.PlayOneShot(_audioManager.sfxMonsterScream, transform.position);
                    }))
            .Join(_playerRef.transform.DOPath(pathPlayerPos, 4, PathType.CatmullRom, PathMode.Sidescroller2D)
                .SetEase(Ease.InOutSine)
                .SetLookAt(0.001f, transform.forward, Vector3.right)
                .SetDelay(2))
            .OnComplete(() =>
            {
                //enable call
                //show QTE
                _uiManager.ToggleCallIcons(true);
                pInput.actions.FindAction("Call").Enable();
                pInput.actions.FindAction("Call").performed += DoCinematicMonsterEncounterPt2;
                Debug.Log("Enable call again");
                
            });



    }

    //#3 earthquake and Blue going to investigate
    public void DoCinematicEarthquake()
    {
        BeforeCinematicStarts(true);
       
        Transform[] bluePathEarthquakeTransforms = pathBlueEarthquake.GetComponentsInChildren<Transform>();
        Vector3[] bluePathEarthquakePos = new Vector3[bluePathEarthquakeTransforms.Length-1];
        for (int i = 1; i < bluePathEarthquakeTransforms.Length; i++)
        {
            bluePathEarthquakePos[i-1] = bluePathEarthquakeTransforms[i].position;
        }
        
        //path to near player
        Vector3 dif = _blueNpc.targetRef.position - _blueNpc.transform.position;
        Vector3[] closeToPlayerPath = new Vector3[] { (_blueNpc.transform.position + dif * 0.8f) };
        
        //we  need to position Blue for the next cinematic
        Transform[] tBluePathMonster = pathBlueMonster.GetComponentsInChildren<Transform>();
        Vector3[] bluePathMonsterPos = new Vector3[tBluePathMonster.Length-1];
        for (int i = 1; i < tBluePathMonster.Length; i++)
        {
            bluePathMonsterPos[i-1] = tBluePathMonster[i].position;
        }
        
        Sequence seq = DOTween.Sequence()
            .Append(_blueNpc.transform.DOPath(closeToPlayerPath, 2, PathType.CatmullRom, PathMode.Sidescroller2D)
                .SetEase(Ease.InOutSine)
                .SetLookAt(0.001f, transform.forward, Vector3.right))
            .JoinCallback(() => {_audioManager.ToggleMusicVolume(true);})
            .AppendCallback(() =>
            {
                _cameraManager.ShakeCamera();
                AudioSource.PlayClipAtPoint(sfxExplosion, Camera.main.transform.position);

            })
            .AppendInterval(3)
            .Append(_blueNpc.transform.DOPath(bluePathEarthquakePos, 7, PathType.CatmullRom, PathMode.Sidescroller2D)
                .SetEase(Ease.InOutSine)
                .SetLookAt(0.001f, transform.forward, Vector3.right)
                .OnWaypointChange(
                    (int waypointIndex) =>
                    {
                        if (waypointIndex == 0)
                            StartCoroutine(_blueNpc.PlayCallSFX());
                        else if(waypointIndex == 1)
                            StartCoroutine(_playerRef.PlayCallSFX(false));
                    }))
            .OnComplete(
                () =>
                {
                    _playerRef.SetBlueReference(null);
                    _blueNpc.transform.position = bluePathMonsterPos[0];
                    _audioManager.ToggleMusicVolume(false);
                    AfterCinematicEnds();
                    //Now put Blue in the next pos
                    //prepare blue for next cinematic, for just destroy it
                    
                }
            );

    }
    
    public void DoCinematicTitles()
    {
        BeforeCinematicStarts(false);
        _uiManager.SetupWorldUIForTitles();
        _cameraManager.ToggleDefaultNoise(false);
        
        Transform[] playerPathC0Transforms = pathPlayerTitles.GetComponentsInChildren<Transform>();
        Transform[] bluePathC0Transforms = pathBlueTitles1.GetComponentsInChildren<Transform>();
        Vector3[] bluePathC0Pos = new Vector3[bluePathC0Transforms.Length-1];
        for (int i = 1; i < bluePathC0Transforms.Length; i++)
        {
            bluePathC0Pos[i-1] = bluePathC0Transforms[i].position;
        }

        Transform[] bluePathC1Transforms = pathBlueTitles2.GetComponentsInChildren<Transform>();
        Vector3[] bluePathC1Pos = new Vector3[bluePathC1Transforms.Length-1];
        for (int i = 1; i < bluePathC1Transforms.Length; i++)
        {
            bluePathC1Pos[i-1] = bluePathC1Transforms[i].position;
        }

        
        Sequence introLogos = DOTween.Sequence()
            .AppendInterval(3f)
            .Append(_uiManager.logoUsc.DOFade(1, 3).SetEase(Ease.InQuart))
            .Join(_blueNpc.transform.DOPath(bluePathC0Pos, blueC0Time, PathType.CatmullRom, PathMode.Sidescroller2D)
                .SetEase(Ease.InOutSine)
                .SetLookAt(0.001f, transform.forward, Vector3.right)
                .OnWaypointChange(
                    (int waypointIndex) =>
                    {
                        if (waypointIndex == 2)
                            StartCoroutine(_blueNpc.PlayCallSFX());
                    }))
            .Join(_uiManager.logoUsc.DOFade(0, 3).SetDelay(4f))
            .Join(_uiManager.logoBerklee.DOFade(1, 3).SetEase(Ease.InQuart).SetDelay(4f))
            .Join(_uiManager.logoBerklee.DOFade(0, 3).SetDelay(4f))
            .AppendCallback(() =>
            {
                _blueNpc.transform.position = bluePathC1Pos[0];
            })
            .Join(_blueNpc.transform
                .DOPath(bluePathC1Pos, blueC01Time, PathType.CatmullRom, PathMode.Sidescroller2D)
                .SetEase(Ease.InOutSine)
                .SetLookAt(0.001f, transform.forward, Vector3.right)
                .SetDelay(2f))
            .Join(_uiManager.logoTitle.DOFade(1, 3.5f).SetEase(Ease.InQuart).SetDelay(3f ));

        Sequence introMover = DOTween.Sequence()
        .Append(mainMenuObject.transform.DOMoveY(playerPathC0Transforms[^1].position.y, 26)
            .SetEase(Ease.InOutSine))
        .Join(logosFollowCamObj.DOMoveY(playerPathC0Transforms[^1].position.y, 26).SetEase(Ease.InOutSine).OnComplete(
            () => {
                mainMenuObject.parent = null;
            }
         ))
        .Join(_uiManager.blackout.DOFade(0, 3).SetDelay(0.5f))
        .Join(introLogos)
        .OnComplete(
            () =>
            {
                //showMainMenu
                _gameManager.ShowMainMenuFirstTime();
            }
            
        );
    }
    
    //Cinematic when player press Start in the  Main Menu
    public void DoCinematicStartGame()
    {
        //Blue little swim away
        Transform[] bluePathC2Transforms = pathBlueTitles3.GetComponentsInChildren<Transform>();
        Vector3[] bluePathC2Pos = new Vector3[bluePathC2Transforms.Length-1];
        for (int i = 1; i < bluePathC2Transforms.Length; i++)
        {
            bluePathC2Pos[i-1] = bluePathC2Transforms[i].position;
        }
        
        //Green appears from the cave
        Transform[] playerPathC0Transforms = pathPlayerTitles.GetComponentsInChildren<Transform>();
        Vector3[] playerPathC0Pos = new Vector3[playerPathC0Transforms.Length-1];
        for (int i = 1; i < playerPathC0Transforms.Length; i++)
        {
            playerPathC0Pos[i-1] = playerPathC0Transforms[i].position;
        }
        
        //we need the first point of the next Blue cinematic to position her
        Transform[] nextBlueCinematicTransforms = pathBlueMeetup.GetComponentsInChildren<Transform>();
       

        Sequence startGameCinematic = DOTween.Sequence()
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                StartCoroutine(_blueNpc.PlayCallSFX());
                _playerRef.transform.position = playerPathC0Pos[0];
            })
            .Append(_blueNpc.transform.DOPath(bluePathC2Pos, 5, PathType.CatmullRom, PathMode.Sidescroller2D)
                .SetEase(Ease.InOutSine)
                .SetLookAt(0.001f, transform.forward, Vector3.right)
                .OnComplete(() =>
                {
                    var blueTransform = _blueNpc.transform;
                    blueTransform.position = nextBlueCinematicTransforms[1].position + Vector3.down * 5;
                    blueTransform.rotation = Quaternion.identity;
                    _blueNpc.transform.DOMoveY(nextBlueCinematicTransforms[1].position.y, 1);
                }))
            .Join(_playerRef.transform.DOPath(playerPathC0Pos, 6, PathType.CatmullRom, PathMode.Sidescroller2D)
                .SetEase(Ease.InOutSine)
                .SetLookAt(0.001f, transform.forward, Vector3.right)
                .SetDelay(1f)
                .OnComplete(
                    () =>
                    {
                        AfterCinematicEnds();
                        _cameraManager.StartFollowingTargetGroup(logosFollowCamObj);
                        _uiManager.ShowMoveControls();
                        _playerRef.playerInput.actions.FindAction("Call").Disable();
                    }

                ))
            .AppendCallback(() => { _cameraManager.ToggleDefaultNoise(true); });
        
    }




}
