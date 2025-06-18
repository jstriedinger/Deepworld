using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using Pathfinding;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class BlueNPC : MonoBehaviour
{
    [SerializeField] private GameObject disposableTentacle;
    [Header("Swim")]
    [SerializeField] Transform headPart;
    [SerializeField] ParticleSystem VFXSwimBubbles;
    [SerializeField] EventReference SFXSwim;
    
    
    [Header("Audio")]
    [SerializeField] private bool canReactToPlayerCall = false;
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
    private Rigidbody2D _rigidBody;
    
    
    //Procedural bodies
    private Tentacle[] _proceduralTentacles;
    private TentacleDynamic[] _proceduralDynamicTentacles;
    private BodyTentacle _proceduralBody;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
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
    
    private void OnEnable()
    {
        PlayerCharacter.OnPlayerCall += OnPlayerCall;
    }

    private void OnDisable()
    {
        PlayerCharacter.OnPlayerCall -= OnPlayerCall;
    }

    public void ToggleReactToCall(bool newReactToCall)
    {
        canReactToPlayerCall = newReactToCall;
    }

    public Transform GetFollowTarget()
    {
        return _aiBlue.target;
    }

    private void OnPlayerCall()
    {
        if (canReactToPlayerCall)
        {
            StartCoroutine(PlayCallSfx(true));
        }
    }
    
    //offset time used when responding from player call
    public IEnumerator PlayCallSfx(bool offsetTime = false)
    {
        if(offsetTime)
            yield return new WaitForSeconds(Random.Range(1.5f, 2.5f));
        
        vfxVoice.Play();
        yield return new WaitForSeconds(0.2f);
        //visual feedback
        transform.DOPunchScale(new Vector3(.1f, .4f, 0), .75f, 1, 0f).SetDelay(0.2f);
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
    public void SwimEffect()
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
    

    public void GetHurt()
    {
        Destroy(disposableTentacle);
        
    }
    

    /**
     * Prepare blue to be faster in the last section of the game
     */
    public void ChangeBlueStats(Transform newTarget)
    {
        _aiBlue.ChangeBlueStats(newTarget);
    }
    
    //change who Blue msut follow. When it does it also changes the eye follow target
    public void ChangeFollowTarget(Transform newTarget, float newMinDistance = -1, bool newCanSwim = true)
    {
        ToggleEyeFollowTarget(true, newTarget);
        _aiBlue.ChangeFollowTarget(newTarget, newMinDistance, newCanSwim);
    }

    public void ToggleFireReachedDestinationEvent(bool toggle)
    {
        _aiBlue.ToggleFireReachedDestinationEvent(toggle);
    }
    
    public void StopMovement()
    {
        _rigidBody.linearVelocity = Vector2.zero;
    }

}
