using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    public int index { get; set; }
    private Transform spawnPoint;

    private void Awake()
    {
        spawnPoint = transform.GetChild(0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("checkpoint");
            gameManager.UpdateCheckPoint(this);
        }
    }

    public Transform GetSpawnPoint()
    {
        return spawnPoint? spawnPoint : null;
    }
}
