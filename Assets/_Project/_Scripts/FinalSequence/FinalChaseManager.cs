using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class FinalChaseManager : MonoBehaviour
{
    [Header("Part1")] 
    [SerializeField] private MonsterCinematic[] monsters;
    [SerializeField] private GameObject blockRock;
    [SerializeField] private BlueNPC blueNpc;
    
    private Vector3[] _monstersPos;
        
    
    // Start is called before the first frame update
    private void OnEnable()
    {
        int index = 0;
        _monstersPos = new Vector3[monsters.Length];
        foreach (MonsterCinematic monsterCinematic in monsters)
        {
            _monstersPos[index] = monsterCinematic.transform.position;
            index++;
        }
        //blueNpc.ToggleFollow(true);
        GameManager.OnRestartingGame += ResetFinalChase;
    }

    private void OnDisable()
    {
        GameManager.OnRestartingGame -= ResetFinalChase;
    }

    //manually remove the action
    public void RemoveRestartAction()
    {
        GameManager.OnRestartingGame -= ResetFinalChase;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /**
     * Reset actin when player dies in Final Chase Pt1
     */
    public void ResetFinalChase()
    {
        int index = 0;
        foreach (MonsterCinematic monsterCinematic in monsters)
        {
            monsterCinematic.TogglePursuit(false);
            monsterCinematic.transform.position = _monstersPos[index];
            index++;
        }
        blueNpc.ToggleFollow(true);
        
    }
    
    /**
     * Used for final chase pt1. Meaning the first 2 monsters
     */
    public void Pt1TriggerMonsterChase(int index)
    {
        if(index == 1)
            monsters[0].TogglePursuit(true);
        else if(index == 2)
            monsters[1].TogglePursuit(true);
    }

    public void Pt1EndMonsterChase()
    {
        StartCoroutine(EndPt1Chase());
        // now we want to wait a little and make them go back
    }

    /**
     * Trigger the monsters of the second part of the final chase
     */
    public void Pt2TriggerMonsters(int index)
    {
        switch (index)
        {
            case 1 :
                monsters[2].TogglePursuit(true);
                monsters[3].TogglePursuit(true);
                break;
            case 2:
                monsters[4].TogglePursuit(true);
                break;
            case 3:
                monsters[5].TogglePursuit(true);
                break;
            case 4:
                monsters[6].TogglePursuit(true);
                break;
        }
    }

    private IEnumerator EndPt1Chase()
    {
        blockRock.SetActive(true);
        monsters[0].TogglePursuit(false);
        monsters[1].TogglePursuit(false);
        yield return new WaitForSeconds(1);
        monsters[0].GoToPosition(_monstersPos[0]);
        monsters[1].GoToPosition(_monstersPos[1]);

    }
}
