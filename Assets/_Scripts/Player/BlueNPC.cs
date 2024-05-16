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
    public float _distanceToSwim = 0f;
    public float swimForce;
    public float timeBetweenSwim;
    [SerializeField] Transform headPart;
    [SerializeField] ParticleSystem VFXSwimBubbles;
    [SerializeField] EventReference SFXSwim;
    
    private Transform playerRef;
    [Header("Audio")]
    [SerializeField] private ParticleSystem vfxVoice;
    [SerializeField] private EventReference sfxCall1;
    [SerializeField] private EventReference sfxCall2;
    [SerializeField] private EventReference sfxScream;

    private EventReference _sfxLastTime;
    private bool _playBubbles;
    private AIPath _aiBlue;
    private AIDestinationSetter _aiDestinationSetter;
    private Rigidbody2D _rigidbody2D;
    private float _nextSwim;
    private Vector3 _swimDir;

    private void Awake()
    {
        _aiBlue = GetComponent<BlueMovement>();
        _aiDestinationSetter = GetComponent<AIDestinationSetter>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _aiBlue.canMove = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        _playBubbles = true;
        StartCoroutine("NPCBubbleSwim");
        playerRef = _aiDestinationSetter.target;
        //AIBluePath.enabled = false;
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

    private void Update()
    {
        //_swimDir = (_rigidbody2D.velocity).normalized;
        _swimDir = (playerRef.transform.position - transform.position).normalized;
    }

    public IEnumerator Swim()
    {
        _aiBlue.canMove = false;
        _rigidbody2D.velocity = Vector3.zero;
        _rigidbody2D.AddForce((_swimDir * swimForce ), ForceMode2D.Impulse);
        //Head scale animation
        VFXSwimBubbles.Play();
        FMODUnity.RuntimeManager.PlayOneShot(SFXSwim, transform.position);
        Sequence seq = DOTween.Sequence();
        seq.SetEase(Ease.OutCubic);
        seq.Append(headPart.DOScaleY(1.75f, 0.5f));
        seq.Append(headPart.DOScaleY(1f, 0.5f  * 1.5f));
        
        yield return new WaitForSeconds(1.5f);
        _rigidbody2D.velocity = Vector3.zero;
       _aiBlue.canMove = true;
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + _swimDir*swimForce  );
    }
}
