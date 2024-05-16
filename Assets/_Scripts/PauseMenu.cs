using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    private bool isPaused;
    public PlayerInput playerInput;


    void Start()
    {
        pauseMenu.SetActive(false);
    }

    
    void Update()
    {
        //This is where we should check for a MenuOpenClose input.
        //The isPaused bool determines which script we run.\
        if(Input.GetKeyDown("escape") || Input.GetButtonDown("pause") || Input.GetButtonDown("start")){
            if(isPaused){
                PauseGame();
            }
            else{
                ResumeGame();
            }
        }
        
    }

    public void PauseGame(){

        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame(){
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void QuitGame(){
        Application.Quit();
    }
}
