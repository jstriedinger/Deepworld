using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class FinalChaseManager : MonoBehaviour
{
    [Header("Part1")] 
    [SerializeField] private MonsterCinematic[] monsters;
    [SerializeField] private GameObject blockRock;
    [SerializeField] private BlueNPC blueNpc;

    [Header("Part2")] 
    [SerializeField] private GameObject[] monstersPath;
    
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
                TriggerMonster(2, monstersPath[0]);
                break;
            case 2:
                TriggerMonster(3, monstersPath[1]);
                break;
            case 3:
                TriggerMonster(4, monstersPath[2]);
                break;
            case 4:
                TriggerMonster(5, monstersPath[3]);
                TriggerMonster(6, monstersPath[4]);
                break;
            case 5:
                TriggerMonster(7, monstersPath[5]);
                TriggerMonster(8, monstersPath[6]);
                break;
            case 6:
                TriggerMonster(9, monstersPath[7]);
                TriggerMonster(10, monstersPath[8]);
                break;
        }
    }

    private void TriggerMonster(int index, GameObject pathObj)
    {
        Transform[] monsterTransforms = pathObj.GetComponentsInChildren<Transform>();
        Vector3[] monsterPathPos = new Vector3[monsterTransforms.Length-1];
        for (int i = 1; i < monsterTransforms.Length; i++)
        {
            monsterPathPos[i-1] = monsterTransforms[i].position;
        }
        
        Sequence seq = DOTween.Sequence();
        seq.Append(monsters[index].transform.DOPath(monsterPathPos, 2, PathType.CatmullRom, PathMode.Sidescroller2D)
            .SetEase(Ease.InOutSine));
        seq.OnComplete(() =>
        {
            monsters[index].TogglePursuit(true);
        });
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
