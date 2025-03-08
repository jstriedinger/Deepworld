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
    public static GameManager Instance { get; private set; }

   
    private enum StartSection
    {
        Default,
        Checkpoint1,
        Checkpoint2,
        Checkpoint3,
        Checkpoint4,
        Checkpoint5,
        Checkpoint6,
        Checkpoint7,
        
    }

    public static event Action OnRestartingGame;
    
    [Header("Level management")] 
    [SerializeField] private StartSection startSection;
    [SerializeField] private GameObject[] levelSections;
    [SerializeField] CheckPoint[] checkPoints;

    [Header("Others")] 
    [SerializeField] private Light2D globalPlayerLight;
    [SerializeField] private Light2D globalEnvLight;
    [SerializeField] private Light2D globalPropsLight;
    public PlayerCharacter playerRef;
    public GameObject playerLastPosition;
    public BlueNPC blueNpcRef;

    public bool skipTemp = true;
    [HideInInspector]
    public bool isPlayerDead;
    private int _currentCheckPointIndex = -1;
    private GameState _gameState;
    private Vector3 _originPos;

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

    private void OnEnable()
    {
        PlayerCharacter.PlayerOnPause += TogglePauseGame;
    }

    private void OnDisable()
    {
        PlayerCharacter.PlayerOnPause -= TogglePauseGame;
    }
    
    public void StartGame()
    {
        UIManager.Instance.HideMainMenu();
        CinematicsManager.Instance.DoCinematicStartGame();
        AudioManager.Instance.OnStartGame();
    }


    void Start()
    {
        //framerate
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        
        //Setting up
        _originPos = playerRef.transform.position;
        isPlayerDead = false;
        _gameState = GameState.Cinematic;
        playerLastPosition.transform.position = _originPos;
        
        AudioManager.Instance.Initialize(playerRef.transform);

        Cursor.visible = false;
        if (!skipTemp)
        {
            _currentCheckPointIndex = (int)startSection;
            //now lets decide how to actually start the game
            Transform cp;
            if (startSection != StartSection.Default)
            {
                //if not default lets change our camera tracking
                CameraManager.Instance.ChangeCameraTracking();
                //CameraManager.Instance.ChangePlayerRadius(30, true);
                ChangeGameState(GameState.Default);
                playerRef.isHidden = true;
                
            }

            if (_currentCheckPointIndex >= 3)
            {
                UIManager.Instance.SetupWorldUIForTitles();
                UIManager.Instance.OnControlsChanged();
                ToggleLigthing(true);
                playerRef.ToggleMonsterEyeDetection(true);
                playerRef.ToggleEyeFollowTarget(true);
                AudioManager.Instance.ChangeBackgroundMusic(5);
                
            }

            if (_currentCheckPointIndex == 6)
            {
                AudioManager.Instance.ToggleCanPlayDangerMusic(false);
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
                    
                    CinematicsManager.Instance.DoCinematicTitles();
                    break;
                case StartSection.Checkpoint1:
                    UIManager.Instance.isWorldUiActive = true;
                    AudioManager.Instance.ChangeBackgroundMusic(1);
                    CinematicsManager.Instance.PrepareBlueForMeetup();
                    //blueNpcRef.ChangeBlueStats(playerRef.transform);
                    break;
                case StartSection.Checkpoint2:
                    UIManager.Instance.isWorldUiActive = true;
                    AudioManager.Instance.ChangeBackgroundMusic(-1);
                    //place blue in the first point of path
                    CinematicsManager.Instance.PrepareBlueForMonsterEncounter();
                    break;
                case StartSection.Checkpoint3:
                    playerRef.ToggleMonsterEyeDetection(true);
                    playerRef.ToggleEyeFollowTarget(true);
                    break;
                case StartSection.Checkpoint4:
                    break;
                case StartSection.Checkpoint5:
                    break;
                case StartSection.Checkpoint6:
                    playerRef.SetBlueReference(blueNpcRef);
                    blueNpcRef.ToggleFollow(true);
                    blueNpcRef.GetHurt();
                    blueNpcRef.ChangeBlueStats(playerRef.transform);
                    break;
                case StartSection.Checkpoint7:
                    playerRef.SetBlueReference(blueNpcRef);
                    blueNpcRef.ToggleFollow(true);
                    blueNpcRef.GetHurt();
                    blueNpcRef.ChangeBlueStats(playerRef.transform);
                    break;
                
            }
            cp = checkPoints[_currentCheckPointIndex].GetSpawnPoint();
            playerRef.transform.position = cp.position;
            Transform blueSpawn = checkPoints[_currentCheckPointIndex].GetBlueSpawnPoint();
            if (blueSpawn)
                blueNpcRef.transform.position = blueSpawn.position;
            
        }
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
                    AudioManager.Instance.TogglePauseAudio(true);
                    UIManager.Instance.PauseGame(true);
                    break;
                case GameState.Default:
                    AudioManager.Instance.TogglePauseAudio(false);
                    playerRef.ToggleInputMap(false);
                    if (_gameState == GameState.Paused)
                    {
                        UIManager.Instance.PauseGame(false);
                    }
                    
                    //always come back to playerCharacter action map
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

    //When playerCharacter gets eaten
    public void GameOver(GameObject monster)
    {
        isPlayerDead = true;
        CameraManager.Instance.OnGameOver(monster);
        ChangeGameState(GameState.Cinematic);
        //MetricManagerScript.instance?.LogString("Death", "1");
        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(1);
        seq.Append(UIManager.Instance.blackout.DOFade(1, 2).SetEase(Ease.InCubic).OnComplete(
            () =>
            {
                //Put playerCharacter on checkpoint
                if (_currentCheckPointIndex >= 0)
                {
                    Transform cp = checkPoints[_currentCheckPointIndex].GetSpawnPoint();
                    Transform blueSpawn = checkPoints[_currentCheckPointIndex].GetBlueSpawnPoint();
                    playerRef.transform.position = cp.position;
                    if (blueSpawn)
                        blueNpcRef.transform.position = blueSpawn.position;
                }
                else
                {
                    playerRef.transform.position = _originPos;
                }
                CameraManager.Instance.ResetTargetGroup();

            }
        ));
        seq.AppendCallback(() =>
        {
            ChangeGameState(GameState.Default);
            AudioManager.Instance.UpdateMonstersChasing(false,true);
            //playerRef.OnRestartingGame();
            OnRestartingGame?.Invoke();
        });
        seq.AppendInterval(1.5f);
        seq.Append(UIManager.Instance.blackout.DOFade(0, 2).OnComplete(
            () =>
            {
                isPlayerDead = false;
                AudioManager.Instance.numMonstersChasing = 0;
            }
        ));
       

    }

    public void ResetGame()
    {
        //stop all music
        AudioManager.Instance.StopAllFMODInstances();
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
        if (!UIManager.Instance.isPauseFading)
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
        UIManager.Instance.ShowMainMenu();
        
    }

    public void UIShowCredits()
    {
        UIManager.Instance.ShowMenuCredits();
    }

    public void UIShowMenu()
    {
        UIManager.Instance.ShowMainMenu();
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
        Destroy(levelSections[level]);
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

    //Change the global lighting of the game.
    public void ToggleLigthing(bool toogle)
    {
        
        float envLight, playerLight, propsLight;
        if (toogle)
        {
            envLight = 0.3f;
            propsLight = 0.35f;
            playerLight = .5f;
        }
        else
        {
            envLight = 0.6f;
            propsLight = 0.6f;
            playerLight = .7f;
        }
        Sequence seq = DOTween.Sequence();
        seq.Append(DOTween.To(() => globalEnvLight.intensity, x => globalEnvLight.intensity = x, 
            envLight, 1f));
        seq.Join(DOTween.To(() => globalPlayerLight.intensity, x => globalPlayerLight.intensity = x, 
            playerLight, 1f));
        seq.Join(DOTween.To(() => globalPropsLight.intensity, x => globalPropsLight.intensity = x, 
            propsLight, 1f));
        

    }

}

