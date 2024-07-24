using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    public int index { get; set; }
    private Transform _spawnPoint;

    private void Awake()
    {
        if(transform.childCount >= 1)
            _spawnPoint = transform.GetChild(0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            gameManager.UpdateCheckPoint(this);
        }
    }

    public Transform GetSpawnPoint()
    {
        return _spawnPoint? _spawnPoint : null;
    }
}
