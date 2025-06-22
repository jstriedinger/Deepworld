using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using DG.Tweening;
using FMODUnity;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class SwimmerFish : MonoBehaviour
{
    [SerializeField] private ParticleSystem vfxCall;
    [SerializeField] private EventReference sfxCall;
    [SerializeField, Range(8,50)] private float playerCallReactRadius = 8;
    [SerializeField] private Transform patrolObject;
    [SerializeField] private bool startAutomatically =  true;
    private BehaviorTree _behaviorTree;
    private bool _canReactToPlayer;
    
    [Header("Eye following")]
    [SerializeField] private bool canEyeFollow = false;
    [SerializeField] private Transform eye;
    [SerializeField] private Transform pupil;
    private Transform _eyeTarget;
    private Vector3 _smoothVelocity;

    private void Awake()
    {
        _behaviorTree = GetComponent<BehaviorTree>();
    }
    
    private void OnEnable()
    {
        PlayerCharacter.OnPlayerCall += OnPlayerCallNear;
    }

    private void OnDisable()
    {
        PlayerCharacter.OnPlayerCall -= OnPlayerCallNear;
    }

    private void OnPlayerCallNear()
    {
        if ((GameManager.Instance.playerRef.transform.position - transform.position).sqrMagnitude
            < playerCallReactRadius * playerCallReactRadius && _canReactToPlayer)
        {
            //player near enough
            StartCoroutine(PlayCallSfx());
        }
    }

    public void ToggleCanReactToPlayer(bool toggle)
    {
        _canReactToPlayer = toggle;
    }
    
    public IEnumerator PlayCallSfx()
    {
        yield return new WaitForSeconds(Random.Range(1.8f, 2.5f));
        PlayCallSfxSimple();
    }

    public void PlayCallSfxSimple()
    {
        vfxCall.Play();
        AudioManager.Instance.PlayOneShotEvent(sfxCall, transform.position);
        transform.DOPunchScale(new Vector3(.1f, .4f, 0), .75f, 1, 0f).SetDelay(0.2f);
    }

    // Start is called before the first frame update
    void Start()
    {
        List<GameObject> patrolPoints = new List<GameObject>();
        if (patrolObject)
        {
            foreach (Transform child in patrolObject)
            {
                if(child != patrolObject.transform)
                    patrolPoints.Add(child.gameObject);
            }
            _behaviorTree.SetVariableValue("PatrolInfo",patrolPoints);
            if(startAutomatically)
                StarTree();
            
        }

        _eyeTarget = GameManager.Instance.playerRef.transform;
    }

    private void Update()
    {
        //check eyesifght every 2 frames
        if (Time.frameCount % 2 == 0 && canEyeFollow)
        {
            if ((_eyeTarget.position - transform.position).sqrMagnitude <=
                playerCallReactRadius * playerCallReactRadius)
            {
                //within distance
                if (_eyeTarget)
                {
                    Vector3 targetPos = _eyeTarget.position;
                    Vector3 localDir = eye.InverseTransformDirection((targetPos - eye.position).normalized);
                    localDir.x = Mathf.Clamp(localDir.x, -0.07f, 0.07f);
                    localDir.y = 0;
                    localDir.z = 0;
                    pupil.localPosition = Vector3.SmoothDamp(pupil.localPosition, localDir, ref _smoothVelocity, 0.2f);
                
                }
            }
            else
            {
                pupil.localPosition = Vector3.SmoothDamp(pupil.localPosition, Vector3.zero, ref _smoothVelocity, 0.2f);
            }
        }
        
    }

    public void StarTree()
    {
        _behaviorTree.Start();
        _behaviorTree.EnableBehavior();
    }

    

   
}
