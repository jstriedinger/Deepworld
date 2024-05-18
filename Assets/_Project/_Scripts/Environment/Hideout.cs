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

    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player") && !GameManager.IsPlayerDead)
        {
            MonsterPlayer monsterPlayer = collision.gameObject.GetComponent<MonsterPlayer>();
            monsterPlayer.isHidden = true;
            //Sound effect
            FMODUnity.RuntimeManager.PlayOneShot(configuration.SfxEnter, transform.position);

            Collider2D[] monsterHits = Physics2D.OverlapCircleAll(transform.position, 40, LayerMask.GetMask("Monster"));
            //lets tell the monster chasing that were near the shelter
            foreach (Collider2D monsterCollider in monsterHits)
            {
                EnemyMonster enemyMonster = monsterCollider.GetComponent<EnemyMonster>();
                enemyMonster?.UpdateMonsterState(MonsterState.Frustrated);
                
                BehaviorTree tree = monsterCollider.gameObject.GetComponent<BehaviorTree>();
                tree?.SendEvent("PlayerHidesDuringChase");

            }

            //metric handler
            MetricManagerScript.instance?.LogString("Hideout", "Enter");

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
    }

}

