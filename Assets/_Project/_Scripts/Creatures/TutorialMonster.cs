using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using DG.Tweening;
using FMODUnity;
using UnityEngine;
using UnityEngine.Rendering.Universal;

//TUtorial monster only does
/// <summary>
/// Tutorial monster has no movement mechanic at all
/// is only moved through dotween for the specific cinematics
/// Only has the update state stuff
/// </summary>
public class TutorialMonster : MonoBehaviour
{
    [SerializeField] private MonsterSO monsterStats;

    private MonsterState _monsterState;
    
    [SerializeField] private LineRenderer[] _tentacles;
    [SerializeField] private Transform _headObj;
    [SerializeField] private Light2D _headLight;
    
    private EyeManager _eyeManager;
    private BehaviorTree _behaviorTree;
    private StudioEventEmitter _monsterChaseMusicEmitter;
    private EyeTracker _eyeTracker;
    
    private Rigidbody2D _rigidbody2D;
    private ParticleSystem _vfxDetect;
    private Tween _headTween;
    private Sequence _colorTweenSequence;
    private Tween _chaseScaleTween;
    
    // Start is called before the first frame update
    void Start()
    {
        _eyeManager = GetComponent<EyeManager>();
        _monsterChaseMusicEmitter = GetComponent<StudioEventEmitter>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _eyeTracker = GetComponentInChildren<EyeTracker>();

        _vfxDetect = _headObj.GetComponentInChildren<ParticleSystem>();

        _chaseScaleTween = _headObj.DOScale(1.2f, 0.3f).SetLoops(-1, LoopType.Yoyo).SetAutoKill(false).Pause();
        
        foreach (LineRenderer tentacle in _tentacles)
        {
            tentacle.startColor = monsterStats.DefaultColor;
        }

        _headLight.color = monsterStats.DefaultColor*1.5f;
        _colorTweenSequence = DOTween.Sequence();
        _behaviorTree = GetComponent<BehaviorTree>();
        
        
    }
    
    public void ToggleTrackTarget(GameObject obj)
    {
        _eyeTracker.ToggleTrackTarget(obj);
    }
    
    public void UpdateMonsterState(MonsterState newState)
    {
        if (_monsterState == MonsterState.Chasing)
        {
            //we were on a chase
            _chaseScaleTween.Rewind();
        }

        if (newState != MonsterState.Investigate)
        {
        }
        //kill whatever is happening with head animation
        if (_monsterState != MonsterState.Follow)
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
                
                UpdateColors(monsterStats.FollowColor, monsterStats.FollowColor);
                
                StartCoroutine(PlayReactSound(true, true));
                break;
            case MonsterState.Chasing:
                _chaseScaleTween.Play();
                UpdateColors(monsterStats.ChaseColor, monsterStats.ChaseColor);
                
                StartCoroutine(PlayReactSound(false, false));
                break;
            case MonsterState.Frustrated:
                StartCoroutine(PlayReactSound(false, false));
                //DOTween.To(() => _light2D.color, x => _light2D.color = x, monsterStats.FollowColor, 0.5f);
                break;
            
        }

        _monsterState = newState;
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
    
    public IEnumerator PlayReactSound(bool showEffect, bool animate)
    {
        if (showEffect)
        {
            _vfxDetect.Play();
            yield return new WaitForSeconds(0.25f);
        }
        if (animate)
        {
            _headTween = _headObj.DOPunchScale(new Vector3(.75f, .75f, 0), .4f, 2, 0f);
        }

        FMODUnity.RuntimeManager.PlayOneShot(monsterStats.SfxMonsterReact, transform.position);
    }


    
}
