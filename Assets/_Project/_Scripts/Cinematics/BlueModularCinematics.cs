using System;
using System.Collections;
using DG.Tweening;
using FMODUnity;
using UnityEngine;

public class BlueModularCinematics : MonoBehaviour
{
    [Header("Scare fishes")]
    [SerializeField] GameObject flora;
    private Material _floraMatInstance;
    [SerializeField] ParticleSystem vfxBubbles;
    [SerializeField] BoidFlockJob flock;
    [SerializeField] Transform followObj;
    [SerializeField] GameObject bluePathAfter;
    [SerializeField] float bluePathAfterTime;
    
    [Header("For player")]
    [SerializeField] bool forPlayer;
    private bool _triggeredByPlayer, _activeForPlayer;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Create a unique instance of the material
        if (flora)
        {
            _floraMatInstance = new Material(flora.GetComponent<SpriteRenderer>().material);
            flora.GetComponent<Renderer>().material = _floraMatInstance;
            _floraMatInstance.SetFloat("_GlowGlobal", 2);
        }

        

    }

    

    public void ToggleUIPlayerNear(bool toggle)
    {
        if (_activeForPlayer)
        {
            UIManager.Instance?.TogglePlayerUIPrompt(toggle);
            Debug.Log("Toggle for player");
            if (toggle)
                PlayerCharacter.OnPlayerCall += OnPlayerCall;
            else
                PlayerCharacter.OnPlayerCall -= OnPlayerCall;
        }
    }

    private void OnPlayerCall()
    {
        flock.ToggleContainment(false);
        flock.ToggleAvoidPlayer(true);
        flock.ToggleAvoidBlue(true);
        AudioManager.Instance?.PlaySwimSmallSfx(transform.position);
        vfxBubbles.Play();
        PlayerCharacter.OnPlayerCall -= OnPlayerCall;
        ToggleUIPlayerNear(false);
        _activeForPlayer = false;
        _triggeredByPlayer = true;
        
        
        
        Transform[] bluePathTransforms = bluePathAfter.GetComponentsInChildren<Transform>();
        Vector3[] bluePathPos2 = new Vector3[bluePathTransforms.Length-1];
        for (int i = 1; i < bluePathTransforms.Length; i++)
        {
            bluePathPos2[i-1] = bluePathTransforms[i].position;
        }
        GameManager.Instance.blueNpcRef.StopMovement();
        GameManager.Instance?.blueNpcRef.ToggleFollow(false);
        GameManager.Instance.blueNpcRef.transform
            .DOPath(bluePathPos2, bluePathAfterTime, PathType.CatmullRom, PathMode.Sidescroller2D)
            .SetEase(Ease.InOutSine)
            .SetLookAt(0.001f, transform.forward, Vector3.right)
            .OnComplete(() =>
            {
                GameManager.Instance?.blueNpcRef.ToggleReactToCall(true);
                GameManager.Instance.blueNpcRef.ChangeFollowTarget(GameManager.Instance.playerRef.transform, -1,true);
            });
    }

    public void TriggerFisbowlBlueFishesPt1()
    {
        
        GameManager.Instance?.blueNpcRef.ToggleReactToCall(false);
        StartCoroutine(GameManager.Instance.blueNpcRef.PlayCallSfx());
        GameManager.Instance.blueNpcRef.ChangeFollowTarget(followObj, 3, false);
        GameManager.Instance.blueNpcRef.ToggleFireReachedDestinationEvent(true);
        BlueMovement.OnBlueReachedDestination += TriggerFisbowlBlueFishesPt2;
        
    }
    
    public void TriggerFisbowlBlueFishesForPlayerPt1()
    {
        
        GameManager.Instance?.blueNpcRef.ToggleReactToCall(false);
        StartCoroutine(GameManager.Instance.blueNpcRef.PlayCallSfx());
        GameManager.Instance.blueNpcRef.ChangeFollowTarget(followObj, 3, false);
        GameManager.Instance.blueNpcRef.ToggleFireReachedDestinationEvent(true);
        BlueMovement.OnBlueReachedDestination += CallForPlayer;
        
    }

    public void CallForPlayer()
    {
        _activeForPlayer = true;
        StartCoroutine(RepeatCallForPlayer());
    }

  

    IEnumerator RepeatCallForPlayer()
    {
        while (!_triggeredByPlayer)
        {
            StartCoroutine(GameManager.Instance.blueNpcRef.PlayCallSfx());
            yield return new WaitForSeconds(4);
        }
    }
    
    //this is fired when Blue AI reaches the destination on our fishbowl fishes event 1
    public void TriggerFisbowlBlueFishesPt2()
    {
        Transform[] bluePathTransforms = bluePathAfter.GetComponentsInChildren<Transform>();
        Vector3[] bluePathPos2 = new Vector3[bluePathTransforms.Length-1];
        for (int i = 1; i < bluePathTransforms.Length; i++)
        {
            bluePathPos2[i-1] = bluePathTransforms[i].position;
        }
        BlueMovement.OnBlueReachedDestination -= TriggerFisbowlBlueFishesPt2;
        GameManager.Instance.blueNpcRef.StopMovement();
        
        Sequence seq = DOTween.Sequence()
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                StartCoroutine(GameManager.Instance.blueNpcRef.PlayCallSfx());
                flock.ToggleContainment(false);
                flock.ToggleAvoidPlayer(true);
                flock.ToggleAvoidBlue(true);
                AudioManager.Instance?.PlaySwimSmallSfx(transform.position);
                vfxBubbles.Play();
                GameManager.Instance?.blueNpcRef.ToggleFollow(false);
            })
            .Append(GameManager.Instance.blueNpcRef.transform
                .DOPath(bluePathPos2, bluePathAfterTime, PathType.CatmullRom, PathMode.Sidescroller2D)
                .SetEase(Ease.InOutSine)
                .SetLookAt(0.001f, transform.forward, Vector3.right))
            .Join(DOTween.To(() => _floraMatInstance.GetFloat("_GlowGlobal"), x =>
                {
                    _floraMatInstance.SetFloat("_GlowGlobal", x);
                },
                15, 2))
            .AppendInterval(0.25f)
            .AppendCallback(() =>
            {
                GameManager.Instance?.blueNpcRef.ToggleReactToCall(true);
                GameManager.Instance.blueNpcRef.ChangeFollowTarget(GameManager.Instance.playerRef.transform, -1,true);
            });
    }

}
