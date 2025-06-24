using System;
using System.Collections;

using System.Collections.Generic;
using System.Numerics;
using BehaviorDesigner.Runtime;
using DG.Tweening;
using UnityEngine;
using System.Linq;

using Pathfinding;

using FMODUnity;
using Unity.Mathematics;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Hideout : MonoBehaviour
{
    [SerializeField] private CoverSO configuration;
    [SerializeField] private Transform body;
    [SerializeField] private GameObject inside;
    [SerializeField] private GameObject outside;
    [SerializeField] private Light2D light2D;
    [SerializeField] private LayerMask trackerLayers;
    [SerializeField] private bool onlyCheckPlayer;
    [SerializeField] private ParticleSystem[] vfxBubbles;
    private PolypTracker[] _insideTrackers;
    private PolypTracker[] _outsideTrackers;

    private Tween _punchDownTween;
    private Tween _punchUpTween;
    private Tween _lightGlowTween;



    private void Start()
    {
        SetupTentacles();
    }
    
    
    private void SetupTentacles()
    {
        _insideTrackers = inside.GetComponentsInChildren<PolypTracker>();
        Transform target = GameObject.Find("Player").transform;
        foreach (PolypTracker tracker in _insideTrackers)
        {
            tracker.Setup(trackerLayers, onlyCheckPlayer ? target : null);
        }
        
        _outsideTrackers = outside.GetComponentsInChildren<PolypTracker>();
        foreach (PolypTracker tracker in _outsideTrackers)
        {
            tracker.Setup(trackerLayers, target);
        }
        
        _punchDownTween = body.DOPunchScale(Vector3.down * .1f, .75f, 1).SetEase(Ease.InOutSine).SetAutoKill(false).Pause();
        _punchUpTween = body.DOPunchScale(Vector3.up * .1f, .75f, 1).SetEase(Ease.InOutSine).SetAutoKill(false).Pause();
        _lightGlowTween = DOTween.To(() => light2D.intensity, x => light2D.intensity = x, light2D.intensity + 0.45f,
            1.5f).SetLoops(-1, LoopType.Yoyo);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if( collision.gameObject.CompareTag("Player") && !GameManager.Instance.isPlayerDead)
        {
            _punchDownTween.Rewind();
            _punchDownTween.Play();
            //Add force for player away
            Vector3 d = (transform.position -  collision.transform.position).normalized;
            StartCoroutine(GameManager.Instance.playerRef.AddImpulseForceToPlayer(d, configuration.CoverForce));
            //vfx bubbles 
            ParticleSystem vfxClosest = vfxBubbles.OrderBy(obj => Vector2.Distance(collision.transform.position, obj.transform.position)).First();
            vfxClosest.Play();
            //Sound effect
            AudioManager.Instance.PlayOneShotEvent(configuration.SfxEnter, transform.position);
            
            GameManager.Instance.playerRef.ToggleHidePlayer(true);
            //metric handler
            MetricManagerScript.instance?.LogString("Hideout", "Enter");

        }
        else if (collision.gameObject.CompareTag("Blue"))
        {
            //just the sound
            ParticleSystem vfxClosest = vfxBubbles.OrderBy(obj => Vector2.Distance(collision.transform.position, obj.transform.position)).First();
            vfxClosest.Play();
            AudioManager.Instance.PlayOneShotEvent(configuration.SfxEnter, transform.position);
        }
    }



    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !GameManager.Instance.isPlayerDead)
        {
            _punchUpTween.Rewind();
            _punchUpTween.Play();
            //Add force for player away
            Vector3 d = (collision.transform.position - transform.position).normalized;
            StartCoroutine(GameManager.Instance.playerRef.AddImpulseForceToPlayer(d, configuration.CoverForce));
            //vfx bubble
            ParticleSystem vfxClosest = vfxBubbles.OrderBy(obj => Vector2.Distance(collision.transform.position, obj.transform.position)).First();
            vfxClosest.Play();
            //sound effect
            AudioManager.Instance.PlayOneShotEvent(configuration.SfxEnter, transform.position);
            
            GameManager.Instance.playerRef.ToggleHidePlayer(false);
            MetricManagerScript.instance?.LogString("Hideout", "Exit");
        }
        else if (collision.gameObject.CompareTag("Blue"))
        {
            //just the sound
            AudioManager.Instance.PlayOneShotEvent(configuration.SfxExit,transform.position);
            ParticleSystem vfxClosest = vfxBubbles.OrderBy(obj => Vector2.Distance(collision.transform.position, obj.transform.position)).First();
            vfxClosest.Play();
        }
    }

    private void OnDrawGizmosSelected()
    {
        
    }
}

