using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public static event Action PlayerOnControlsChanged;
    private PlayerInput _playerInput;
    private Vector2 _moveInputValue;
    public Vector2 MoveInputValue
    {
        get => _moveInputValue;
        set => _moveInputValue = value;
    }

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    #region InputSystem
    /**
     * Function triggeres when theres a movement input. 
     */
    private void OnMove(InputValue inputValue)
    {
        _moveInputValue = inputValue.Get<Vector2>();
    }

    private void OnSwim(InputValue inputValue)
    {
       /* if (inputValue.isPressed)
            Swim();*/
    }

    private void OnCall(InputValue inputValue)
    {
        _playerInput.actions.FindAction("Call").Disable();
        /*if (inputValue.isPressed)
            //StartCoroutine(PlayCallSFX()); **/
    }

    private void OnControlsChanged()
    {
        PlayerOnControlsChanged?.Invoke();
    }

    #endregion

}
