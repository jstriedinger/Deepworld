using System;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks.Movement;
using Pathfinding;
using UnityEngine;
using UnityEngine.Serialization;

public class MonsterLaunchMechanic : MonoBehaviour
{
    [SerializeField] private ParticleSystem vfxBubblesLaunch;
    [SerializeField] private float launchForce;
    [SerializeField] private float timeBetweenLaunch;
    [SerializeField] private float launchRadius;
    BehaviorTree _behaviorTree;
    AIPath _aiPath;
    Rigidbody2D _rigidbody;
    private float _nextLaunch;
    private Coroutine _resetAIMove;
    private GameManager _gameManager;
    private MonsterBase _monsterBase;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _aiPath = GetComponent<AIPath>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _gameManager = GameObject.FindFirstObjectByType<GameManager>();
        _behaviorTree = GetComponent<BehaviorTree>();
        _monsterBase =  GetComponent<MonsterBase>();
        
    }

    private void Update()
    {
        //check if it can see me
        //MovementUtility.WithinSight2D(transform, Vector3.zero, 360, _monsterBase.GetMonsterStats().ChasingRange, _gameManager.playerRef.gameObject, Vector3.zero, 0, LayerMask.GetMask("Monster"), false, null, true);
        if (Time.time >= _nextLaunch && (bool)_behaviorTree.GetVariable("isChasing").GetValue())
        {
            Collider2D col = Physics2D.OverlapCircle(transform.position, launchRadius, LayerMask.GetMask("Player"));
            if (col != null)
            {
                Debug.Log("LAUNCHED!");
                if(_resetAIMove != null)
                    StopCoroutine(_resetAIMove);
                _behaviorTree.DisableBehavior();
                Vector3 dir = col.transform.position - transform.position;
                _rigidbody.AddForce(dir * launchForce, ForceMode2D.Impulse);
                _nextLaunch = Time.time + timeBetweenLaunch;
                vfxBubblesLaunch.Play();
                //rotate towards the player
                Vector3 rotatedTemp = Quaternion.Euler(0, 0, 0) * dir;
                Quaternion tempRotation = Quaternion.LookRotation(Vector3.forward, rotatedTemp);
                float angles = Quaternion.Angle(transform.rotation, tempRotation);

                _monsterBase.AttackPlayerAnim();
                transform.rotation = tempRotation;
                _resetAIMove = StartCoroutine(ResetAIMove());

            }
            
        }
       
    }

    IEnumerator ResetAIMove()
    {
        yield return new WaitForSeconds(timeBetweenLaunch+0.1f);
        if(!_monsterBase.isKillingPlayer)
            _behaviorTree.EnableBehavior();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, launchRadius);
    }
}
