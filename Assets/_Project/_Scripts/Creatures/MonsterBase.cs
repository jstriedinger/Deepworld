using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using DG.Tweening;
using FMODUnity;
using Pathfinding;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Gilzoide.UpdateManager;

public enum MonsterState {  Default, Investigate, Follow, Chasing, Frustrated };

[RequireComponent(typeof(MonsterEyeManager), typeof(StudioEventEmitter))]
public abstract class MonsterBase : AManagedBehaviour
{
    public MonsterState CurrentState { get; protected set; }

    [SerializeField] protected MonsterSO monsterStats;
    [SerializeField] protected MonsterTails monsterTails;
    [SerializeField] protected ParticleSystem vfxSwimBubbles;
    [SerializeField] protected ParticleSystem vfxDetect;
    [SerializeField] protected Transform _tail;
    protected LineRenderer[] Tentacles;
    protected IEnumerator[] AttackColorTentacleLoop;
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
    protected MonsterEyeManager MonsterEyeManager;
    protected BehaviorTree _behaviorTree;
    protected AIPath _aiPath;
    
    protected StudioEventEmitter _monsterChaseMusicEmitter;
    protected Rigidbody2D _rigidbody2D;
    protected Sequence _colorTweenSequence;
    protected Tween _headTween;
    protected Tween _chaseScaleTween;
    //protected bool _canReactToCall = false;
    
    
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
        MonsterEyeManager = GetComponent<MonsterEyeManager>();
    }

    protected virtual void Start()
    {
        Tentacles = _tail.GetComponentsInChildren<LineRenderer>();
        AttackColorTentacleLoop = new IEnumerator[Tentacles.Length];
        int i = 0;
        foreach (LineRenderer tentacle in Tentacles)
        {
            tentacle.colorGradient = monsterStats.DefaultColorGradient;
            AttackColorTentacleLoop[i] = AnimateColorLoop(tentacle,0.03f);
            i++;
        }
        _headLight.color = monsterStats.DefaultLightColor;
        
        float currentScale = _headObj.localScale.x;
        _chaseScaleTween = _headObj.DOScale(currentScale + 0.2f, 0.3f).SetLoops(-1, LoopType.Yoyo).SetAutoKill(false).Pause();
        _colorTweenSequence = DOTween.Sequence();
        
        
        if (patrolObject)
        {
            List<GameObject> patrolPoints = new List<GameObject>();
            foreach (Transform child in patrolObject)
            {
                patrolPoints.Add(child.gameObject);
            }
            _behaviorTree.SetVariableValue("PatrolInfo",patrolPoints);
            
        }
        
        StartCoroutine("SwimBubbles");
        //first mosnter state update
        if (canReactToPlayer)
        {
            _behaviorTree.SetVariableValue("CanReactToCall",false);
        }
        UpdateColorsAndToggleAnimation(monsterStats.DefaultColorGradient,false, monsterStats.DefaultLightColor);
        CurrentState = MonsterState.Default;
        MonsterEyeManager.OnUpdateMonsterState();
    }

    IEnumerator SwimBubbles()
    {
        while(true)
        {
            vfxSwimBubbles.Play();
            yield return new WaitForSeconds(5); 
        }
    }
    
    public IEnumerator PlayReactSfx(bool showEffect, bool animate)
    {
        if (showEffect)
        {
            Debug.Log("PLaying react vfx");
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
    public void UpdateMonsterState(MonsterState newState, bool withSound = true, bool showEffect = false)
    {
        if (CurrentState == newState && CurrentState != MonsterState.Default)
            return;
        //Kill all transitions and return to normal before deciding what to do
        _chaseScaleTween.Rewind();
        _headObj.transform.localScale = Vector3.one;
        _colorTweenSequence.Kill();
        _headTween.Kill();
        
        if (CurrentState == MonsterState.Chasing)
        {
            //we were on a chase
            if(canAddChaseMusic)
                AudioManager.Instance?.UpdateMonstersChasing(false);
            
        }

        //If the new state is not Investigating then it can not react to Calls
        if (newState != MonsterState.Investigate && canReactToPlayer)
        {
            _behaviorTree.SetVariableValue("CanReactToCall",false);
            //_canReactToCall = true;
        }
        
        switch (newState)
        {
            case MonsterState.Default:
                UpdateColorsAndToggleAnimation(monsterStats.DefaultColorGradient,false, monsterStats.DefaultLightColor);
                
                break;
            case MonsterState.Follow:
                UpdateColorsAndToggleAnimation(monsterStats.FollowColorGradient,false,  monsterStats.DefaultLightColor);
                if(withSound)
                    StartCoroutine(PlayReactSfx(true, true));
                break;
            case MonsterState.Investigate:
                _behaviorTree?.SetVariableValue("CanReactToCall",true);
                //_canReactToCall = true;
                
                UpdateColorsAndToggleAnimation(monsterStats.FollowColorGradient,false,  monsterStats.DefaultLightColor);
                if(withSound)
                    StartCoroutine(PlayReactSfx(true, true));
                break;
            case MonsterState.Chasing:
                if (canAddChaseMusic)
                    AudioManager.Instance?.UpdateMonstersChasing(true);
                _chaseScaleTween.Play();
                
                UpdateColorsAndToggleAnimation(monsterStats.ChaseColorGradient,true,  monsterStats.ChaseLightColor);
                if(withSound)
                    StartCoroutine(PlayReactSfx(showEffect, false));
                break;
            case MonsterState.Frustrated:
                if(withSound)
                    StartCoroutine(PlayReactSfx(showEffect, false));
                break;
            
        }

        CurrentState = newState;
        MonsterEyeManager.OnUpdateMonsterState();
        monsterTails.OnUpdateMonsterState(newState);
    }


    private void UpdateColorsAndToggleAnimation(Gradient newColorGradient, bool animate, Color newLightColor)
    {
        //always try to stop animation just in case
        foreach (IEnumerator colorLoopRoutine in AttackColorTentacleLoop)
        {
            StopCoroutine(colorLoopRoutine);
        }
        
        if (!animate)
        {
            
            //now we put back the current color keuy back to their default time positions
            //Remember we assume a structure of 4 color keys in every gradient
            foreach (LineRenderer tentacle in Tentacles)
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
        foreach (LineRenderer tentacle in Tentacles)
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
                
                foreach (IEnumerator colorLoopRoutine in AttackColorTentacleLoop)
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

    //Attack player sequence
    public void AttackPlayerAnim()
    {
        _chaseScaleTween.Rewind();
        FMODUnity.RuntimeManager.PlayOneShot(monsterStats.SfxMonsterAttack);
        Sequence seq = DOTween.Sequence();
        seq.SetEase(Ease.OutCubic);
        seq.Append(_headObj.DOScaleY(2f, 0.65f));
        seq.Append(_headObj.DOScaleY(1f, 0.65f  * 1.5f));
    }

    //extra method to play swim sfx
    public void PlaySwimSfx()
    {
        FMODUnity.RuntimeManager.PlayOneShot(monsterStats.SfxMonsterAttack);
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
            _behaviorTree.SetVariableValue("isChasing",false);
            _behaviorTree.SetVariableValue("isFollowing",false);
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

