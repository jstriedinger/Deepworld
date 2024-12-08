using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using BehaviorDesigner.Runtime;
using DG.Tweening;
using FMODUnity;
using TMPro;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using LineRenderer = UnityEngine.LineRenderer;
using Random = UnityEngine.Random;

public enum MonsterState {  Default, Investigate, Follow, Chasing, Frustrated };

[RequireComponent(typeof(EyeManager), typeof(StudioEventEmitter))]
public abstract class MonsterBase : MonoBehaviour
{
    public MonsterState CurrentState { get; protected set; }

    [SerializeField] protected MonsterSO monsterStats;
    [SerializeField] protected ParticleSystem vfxSwimBubbles;
    [SerializeField] protected ParticleSystem vfxDetect;
    [SerializeField] protected Transform _tail;
    protected LineRenderer[] _tentacles;
    protected IEnumerator[] _attackColorTentacleLoop;
    [SerializeField] protected Transform _headObj;
    [SerializeField] protected Light2D _headLight;
    [SerializeField] protected Transform patrolObject;
    [Header("Reactions")]
    [SerializeField] protected bool canReactToPlayer;
    [SerializeField] protected bool canReactToCamera;
    [SerializeField] protected bool canAddChaseMusic;
    [SerializeField] private bool addForceWhenKillPlayer = true;
    [HideInInspector]
    public bool isKillingPlayer = false;

    //protected IEnumerator _attackColorAnimationLoop1,_attackColorAnimationLoop2,_attackColorAnimationLoop3,_attackColorAnimationLoop4;
    protected EyeManager _eyeManager;
    protected BehaviorTree _behaviorTree;
    protected AIPath _aiPath;
    
    protected StudioEventEmitter _monsterChaseMusicEmitter;
    protected Rigidbody2D _rigidbody2D;
    protected Sequence _colorTweenSequence;
    protected Tween _headTween;
    protected Tween _chaseScaleTween;
    protected bool _canReactToCall = false;
    
    protected AudioManager _audioManager;
    
    public MonsterSO GetMonsterStats()
    {
        return monsterStats;
    }
    

    protected virtual void Awake()
    {
        _behaviorTree = GetComponent<BehaviorTree>();
        _aiPath = GetComponent<AIPath>();
        _monsterChaseMusicEmitter = GetComponent<StudioEventEmitter>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _eyeManager = GetComponent<EyeManager>();

        float currentScale = _headObj.localScale.x;
        _chaseScaleTween = _headObj.DOScale(currentScale + 0.2f, 0.3f).SetLoops(-1, LoopType.Yoyo).SetAutoKill(false).Pause();

        _tentacles = _tail.GetComponentsInChildren<LineRenderer>();
        _attackColorTentacleLoop = new IEnumerator[_tentacles.Length];
        int i = 0;
        foreach (LineRenderer tentacle in _tentacles)
        {
            tentacle.colorGradient = monsterStats.DefaultColorGradient;
            _attackColorTentacleLoop[i] = AnimateColorLoop(tentacle,0.03f);
            i++;
        }
        _headLight.color = monsterStats.DefaultLightColor;
        UpdateMonsterState(MonsterState.Default);
        
        //testing color animation for attacking
        /*_attackColorAnimationLoop1 = AnimateColorLoop(_tentacles[0],0.03f);
        _attackColorAnimationLoop2 = AnimateColorLoop(_tentacles[1],0.03f);
        _attackColorAnimationLoop3 = AnimateColorLoop(_tentacles[2],0.03f);
        _attackColorAnimationLoop4 = AnimateColorLoop(_tentacles[3],0.03f);*/
        
        if (patrolObject)
        {
            List<GameObject> patrolPoints = new List<GameObject>();
            foreach (Transform child in patrolObject)
            {
                patrolPoints.Add(child.gameObject);
            }
            _behaviorTree.SetVariableValue("PatrolInfo",patrolPoints);
            
        }
    }

    protected virtual void Start()
    {
        _audioManager = GameObject.FindFirstObjectByType<AudioManager>();
        _colorTweenSequence = DOTween.Sequence();
    }

    protected virtual void OnEnable()
    {
        StartCoroutine("SwimBubbles");
    }
    
    IEnumerator SwimBubbles()
    {
        while(true)
        {
            vfxSwimBubbles.Play();
            yield return new WaitForSeconds(5); 
        }
    }
    
    public IEnumerator PlayReactSound(bool showEffect, bool animate)
    {
        if (showEffect)
        {
            vfxDetect.Play();
            yield return new WaitForSeconds(0.25f);
        }
        if (animate)
        {
            _headTween = _headObj.DOPunchScale(new Vector3(.75f, .75f, 0), .4f, 2, 0f);
        }

        FMODUnity.RuntimeManager.PlayOneShot(monsterStats.SfxMonsterReact, transform.position);
    }
    
    public void ToggleBehaviorTree(bool toggle)
    {
        if (toggle)
        {
            _aiPath.enabled = true;
            _behaviorTree.enabled = true;
            _behaviorTree.EnableBehavior();
        }
        else
        {
            _behaviorTree.DisableBehavior(true);
            _behaviorTree.enabled = false;
            _aiPath.enabled = false;

        }
    }
    
      /**
     * Function that updates the monster state visually
     */
    public void UpdateMonsterState(MonsterState newState, bool withSound = true)
    {
        //Kill all transitions and return to normal before deciding what to do
        _chaseScaleTween.Rewind();
        _headObj.transform.localScale = Vector3.one;
        _colorTweenSequence.Kill();
        _headTween.Kill();
        
        if (CurrentState == MonsterState.Chasing)
        {
            //we were on a chase
            if(canAddChaseMusic)
                _audioManager.UpdateMonstersChasing(false);
            
        }

        //If the new state is not Investigating then it can not react to Calls
        if (newState != MonsterState.Investigate && canReactToPlayer)
        {
            _behaviorTree.SetVariableValue("CanReactToCall",false);
            _canReactToCall = true;
        }
        
        switch (newState)
        {
            case MonsterState.Default:
                UpdateColorsAndToggleAnimation(monsterStats.DefaultColorGradient,false, monsterStats.DefaultLightColor);
                
                break;
            case MonsterState.Follow:
                UpdateColorsAndToggleAnimation(monsterStats.FollowColorGradient,false,  monsterStats.DefaultLightColor);
                if(withSound)
                    StartCoroutine(PlayReactSound(true, true));
                break;
            case MonsterState.Investigate:
                _behaviorTree?.SetVariableValue("CanReactToCall",true);
                _canReactToCall = true;
                
                UpdateColorsAndToggleAnimation(monsterStats.FollowColorGradient,false,  monsterStats.DefaultLightColor);
                if(withSound)
                    StartCoroutine(PlayReactSound(true, true));
                break;
            case MonsterState.Chasing:
                if (canAddChaseMusic)
                    _audioManager.UpdateMonstersChasing(true);
                _chaseScaleTween.Play();
                
                UpdateColorsAndToggleAnimation(monsterStats.ChaseColorGradient,true,  monsterStats.ChaseLightColor);
                if(withSound)
                    StartCoroutine(PlayReactSound(false, false));
                break;
            case MonsterState.Frustrated:
                if(withSound)
                    StartCoroutine(PlayReactSound(false, false));
                break;
            
        }

        CurrentState = newState;
        _eyeManager.OnUpdateMonsterState();
    }


    private void UpdateColorsAndToggleAnimation(Gradient newColorGradient, bool animate, Color newLightColor)
    {
        //always try to stop animation just in case
        foreach (IEnumerator colorLoopRoutine in _attackColorTentacleLoop)
        {
            StopCoroutine(colorLoopRoutine);
        }
        /*StopCoroutine(_attackColorAnimationLoop1);
        StopCoroutine(_attackColorAnimationLoop2);
        StopCoroutine(_attackColorAnimationLoop3);
        StopCoroutine(_attackColorAnimationLoop4);*/
        if (!animate)
        {
            
            //now we put back the current color keuy back to their default time positions
            //Remember we assume a structure of 4 color keys in every gradient
            foreach (LineRenderer tentacle in _tentacles)
            {
                Gradient colorGradient = tentacle.colorGradient;
                GradientColorKey[] colorKeys = colorGradient.colorKeys;

                colorKeys[0].time = monsterStats.DefaultColorGradient.colorKeys[0].time;
                colorKeys[1].time = monsterStats.DefaultColorGradient.colorKeys[1].time;
                colorKeys[2].time = monsterStats.DefaultColorGradient.colorKeys[2].time;
                colorKeys[3].time = monsterStats.DefaultColorGradient.colorKeys[3].time;
                
                colorGradient.SetKeys(colorKeys, monsterStats.DefaultColorGradient.alphaKeys);
                tentacle.colorGradient = colorGradient;
            }
            
        }
        //we assume all color gradient have the same key spacing, so we only have to change the colors of those keys
        _colorTweenSequence = DOTween.Sequence();
        foreach (LineRenderer tentacle in _tentacles)
        {
            //current gradient of line renderer
            Gradient colorGradient = tentacle.colorGradient;
            GradientColorKey[] colorKeys = colorGradient.colorKeys;
            
            _colorTweenSequence.Join(DOTween.To(() =>  colorKeys[0].color, x => colorKeys[0].color = x,
                newColorGradient.colorKeys[0].color, 1f).OnUpdate(() =>
            {
                colorGradient.SetKeys(colorKeys, colorGradient.alphaKeys);
                tentacle.colorGradient = colorGradient;
            }));
            _colorTweenSequence.Join(DOTween.To(() =>  colorKeys[1].color, x => colorKeys[1].color = x,
                newColorGradient.colorKeys[1].color, 1f).OnUpdate(() =>
            {
                colorGradient.SetKeys(colorKeys, colorGradient.alphaKeys);
                tentacle.colorGradient = colorGradient;
            }));
            _colorTweenSequence.Join(DOTween.To(() =>  colorKeys[2].color, x => colorKeys[2].color = x,
                newColorGradient.colorKeys[2].color, 1f).OnUpdate(() =>
            {
                colorGradient.SetKeys(colorKeys, colorGradient.alphaKeys);
                tentacle.colorGradient = colorGradient;
            }));
            _colorTweenSequence.Join(DOTween.To(() =>  colorKeys[3].color, x => colorKeys[3].color = x,
                newColorGradient.colorKeys[3].color, 1f).OnUpdate(() =>
            {
                colorGradient.SetKeys(colorKeys, colorGradient.alphaKeys);
                tentacle.colorGradient = colorGradient;
            }));
        }
        //the new light color will always be the first color fo the gradient. But a little darker
        _colorTweenSequence.Join(DOTween.To(() => _headLight.color, x => _headLight.color = x,
          newLightColor, 1f));

        if (animate)
        {
            //begin animation after smooth color transition
            _colorTweenSequence.OnComplete(() =>
            {
                
                foreach (IEnumerator colorLoopRoutine in _attackColorTentacleLoop)
                {
                    StartCoroutine(colorLoopRoutine);
                }
            });
        }

    }


    #region tentacleColorAnimation
    //remove first and last keys since they dont shift.
    List<GradientColorKey> RemoveFirstAndLast(Gradient incomingGradient)
    {
        List<GradientColorKey> currentColorKeys = new List<GradientColorKey>(incomingGradient.colorKeys);
        currentColorKeys.RemoveAt(currentColorKeys.Count-1);
        currentColorKeys.RemoveAt(0);
        return currentColorKeys;
    }
    
    IEnumerator AnimateColorLoop(LineRenderer lineRendererToChange, float movementPerTick = .001f)
    {
        lineRendererToChange.colorGradient = AddInitialCopy(lineRendererToChange.colorGradient);
        while(true)
        {
            List<GradientColorKey> currentColorKeys = RemoveFirstAndLast(lineRendererToChange.colorGradient);
            float highestTime=0;
            float lowestTime=1;
            int highestIndex = currentColorKeys.Count-1;
            int lowestIndex = 0;
            //Move all inner ones.
            for(int i = 0 ;i<currentColorKeys.Count;i++)
            {
                GradientColorKey tempColorKey = currentColorKeys[i];
                float newTime = tempColorKey.time + movementPerTick;
                
                if(newTime>1)
                {
                    newTime = newTime-1;
                }
                tempColorKey.time = newTime;
                currentColorKeys[i] = tempColorKey;
                if(newTime<lowestTime)
                {
                    lowestTime = newTime;
                    lowestIndex = i;
                }
                if(newTime>highestTime)
                {
                    highestTime = newTime;
                    highestIndex = i;
                }
            }
            Color newIntersectionColor = GetIntersectionColor(currentColorKeys,lowestIndex,highestIndex);
            currentColorKeys.Insert(0,new GradientColorKey(newIntersectionColor,0));
            currentColorKeys.Add(new GradientColorKey(newIntersectionColor,1));
            Gradient tempGradient = lineRendererToChange.colorGradient;
            tempGradient.colorKeys = currentColorKeys.ToArray();
            lineRendererToChange.colorGradient = tempGradient;  
            yield return new WaitForSeconds(0.01f);
        }
    }
    
    //returns the gradient with a copy of the first key for intersection purposes.
    Gradient AddInitialCopy(Gradient incomingGradient)
    {
        List<GradientColorKey> newColorKeys = new List<GradientColorKey>(incomingGradient.colorKeys);
        Color interSectionColor = newColorKeys[0].color;
        newColorKeys.Insert(0,new GradientColorKey(interSectionColor,0));
        Gradient newInitGradient = new Gradient();
        newInitGradient.colorKeys = newColorKeys.ToArray();
        return newInitGradient;
    }
    
    Color GetIntersectionColor(List<GradientColorKey> incomingKeys, int lowestIndex, int highestIndex)
    {
        Color firstColor = incomingKeys[lowestIndex].color;
        Color lastColor = incomingKeys[highestIndex].color;
        float distance = 1 - (incomingKeys[highestIndex].time - incomingKeys[lowestIndex].time);
        float colorLerpAmount = (1f-incomingKeys[highestIndex].time) / distance;;
        Color newIntersectionColor = Color.Lerp(lastColor,firstColor,colorLerpAmount);
        return newIntersectionColor;
    }
    
    #endregion

    public void AttackPlayerAnim()
    {
        _chaseScaleTween.Rewind();
        Sequence seq = DOTween.Sequence();
        seq.SetEase(Ease.OutCubic);
        seq.Append(_headObj.DOScaleY(2f, 0.65f));
        seq.Append(_headObj.DOScaleY(1f, 0.65f  * 1.5f));
    }
    public void EatPlayerAnimation()
    {
        _chaseScaleTween.Rewind();
        if (addForceWhenKillPlayer)
        {
            _rigidbody2D.AddForce(transform.up * 20f, ForceMode2D.Impulse);
        }
        Sequence seq = DOTween.Sequence();
        seq.AppendCallback(() =>
        {
            isKillingPlayer = true;
            _behaviorTree.DisableBehavior();
        });
        seq.SetEase(Ease.OutCubic);
        seq.Append(_headObj.DOScaleY(1.7f, 0.5f));
        seq.Append(_headObj.DOScaleY(1f, 0.5f  * 1.5f));
        //seq.Append(_headObj.DOPunchScale(new Vector3(1f, .25f, 0), 0.5f, 5, 1));
        seq.Append(_headObj.DOPunchRotation(new Vector3(0,0,80), 1f, 5, 1));
        seq.AppendInterval(2);
        seq.AppendCallback(() =>
        {
            _behaviorTree.EnableBehavior();
            isKillingPlayer = false;
        });
    }
}

public class MonsterReactive : MonsterBase
{
    private CameraManager _cameraManager;
    private GameManager _gameManager;
    
    [HideInInspector]
    public bool inCamera = false;
    
    protected override void Awake()
    {
        base.Awake();

        if (canReactToPlayer)
        {
            _behaviorTree.SetVariableValue("PatrolSpeed",monsterStats.PatrolSpeed);
            _behaviorTree.SetVariableValue("FollowSpeed",monsterStats.FollowSpeed);
            _behaviorTree.SetVariableValue("ChasingSpeed",monsterStats.ChasingSpeed);
            _behaviorTree.SetVariableValue("ChasingRange",monsterStats.ChasingRange);
            _behaviorTree.SetVariableValue("FollowRange",monsterStats.FollowRange);
            _behaviorTree.SetVariableValue("isPatrolType",true);
        }
        
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        if (canReactToPlayer )
        {
            _gameManager = GameObject.FindFirstObjectByType<GameManager>();
            _cameraManager = GameObject.FindFirstObjectByType<CameraManager>();
            _behaviorTree.SetVariableValue("playerRef",_gameManager.playerRef.gameObject);
            _behaviorTree.SetVariableValue("playerLastPosition",_gameManager.playerLastPosition);
        }
    }

    
    private void FixedUpdate()
    {
        if(canReactToCamera && !GameManager.IsPlayerDead)
            UpdateEnemyInCamera();
    }
    
    
    /*public void UpdatePatrolInfo(Transform newPatrolInfo)
    {
        List<GameObject> patrolPoints = new List<GameObject>();
        foreach (Transform child in newPatrolInfo)
        {
            patrolPoints.Add(child.gameObject);
        }
        _behaviorTree.SetVariableValue("PatrolInfo",patrolPoints);
    }*/

    public bool CanReactToPlayer()
    {
        return canReactToPlayer;
    }
    

    //Always check if its near playerCharacter to visualize on vcam target group
    private void UpdateEnemyInCamera()
    {
        Collider2D playerCheckCamera = Physics2D.OverlapCircle(transform.position, monsterStats.DistanceToShowOnCamera, LayerMask.GetMask("Player"));

        if(playerCheckCamera)
        {
            if(!inCamera)
            {
                //add to the
                _cameraManager.AddObjectToCameraView(transform, true, false);
                inCamera = true;

            }
        }
        else if(inCamera)
        {
            _cameraManager.RemoveObjectFromCameraView(transform, true);
            inCamera = false;
        }


    }


    //react to playerCharacter call, go investigate a position using a lastpos object since btree needs an object
    public void ReactToPlayerCall()
    {
        if (canReactToPlayer && CurrentState != MonsterState.Chasing && CurrentState != MonsterState.Follow)
        {
            if (_canReactToCall || CurrentState == MonsterState.Investigate) 
            {
                //if mosnter is not chasing or following, it can react to playerCharacter call
                UpdateMonsterState(MonsterState.Investigate);
            }
        }
    }


    //Fire when entering chase mode
    protected void EnterChaseMode()
    {
        Debug.Log("chase mode sfx");
        _monsterChaseMusicEmitter.Play();
    }
    
    #region BehaviourTreeEvents
    public void OnAIReactToCall()
    {
        //only if we are not already chasing or following a playerCharacter
        UpdateMonsterState(MonsterState.Investigate);
    }
    //AI tree begins following playerCharacter
    public void OnAINoticePlayer()
    {
        UpdateMonsterState(MonsterState.Follow);
    }
    
    //AI tree begins chasing playerCharacter
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


    

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player") && canReactToPlayer)
        {
            //biting game feel
            _audioManager.StopChaseMusic();
            EatPlayerAnimation();
            //signal to the tree that we killed the playerCharacter
            //_behaviorTree.SendEvent("PlayerKilled");
        }
    }

    //show distance on camera
    private void OnDrawGizmosSelected()
    {
        if(canReactToPlayer)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, monsterStats.DistanceToShowOnCamera);
            
        }
    }
    
    
}

