using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalChaseManager : MonoBehaviour
{
    [Header("Part1")] 
    [SerializeField] private MonsterCinematic Pt1Monster1;
    [SerializeField] private MonsterCinematic Pt1Monster2;
    private Vector3 Pt1Monster1InitPos;
    private Vector3 Pt1Monster2InitPos;
        
    
    // Start is called before the first frame update
    private void OnEnable()
    {
        Pt1Monster1InitPos = Pt1Monster1.transform.position;
        Pt1Monster2InitPos = Pt1Monster2.transform.position;
        GameManager.OnRestartingGame += ResetFinalChasePt1;
    }

    private void OnDisable()
    {
        GameManager.OnRestartingGame -= ResetFinalChasePt1;
    }

    //manually remove the action
    public void RemoveRestartAction()
    {
        GameManager.OnRestartingGame -= ResetFinalChasePt1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //when player dies
    public void ResetFinalChasePt1()
    {
        Pt1Monster1.TogglePursuit(false);
        Pt1Monster1.transform.position = Pt1Monster1InitPos;
        
        Pt1Monster2.TogglePursuit(false);
        Pt1Monster2.transform.position = Pt1Monster2InitPos;
    }


    public void Pt1TriggerMonsterChase(int index)
    {
        if(index == 1)
            Pt1Monster1.TogglePursuit(true);
        else if(index == 2)
            Pt1Monster2.TogglePursuit(true);
    }
}
