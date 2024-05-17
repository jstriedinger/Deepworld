using System.Collections;

using System.Collections.Generic;

using UnityEngine;

using DG.Tweening;

using FMOD.Studio;

using FMODUnity;
using UnityEngine.Serialization;


public class TutorialEncounter1 : MonoBehaviour
{
    
    private Level1Manager _level1Manager;

    // Start is called before the first frame update
    void Start()
    {
        _level1Manager = FindFirstObjectByType<Level1Manager>();
    }

    private void Awake()
    {
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            _level1Manager.TriggerSwarmEncounter(true);
        }
    }



    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _level1Manager.TriggerSwarmEncounter(false);
        }
    }

}

