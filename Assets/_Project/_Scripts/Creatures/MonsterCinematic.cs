using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Pathfinding;

public class MonsterCinematic : MonsterBase
{
    //tutorialmonster
    private EyeTracker _eyeTracker;

    private AIDestinationSetter _aiDestinationSetter;
    
    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        _eyeTracker = GetComponentInChildren<EyeTracker>();
        _aiPath.canMove = false;
        _aiDestinationSetter = GetComponent<AIDestinationSetter>();
    }

    protected override void Start()
    {
        base.Start();
    }


    public void ToggleTrackTarget(GameObject obj)
    {
        _eyeTracker.ToggleTrackTarget(obj);
    }

    /**
     * Toggle manual pursuit
     */
    public void TogglePursuit(bool pursue)
    {
        if (pursue)
        {
            UpdateMonsterState(MonsterState.Chasing);
            _aiPath.maxSpeed = monsterStats.ChasingSpeed;
            _aiPath.canMove = true;
        }
        else
        {
            UpdateMonsterState(MonsterState.Default);
            _aiPath.canMove = false;
        }
    }

    public void GoToPosition(Vector3 d)
    {
        _aiPath.canMove = true;
        _aiDestinationSetter.enabled = false;
        _aiPath.destination = d;
    }
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            StartCoroutine(EatPlayerAnimation());
        }
    }
    IEnumerator EatPlayerAnimation()
    {
        _aiPath.canMove = false;
        _chaseScaleTween.Rewind();
        _rigidbody2D.AddForce(transform.up * 20f, ForceMode2D.Impulse);
        Sequence seq = DOTween.Sequence();
        seq.SetEase(Ease.OutCubic);
        seq.Append(_headObj.DOScaleY(1.7f, 0.5f));
        seq.Append(_headObj.DOScaleY(1f, 0.5f  * 1.5f));
        //seq.Append(_headObj.DOPunchScale(new Vector3(1f, .25f, 0), 0.5f, 5, 1));
        seq.Append(_headObj.DOPunchRotation(new Vector3(0,0,80), 1f, 5, 1));
        yield return new WaitForSecondsRealtime(1.5f);
    }
    
    
}
