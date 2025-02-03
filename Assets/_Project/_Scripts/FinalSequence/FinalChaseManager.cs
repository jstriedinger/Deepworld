using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class FinalChaseManager : MonoBehaviour
{
    [SerializeField] private BlueNPC blueNpc;
    [SerializeField] private GameObject monstersObj;
    private MonsterCinematic[] _monsters;
    [SerializeField] private GameObject[] monstersPaths;
    
    //Save default initial positions of monsters
    private Vector3[] _monstersPos;
        
    // Start is called before the first frame update
    private void OnEnable()
    {
        
        //blueNpc.ToggleFollow(true);
        GameManager.OnRestartingGame += ResetFinalChase;
    }

    private void Start()
    { 
        //lets get the monsters from the parents. Principally in pt2
        _monsters = monstersObj.GetComponentsInChildren<MonsterCinematic>();
        
        int index = 0;
        _monstersPos = new Vector3[_monsters.Length];
        foreach (MonsterCinematic monsterCinematic in _monsters)
        {
            _monstersPos[index] = monsterCinematic.transform.position;
            monsterCinematic.ToggleTrackTarget();
            index++;
        }
       
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
        foreach (MonsterCinematic monsterCinematic in _monsters)
        {
            monsterCinematic.TogglePursuit(false);
            monsterCinematic.transform.position = _monstersPos[index];
            index++;
        }
        
        blueNpc.ToggleFollow(true);
        
    }
    
    /**
     * Used for final chase pt1. Meaning the first 2 monsters
     * Use starting 0
     */
    public void Pt1TriggerMonsterChase(int index)
    {
        //_monstersPt1[index].TogglePursuit(true, 1.5f, true);
    }

    /**
     * Trigger the monsters of the second part of the final chase
     */
    public void FinalChaseTriggerMonsters(int index)
    {
        int i = 0;
        switch (index)
        {
            case 1 :
                //Monster 1
                TriggerMonsterWithPath(_monsters[0],monstersPaths[0],2);
                break;
            case 2:
                TriggerMonsterWithPath(_monsters[1],monstersPaths[1],2);
                
                break;
            case 3:
                TriggerMonsterWithPath(_monsters[2],monstersPaths[2]);
                
                break;
            case 4:
                TriggerMonsterWithPath(_monsters[3],monstersPaths[3]);
                TriggerMonsterWithPath(_monsters[4],monstersPaths[4]);
                
                break;
            case 5:
                TriggerMonsterWithPath(_monsters[5],monstersPaths[5]);
                TriggerMonsterWithPath(_monsters[6],monstersPaths[6]);
                _monsters[7].TogglePursuit(true,2);
                _monsters[8].TogglePursuit(true,2);
                break;
            case 6:
                TriggerMonsterWithPath(_monsters[9],monstersPaths[7]);
                TriggerMonsterWithPath(_monsters[10],monstersPaths[8]);
                _monsters[11].TogglePursuit(true,2);
                _monsters[12].TogglePursuit(true,2);
                _monsters[13].TogglePursuit(true,2);
                _monsters[14].TogglePursuit(true,2);
                break;
            case 7:
                _monsters[15].TogglePursuit(true,2);
                _monsters[16].TogglePursuit(true,2);
                _monsters[17].TogglePursuit(true,2);
                _monsters[18].TogglePursuit(true,2);
                _monsters[19].TogglePursuit(true,2);
                _monsters[20].TogglePursuit(true,2);
                break;
                
        }
    }

    private void TriggerMonsterWithPath(MonsterCinematic monster, GameObject pathObj, float timeOffset = 1)
    {
        Transform[] monsterTransforms = pathObj.GetComponentsInChildren<Transform>();
        Vector3[] monsterPathPos = new Vector3[monsterTransforms.Length-1];
        for (int i = 1; i < monsterTransforms.Length; i++)
        {
            monsterPathPos[i-1] = monsterTransforms[i].position;
        }
        
        Sequence seq = DOTween.Sequence();
        seq.Append(monster.transform.DOPath(monsterPathPos, 2, PathType.CatmullRom, PathMode.Sidescroller2D)
            .SetEase(Ease.InOutSine));
        seq.OnComplete(() =>
        {
            monster.TogglePursuit(true,timeOffset);
        });
    }

   
}
