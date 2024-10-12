using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class FinalChaseManager : MonoBehaviour
{
    [Header("Part1")] 
    [SerializeField] private MonsterCinematic Pt1Monster1;
    [SerializeField] private MonsterCinematic Pt1Monster2;
    [SerializeField] private GameObject blockRock;
    [SerializeField] private BlueNPC blueNpc;
    private Vector3 _pt1Monster1Init;
    private Vector3 _pt1Monster2Init;
        
    
    // Start is called before the first frame update
    private void OnEnable()
    {
        _pt1Monster1Init = Pt1Monster1.transform.position;
        _pt1Monster2Init = Pt1Monster2.transform.position;
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
        Pt1Monster1.transform.position = _pt1Monster1Init;
        
        Pt1Monster2.TogglePursuit(false);
        Pt1Monster2.transform.position = _pt1Monster2Init;
        
        blueNpc.ToggleFollow(true);
        
    }


    public void Pt1TriggerMonsterChase(int index)
    {
        if(index == 1)
            Pt1Monster1.TogglePursuit(true);
        else if(index == 2)
            Pt1Monster2.TogglePursuit(true);
    }

    public void Pt1EndMonsterChase()
    {
        StartCoroutine(EndPt1Chase());
        // now we want to wait a little and make them go back
    }

    private IEnumerator EndPt1Chase()
    {
        blockRock.SetActive(true);
        Pt1Monster1.TogglePursuit(false);
        Pt1Monster2.TogglePursuit(false);
        yield return new WaitForSeconds(1);
        Pt1Monster1.GoToPosition(_pt1Monster1Init);
        Pt1Monster2.GoToPosition(_pt1Monster2Init);
        yield return new WaitForSeconds(4);
        Destroy(Pt1Monster1.gameObject);
        Destroy(Pt1Monster2.gameObject);

    }
}
