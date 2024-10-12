using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    public int index { get; set; }
    private Transform _spawnPoint;
    private Transform _blueSpawnPoint;

    private void Awake()
    {
        if (transform.childCount > 0)
        {
            _spawnPoint = transform.GetChild(0);
            if (transform.childCount > 1)
                _blueSpawnPoint = transform.GetChild(1);

        }
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

    public Transform GetBlueSpawnPoint()
    {
        return _blueSpawnPoint;
    }
}
