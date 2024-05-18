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
    
    private CameraManager _cameraManager;
    private GameManager _gameManager;
    private AudioManager _audioManager;

    private EyeManager _eyeManager;
    private BehaviorTree _behaviorTree;
    private StudioEventEmitter _monsterChaseMusicEmitter;
    private Vector3 origWorldPos;
    [HideInInspector]
    public bool isChasing = false, inCamera = false;

    private bool _canReactToCall = false;
    private Rigidbody2D _rigidbody2D;
    private ParticleSystem _vfxDetect;
    private Tween _headTween;
    private Sequence _colorTweenSequence;
    private Tween _scaleTween;


    private void Awake()
    {
        _eyeManager = GetComponent<EyeManager>();
        _behaviorTree = GetComponent<BehaviorTree>();
        _monsterChaseMusicEmitter = GetComponent<StudioEventEmitter>();
        isChasing = false;
        _rigidbody2D = GetComponent<Rigidbody2D>();

        _vfxDetect = _headObj.GetComponentInChildren<ParticleSystem>();

        _scaleTween = _headObj.DOScale(1.25f, 0.3f).SetLoops(-1, LoopType.Yoyo).SetAutoKill(false).Pause();
        
        foreach (LineRenderer tentacle in _tentacles)
        {
            tentacle.startColor = monsterStats.DefaultColor;
        }

        _headLight.color = monsterStats.DefaultColor*1.5f;
        //pass data from SO to the AI Tree
        if (_behaviorTree)
        {
            _behaviorTree.SetVariableValue("PatrolSpeed",monsterStats.PatrolSpeed);
            if (!monsterStats.IsSimpleType)
            {
                _behaviorTree.SetVariableValue("FollowSpeed",monsterStats.FollowSpeed);
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
        _cameraManager = GameObject.FindFirstObjectByType<CameraManager>();
        _gameManager = GameObject.FindFirstObjectByType<GameManager>();
        _audioManager = GameObject.FindFirstObjectByType<AudioManager>();
        if (_behaviorTree && !monsterStats.IsSimpleType)
        {
            _behaviorTree.SetVariableValue("playerRef",_gameManager.playerRef.gameObject);
            _behaviorTree.SetVariableValue("playerLastPosition",_gameManager.playerLastPosition);
        }

        _colorTweenSequence = DOTween.Sequence();
        //_light2D.color = monsterStats.DefaultColor;
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
            if(!inCamera)
            {
                //add to the
                _cameraManager.AddMonsterToView(gameObject);
                inCamera = true;

            }
        }
        else if(inCamera)
        {
            _cameraManager.RemoveEnemyFromCameraView(gameObject);
            inCamera = false;
        }


    }

    public MonsterSO GetMonsterStats()
    {
        return monsterStats;
    }



    //react to player call, go investigate a position using a lastpos object since btree needs an object
    public void ReactToPlayerCall()
    {
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
        if (CurrentState == MonsterState.Chasing)
        {
            //we were on a chase
            _audioManager.UpdateMonstersChasing(false);
            _scaleTween.Rewind();
        }

        if (newState != MonsterState.Investigate)
        {
            if (_canReactToCall)
            {
                _behaviorTree.SetVariableValue("CanReactToCall",false);
                _canReactToCall = false;
            }
        }
        //kill whatever is happening with head animation
        if (CurrentState != MonsterState.Follow)
        {
            //it wasnt following, kill whatever animation is happening and go back to head scale
            _headTween.Kill();
            _headObj.transform.localScale = Vector3.one;
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
                
                StartCoroutine(PlayReactSound(true, true));
                break;
            case MonsterState.Chasing:
                //important for the chase music bg
                _audioManager.UpdateMonstersChasing(true);
                _scaleTween.Play();
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
        if (showEffect)
        {
            _vfxDetect.Play();
            yield return new WaitForSeconds(0.25f);
        }
        if (animate)
        {
            _headTween = _headObj.DOPunchScale(new Vector3(1.25f, 1.25f, 0), .5f, 2, 0f);
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
            _audioManager.StopChaseMusic();
            StartCoroutine(EatPlayerAnimation());
            //signal to the tree that we killed the player
            //_behaviorTree.SendEvent("PlayerKilled");
        }
    }

    IEnumerator EatPlayerAnimation()
    {
        _scaleTween.Rewind();
        _behaviorTree.DisableBehavior();
        _rigidbody2D.AddForce(transform.up * 5f, ForceMode2D.Impulse);
        Sequence seq = DOTween.Sequence();
        seq.Append(_headObj.DOPunchRotation(new Vector3(0,0,60), .75f, 5, 1));
        //seq.Append(_headObj.DOPunchScale(new Vector3(.25f, 1f, 0), 0.75f, 5, 1));
        yield return new WaitForSecondsRealtime(2);
        _behaviorTree.EnableBehavior();
    }
}

