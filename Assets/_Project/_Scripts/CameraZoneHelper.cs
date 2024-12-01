using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoneHelper : MonoBehaviour
{
    private CameraManager _cameraManager;
    [SerializeField] private float desiredRadius;
    [Range(0,2)]
    [SerializeField] private float desiredWeight;
    

    private bool inCamera;
    // Start is called before the first frame update
    void Start()
    {
        _cameraManager = FindFirstObjectByType<CameraManager>();
        inCamera = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && !GameManager.IsPlayerDead && inCamera)
        {
            //add to target group 
            _cameraManager.RemoveObjectFromCameraView(gameObject.transform, false);
            inCamera = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!inCamera && other.gameObject.CompareTag("Player") && !GameManager.IsPlayerDead )
        {
            //add to target group 
            _cameraManager.AddObjectToCameraView(gameObject.transform, false, true, desiredRadius, desiredWeight);
            inCamera = true;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        //extra thing to check in case of death and restart
        if (!inCamera && other.gameObject.CompareTag("Player") && !GameManager.IsPlayerDead )
        {
            //add to target group 
            _cameraManager.AddObjectToCameraView(gameObject.transform, false, false, desiredRadius, desiredWeight);
            inCamera = true;
        }
    }
}
