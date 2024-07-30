using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using FMODUnity;
using Unity.Mathematics;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class MonsterPlayer : MonoBehaviour
{

    public static event Action PlayerOnControlsChanged;
    public static event Action PlayerOnPause;
    //managers
    private GameManager gameManager;

    
    private Tentacle[] _proceduralTentacles;
    private TentacleDynamic[] _proceduralDynamicTentacles;
    private BodyTentacle _proceduralBody;
    private EyeFollower _eyeFollower;

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
    private BlueNPC _blueRef;


    [Header("Animation")]
    [Tooltip("How much the head scales doing the swim")]

    [Header("Call action")]
    [SerializeField] private ParticleSystem vfxVoice;
    [SerializeField] private EventReference sfxCall1;
    [SerializeField] private EventReference sfxCall2;
    [SerializeField] private EventReference sfxCall3;
    private EventReference _sfxCallLastTime;
    [SerializeField] private float callRadiusForEnemies;

    [Header("Cheatcodes")] public bool inmortal = false;

    private void Awake()
    {

        _collider = GetComponent<Collider2D>();
        _rigidBody = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        _normalSwimSfxEmitter = GetComponent<StudioEventEmitter>();

        _proceduralDynamicTentacles = GetComponentsInChildren<TentacleDynamic>();
        _proceduralTentacles = GetComponentsInChildren<Tentacle>();
        _proceduralBody = GetComponentInChildren<BodyTentacle>();
        _eyeFollower = GetComponentInChildren<EyeFollower>();

    }

    private void OnEnable()
    {
        GameManager.OnRestartingGame += OnRestartingGame;
    }

    private void OnDisable()
    {
        GameManager.OnRestartingGame -= OnRestartingGame;
    }


    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.FindFirstObjectByType<GameManager>();
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
            Swim();
    }

    private void OnCall(InputValue inputValue)
    {
        //playerInput.actions.FindAction("Call").Disable();
        if (inputValue.isPressed)
        {
            if (Time.time >= _nextCall)
            {
                _nextCall = Time.time + 2f;
                StartCoroutine(PlayCallSFX(true));
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
        PlayerOnPause?.Invoke();
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
        _rigidBody.velocity = Vector2.zero;
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

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            //reset checkpoint
            GameOver(null);
        }
        Move();
        SlowTurn();
        if (Time.time >= _nextSwim + 1f)
            swimStage = false;
        _rigidBody.AddForce(_finalMovement);
    }

    private void FixedUpdate()
    {
        //dot he final movement on fixedupdate since we are using rigidbody
    }

    //Main function that moves the player very very slowly
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
    
    //Main function that moves the player like a squid. Propulsion by adding force to rigidbody
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
            _rigidBody.velocity = Vector3.ClampMagnitude(_rigidBody.velocity, maxSpeed);
            _nextSwim = Time.time + timeBetweenSwim;

            //Head scale animation
            if (magnitude > 0)
            {
                vfxSwimBubbles.Play();
                FMODUnity.RuntimeManager.PlayOneShot(sfxSwim, transform.position);
                Sequence seq = DOTween.Sequence();
                seq.SetEase(Ease.OutCubic);
                seq.Append(headPart.DOScaleY(1.5f, 0.5f));
                seq.Append(headPart.DOScaleY(1f, 0.5f  * 1.5f));
            }


            //metric handler
            MetricManagerScript.instance?.LogString("Player Action", "Swim");
        }
        

    }



    //Handles the ability for the player to slowly turn, making it more realistic
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
        if(collision.gameObject.CompareTag("Enemy") && !GameManager.IsPlayerDead && !inmortal)
        {
            GameOver(collision.gameObject);
        }
    }

    //Events when mosnter eats our player
    private void GameOver(GameObject monster)
    {
        StopMovement();
        playerInput.DeactivateInput();
        StartCoroutine(RumbleController());
        Instantiate(vfxDeath, transform.position, quaternion.identity);
        FMODUnity.RuntimeManager.PlayOneShot(sfxDeath, transform.position);
        gameManager.GameOver(monster);
        bodyPart.SetActive(false);
        _collider.enabled = false;
    }

    //When player is ready to restart
    public void OnRestartingGame()
    {
        bodyPart.SetActive(true);
        _collider.enabled = true;
        transform.rotation = Quaternion.identity;
        playerInput.ActivateInput();
        
        //reset tentacles?

    }

   

    public IEnumerator PlayCallSFX(bool respond)
    {
        vfxVoice.Play();
        yield return new WaitForSeconds(0.25f);
        if(_sfxCallLastTime.Equals(sfxCall1))
        {
            _sfxCallLastTime = sfxCall2;
        }
        else
        {
            _sfxCallLastTime = sfxCall1;
        }
        FMODUnity.RuntimeManager.PlayOneShot(_sfxCallLastTime, transform.position);
        
        if (_blueRef && respond)
        {
            //tell blue to do a call as well
            yield return new WaitForSeconds(Random.Range(1f, 1.75f));
            StartCoroutine(_blueRef.PlayCallSFX());
        }

        gameManager.playerLastPosition.transform.position =  transform.position;
        if (!isHidden)
        {
            Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, callRadiusForEnemies, LayerMask.GetMask("Monster"));
            
            for (int i = 0; i < enemies.Length; i++)
            {
                EnemyMonster monster = enemies[i].GetComponent<EnemyMonster>();
                if (monster.CurrentState == MonsterState.Default || monster.CurrentState == MonsterState.Investigate)
                {
                    monster.ReactToPlayerCall();
                }
                //tell to check a position
            }
            
        }
        
        
        

    }

    //set our blue reference
    public void SetBlueReference(BlueNPC blueRef)
    {
        _blueRef = blueRef;
    }

    private void OnDrawGizmosSelected()
    {
        //show call radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position,callRadiusForEnemies);
    }
}


