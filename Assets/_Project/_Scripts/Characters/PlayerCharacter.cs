using System;
using System.Collections;
using BehaviorDesigner.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using FMODUnity;
using Unity.Mathematics;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using Gilzoide.UpdateManager;

public class PlayerCharacter : AManagedBehaviour, IUpdatable
{

    public static event Action PlayerOnControlsChanged, OnPauseGame, OnPlayerSwim, OnPlayerCall
        , OnPlayerHide;
    //managers
    private Tentacle[] _proceduralTentacles;
    private TentacleDynamic[] _proceduralDynamicTentacles;
    private BodyTentacle _proceduralBody;
    private EyeFollower _eyeFollower;
    private TentacleGate _closeTentacleGate;

    [Header("Movement settings")] 
    [SerializeField] private GameObject bodyPart;
    [SerializeField] Transform headPart;
    [SerializeField] float maxSpeed;
    [SerializeField] float slowSpeed;
    [SerializeField] float rotationSpeed;
    [SerializeField] float swimForce;
    [SerializeField] float timeBetweenSwim;
    [SerializeField] ParticleSystem vfxSwimBubbles;
    [SerializeField] private ParticleSystem vfxDeath;
    [SerializeField] EventReference sfxDeath;
    [SerializeField] EventReference sfxSwim;
    private StudioEventEmitter _normalSwimSfxEmitter;

    //Input and movement handler
    [HideInInspector] public PlayerInput playerInput;
    [HideInInspector] public bool swimStage;
    private Vector2 _moveInputValue;
    private Vector3 _finalMovement;
    private Rigidbody2D _rigidBody;
    private Collider2D _collider;
    private float _nextSwim;
    private float _nextCall;
    
    public bool isHidden = false;
    public BlueNPC _blueRef;

    [Header("UI")]
    [SerializeField] Transform playerUIPrompt;
    private bool _isPlayerPromptActive = false;

    [Header("Animation")]
    [Tooltip("How much the head scales doing the swim")]

    [Header("Call action")]
    [SerializeField] private ParticleSystem vfxVoice;
    [SerializeField] private EventReference sfxCall1;
    [SerializeField] private EventReference sfxCall2;
    private EventReference _sfxCallLastTime;
    [SerializeField] private float callRadiusForEnemies;
    [SerializeField] private float callRadiusDoor;

    [Header("Cheatcodes")] public bool inmortal = false;

    private void Awake()
    {

        _collider = GetComponent<Collider2D>();
        _rigidBody = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        _normalSwimSfxEmitter = GetComponent<StudioEventEmitter>();
        _normalSwimSfxEmitter.EventInstance.setVolume(0.5f);

        _proceduralDynamicTentacles = GetComponentsInChildren<TentacleDynamic>();
        _proceduralTentacles = GetComponentsInChildren<Tentacle>();
        _proceduralBody = GetComponentInChildren<BodyTentacle>();
        _eyeFollower = GetComponentInChildren<EyeFollower>();

    }

    protected override void OnEnable()
    {
        base.OnEnable();
        GameManager.OnRestartingGame += OnRestartingGame;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        GameManager.OnRestartingGame -= OnRestartingGame;
    }

    #region InputSystem
    /**
     * Function triggeres when theres a movement input. 
     */
    private void OnMove(InputValue inputValue)
    {
        _moveInputValue = inputValue.Get<Vector2>();
    }

    private void OnSwim(InputValue inputValue)
    {
        if (inputValue.isPressed)
        {
            Swim();
            
        }
    }

    public IEnumerator AddImpulseForceToPlayer(Vector3 dir, float force)
    {
        StopMovement();
        _rigidBody.AddForce(dir * force,ForceMode2D.Impulse);
        playerInput.actions.FindAction("Move").Disable();
        yield return new WaitForSeconds(0.2F);
        playerInput.actions.FindAction("Move").Enable();
    }

    private void OnCall(InputValue inputValue)
    {
        //playerInput.actions.FindAction("Call").Disable();
        if (inputValue.isPressed)
        {
            if (Time.time >= _nextCall)
            {
                _nextCall = Time.time + 2f;
                PlayCallSfx(true);
            }
        }
    }

    IEnumerator RumbleController()
    {
        Gamepad pad = Gamepad.current;

        if (pad != null && playerInput.currentControlScheme == "Gamepad")
        {
            pad.SetMotorSpeeds(0.25f, 1f);
            yield return new WaitForSecondsRealtime(.5f);
            pad.SetMotorSpeeds(0,0);

        }

        yield return null;
    }
    
    

    private void OnControlsChanged()
    {
        PlayerOnControlsChanged?.Invoke();
    }

    private void OnPause(InputValue inputValue)
    {
        OnPauseGame?.Invoke();
    }

    public void ToggleInput(bool activate)
    {
        
        if(activate)
            playerInput.ActivateInput();
        else
            playerInput.DeactivateInput();
    }

    //toggle any input action by name and bool
    
    public void ToggleInputMap(bool toUI)
    {
        if (toUI)
            playerInput.SwitchCurrentActionMap("UI");
        else
            playerInput.SwitchCurrentActionMap("Player");
    }
    #endregion
    
    //Completely stop all movement
    public void StopMovement()
    {
        _moveInputValue = Vector2.zero;
        _rigidBody.linearVelocity = Vector2.zero;
    }

    public void ToggleEyeFollowTarget(bool willFollow = false, Transform newTarget = null)
    {
        _eyeFollower.ToggleFollowTarget(willFollow,newTarget);
    }

    public void ToggleMonsterEyeDetection(bool canDetectMonsters)
    {
        _eyeFollower.ToggleMonsterDetect(canDetectMonsters);
    }

   
    
    private void Update()
    {
        
    }

    private void LateUpdate()
    {
        if (_isPlayerPromptActive)
        {
            playerUIPrompt.transform.position = transform.position + new Vector3(2,2,0);
            playerUIPrompt.rotation = Quaternion.identity;
        }
    }

    private void FixedUpdate()
    {
        //dot he final movement on fixedupdate since we are using rigidbody
    }

    //Main function that moves the playerCharacter very very slowly
    public void Move()
    {
        Vector2 dir = _moveInputValue.normalized;
        float magnitude = _moveInputValue.magnitude;
        //myAnim.SetFloat("Speed", magnitude);
        _finalMovement = ( slowSpeed * magnitude * Time.deltaTime * transform.up);

        

        if(magnitude > 0 )
        {
            if (!_normalSwimSfxEmitter.IsPlaying())
            {
                _normalSwimSfxEmitter.Play();
            }

        }
        else
        {
            if(_normalSwimSfxEmitter.IsPlaying())
            {
                _normalSwimSfxEmitter.Stop();
            }
        }
    }
    
    //Main function that moves the playerCharacter like a squid. Propulsion by adding force to rigidbody
    void Swim()
    {
        //swimTimer += Time.deltaTime;
        Vector2 dir = _moveInputValue.normalized;
        if (Time.time >= _nextSwim)
        {
            swimStage = true;
            //swimm
            float magnitude = _moveInputValue.magnitude;
            //myAnim.SetFloat("Speed", magnitude);
            _finalMovement = (transform.up * swimForce * magnitude);

            _rigidBody.AddForce(_finalMovement, ForceMode2D.Impulse);
            _rigidBody.linearVelocity = Vector3.ClampMagnitude(_rigidBody.linearVelocity, maxSpeed);
            _nextSwim = Time.time + timeBetweenSwim;

            //Head scale animation
            if (magnitude > 0)
            {
                vfxSwimBubbles.Play();
                //FMODUnity.RuntimeManager.PlayOneShot(sfxSwim, transform.position);
                var instance = FMODUnity.RuntimeManager.CreateInstance(sfxSwim);
                instance.setVolume(0.75f);
                instance.start();
                instance.release();
                Sequence seq = DOTween.Sequence();
                seq.SetEase(Ease.OutCubic);
                seq.Append(headPart.DOScaleY(1.5f, 0.5f));
                seq.Append(headPart.DOScaleY(1f, 0.5f  * 1.5f));
                
                OnPlayerSwim?.Invoke();
            }


            //metric handler
            MetricManagerScript.instance?.LogString("Player Action", "Swim");
        }
        

    }



    //Handles the ability for the playerCharacter to slowly turn, making it more realistic
    private void SlowTurn()
    {
        if (_moveInputValue.magnitude > 0f)
        {

            Vector3 rotatedTemp = Quaternion.Euler(0, 0, 0) * new Vector3(_moveInputValue.x, _moveInputValue.y, 0f);

            Quaternion tempRotation = Quaternion.LookRotation(Vector3.forward, rotatedTemp);
            this.transform.rotation = Quaternion.RotateTowards(transform.rotation, tempRotation, rotationSpeed * Time.deltaTime);
        }
    }

   

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Enemy") && !GameManager.Instance.isPlayerDead && !inmortal)
        {
            GameOver(collision.gameObject);
        }
    }

    //Events when monster eats our playerCharacter
    private void GameOver(GameObject monster)
    {
        _blueRef?.ToggleFollow(false);
        StopMovement();
        playerInput.DeactivateInput();
        StartCoroutine(RumbleController());
        Instantiate(vfxDeath, transform.position, quaternion.identity);
        FMODUnity.RuntimeManager.PlayOneShot(sfxDeath, transform.position);
        GameManager.Instance.GameOver(monster);
        bodyPart.SetActive(false);
        _collider.enabled = false;
    }

    //When playerCharacter is ready to restart
    public void OnRestartingGame()
    {
        Debug.Log("Restarting Action");
        isHidden = true;
        bodyPart.SetActive(true);
        _collider.enabled = true;
        transform.rotation = Quaternion.identity;
        playerInput.ActivateInput();

    }


    public void ToggleHidePlayer(bool hide)
    {
        //global behaviordesigner variables
        GlobalVariables.Instance.SetVariableValue("isPlayerHidden", hide);
        isHidden = hide;
        if(hide)
            OnPlayerHide?.Invoke();
    }

   

    public void PlayCallSfx(bool triggerEvent)
    {
        //vfx
        GameManager.Instance.playerLastPosition.transform.position =  transform.position;
        if(_sfxCallLastTime.Equals(sfxCall1))
        {
            _sfxCallLastTime = sfxCall2;
        }
        else
        {
            _sfxCallLastTime = sfxCall1;
        }
        vfxVoice.Play();
        //audio
        AudioManager.Instance.PlayOneShotEvent(_sfxCallLastTime, transform.position);
        //visual feedback
        transform.DOPunchScale(new Vector3(.1f, .4f, 0), .75f, 1, 0f).SetDelay(0.25f);
        if(triggerEvent)
            OnPlayerCall?.Invoke();

    }

    //set our blue reference
    public void SetBlueReference(BlueNPC blueRef)
    {
        _blueRef = blueRef;
    }

    private void OnDrawGizmosSelected()
    {
        //show call radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position,callRadiusForEnemies);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position,callRadiusDoor);
        
    }

    public void ManagedUpdate()
    {
        Move();
        SlowTurn();
        if (Time.time >= _nextSwim + 1f)
            swimStage = false;
        _rigidBody.AddForce(_finalMovement);
    }

    public void ToggleUIPromptPositioning(bool toggle)
    {
        _isPlayerPromptActive = toggle;
        playerUIPrompt.gameObject.SetActive(true);
    }
}


