using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoneHelper : MonoBehaviour
{
    [SerializeField] private float desiredRadius;
    [Range(0,2)]
    [SerializeField] private float desiredWeight;

    [SerializeField] private bool makeNoise = true;

    private bool _trigerred;
    private bool _inCamera;
    // Start is called before the first frame update
    void Start()
    {
        _inCamera = false;
        _trigerred = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && !GameManager.Instance.isPlayerDead && _inCamera)
        {
            //add to target group 
            CameraManager.Instance.RemoveObjectFromCameraView(gameObject.transform, false);
            _inCamera = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_inCamera && other.gameObject.CompareTag("Player") && !GameManager.Instance.isPlayerDead )
        {
            //add to target group 
            CameraManager.Instance.AddObjectToCameraView(gameObject.transform, false,
                (!_trigerred && makeNoise) ? true : false, desiredRadius, desiredWeight);
            _inCamera = true;
            _trigerred = true;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        //extra thing to check in case of death and restart
        if (!_inCamera && other.gameObject.CompareTag("Player") && !GameManager.Instance.isPlayerDead )
        {
            //add to target group 
            CameraManager.Instance.AddObjectToCameraView(gameObject.transform, false, false, desiredRadius, desiredWeight);
            _inCamera = true;
        }
    }
}
