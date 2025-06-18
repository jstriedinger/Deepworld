using System;
using UnityEngine;

public class TempPause : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayerCharacter.OnPauseGame += OnPauseGame;
    }

    private void OnEnable()
    {
        PlayerCharacter.OnPauseGame += OnPauseGame;
    }

    private void OnDisable()
    {
        PlayerCharacter.OnPauseGame -= OnPauseGame;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnPauseGame()
    {
        Time.timeScale = Time.timeScale > 0 ? 0 : 1;
    }
}
