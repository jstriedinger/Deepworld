using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using Pathfinding;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

public class BlueNPC : MonoBehaviour
{
    [Header("Swim")]
    [SerializeField] Transform headPart;
    [SerializeField] ParticleSystem VFXSwimBubbles;
    [SerializeField] EventReference SFXSwim;
    
    
    [Header("Audio")]
    [SerializeField] private ParticleSystem vfxVoice;
    [SerializeField] private EventReference sfxCall1;
    [SerializeField] private EventReference sfxCall2;
    [SerializeField] private EventReference sfxScream;

    private EventReference _sfxLastTime;
    private bool _playBubbles;
    private BlueMovement _aiBlue;
    private AIDestinationSetter _aiDestinationSetter;
    private float _nextSwim;
    private EyeFollower _eyeFollower;
    
    
    //Procedural bodies
    private Tentacle[] _proceduralTentacles;
    private TentacleDynamic[] _proceduralDynamicTentacles;
    private BodyTentacle _proceduralBody;

    private void Awake()
    {
        _aiBlue = GetComponent<BlueMovement>();
        _aiDestinationSetter = GetComponent<AIDestinationSetter>();
        _aiBlue.canMove = false;
        
        _proceduralDynamicTentacles = GetComponentsInChildren<TentacleDynamic>();
        _proceduralTentacles = GetComponentsInChildren<Tentacle>();
        _proceduralBody = GetComponentInChildren<BodyTentacle>();
        _eyeFollower = GetComponentInChildren<EyeFollower>();

    }

    // Start is called before the first frame update
    void Start()
    {
        _playBubbles = true;
        StartCoroutine("NPCBubbleSwim");
    }

    public Transform GetFollowTarget()
    {
        return _aiBlue.target;
    }
    


    public IEnumerator PlayCallSFX()
    {
        vfxVoice.Play();
        yield return new WaitForSeconds(0.2f);
        if(_sfxLastTime.Equals(sfxCall1))
        {
            _sfxLastTime = sfxCall2;
        }
        else
        {
            _sfxLastTime = sfxCall1;
        }

        FMODUnity.RuntimeManager.PlayOneShot(_sfxLastTime, transform.position);
    }

    public void ResetProceduralBody()
    {
        for (int i = 0; i < _proceduralTentacles.Length; i++)
        {
            _proceduralTentacles[i].ResetPos();
        }
        for (int i = 0; i < _proceduralDynamicTentacles.Length; i++)
        {
            _proceduralDynamicTentacles[i].ResetPositions();
        }

        _proceduralBody.ResetPositions();
    }

    //Swim effects with adding the force
    public void SwimEffect(Vector3 dir)
    {
        //Head scale animation
        VFXSwimBubbles.Play();
        FMODUnity.RuntimeManager.PlayOneShot(SFXSwim, transform.position);
        Sequence seq = DOTween.Sequence();
        seq.SetEase(Ease.OutCubic);
        seq.Append(headPart.DOScaleY(1.75f, 0.5f));
        seq.Append(headPart.DOScaleY(1f, 0.5f  * 1.5f));
    }

 

    public void PlayScreamSFX()
    {
        FMODUnity.RuntimeManager.PlayOneShot(sfxScream, transform.position);
    }

    IEnumerator NPCBubbleSwim()
    {
        while(_playBubbles)
        {
            VFXSwimBubbles.Play();
            yield return new WaitForSeconds(5); 

        }

    }

    public void ToggleFollow(bool shouldFollow)
    {
        _aiBlue.canMove = shouldFollow;
    }
    
    public void ToggleEyeFollowTarget(bool willFollow = false, Transform newTarget = null)
    {
        _eyeFollower.ToggleFollowTarget(willFollow,newTarget);
    }

}
