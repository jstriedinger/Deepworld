using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseStart : MonoBehaviour
{

    [SerializeField] private GameObject monster;
    [SerializeField] private GameObject player;

    private void OnTriggerEnter2D(Collider2D col){
        if(col.gameObject == player){
            monster.SetActive(true);
            
        }
        //Debug.Log("Monster Spawn!");
    }
}
