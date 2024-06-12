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

    [Header("Data")]
    [SerializeField] private MonsterSO monsterStats;

    private MonsterState _monsterState;
    
    [SerializeField] private LineRenderer[] _tentacles;
    [SerializeField] private Transform _headObj;
    [SerializeField] private Light2D _headLight;
    
    private EyeManager _eyeManager;
    private StudioEventEmitter _monsterChaseMusicEmitter;
    
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

        _vfxDetect = _headObj.GetComponentInChildren<ParticleSystem>();

        _chaseScaleTween = _headObj.DOScale(1.2f, 0.3f).SetLoops(-1, LoopType.Yoyo).SetAutoKill(false).Pause();
        
        foreach (LineRenderer tentacle in _tentacles)
        {
            tentacle.startColor = monsterStats.DefaultColor;
        }

        _headLight.color = monsterStats.DefaultColor*1.5f;
        
        //_audioManager = GameObject.FindFirstObjectByType<AudioManager>();

        _colorTweenSequence = DOTween.Sequence();
        //pass data from SO to the AI Tree
        
    }

    
    // Update is called once per frame
    void Update()
    {
        
    }


    void ChangeTutorialMonsterState()
    {
        
    }
}
