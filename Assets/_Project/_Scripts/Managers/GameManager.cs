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
        Checkpoint4
    }

    public static event Action OnRestartingGame;
    
    [Header("Systems")]
    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private UIManager uiManager;
    

    [Header("Level management")] 
    [SerializeField] private StartSection startSection;
    [SerializeField] private Level1Manager level1;
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
        foreach (GameObject levelSection in levelSections)
        {
            levelSection.SetActive(false);
        }
        
        _originPos = playerRef.transform.position;
        IsPlayerDead = false;
        _gameState = GameState.Cinematic;

        playerLastPosition.transform.position = _originPos;
        
        //bind pause
        
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


    void Start()
    {
        _currentCheckPointIndex = (int)startSection;
        Debug.Log("Checkpoint is: "+(int)startSection);
        //now lets decide how to actually start the game
        Transform cp;
        if (startSection != StartSection.Default)
        {
            //if not default lets change our camera tracking
            cameraManager.ChangeCameraTracking();
            cameraManager.ChangePlayerRadius(30, true);
            ChangeGameState(GameState.Default);
            playerRef.isHidden = true;
            

            if (startSection != StartSection.Checkpoint1)
            {
                audioManager.ChangeBackgroundMusic(1);
                DOTween.To(() => globalPlayerLight.intensity, x => globalPlayerLight.intensity = x, 0.15f, 1f);
                levelSections[2].SetActive(true);
            }
            else
            {
                ///for now the 3 section is just the "second" level
                levelSections[0].SetActive(true);
            }
        }
        else
        {
            audioManager.ChangeBackgroundMusic(0);
        }
        
        switch (startSection)
        {
            case StartSection.Default:
                //on level 1
                ChangeGameState(GameState.Cinematic);
                level1.StartLevel();
                break;
            case StartSection.Checkpoint1:
                levelSections[_currentCheckPointIndex].SetActive(true);
                break;
            case StartSection.Checkpoint2:
                break;
            case StartSection.Checkpoint3:
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
                    uiManager.PauseGame(true);
                    break;
                case GameState.Default:
                    playerRef.ToggleInput(true);
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
                if(_gameState == GameState.Paused)
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

    public void StartGame()
    {
        uiManager.StartGame();
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

    //adds an enemy to our camera so player can see them
    public void AddEnemyToCameraView(GameObject monsterToAdd)
    {
        
        cameraManager.AddMonsterToView(monsterToAdd);
    }


    //update lighting properties for dangerous level
    public void UpdateLightingLevel()
    {
        DOTween.To(() => globalPlayerLight.intensity, x => globalPlayerLight.intensity = x, 0.15f, 1f);
        //globalPlayerLight.intensity = 0.2f;
        
    }
    

}

