using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using FMODUnity;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.InputSystem;

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
    
    [Header("Systems")]
    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private AudioManager audioManager;

    [Header("Level management")] 
    [SerializeField] private HowToStart howToStart;
    [SerializeField] private Level1Manager level1;
    [SerializeField] private GameObject level2;
    [SerializeField] CheckPoint[] checkPoints;

    [Header("Others")] 
    [SerializeField] private Light2D globalPlayerLight;
    public MonsterPlayer playerRef;
    public GameObject playerLastPosition;
    [SerializeField] private Volume _volume;

    
    [Header("UI")]
    [SerializeField] private CanvasGroup fadeOut;
    [SerializeField] private CanvasGroup uiPause;
    [SerializeField] private Button uiContinueBtn;
    private bool isPauseFading = false;



    [HideInInspector]
    public static bool IsPlayerDead;
    private int _currentCheckPointIndex = -1;
    private GameState _gameState;
    private Vector3 _originPos;

    // Start is called before the first frame update
    private void Awake()
    {
        //UI stuff
        fadeOut.gameObject.SetActive(true);
        uiPause.gameObject.SetActive(false);
        uiPause.alpha = 0;
        
        _originPos = playerRef.transform.position;
        IsPlayerDead = false;
        _gameState = GameState.Cinematic;

        playerLastPosition.transform.position = playerRef.transform.position;
        
        //bind pause
        MonsterPlayer.PlayerOnPause += OnPauseGame;
        
        audioManager.Initialize(playerRef.transform);
    }


    void Start()
    {
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
            //UpdateVolumeToLevel();
            playerRef.isHidden = true;
            
            audioManager.ChangeBackgroundMusic(1);
            DOTween.To(() => globalPlayerLight.intensity, x => globalPlayerLight.intensity = x, 0.15f, 1f);

        }
        else
        {
            level2.SetActive(false);
            audioManager.ChangeBackgroundMusic(0);
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
                Gamepad.current?.PauseHaptics();
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
            default:
                Gamepad.current?.ResumeHaptics();
                break;

        }
        _gameState = pNewState;

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
                cameraManager.ResetTargetGroup();
                //fadeOut.alpha = 1;

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
        seq.Append(fadeOut.DOFade(0, 2).OnComplete(
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

