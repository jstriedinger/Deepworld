using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    public int index { get; set; }
    private Transform spawnPoint;
    private GameManager gameManager;

    private void Awake()
    {
        spawnPoint = transform.GetChild(0);
    }

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
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
