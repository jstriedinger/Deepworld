using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using FMODUnity;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public enum GameState
{
    Default,
    Paused,
    MainMenu,
    Cinematic
}


public class GameManager : MonoBehaviour
{
    private enum StartSection
    {
        Default,
        Checkpoint1,
        Checkpoint2,
        Checkpoint3,
        Checkpoint4,
        Checkpoint5,
        
    }

    public static event Action OnRestartingGame;
    
    [Header("Systems")]
    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private CinematicsManager cinematicsManager;
    

    [Header("Level management")] 
    [SerializeField] private StartSection startSection;
    [SerializeField] private GameObject[] levelSections;
    [SerializeField] CheckPoint[] checkPoints;

    [Header("Others")] 
    [SerializeField] private Light2D globalPlayerLight;
    public MonsterPlayer playerRef;
    public GameObject playerLastPosition;

    
    [HideInInspector]
    public static bool IsPlayerDead;
    private int _currentCheckPointIndex = -1;
    private GameState _gameState;
    private Vector3 _originPos;

    // Start is called before the first frame update
    private void Awake()
    {
        //all sections are off except for the first one
        
        
        _originPos = playerRef.transform.position;
        IsPlayerDead = false;
        _gameState = GameState.Cinematic;

        playerLastPosition.transform.position = _originPos;
        
        audioManager.Initialize(playerRef.transform);
        
        
        
    }

    private void OnEnable()
    {
        MonsterPlayer.PlayerOnPause += TogglePauseGame;
    }

    private void OnDisable()
    {
        MonsterPlayer.PlayerOnPause -= TogglePauseGame;
    }
    
    public void StartGame()
    {
        uiManager.HideMainMenu();
        cinematicsManager.DoCinematicStartGame();
    }


    void Start()
    {
        uiManager.Initialize(playerRef);
        _currentCheckPointIndex = (int)startSection;
        //now lets decide how to actually start the game
        Transform cp;
        if (startSection != StartSection.Default)
        {
            //if not default lets change our camera tracking
            cameraManager.ChangeCameraTracking();
            cameraManager.ChangePlayerRadius(30, true);
            ChangeGameState(GameState.Default);
            playerRef.isHidden = true;
            

            if (_currentCheckPointIndex > 2)
            {
                DOTween.To(() => globalPlayerLight.intensity, x => globalPlayerLight.intensity = x, 0.25f, 1f);
            }
            
        }
        
		//Prepare everything to start from a checkpoint or something
        switch (startSection)
        {
            case StartSection.Default:
                //playing the way it is supposed to be played
                //by default all sections are disable except the first one
                for (int i = 1; i < levelSections.Length; i++)
                {
                    levelSections[i].SetActive(false);
                }
                LoadLevelSection(0);
                ChangeGameState(GameState.Cinematic);
                audioManager.ChangeBackgroundMusic(1);
                cinematicsManager.DoCinematicTitles();
                break;
            case StartSection.Checkpoint1:
                uiManager.isWorldUiActive = true;
                audioManager.ChangeBackgroundMusic(1);
                cinematicsManager.PrepareBlueForMeetup();
                break;
            case StartSection.Checkpoint2:
                uiManager.isWorldUiActive = true;
                audioManager.ChangeBackgroundMusic(3);
                //place blue in the first point of path
                cinematicsManager.PrepareBlueForMonsterEncounter();
                break;
            case StartSection.Checkpoint3:
                playerRef.ToggleMonsterEyeDetection(true);
                playerRef.ToggleEyeFollowTarget(true);
                audioManager.ChangeBackgroundMusic(5);
                break;
            case StartSection.Checkpoint4:
                playerRef.ToggleMonsterEyeDetection(true);
                playerRef.ToggleEyeFollowTarget(true);
                audioManager.ChangeBackgroundMusic(5);
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
        if (pNewState != _gameState)
        {
            switch(pNewState) 
            { 
                case GameState.MainMenu:
                    playerRef.ToggleInputMap(true);
                    playerRef.ToggleInput(true);
                    break;
                case GameState.Cinematic:
                    playerRef.ToggleInput(false);
                    break;
                case GameState.Paused:
                    playerRef.ToggleInputMap(true);
                    audioManager.TogglePauseAudio(true);
                    uiManager.PauseGame(true);
                    break;
                case GameState.Default:
                    audioManager.TogglePauseAudio(false);
                    playerRef.ToggleInputMap(false);
                    if (_gameState == GameState.Paused)
                    {
                        uiManager.PauseGame(false);
                    }
                    
                    //always come back to player action map
                    playerRef.ToggleInputMap(false);
                    Time.timeScale = 1;
                    break;
                default:
                    Gamepad.current?.ResumeHaptics();
                    break;

            }
            _gameState = pNewState;
            
        }

    }

    //When player gets eaten
    public void GameOver(GameObject monster)
    {
        IsPlayerDead = true;
        cameraManager.OnGameOver(monster);
        ChangeGameState(GameState.Cinematic);
        //MetricManagerScript.instance?.LogString("Death", "1");
        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(1);
        seq.Append(uiManager.blackout.DOFade(1, 2).SetEase(Ease.InCubic).OnComplete(
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
                cameraManager.ResetTargetGroup();

            }
        ));
        seq.AppendCallback(() =>
        {
            ChangeGameState(GameState.Default);
            audioManager.UpdateMonstersChasing(false,true);
            playerRef.OnRestartingGame();
            //OnRestartingGame?.Invoke();
        });
        seq.AppendInterval(1.5f);
        seq.Append(uiManager.blackout.DOFade(0, 2).OnComplete(
            () =>
            {
                IsPlayerDead = false;
                audioManager.numMonstersChasing = 0;
            }
        ));
       

    }

    public void ResetGame()
    {
        //stop all music
        audioManager.StopAllFMODInstances();
        SceneManager.LoadScene(0);
        _currentCheckPointIndex = 0;
        
    }

    
    public void UpdateCheckPoint(CheckPoint cp)
    {
        int index = System.Array.IndexOf(checkPoints, cp);
        if (index > _currentCheckPointIndex)
        {
            _currentCheckPointIndex = index;
        }
    }
    
    #region UI
    public void TogglePauseGame()
    {
        if (!uiManager.isPauseFading)
        {
            //only pause if we are not in a cinematic
            if (_gameState != GameState.Cinematic && _gameState != GameState.MainMenu)
            {
                if (_gameState == GameState.Paused)
                    ChangeGameState(GameState.Default);
                else
                    ChangeGameState(GameState.Paused);
            }
        }
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                        Application.Quit();
        #endif
    }

    public void ShowMainMenuFirstTime()
    {
        ChangeGameState(GameState.MainMenu);
        uiManager.ShowMainMenu();
        
    }

    public void UIShowCredits()
    {
        uiManager.ShowMenuCredits();
    }

    public void UIShowMenu()
    {
        uiManager.ShowMainMenu();
    }
    
    
    
    #endregion


    public void LoadLevelSection(int level)
    {
        //depends on the section multiples things get activated and deactivated
        levelSections[level].SetActive(true);
        if (level == 0)
        {
            //also L2
            levelSections[1].SetActive(true);
        }
        
    }

    public void UnloadLevelSection(int level)
    {
        levelSections[level].SetActive(false);
    }

    private void OnApplicationQuit()
    {
        Gamepad.current?.PauseHaptics();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if(pauseStatus)
            Gamepad.current?.PauseHaptics();
        else
            Gamepad.current?.ResetHaptics();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if(!hasFocus)
            Gamepad.current?.PauseHaptics();
        else
            Gamepad.current?.ResetHaptics();
    }

}

