using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class FinalChaseManager : MonoBehaviour
{
    public static FinalChaseManager Instance { get; private set; }
    [HideInInspector] public Transform finalChaseAnchor;
    [SerializeField] private BlueNPC blueNpc;
    [SerializeField] private GameObject monstersObj;
    [SerializeField] private GameObject[] monstersPaths;
    private MonsterCinematic[] _monsters;
    private ColliderEventTrigger[] _triggers;
    
    //Save default initial positions of monsters
    private Vector3[] _monstersPos;

    [HideInInspector] public bool inFinalChase;
    
    private void Awake() 
    { 
        // If there is an instance, and it's not me, delete myself.
    
        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            Instance = this; 
        } 
    }
        
    private void Start()
    { 
        //lets get the monsters from the parents. Principally in pt2
        _monsters = monstersObj.GetComponentsInChildren<MonsterCinematic>();
        _triggers = monstersObj.GetComponentsInChildren<ColliderEventTrigger>();
        
        int index = 0;
        _monstersPos = new Vector3[_monsters.Length];
        foreach (MonsterCinematic monsterCinematic in _monsters)
        {
            _monstersPos[index] = monsterCinematic.transform.position;
            monsterCinematic.ToggleTrackTarget(false);
            index++;
        }
       
    }
    
    // Start is called before the first frame update
    private void OnEnable()
    {
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
        //rumble linear
        if (inFinalChase)
        {
            Gamepad pad = Gamepad.current;
            if (pad != null && GameManager.Instance.playerRef.playerInput.currentControlScheme == "Gamepad")
            {
                float d = 1 - Mathf.Clamp(Vector2.Distance(GameManager.Instance.playerRef.transform.position, finalChaseAnchor.transform.position) / AudioManager.Instance.finalCloseDangerRadius, 0.2f,.9f) ;
                pad.SetMotorSpeeds(d, d);
            }
            
        }
      
    }

    /**
     * Reset actin when player dies in Final Chase Pt1
     */
    public void ResetFinalChase()
    {
        Debug.Log("Reset final chase");
        inFinalChase = false;
        Gamepad pad = Gamepad.current;
        if (pad != null)
        {
            pad.SetMotorSpeeds(0,0);
        }
        
        int index = 0;
        //reset monster positions
        foreach (MonsterCinematic monsterCinematic in _monsters)
        {
            monsterCinematic.TogglePursuit(false);
            monsterCinematic.transform.position = _monstersPos[index];
            index++;
        }

        //reset cinematicTriggers
        foreach (ColliderEventTrigger trigger in _triggers)
        {
            trigger.RestartTrigger();
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
        Gamepad pad = Gamepad.current;
        Sequence seq = DOTween.Sequence();
        
        switch (index)
        {
            case 1 :
                //Monster 1
                TriggerMonsterWithPath(_monsters[0],monstersPaths[0], true, 1.5f);
                inFinalChase = true;
                AudioManager.Instance.TriggerFinalChaseAudio(true);
                break;
            case 2:
                TriggerMonsterWithPath(_monsters[1],monstersPaths[1], true, 1f);
                
                break;
            case 3:
                TriggerMonsterWithPath(_monsters[2],monstersPaths[2], false, .5f);
                break;
            case 4:
                
                TriggerMonsterWithPath(_monsters[3],monstersPaths[3], true, .5f );
                TriggerMonsterWithPath(_monsters[4],monstersPaths[4], false);
                
                break;
            case 5:
                
                TriggerMonsterWithPath(_monsters[5],monstersPaths[5]);
                TriggerMonsterWithPath(_monsters[6],monstersPaths[6], false);
                seq.PrependInterval(1)
                .AppendCallback(() =>
                {
                    _monsters[7].TogglePursuit(true);
                })
                .AppendInterval(0.15f)
                .AppendCallback(() =>
                {
                    _monsters[8].TogglePursuit(true);

                })
                .AppendInterval(0.25f)
                .AppendCallback(() =>
                {
                    _monsters[7].UpdateMonsterState(MonsterState.Chasing, true, true);
                    _monsters[8].UpdateMonsterState(MonsterState.Chasing, false);
                });
                break;
            case 6:
                TriggerMonsterWithPath(_monsters[9],monstersPaths[7], false);
                TriggerMonsterWithPath(_monsters[10],monstersPaths[8], false);
                seq.PrependInterval(1)
                    .AppendCallback(() =>
                    {
                        _monsters[11].TogglePursuit(true);
                        _monsters[13].TogglePursuit(true);
                    })
                    .AppendInterval(0.25f)
                    .AppendCallback(() =>
                    {
                        _monsters[12].TogglePursuit(true, true);
                        _monsters[14].TogglePursuit(true, true);

                    })
                    .AppendInterval(0.2f)
                    .AppendCallback(() =>
                    {
                        _monsters[11].UpdateMonsterState(MonsterState.Chasing, true,true);
                        _monsters[13].UpdateMonsterState(MonsterState.Chasing, false);
                    });
                break;
            case 7:
                
                _monsters[15].TogglePursuit(true,true,true);
                _monsters[16].TogglePursuit(true,true,false);
                break;
            case 8:
                
                //the rest of monsters
                bool toggle = true;
                for (int j = 17; j < _monsters.Length; j++)
                {
                    _monsters[j].TogglePursuit(true,toggle, false);
                    toggle = !toggle;
                }
                break;
            
        }
    }

    private void TriggerMonsterWithPath(MonsterCinematic monster, GameObject pathObj, bool withSound = true,float timeOffset = 1)
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
        seq.AppendInterval(.1f);
        seq.AppendCallback(() =>
        {
            monster.TogglePursuit(true, false, false);
        });
        seq.AppendInterval(timeOffset);
        seq.AppendCallback(() =>
        {
            monster.UpdateMonsterState(MonsterState.Chasing,true,true);
        });
    }

   
}
