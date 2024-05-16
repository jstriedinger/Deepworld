using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using FMODUnity;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using UnityEngine.UI;


public enum GameState
{
    Default,
    Paused,
    Cinematic
}


public class GameManager : MonoBehaviour
{
    private enum HowToStart
    {
        Default,
        Checkpoint1,
        Checkpoint2,
        Checkpoint3
    }

    public static event Action OnRestartingGame;

    [Header("Level management")] 
    [SerializeField] private HowToStart howToStart;
    [SerializeField] private Level1Manager level1;
    [SerializeField] private GameObject level2;
    [SerializeField] CheckPoint[] checkPoints;
    [SerializeField] private CameraManager cameraManager;

    [Header("Others")]
    public MonsterPlayer playerRef;
    public GameObject playerLastPosition;
    [SerializeField] private Volume _volume;

    
    [Header("UI")]
    [SerializeField] private CanvasGroup fadeOut;
    [SerializeField] private CanvasGroup uiPause;
    [SerializeField] private Button uiContinueBtn;
    private bool isPauseFading = false;


    [Header("FMOD Music")]
    [SerializeField] private EventReference musicAmbientIntro;
    [SerializeField] private EventReference musicAmbientDanger;
    [SerializeField] private EventReference musicBlue;
    private FMOD.Studio.EventInstance _instanceAmbientIntro, _instanceAmbientDanger, _instanceFriend, _currentInstancePlaying;
    // order of above 0 = ambient intro 1= danger 2=friend
    private int _currentMusicIndex;
    
    [Header("FMOD audio")]
    public EventReference sfxMonsterAtDistance;
    public EventReference sfxShelterEnter;
    public EventReference sfxShelterExit;
    private StudioEventEmitter _sfxMonsterChaseLoop;

    [HideInInspector]
    public int numMonstersChasing;
    public int numMonstersOnScreen = 0;
    public static bool IsPlayerDead;
    private int _currentCheckPointIndex = -1;
    private GameState _gameState;
    private Vector3 _originPos;


    // Start is called before the first frame update
    private void Awake()
    {
        _sfxMonsterChaseLoop = GetComponent<StudioEventEmitter>();
        fadeOut.gameObject.SetActive(true);
        uiPause.gameObject.SetActive(false);
        uiPause.alpha = 0;
        _originPos = playerRef.transform.position;
        IsPlayerDead = false;
        _gameState = GameState.Cinematic;

        playerLastPosition.transform.position = playerRef.transform.position;
        
        //bind pause
        MonsterPlayer.PlayerOnPause += OnPauseGame;
        
        
        

    }

    public void StopAllFMODInstances()
    {
        _instanceAmbientIntro.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _instanceAmbientDanger.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _instanceFriend.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    void Start()
    {
        //getting all music ready
        _instanceAmbientIntro = RuntimeManager.CreateInstance(musicAmbientIntro.Guid);
        _instanceAmbientDanger = RuntimeManager.CreateInstance(musicAmbientDanger.Guid);
        _instanceFriend = RuntimeManager.CreateInstance(musicBlue.Guid);
        RuntimeManager.AttachInstanceToGameObject(_instanceAmbientIntro, transform);
        RuntimeManager.AttachInstanceToGameObject(_instanceAmbientDanger, transform);
        RuntimeManager.AttachInstanceToGameObject(_instanceFriend, transform);
        
        
        fadeOut.DOFade(0, 3).SetEase(Ease.InQuad);
        
        
        //now lets decide how to actually start the game
        Transform cp;
        if (howToStart != HowToStart.Default)
        {
            //if not default lets change our camera tracking
            cameraManager.ChangeCameraTracking();
            cameraManager.ChangePlayerRadius(30, true);
            ChangeGameState(GameState.Default);
            level2.SetActive(true);
            UpdateVolumeToLevel();
            playerRef.isHidden = true;
            
            _instanceAmbientDanger.start();
            _instanceAmbientDanger.release();
            _currentMusicIndex = 1;
        }
        else
        {
            _currentMusicIndex = 0;
            _instanceAmbientIntro.start();
            _instanceAmbientIntro.release();
        }
        switch (howToStart)
        {
            case HowToStart.Default:
                //on level 1
                ChangeGameState(GameState.Cinematic);
                _currentCheckPointIndex = 0;
                level1.StartLevel();
                break;
            case HowToStart.Checkpoint1:
                _currentCheckPointIndex = 1;
                break;
            case HowToStart.Checkpoint2:
                _currentCheckPointIndex = 2;
                break;
            case HowToStart.Checkpoint3:
                _currentCheckPointIndex = 3;
                break;
            
        }
        cp = checkPoints[_currentCheckPointIndex].GetSpawnPoint();
        playerRef.transform.position = cp.position;
    }

    /**
     * Changes the game state and takes care of anything that must be done when the game enters that state
     */
    public void ChangeGameState(GameState pNewState)
    {
        
        switch(pNewState) 
        { 
            case GameState.Cinematic:
                playerRef.ToggleInput(false);
                break;
            case GameState.Paused: 
                
                isPauseFading = true;
                uiPause.gameObject.SetActive(true);
                playerRef.ToggleInputMap(true);
                uiPause.DOFade(1, 0.2f).OnComplete(()=>{
                    Time.timeScale = 0;
                    isPauseFading = false;
                });
                break;
            case GameState.Default:
                playerRef.ToggleInput(true);
                if (_gameState == GameState.Paused)
                {
                    //it was paused
                    isPauseFading = true;
                    Time.timeScale = 1;
                    uiPause.DOFade(0, 0.2f).OnComplete(() =>
                    {
                        uiContinueBtn.Select();
                        uiPause.gameObject.SetActive(false);
                        isPauseFading = false;
                    });
                }
                else if(_gameState == GameState.Cinematic)
                    playerRef.ToggleInput(true);
                //always come back to player action map
                playerRef.ToggleInputMap(false);
                Time.timeScale = 1;
                break;

        }
        _gameState = pNewState;

    }

    //When player gets eaten
    public void GameOver()
    {
        ChangeGameState(GameState.Cinematic);
        IsPlayerDead = true;
        //MetricManagerScript.instance?.LogString("Death", "1");
        Sequence seq = DOTween.Sequence();
        seq.Append(fadeOut.DOFade(1, 2).SetEase(Ease.InCubic).OnComplete(
            () =>
            {
                //Put player on checkpoint
                if (_currentCheckPointIndex >= 0)
                {
                    Transform cp = checkPoints[_currentCheckPointIndex].GetSpawnPoint();
                    playerRef.transform.position = cp.position;
                }
                else
                {
                    playerRef.transform.position = _originPos;
                }
                //cameraManager.ResetTargetGroup();
                //fadeOut.alpha = 1;

            }
        ));
        seq.AppendCallback(() =>
        {
            ChangeGameState(GameState.Default);
            UpdateMonstersChasing(false,true);
            playerRef.OnRestartingGame();
            //OnRestartingGame?.Invoke();
        });
        seq.AppendInterval(1.5f);
        seq.Append(fadeOut.DOFade(0, 2).OnComplete(
            () =>
            {
                IsPlayerDead = false;
                numMonstersChasing = 0;
            }
        ));
       

    }

    public void UpdateMonstersChasing(bool _newMonster, bool reset = false)
    {
        if (reset)
        {
            numMonstersChasing = 0;
            _sfxMonsterChaseLoop.Stop();
        }
        else
        {
            
            if (_newMonster)
            {
                numMonstersChasing++;
                if (!_sfxMonsterChaseLoop.IsPlaying())
                {
                    _sfxMonsterChaseLoop.Play();
                }
            }
            else
            {
                numMonstersChasing--;
                if (numMonstersChasing < 1)
                {
                    _sfxMonsterChaseLoop.Stop();
                }
            }
        }
    }
    public void UpdateCheckPoint(CheckPoint cp)
    {
        int index = System.Array.IndexOf(checkPoints, cp);
        if (index > _currentCheckPointIndex)
        {
            _currentCheckPointIndex = index;
            MetricManagerScript.instance?.LogString("Checkpoint",index.ToString());
        }
    }

    //callback when controls changes to update the icons on the menu
   

    //This assumes we are comming from ambient intro, which might not work later lol
    public void ChangeBackgroundMusic(int newMusic)
    {
        switch (_currentMusicIndex)
        {
            case 0:
                _instanceAmbientIntro.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                break;
            case 1:
                _instanceAmbientDanger.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                break;
            case 2:
                _instanceFriend.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                break;
        }
        switch (newMusic)
        {
            case 0:
                _instanceAmbientIntro.start();
                break;
            case 1:
                _instanceAmbientDanger.start();
                break;
            case 2:
                _instanceFriend.start();
                break;
            
        }

        _currentMusicIndex = newMusic;
    }

    //Handles when to play the audio of monsters appearing
    public void HandleMonstersAppearSFX()
    {
        if(numMonstersOnScreen == 0 && !sfxMonsterAtDistance.IsNull)
        {
            FMODUnity.RuntimeManager.PlayOneShot(sfxMonsterAtDistance.Guid, playerRef.transform.position);
        }
    }
    
    public FMOD.Studio.EventInstance GetBlueMusicInstance()
    {
        return _instanceFriend;
    }

    public void OnPauseGame()
    {
        if (!isPauseFading)
        {
            //hold up the 0.2 it takes for the text fading animation to happen
            Debug.Log("Trying to pause/unpause");
            //only pause if we are not in a cinematic
            if (_gameState != GameState.Cinematic)
            {
                if(_gameState == GameState.Paused)
                    ChangeGameState(GameState.Default);
                else
                    ChangeGameState(GameState.Paused);
            }
        }
    }

    public void UIQuitGame()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }

    //update some volume properties to let the player know now is dangerous
    public void UpdateVolumeToLevel()
    {
        Vignette vig;
        ColorCurves colorCurves;
        Bloom bloom;

        if (_volume.profile.TryGet<Vignette>(out vig) && _volume.profile.TryGet<Bloom>(out bloom)
            && _volume.profile.TryGet<ColorCurves>(out colorCurves))
        {
            //colorCurves.red.value.MoveKey(1, new Keyframe(0.6f, 1.0f));
            float curveVal2 = 0.95f;
            Sequence seq = DOTween.Sequence()
                .Append(
                    DOTween.To(() => vig.intensity.value,
                        x => { vig.intensity.value = x; }, 0.2f, 3)
                )
                .Join(
                    DOTween.To(() => bloom.intensity.value,
                        x => { bloom.intensity.value = x; }, 2, 3)
                )
                .Join(
                    DOTween.To(() => curveVal2,
                        x => { colorCurves.green.value.MoveKey(1, new Keyframe(x, 1.0f)); }, 1, 3)
                );
        }
    }


}

