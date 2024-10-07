using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;

public class EyeFollower : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private bool _canFollow;
    private bool _canDetectMonsters;

    [SerializeField] private Transform eyeballLeft;
    [SerializeField] private Transform pupilLeft;
    [SerializeField] private Transform pupilLeftMidpoint;
    [SerializeField] private Transform eyeballRight;
    [SerializeField] private Transform pupilRight;
    [SerializeField] private Transform pupilRightMidpoint;
    
    private Vector3 _smoothVelocity = Vector3.zero;
    private Vector3 _initPupilLPos, _initPupilRPos;
    
    //blink
    [SerializeField] private float timer = 4f;
    private int blinkProcess = 0;
    private bool isBlinking = false;

    // Start is called before the first frame update
    void Start()
    {
        _initPupilLPos = pupilLeft.localPosition;
        _initPupilRPos = pupilRight.localPosition;
        _canDetectMonsters = false;
    }

    // Update is called once per frame
    void Update()
    {
        BlinkTimer();
        if(isBlinking)
            EyeBlink();
    }
    
    void BlinkTimer()
    {
        timer-=Time.deltaTime;
        //when the timer expires, go to Blinking (Blinking then goes to Tracking)
        if(timer > 0){
            return;
        }

        isBlinking = true;
        timer = Random.Range(4f, 8f);
    }
    
    void EyeBlink()
    {
        if(blinkProcess == 0)
        {
            //If you really think about it, blinking is just getting smaller in one direction
            eyeballLeft.localScale = new Vector3(eyeballLeft.localScale.x, eyeballLeft.localScale.y + (Vector3.down * 0.135f).y, eyeballLeft.localScale.z);
            eyeballRight.localScale = new Vector3(eyeballRight.localScale.x, eyeballRight.localScale.y + (Vector3.down * 0.135f).y, eyeballRight.localScale.z);

            if(eyeballLeft.localScale.y <= 0)
            {
                blinkProcess++;
                
            }
        }
        else if(blinkProcess == 1)
        {
            //If you really think about it, blinking is just getting bigger in one direction
            eyeballLeft.localScale = new Vector3(eyeballLeft.localScale.x, eyeballLeft.localScale.y + (Vector3.up * 0.135f).y, eyeballLeft.localScale.z);
            eyeballRight.localScale = new Vector3(eyeballRight.localScale.x, eyeballRight.localScale.y + (Vector3.up * 0.135f).y, eyeballRight.localScale.z);
            
            if(eyeballLeft.localScale.y == 1)
            {
                blinkProcess++;
            }
        }
        else
        {
            //if not tracking playerCharacter, then move ir forward so it looks like is looking at the direction of the movemnet
            blinkProcess = 0;
            isBlinking = false;
        }
    }

    public void ToggleMonsterDetect(bool canDetectMonsters)
    {
        _canDetectMonsters = canDetectMonsters;
    }

    private void FixedUpdate()
    {
        
        if (_canFollow)
        {
            if (_canDetectMonsters)
            {
                //give me all nearest monsters
                Collider2D[] closeMonsters = Physics2D.OverlapCircleAll(transform.position,50,LayerMask.GetMask("Monster")); 
                // If there is at least one collider in the array...
                if (closeMonsters.Length >= 1)
                {
                    float shortestDistanceSoFar = Vector2.Distance(gameObject.transform.position, closeMonsters[0].gameObject.transform.position);
                    Transform closestMonster = closeMonsters[0].transform;

                    for (int i = 1; i < closeMonsters.Length; i++)
                    {
                        float currentDistance = Vector2.Distance(this.gameObject.transform.position, closeMonsters[i].gameObject.transform.position);
                        if (currentDistance < shortestDistanceSoFar)
                        {
                            closestMonster = closeMonsters[i].transform;
                            shortestDistanceSoFar = currentDistance;
                        }
                    }
     
                    //we have the closest monster
                    if (closestMonster.transform != _target)
                    {
                        ToggleFollowTarget(true, closestMonster);
                    }
                }
                else
                {
                    if (_target)
                        ToggleFollowTarget(true, null);
                }
                
            }

            if (_target)
            {
                Vector3 targetPos = _target.position;
                
                //get direction it should be pointing at
                //Normalized % 10 bc our max offset is around 0.1f 
                Vector3 dirPupilL = (targetPos - pupilLeftMidpoint.position).normalized / 8;
                pupilLeft.position = Vector3.SmoothDamp(pupilLeft.position, pupilLeftMidpoint.position + dirPupilL, ref _smoothVelocity, 0.2f);
                
                //now the right pupil
                Vector3 dirPupilR = (targetPos - pupilRightMidpoint.position).normalized / 8;
                pupilRight.position = Vector3.SmoothDamp(pupilRight.position, pupilRightMidpoint.position + dirPupilR, ref _smoothVelocity, 0.2f);

                //now move the pupil in that direction with a max value
                
            }
           
        }
    }

    public void ToggleFollowTarget(bool willFollow, Transform newTarget)
    {
        //if we are manually setting up a target, it means to overwrite the monster detection one
        _canFollow = willFollow;
        if (newTarget != _target)
        {
            _target = newTarget;
            if (!_target || !_canFollow)
            {
                //removing target. Reset pupil positions. Look forwar always
                DOTween.To(() => pupilLeft.localPosition, x => pupilLeft.localPosition = x,
                    _initPupilLPos, 1f);
                DOTween.To(() => pupilRight.localPosition, x => pupilRight.localPosition = x,
                    _initPupilRPos, 1f);
            }
            
        }
        
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position,50);
    }
}
