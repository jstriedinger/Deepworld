using System;
using UnityEngine;

public class TempPause : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayerCharacter.PlayerOnPause += OnPause;
    }

    private void OnEnable()
    {
        PlayerCharacter.PlayerOnPause += OnPause;
    }

    private void OnDisable()
    {
        PlayerCharacter.PlayerOnPause -= OnPause;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnPause()
    {
        Time.timeScale = Time.timeScale > 0 ? 0 : 1;
    }
}
