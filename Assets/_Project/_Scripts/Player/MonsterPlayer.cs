using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using DG.Tweening;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks.Unity.Timeline;
using FMODUnity;
using Unity.Mathematics;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class MonsterPlayer : MonoBehaviour
{

    public static event Action PlayerOnControlsChanged;
    public static event Action PlayerOnPause;
    //managers
    private GameManager gameManager;

    [Header("Movement settings")] 
    [SerializeField] private GameObject _body;
    [SerializeField] float maxSpeed;
    [SerializeField] float slowSpeed;
    [SerializeField] float rotationSpeed;
    [SerializeField] float swimForce;
    [SerializeField] float timeBetweenSwim;
    [SerializeField] ParticleSystem VFXSwimBubbles;
    [SerializeField] private ParticleSystem VFXDeath;
    [SerializeField] EventReference SFXDeath;
    [SerializeField] EventReference SFXSwim;
    private StudioEventEmitter NormalSwimSFXEmitter;

    //Input and movement handler
    [FormerlySerializedAs("_playerInput")] public PlayerInput playerInput;
    private Vector2 _moveInputValue;
    private Vector3 _finalMovement;
    private Rigidbody2D _rigidBody;
    private Collider2D _collider;
    private float nextSwim;
    private float nextCall;
    
    public bool isHidden = false;
    private BlueNPC _blueRef;


    [Header("Animation")]
    [Tooltip("How much the head scales doing the swim")]
    [Range(1, 2)]
    [SerializeField] float headScaleMax;
    [SerializeField] float headShakeDuration = 1;
    [SerializeField] Transform headPart;

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
        GameManager.OnRestartingGame += OnRestartingGame;

        _collider = GetComponent<Collider2D>();
        _rigidBody = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        NormalSwimSFXEmitter = GetComponent<StudioEventEmitter>();

    }

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.FindFirstObjectByType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
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
            if (Time.time >= nextCall)
            {
                nextCall = Time.time + 2f;
                StartCoroutine(PlayCallSFX());
            }
        }
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
    public void ToggleInputAction(string actionName, bool activate)
    {
        if(!activate)
            playerInput.actions.FindAction(actionName).Disable();
        else
            playerInput.actions.FindAction(actionName).Enable();
    }
    public void ToggleInputMap(bool changeToPause)
    {
        if (changeToPause)
            playerInput.SwitchCurrentActionMap("UI");
        else
            playerInput.SwitchCurrentActionMap("Player");
    }
    #endregion
    
    //Completely stop all movement
    public void StopMovement()
    {
        _moveInputValue = Vector2.zero;
    }

   
    //Main function that moves the player like a squid. Propulsion by adding force to rigidbody
    void Swim()
    {
        //swimTimer += Time.deltaTime;
        Vector2 dir = _moveInputValue.normalized;
        if (Time.time >= nextSwim)
        {
            //swimm
            float magnitude = _moveInputValue.magnitude;
            //myAnim.SetFloat("Speed", magnitude);
            _finalMovement = (transform.up * swimForce * magnitude);

            _rigidBody.AddForce(_finalMovement, ForceMode2D.Impulse);
            _rigidBody.velocity = Vector3.ClampMagnitude(_rigidBody.velocity, maxSpeed);
            nextSwim = Time.time + timeBetweenSwim;

            //Head scale animation
            if (magnitude > 0)
            {
                VFXSwimBubbles.Play();
                FMODUnity.RuntimeManager.PlayOneShot(SFXSwim, transform.position);
                Sequence seq = DOTween.Sequence();
                seq.SetEase(Ease.OutCubic);
                seq.Append(headPart.DOScaleY(headScaleMax, headShakeDuration));
                seq.Append(headPart.DOScaleY(1f, headShakeDuration  * 1.5f));
            }

            //metric handler
            MetricManagerScript.instance?.LogString("Player Action", "Swim");
        }
        

    }
    
    private void FixedUpdate()
    {
        Move();
        SlowTurn();
    }
    
     //Main function that moves the player very very slowly
    public void Move()
    {
        Vector2 dir = _moveInputValue.normalized;
        float magnitude = _moveInputValue.magnitude;
        //myAnim.SetFloat("Speed", magnitude);
        _finalMovement = ( slowSpeed * magnitude * Time.fixedDeltaTime * transform.up);

        _rigidBody.AddForce(_finalMovement);

        if(magnitude > 0 )
        {
            if (!NormalSwimSFXEmitter.IsPlaying())
            {
                NormalSwimSFXEmitter.Play();
            }

        }
        else
        {
            if(NormalSwimSFXEmitter.IsPlaying())
            {
                NormalSwimSFXEmitter.Stop();
            }
        }
    }


    //Handles the ability for the player to slowly turn, making it more realistic
    private void SlowTurn()
    {
        if (_moveInputValue.magnitude > 0f)
        {

            Vector3 rotatedTemp = Quaternion.Euler(0, 0, 0) * new Vector3(_moveInputValue.x, _moveInputValue.y, 0f);

            Quaternion tempRotation = Quaternion.LookRotation(Vector3.forward, rotatedTemp);
            this.transform.rotation = Quaternion.RotateTowards(transform.rotation, tempRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

   

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Enemy") && !GameManager.IsPlayerDead && !inmortal)
        {
            GameOver();
        }
    }

    //Before game over
    private void GameOver()
    {
        StopMovement();
        Instantiate(VFXDeath, transform.position, quaternion.identity);
        playerInput.DeactivateInput();
        _body.SetActive(false);
        _collider.enabled = false;
        
        FMODUnity.RuntimeManager.PlayOneShot(SFXDeath, transform.position);
        gameManager.GameOver();
    }

    //When player is ready to restart
    public void OnRestartingGame()
    {
        _body.SetActive(true);
        _collider.enabled = true;
        transform.rotation = Quaternion.identity;

    }

   

    public IEnumerator PlayCallSFX()
    {
        vfxVoice.Play();
        yield return new WaitForSeconds(0.2f);
        if(_sfxCallLastTime.Equals(sfxCall1))
        {
            _sfxCallLastTime = sfxCall2;
        }
        else
        {
            _sfxCallLastTime = sfxCall1;
        }
        FMODUnity.RuntimeManager.PlayOneShot(_sfxCallLastTime, transform.position);
        
        if (_blueRef)
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
                if (monster.CurrentState == MonsterState.Default)
                {
                    Debug.Log("Sending react to call event");
                    monster.ReactToPlayerCall();
                }
                else
                {
                    Debug.Log("Nothing should fucking happen");
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


