using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class FinalChaseManager : MonoBehaviour
{
    [Header("Part1")] 
    [SerializeField] private GameObject monstersPt1Obj;
    private MonsterCinematic[] _monstersPt1;
    [SerializeField] private GameObject blockRock;
    [SerializeField] private BlueNPC blueNpc;

    [Header("Part2")] 
    [SerializeField] private GameObject monstersPt21Obj;
    [SerializeField] private GameObject[] monstersPt21Paths;
    private MonsterCinematic[] _monstersPt21;
    [SerializeField] private GameObject monstersPt22Obj;
    [SerializeField] private GameObject[] monstersPt22Paths;
    private MonsterCinematic[] _monstersPt22;
    
    [SerializeField] private GameObject monstersPt23Obj;
    [SerializeField] private GameObject[] monstersPt23Paths;
    private MonsterCinematic[] _monstersPt23;
    
    [SerializeField] private GameObject monstersPt24Obj;
    [SerializeField] private GameObject[] monstersPt24Paths;
    private MonsterCinematic[] _monstersPt24;
    
    [SerializeField] private GameObject monstersPt25Obj;
    private MonsterCinematic[] _monstersPt25;
    
    private Vector3[] _monstersPos;
        
    
    // Start is called before the first frame update
    private void OnEnable()
    {
        
        //blueNpc.ToggleFollow(true);
        GameManager.OnRestartingGame += ResetFinalChase;
    }

    private void Start()
    { 
        //lets get the mosnter from the parents
        _monstersPt1 = monstersPt1Obj.GetComponentsInChildren<MonsterCinematic>();
        _monstersPt21 = monstersPt21Obj.GetComponentsInChildren<MonsterCinematic>();
        _monstersPt22 = monstersPt22Obj.GetComponentsInChildren<MonsterCinematic>();
        _monstersPt23 = monstersPt23Obj.GetComponentsInChildren<MonsterCinematic>();
        _monstersPt24 = monstersPt24Obj.GetComponentsInChildren<MonsterCinematic>();
        _monstersPt25 = monstersPt25Obj.GetComponentsInChildren<MonsterCinematic>();
        
        
        int index = 0;
        _monstersPos = new Vector3[_monstersPt1.Length + _monstersPt21.Length + _monstersPt22.Length
        + _monstersPt23.Length + _monstersPt24.Length + _monstersPt25.Length];
        foreach (MonsterCinematic monsterCinematic in _monstersPt1)
        {
            _monstersPos[index] = monsterCinematic.transform.position;
            monsterCinematic.ToggleTrackTarget();
            index++;
        }
        foreach (MonsterCinematic monsterCinematic in _monstersPt21)
        {
            _monstersPos[index] = monsterCinematic.transform.position;
            monsterCinematic.ToggleTrackTarget();
            index++;
        }
        foreach (MonsterCinematic monsterCinematic in _monstersPt22)
        {
            _monstersPos[index] = monsterCinematic.transform.position;
            monsterCinematic.ToggleTrackTarget();
            index++;
        }
        foreach (MonsterCinematic monsterCinematic in _monstersPt23)
        {
            _monstersPos[index] = monsterCinematic.transform.position;
            monsterCinematic.ToggleTrackTarget();
            index++;
        }
        foreach (MonsterCinematic monsterCinematic in _monstersPt24)
        {
            _monstersPos[index] = monsterCinematic.transform.position;
            monsterCinematic.ToggleTrackTarget();
            index++;
        }
        foreach (MonsterCinematic monsterCinematic in _monstersPt25)
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
        foreach (MonsterCinematic monsterCinematic in _monstersPt1)
        {
            monsterCinematic.TogglePursuit(false);
            monsterCinematic.transform.position = _monstersPos[index];
            index++;
        }
        foreach (MonsterCinematic monsterCinematic in _monstersPt21)
        {
            monsterCinematic.TogglePursuit(false);
            monsterCinematic.transform.position = _monstersPos[index];
            index++;
        }
        foreach (MonsterCinematic monsterCinematic in _monstersPt22)
        {
            monsterCinematic.TogglePursuit(false);
            monsterCinematic.transform.position = _monstersPos[index];
            index++;
        }
        foreach (MonsterCinematic monsterCinematic in _monstersPt23)
        {
            monsterCinematic.TogglePursuit(false);
            monsterCinematic.transform.position = _monstersPos[index];
            index++;
        }
        foreach (MonsterCinematic monsterCinematic in _monstersPt24)
        {
            monsterCinematic.TogglePursuit(false);
            monsterCinematic.transform.position = _monstersPos[index];
            index++;
        }   
        foreach (MonsterCinematic monsterCinematic in _monstersPt25)
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
        _monstersPt1[index].TogglePursuit(true, 1.5f, true);
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
        int i = 0;
        switch (index)
        {
            case 1 :
                foreach (MonsterCinematic monster in _monstersPt21)
                {
                    if (i < monstersPt21Paths.Length)
                    {
                        TriggerMonsterWithPath(monster,monstersPt21Paths[i]);
                        
                    }
                    else
                        monster.TogglePursuit(true,1.5f);

                    i++;
                }
                break;
            case 2:
                foreach (MonsterCinematic monster in _monstersPt22)
                {
                    if (i < monstersPt22Paths.Length)
                    {
                        TriggerMonsterWithPath(monster,monstersPt22Paths[i]);
                        
                    }
                    else
                        monster.TogglePursuit(true,1.2f);

                    i++;
                }
                break;
            case 3:
                foreach (MonsterCinematic monster in _monstersPt23)
                {
                    if (i < monstersPt23Paths.Length )
                    {
                        TriggerMonsterWithPath(monster,monstersPt23Paths[i]);
                        
                    }
                    else
                        monster.TogglePursuit(true,1.5f);

                    i++;
                }
                break;
            case 4:
                foreach (MonsterCinematic monster in _monstersPt24)
                {
                    if (i < monstersPt24Paths.Length )
                    {
                        TriggerMonsterWithPath(monster,monstersPt24Paths[i]);
                        
                    }
                    else
                        monster.TogglePursuit(true,2);

                    i++;
                }
                break;
            case 5:
                foreach (MonsterCinematic monster in _monstersPt25)
                {
                    monster.TogglePursuit(true,2);
                    i++;
                }
                break;
        }
    }

    private void TriggerMonsterWithPath(MonsterCinematic monster, GameObject pathObj)
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
            monster.TogglePursuit(true,1);
        });
    }

    private IEnumerator EndPt1Chase()
    {
        blockRock.SetActive(true);
        _monstersPt1[0].TogglePursuit(false);
        _monstersPt1[1].TogglePursuit(false);
        yield return new WaitForSeconds(1.5f);
        _monstersPt1[0].GoToPosition(_monstersPos[0]);
        _monstersPt1[1].GoToPosition(_monstersPos[1]);

    }
}
