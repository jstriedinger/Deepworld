using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using BehaviorDesigner.Runtime;
using DG.Tweening;
using FMODUnity;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public enum MonsterState {  Default, Investigate, Follow, Chasing, Frustrated };

public class EnemyMonster : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private MonsterSO monsterStats;
    [SerializeField] private Transform patrolObject;
    [SerializeField] public MonsterState CurrentState { get; private set; }
    
    [SerializeField] private LineRenderer[] _tentacles;
    [SerializeField] private Transform _headObj;
    [SerializeField] private Transform _pupilObj;
    [SerializeField] private Light2D _headLight;
    [SerializeField] private bool _canAffectCamera = true;
    [SerializeField] private bool _Testing = false;
    private CameraManager cameraManager;
    private GameManager _gameManager;

    private EyeManager _eyeManager;
    private BehaviorTree _behaviorTree;
    private StudioEventEmitter _monsterChaseMusicEmitter;
    private Vector3 origWorldPos;
    [HideInInspector]
    public bool isChasing = false;

    private bool _canReactToCall = false;
    private bool InCamera = false;
    private Rigidbody2D _rigidbody2D;
    private ParticleSystem _vfxDetect;
    private float _randomInitialSize = 1;
    private Tween _headTween;
    private Sequence _colorTweenSequence;


    private void Awake()
    {
        _eyeManager = GetComponent<EyeManager>();
        _behaviorTree = GetComponent<BehaviorTree>();
        _monsterChaseMusicEmitter = GetComponent<StudioEventEmitter>();
        isChasing = false;
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _randomInitialSize = Random.Range(1.9f, 2.1f);
        Debug.Log("Randome size for "+gameObject.name + " is: "+_randomInitialSize);
        _headObj.transform.localScale = Vector3.one * _randomInitialSize;
        Debug.Log("Localscale ends up for "+gameObject.name + " is: "+_headObj.transform.localScale.ToString());
        

        _vfxDetect = _headObj.GetComponentInChildren<ParticleSystem>();
        
        
        foreach (LineRenderer tentacle in _tentacles)
        {
            tentacle.startColor = monsterStats.DefaultColor;
        }

        _headLight.color = monsterStats.DefaultColor*1.5f;
        //pass data from SO to the AI Tree
        if (_behaviorTree)
        {
            _behaviorTree.SetVariableValue("FollowSpeed",monsterStats.FollowSpeed);
            if (!monsterStats.IsSimpleType)
            {
                _behaviorTree.SetVariableValue("ChasingSpeed",monsterStats.ChasingSpeed);
                _behaviorTree.SetVariableValue("ChasingRange",monsterStats.ChasingRange);
                _behaviorTree.SetVariableValue("FollowRange",monsterStats.FollowRange);
                _behaviorTree.SetVariableValue("isPatrolType",monsterStats.IsPatrolType);
            }
            
            
            //get the patrol points
            if (monsterStats.IsPatrolType)
            {
                List<GameObject> patrolPoints = new List<GameObject>();
                foreach (Transform child in patrolObject)
                {
                    patrolPoints.Add(child.gameObject);
                }
                _behaviorTree.SetVariableValue("PatrolInfo",patrolPoints);
            }
        }
        
    }

    // Start is called before the first frame update
    void Start()
    {
        cameraManager = GameObject.FindFirstObjectByType<CameraManager>();
        _gameManager = GameObject.FindFirstObjectByType<GameManager>();
        if (_behaviorTree && !monsterStats.IsSimpleType)
        {
            _behaviorTree.SetVariableValue("playerRef",_gameManager.playerRef.gameObject);
            _behaviorTree.SetVariableValue("playerLastPosition",_gameManager.playerLastPosition);
        }

        _colorTweenSequence = DOTween.Sequence();
        //_light2D.color = monsterStats.DefaultColor;
    }

    private void Update()
    {
        if (_Testing)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if(CurrentState == MonsterState.Default)
                    UpdateMonsterState(MonsterState.Follow);
                else if(CurrentState == MonsterState.Follow)
                    UpdateMonsterState(MonsterState.Chasing);
                else if(CurrentState == MonsterState.Chasing)
                    UpdateMonsterState(MonsterState.Frustrated);
                else if(CurrentState == MonsterState.Frustrated)
                    UpdateMonsterState(MonsterState.Default);
                    
               
            } 
            
        }
    }

    private void FixedUpdate()
    {
        if(_canAffectCamera && !GameManager.IsPlayerDead)
            UpdateEnemyInCamera();
    }
    



    //Trigered to put enemy on the player line of sight
    private void UpdateEnemyInCamera()
    {
        Collider2D playerCheckCamera = Physics2D.OverlapCircle(transform.position, monsterStats.DistanceToShowOnCamera, LayerMask.GetMask("Player"));

        if(playerCheckCamera)
        {
            if(!InCamera)
            {
                //add to the
                cameraManager.AddEnemyToCameraView(gameObject);
                InCamera = true;

            }
        }

        else if(InCamera)
        {
            cameraManager.RemoveEnemyFromCameraView(gameObject);
            InCamera = false;
        }


    }

    public MonsterSO GetMonsterStats()
    {
        return monsterStats;
    }



    //react to player call, go investigate a position using a lastpos object since btree needs an object
    public void ReactToPlayerCall()
    {
        Debug.Log(name + "Current monster state: "+CurrentState.ToString());
        if (CurrentState != MonsterState.Chasing && CurrentState != MonsterState.Follow)
        {
            if (!_canReactToCall)
            {
                //if mosnter is not chasing or following, it can react to player call
                UpdateMonsterState(MonsterState.Investigate);
                Debug.Log("player call heard");
                _behaviorTree.SetVariableValue("CanReactToCall",true);
                _canReactToCall = true;
            }
        }
    }

    
    public void OnAIReactToCall()
    {
        //only if we are not already chasing or following a player
        UpdateMonsterState(MonsterState.Investigate);
    }
    public void UpdateMonsterState(MonsterState newState)
    {
        float colorFadeTime = 1f;
        if (CurrentState == MonsterState.Chasing)
        {
            //we were on a chase
            _gameManager.UpdateMonstersChasing(false);
        }

        if (newState != MonsterState.Investigate)
        {
            if (_canReactToCall)
            {
                Debug.Log("Patrol: no longer react to player call");
                _behaviorTree.SetVariableValue("CanReactToCall",false);
                _canReactToCall = false;
            }
        }
        //kill whatever is happening with head animation
        if (CurrentState != MonsterState.Follow)
        {
            Debug.Log("Was not following. Go back to normal");
            //it wasnt following, kill whatever animation is happening and go back to head scale
            _headTween.Kill();
            _headObj.transform.localScale = Vector3.one * _randomInitialSize;
        }
        
        //if there was a color transition, stop it
        _colorTweenSequence.Kill();

        switch (newState)
        {
            case MonsterState.Default:
                //pupil size
                //eyemanager change eyes + pupil size
                UpdateColors(monsterStats.DefaultColor, monsterStats.DefaultColor);
                
                break;
            case MonsterState.Follow:
                UpdateColors(monsterStats.FollowColor, monsterStats.FollowColor);
                StartCoroutine(PlayReactSound(false, true));
                break;
            case MonsterState.Investigate:
                _behaviorTree.SetVariableValue("CanReactToCall",true);
                _canReactToCall = true;
                
                UpdateColors(monsterStats.FollowColor, monsterStats.FollowColor);
                
                StartCoroutine(PlayReactSound(true, false));
                break;
            case MonsterState.Chasing:
                //important for the chase music bg
                _gameManager.UpdateMonstersChasing(true);
                UpdateColors(monsterStats.ChaseColor, monsterStats.ChaseColor);
                
                StartCoroutine(PlayReactSound(false, false));
                break;
            case MonsterState.Frustrated:
                StartCoroutine(PlayReactSound(false, false));
                //DOTween.To(() => _light2D.color, x => _light2D.color = x, monsterStats.FollowColor, 0.5f);
                break;
            
        }

        CurrentState = newState;
        _eyeManager.OnUpdateMonsterState();
    }

    private void UpdateColors(Color tentacleColor, Color lightColor)
    {
        _colorTweenSequence = DOTween.Sequence();
        foreach (LineRenderer tentacle in _tentacles)
        {
            _colorTweenSequence.Join(DOTween.To(() => tentacle.startColor, x => tentacle.startColor = x,
                tentacleColor, 1f));
        }
        //light is a litle lighter
        _colorTweenSequence.Join(DOTween.To(() => _headLight.color, x => _headLight.color = x,
            lightColor*1.5f, 1f));
    }

    IEnumerator PlayReactSound(bool showEffect, bool animate)
    {
        if (animate)
        {
            _headTween = _headObj.DOScale(_randomInitialSize + 0.5f, 0.15f).SetAutoKill(false).SetEase(Ease.OutSine);
            yield return _headTween.WaitForCompletion();
            _headTween.PlayBackwards();
        }
        if (showEffect)
        {
            _vfxDetect.Play();
            yield return new WaitForSeconds(0.25f);
        }

        FMODUnity.RuntimeManager.PlayOneShot(monsterStats.SfxMonsterReact, transform.position);
    }

    //trigerred when the monster enters into chase mode from the eyeManager

    //for now it only does something witht he sfx

    //Fire when entering chase mode
    public void EnterChaseMode()
    {
        Debug.Log("chase mode sfx");
        _monsterChaseMusicEmitter.Play();
        //FMODUnity.RuntimeManager.PlayOneShot(MonsterChaseSFXEvent, transform.position);
    }
    
    #region BehaviourTreeEvents
    //AI tree begins following player
    public void OnAINoticePlayer()
    {
        UpdateMonsterState(MonsterState.Follow);
    }
    
    //AI tree begins chasing player
    public void OnAIChasePlayer()
    {
        UpdateMonsterState(MonsterState.Chasing);
    }

    //AI tree goes back to patrol
    public void OnAIBackToPatrol()
    {
        UpdateMonsterState(MonsterState.Default);
    }
    
    #endregion


    //show distance on camera
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, monsterStats.DistanceToShowOnCamera);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            //biting game feel
            Sequence seq = DOTween.Sequence();
            seq.SetEase(Ease.OutCubic);
            seq.Append(_headObj.DOScaleY(_randomInitialSize + 0.5f, 0.5f));
            seq.Append(_headObj.DOScaleY(_randomInitialSize, 0.5f * 1.5f));
            _rigidbody2D.AddForce(transform.up * 20f, ForceMode2D.Impulse);

            //signal to the tree that we killed the player
            _behaviorTree.SendEvent("PlayerKilled");
        }
    }
}

