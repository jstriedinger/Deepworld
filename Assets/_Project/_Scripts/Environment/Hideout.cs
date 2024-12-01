using System;
using System.Collections;

using System.Collections.Generic;

using BehaviorDesigner.Runtime;
using UnityEngine;

using Pathfinding;

using FMODUnity;
using UnityEngine.Serialization;

public class Hideout : MonoBehaviour
{
    [SerializeField] private CoverSO configuration;
    [SerializeField] private GameObject inside;
    [SerializeField] private GameObject outside;
    [SerializeField] private LayerMask trackerLayers;
    [SerializeField] private bool onlyCheckPlayer;
    private PolypTracker[] _insideTrackers;
    private PolypTracker[] _outsideTrackers;
    private GameManager _gameManager;


    private void Start()
    {
        _gameManager = FindFirstObjectByType<GameManager>();
        SetupTentacles();
    }
    
    
    private void SetupTentacles()
    {
        _insideTrackers = inside.GetComponentsInChildren<PolypTracker>();
        Transform target = GameObject.Find("Player").transform;
        foreach (PolypTracker tracker in _insideTrackers)
        {
            tracker.Setup(trackerLayers, onlyCheckPlayer ? target : null);
        }
        
        _outsideTrackers = outside.GetComponentsInChildren<PolypTracker>();
        foreach (PolypTracker tracker in _outsideTrackers)
        {
            tracker.Setup(trackerLayers, target);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if( collision.gameObject.CompareTag("Player") && !GameManager.IsPlayerDead)
        {
            PlayerCharacter playerCharacter = collision.gameObject.GetComponent<PlayerCharacter>();
            playerCharacter.isHidden = true;
            //Sound effect
            RuntimeManager.PlayOneShot(configuration.SfxEnter, transform.position);

            Collider2D[] monsterHits = Physics2D.OverlapCircleAll(transform.position, 40, LayerMask.GetMask("Monster"));
            //lets tell the monster chasing that were near the shelter
            foreach (Collider2D monsterCollider in monsterHits)
            {
                MonsterReactive monsterReactive = monsterCollider.GetComponent<MonsterReactive>();
                if (monsterReactive.CanReactToPlayer())
                {
                    BehaviorTree tree = monsterCollider.gameObject.GetComponent<BehaviorTree>();
                    monsterReactive?.UpdateMonsterState(MonsterState.Frustrated);
                    tree?.SendEvent("PlayerHidesDuringChase");
                    
                }
                

            }

            //metric handler
            MetricManagerScript.instance?.LogString("Hideout", "Enter");

        }
        else if (collision.gameObject.CompareTag("Blue"))
        {
            //just the sound
            RuntimeManager.PlayOneShot(configuration.SfxEnter, transform.position);
        }
    }



    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !GameManager.IsPlayerDead)
        {
            PlayerCharacter playerCharacter = collision.gameObject.GetComponent<PlayerCharacter>();
            playerCharacter.isHidden = false;
            FMODUnity.RuntimeManager.PlayOneShot(configuration.SfxExit, transform.position);
            //metric handler
            MetricManagerScript.instance?.LogString("Hideout", "Exit");

        }
        else if (collision.gameObject.CompareTag("Blue"))
        {
            //just the sound
            RuntimeManager.PlayOneShot(configuration.SfxExit, transform.position);
        }
    }

}

