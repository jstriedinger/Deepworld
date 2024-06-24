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
    private GameManager _gameManager;


    private void Start()
    {
        _gameManager = FindFirstObjectByType<GameManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if( collision.gameObject.CompareTag("Player") && !GameManager.IsPlayerDead)
        {
            MonsterPlayer monsterPlayer = collision.gameObject.GetComponent<MonsterPlayer>();
            monsterPlayer.isHidden = true;
            //Sound effect
            RuntimeManager.PlayOneShot(configuration.SfxEnter, transform.position);

            Collider2D[] monsterHits = Physics2D.OverlapCircleAll(transform.position, 40, LayerMask.GetMask("Monster"));
            //lets tell the monster chasing that were near the shelter
            foreach (Collider2D monsterCollider in monsterHits)
            {
                EnemyMonster enemyMonster = monsterCollider.GetComponent<EnemyMonster>();
                if (enemyMonster.IsGameplayActiveMonster())
                {
                    BehaviorTree tree = monsterCollider.gameObject.GetComponent<BehaviorTree>();
                    enemyMonster?.UpdateMonsterState(MonsterState.Frustrated);
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
            MonsterPlayer monsterPlayer = collision.gameObject.GetComponent<MonsterPlayer>();
            monsterPlayer.isHidden = false;
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

