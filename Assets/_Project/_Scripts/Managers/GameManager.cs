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

    [Header("Global")] 
    public LayerMask playerLayer;
    public PlayerCharacter playerRef;
    public GameObject playerLastPosition;
    public BlueNPC blueNpcRef;
    
    [Header("Level management")] 
    [SerializeField] private StartSection startSection;
    [SerializeField] private GameObject level0Objects;
    [SerializeField] private GameObject[] levelSections;
    [SerializeField] CheckPoint[] checkPoints;
    
    [Header("Flocks in game")]
    [SerializeField] BoidFlockJob[] flocksSection0;
    [SerializeField] GameObject coverSection0;
    [SerializeField] BoidFlockJob[] flocksSection1_1;
    [SerializeField] BoidFlockJob[] flocksSection1_2;

    [Header("Lighting")] 
    [SerializeField] private Light2D globalPlayerLight;
    [SerializeField] private Light2D globalEnvLight;
    [SerializeField] private Light2D globalPropsLight;

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
        PlayerCharacter.OnPauseGame += TogglePauseGameGame;
    }

    private void OnDisable()
    {
        PlayerCharacter.OnPauseGame -= TogglePauseGameGame;
        DOTween.KillAll();
    }
    
    public void StartGame()
    {
        CinematicsManager.Instance.DoCinematicStartGame();
        AudioManager.Instance.OnStartGame();
        LoadLevelSection(1);
        ToggleFlocksSection1_1(true);
        //cleanup and opti for fishbowl
        Destroy(level0Objects);
        StartCoroutine(UIManager.Instance.HideMainMenu());
    }


    void Start()
    {
        //framerate
        //QualitySettings.vSyncCount = 0;
        //Application.targetFrameRate = 60;
        
        //Setting up
        _originPos = playerRef.transform.position;
        isPlayerDead = false;
        _gameState = GameState.Cinematic;
        playerLastPosition.transform.position = _originPos;
        
        AudioManager.Instance.Initialize(playerRef.transform);

        //all flocks start off
        TurnOffOAllFlocks();

        //Cursor.visible = false;
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
            ToggleLigthing(false);

            if (_currentCheckPointIndex >= 3 )
            {
                UIManager.Instance.SetupWorldUIForTitles();
                UIManager.Instance.OnControlsChanged();
                ToggleLigthing(true);
                playerRef.ToggleMonsterEyeDetection(true);
                playerRef.ToggleEyeFollowTarget(true);
                
            }

            if (_currentCheckPointIndex >= 3 && _currentCheckPointIndex < 6)
            {
                AudioManager.Instance.ChangeBackgroundMusic(5);
            }

            if (_currentCheckPointIndex == 6)
            {
                AudioManager.Instance.ToggleCanPlayDangerMusic(false);
                AudioManager.Instance.ToggleCloseDangerMusic(false);
            }
            
            //always unload everything first
            for (int i = 1; i < levelSections.Length; i++)
            {
                levelSections[i].SetActive(false);
            }
            
		    //Prepare everything to start from a checkpoint or something
            switch (startSection)
            {
                case StartSection.Default:
                    //playing the way it is supposed to be played
                    //by default all sections are disable except the first one
                    
                    LoadLevelSection(0);
                    //ToggleFlocksSection0(true);
                    ChangeGameState(GameState.Cinematic);
                    //CameraManager.Instance?.ToggleConfiner2D(false);
                    CinematicsManager.Instance.DoCinematicTitles();
                    break;
                case StartSection.Checkpoint1:
                    LoadLevelSection(0);
                    LoadLevelSection(1);
                    ToggleFlocksSection1_1(true);
                    AudioManager.Instance.ChangeBackgroundMusic(1);
                    CinematicsManager.Instance.PrepareBlueForMeetup();
                    //blueNpcRef.ChangeBlueStats(playerRef.transform);
                    //temp for testing fishbowl
                    playerRef.SetBlueReference(blueNpcRef);
                    playerRef.ToggleEyeFollowTarget(true,blueNpcRef.transform);
                    blueNpcRef.ToggleEyeFollowTarget(true,playerRef.transform);
                    CameraManager.Instance.UpdatePlayerRadius(4,true);
                    CameraManager.Instance.AddObjectToCameraView(GameManager.Instance.blueNpcRef.transform,false,false,CameraManager.Instance.camZoomPlayer,1);
                    blueNpcRef.ToggleFollow(true);
                    blueNpcRef.ToggleReactToCall(true);
                    break;
                case StartSection.Checkpoint2:
                    LoadLevelSection(3);
                    AudioManager.Instance.ChangeBackgroundMusic(-1);
                    //place blue in the first point of path
                    CinematicsManager.Instance.PrepareBlueForMonsterEncounter();
                    break;
                case StartSection.Checkpoint3:
                    LoadLevelSection(4);
                    playerRef.ToggleMonsterEyeDetection(true);
                    playerRef.ToggleEyeFollowTarget(true);
                    break;
                case StartSection.Checkpoint4:
                    LoadLevelSection(4);
                    playerRef.ToggleMonsterEyeDetection(true);
                    playerRef.ToggleEyeFollowTarget(true);
                    break;
                case StartSection.Checkpoint5:
                    LoadLevelSection(4);
                    playerRef.ToggleMonsterEyeDetection(true);
                    playerRef.ToggleEyeFollowTarget(true);
                    break;
                case StartSection.Checkpoint6:
                    LoadLevelSection(5); //for now
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
    public void TogglePauseGameGame()
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
        Debug.Log("Show main menu");
        ChangeGameState(GameState.MainMenu);
        UIManager.Instance.ShowMainMenu();
        UIManager.Instance?.SelectStartButtonMainMenu();
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


    #region worldLoading
    public void LoadLevelSection(int level)
    {
        //depends on the section multiples things get activated and deactivated
        levelSections[level].SetActive(true);
        
    }

    public void UnloadLevelSection(int level)
    {
        levelSections[level].SetActive(false);
        //Destroy(levelSections[level]);
    }
    
    #endregion

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
    // true deep, false default
    public void ToggleLigthing(bool toogle)
    {
        
        /*float envLight, playerLight, propsLight;
        if (toogle)
        {
            envLight = 0.4f;
            propsLight = 0.4f;
            playerLight = .5f;
        }
        else
        {
            envLight = 0.5f;
            propsLight = 0.5f;
            playerLight = .6f;
        }
        Sequence seq = DOTween.Sequence();
        seq.Append(DOTween.To(() => globalEnvLight.intensity, x => globalEnvLight.intensity = x, 
            envLight, 1f));
        seq.Join(DOTween.To(() => globalPlayerLight.intensity, x => globalPlayerLight.intensity = x, 
            playerLight, 1f));
        seq.Join(DOTween.To(() => globalPropsLight.intensity, x => globalPropsLight.intensity = x, 
            propsLight, 1f));*/
        

    }

    public void TempTeleportToFinal()
    {
        UnloadLevelSection(4);
        LoadLevelSection(5);
        playerRef.SetBlueReference(blueNpcRef);
        blueNpcRef.gameObject.SetActive(true);
        blueNpcRef.ToggleFollow(true);
        blueNpcRef.GetHurt();
        blueNpcRef.ChangeBlueStats(playerRef.transform);

        CheckPoint cp = checkPoints[6];
        Transform sp = cp.GetSpawnPoint();
        playerRef.transform.position = cp.GetSpawnPoint().position;
        Transform blueSpawn = cp.GetBlueSpawnPoint();
        if (blueSpawn)
            blueNpcRef.transform.position = blueSpawn.position;
    }

    #region  opti

    //for performance

    public void TurnOffOAllFlocks()
    {
        foreach (BoidFlockJob flock in flocksSection0)
        {
            flock.ToggleActivity(false);
        };
        foreach (BoidFlockJob flock in flocksSection1_1)
        {
            flock.ToggleActivity(false);
        };
        foreach (BoidFlockJob flock in flocksSection1_2)
        {
            flock.ToggleActivity(false);
        };
    }
    public void ToggleFlocksSection0(bool toggle)
    {
        foreach (BoidFlockJob flock in flocksSection0)
        {
            flock.ToggleActivity(toggle);
        };
        coverSection0.SetActive(false);
        UnloadLevelSection(0);
    }
    public void ToggleFlocksSection1_1(bool toggle)
    {
        foreach (BoidFlockJob flock in flocksSection1_1)
        {
            flock.ToggleActivity(toggle);
        };
    }
    public void ToggleFlocksSection1_2(bool toggle)
    {
        foreach (BoidFlockJob flock in flocksSection1_2)
        {
            flock.ToggleActivity(toggle);
        };
    }

    #endregion

}

