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

public class EnemyMonster : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private MonsterSO monsterStats;
    [SerializeField] private Transform patrolObject;
    [SerializeField] public MonsterState CurrentState { get; private set; }
    
    [SerializeField] ParticleSystem vfxSwimBubbles;
    [SerializeField] private LineRenderer[] _tentacles;
    [SerializeField] private Transform _headObj;
    [SerializeField] private Light2D _headLight;
    [SerializeField] private bool _canAffectCamera = true;
    [SerializeField] private ParticleSystem vfxDetect;
    private IEnumerator _attackColorAnimationLoop1,_attackColorAnimationLoop2,_attackColorAnimationLoop3,_attackColorAnimationLoop4;
    
    private CameraManager _cameraManager;
    private GameManager _gameManager;
    private AudioManager _audioManager;

    private EyeManager _eyeManager;
    private BehaviorTree _behaviorTree;
    private AIPath _aiPath;
    private StudioEventEmitter _monsterChaseMusicEmitter;
    private Vector3 _origWorldPos;
    [HideInInspector]
    public bool isChasing = false, inCamera = false;

    private bool _canReactToCall = false;
    private Rigidbody2D _rigidbody2D;
    private Tween _headTween;
    private Sequence _colorTweenSequence;
    private Tween _chaseScaleTween;
    private EyeTracker _eyeTracker;


    private void Awake()
    {
        isChasing = false;
        _eyeManager = GetComponent<EyeManager>();
        _monsterChaseMusicEmitter = GetComponent<StudioEventEmitter>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _eyeTracker = GetComponentInChildren<EyeTracker>();

        //testing color animation for attacking
        _attackColorAnimationLoop1 = AnimateColorLoop(_tentacles[0],0.03f);
        _attackColorAnimationLoop2 = AnimateColorLoop(_tentacles[1],0.03f);
        _attackColorAnimationLoop3 = AnimateColorLoop(_tentacles[2],0.03f);
        _attackColorAnimationLoop4 = AnimateColorLoop(_tentacles[3],0.03f);
        
       

        _chaseScaleTween = _headObj.DOScale(1.2f, 0.3f).SetLoops(-1, LoopType.Yoyo).SetAutoKill(false).Pause();
        
        foreach (LineRenderer tentacle in _tentacles)
        {
            tentacle.colorGradient = monsterStats.DefaultColorGradient;
        }

        _headLight.color = monsterStats.DefaultLightColor;
        //pass data from SO to the AI Tree
        //Reactive type means it used for gameplay: reacts to player and has behavoir
        _behaviorTree = GetComponent<BehaviorTree>();
        _aiPath = GetComponent<AIPath>();
        if (monsterStats.IsReactive)
        {
            _behaviorTree.SetVariableValue("PatrolSpeed",monsterStats.PatrolSpeed);
            _behaviorTree.SetVariableValue("FollowSpeed",monsterStats.FollowSpeed);
            _behaviorTree.SetVariableValue("ChasingSpeed",monsterStats.ChasingSpeed);
            _behaviorTree.SetVariableValue("ChasingRange",monsterStats.ChasingRange);
            _behaviorTree.SetVariableValue("FollowRange",monsterStats.FollowRange);
            _behaviorTree.SetVariableValue("isPatrolType",monsterStats.IsReactive);
            
        }
        List<GameObject> patrolPoints = new List<GameObject>();
        foreach (Transform child in patrolObject)
        {
            patrolPoints.Add(child.gameObject);
        }
        _behaviorTree.SetVariableValue("PatrolInfo",patrolPoints);
        
    }

    // Start is called before the first frame update
    void Start()
    {
        _audioManager = GameObject.FindFirstObjectByType<AudioManager>();
        if (monsterStats.IsReactive )
        {
            _gameManager = GameObject.FindFirstObjectByType<GameManager>();
            _cameraManager = GameObject.FindFirstObjectByType<CameraManager>();
            _behaviorTree.SetVariableValue("playerRef",_gameManager.playerRef.gameObject);
            _behaviorTree.SetVariableValue("playerLastPosition",_gameManager.playerLastPosition);
        }

        _colorTweenSequence = DOTween.Sequence();
        
    }

    private void OnEnable()
    {
        StartCoroutine("SwimBubbles");
    }


    private void FixedUpdate()
    {
        if(monsterStats.IsReactive && _canAffectCamera && !GameManager.IsPlayerDead)
            UpdateEnemyInCamera();
    }

    public void ToggleTrackTarget(GameObject obj)
    {
        _eyeTracker.ToggleTrackTarget(obj);
    }
    
    public void UpdatePatrolInfo(Transform newPatrolInfo)
    {
        List<GameObject> patrolPoints = new List<GameObject>();
        foreach (Transform child in newPatrolInfo)
        {
            patrolPoints.Add(child.gameObject);
        }
        _behaviorTree.SetVariableValue("PatrolInfo",patrolPoints);
    }

    public bool IsGameplayActiveMonster()
    {
        return monsterStats.IsReactive;
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

    public MonsterSO GetMonsterStats()
    {
        return monsterStats;
    }



    //react to player call, go investigate a position using a lastpos object since btree needs an object
    public void ReactToPlayerCall()
    {
        if (monsterStats.IsReactive && CurrentState != MonsterState.Chasing && CurrentState != MonsterState.Follow)
        {
            if (!_canReactToCall || CurrentState == MonsterState.Investigate) 
            {
                Debug.Log("Reacting!");
                //if mosnter is not chasing or following, it can react to player call
                UpdateMonsterState(MonsterState.Investigate);
            }
        }
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

    
    public void OnAIReactToCall()
    {
        //only if we are not already chasing or following a player
        UpdateMonsterState(MonsterState.Investigate);
    }
    
    /**
     * Function that updates the monster state visually
     */
    public void UpdateMonsterState(MonsterState newState)
    {
        //Kill all transitions and return to normal before deciding what to do
        _chaseScaleTween.Rewind();
        _headObj.transform.localScale = Vector3.one;
        _colorTweenSequence.Kill();
        _headTween.Kill();
        
        if (CurrentState == MonsterState.Chasing)
        {
            //we were on a chase
            if(monsterStats.IsReactive)
                _audioManager.UpdateMonstersChasing(false);
            
        }

        //If the new state is not Investigating then it can not react to Calls
        if (newState != MonsterState.Investigate && monsterStats.IsReactive)
        {
            _behaviorTree.SetVariableValue("CanReactToCall",false);
            _canReactToCall = false;
        }
        
        switch (newState)
        {
            case MonsterState.Default:
                UpdateColorsAndToggleAnimation(monsterStats.DefaultColorGradient,false, monsterStats.DefaultLightColor);
                
                break;
            case MonsterState.Follow:
                UpdateColorsAndToggleAnimation(monsterStats.FollowColorGradient,false,  monsterStats.DefaultLightColor);
                StartCoroutine(PlayReactSound(true, true));
                break;
            case MonsterState.Investigate:
                _behaviorTree.SetVariableValue("CanReactToCall",true);
                _canReactToCall = true;
                
                UpdateColorsAndToggleAnimation(monsterStats.FollowColorGradient,false,  monsterStats.DefaultLightColor);
                StartCoroutine(PlayReactSound(true, true));
                break;
            case MonsterState.Chasing:
                if(monsterStats.IsReactive)
                    _audioManager.UpdateMonstersChasing(true);
                _chaseScaleTween.Play();
                
                UpdateColorsAndToggleAnimation(monsterStats.ChaseColorGradient,true,  monsterStats.ChaseLightColor);
                StartCoroutine(PlayReactSound(false, false));
                break;
            case MonsterState.Frustrated:
                StartCoroutine(PlayReactSound(false, false));
                break;
            
        }

        CurrentState = newState;
        _eyeManager.OnUpdateMonsterState();
    }

    private void UpdateColorsAndToggleAnimation(Gradient newColorGradient, bool animate, Color newLightColor)
    {
        //always try to stop animation just in case
        StopCoroutine(_attackColorAnimationLoop1);
        StopCoroutine(_attackColorAnimationLoop2);
        StopCoroutine(_attackColorAnimationLoop3);
        StopCoroutine(_attackColorAnimationLoop4);
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
                StartCoroutine(_attackColorAnimationLoop1);
                StartCoroutine(_attackColorAnimationLoop2);
                StartCoroutine(_attackColorAnimationLoop3);
                StartCoroutine(_attackColorAnimationLoop4);
            });
        }

    }

    //remove first and last keys since they dont shift.
    List<GradientColorKey> RemoveFirstAndLast(Gradient incomingGradient)
    {
        List<GradientColorKey> currentColorKeys = new List<GradientColorKey>(incomingGradient.colorKeys);
        currentColorKeys.RemoveAt(currentColorKeys.Count-1);
        currentColorKeys.RemoveAt(0);
        return currentColorKeys;
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


    //Fire when entering chase mode
    public void EnterChaseMode()
    {
        Debug.Log("chase mode sfx");
        _monsterChaseMusicEmitter.Play();
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
        if(monsterStats.IsReactive)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, monsterStats.DistanceToShowOnCamera);
            
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player") && monsterStats.IsReactive)
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
        _chaseScaleTween.Rewind();
        _behaviorTree.DisableBehavior();
        _rigidbody2D.AddForce(transform.up * 5f, ForceMode2D.Impulse);
        Sequence seq = DOTween.Sequence();
        seq.Append(_headObj.DOPunchRotation(new Vector3(0,0,60), .75f, 5, 1));
        //seq.Append(_headObj.DOPunchScale(new Vector3(.25f, 1f, 0), 0.75f, 5, 1));
        yield return new WaitForSecondsRealtime(2);
        _behaviorTree.EnableBehavior();
    }
    
    IEnumerator SwimBubbles()
    {
        while(true)
        {
            vfxSwimBubbles.Play();
            yield return new WaitForSeconds(5); 

        }

    }
}

