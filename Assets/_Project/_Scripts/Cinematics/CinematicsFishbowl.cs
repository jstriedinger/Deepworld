using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class CinematicsFishbowl : MonoBehaviour
{
    [Header("Blue meetup")]
    [SerializeField] private GameObject pathBlueMeetup1;
    [SerializeField] private GameObject pathBlueMeetup2;
    [SerializeField] private GameObject pathPlayerMeetup1;
    [SerializeField] private GameObject pathPlayerMeetup2;
    
    [Header("Event 1 - fishes")]
    [SerializeField] BoidFlock flockFishes;
    [SerializeField] private Transform newFollowTmp;
    [SerializeField] GameObject bluePathBefore;
    [SerializeField] GameObject bluePathAfter;
    
    [Header("Event 2 - fishes for green")]
    [SerializeField] BoidFlock flockFishes2;
    [SerializeField] GameObject bluePathBefore3;
    [SerializeField] GameObject bluePathAfter3;
    
    
    [Header("Event 2 - BIg blue")]
    [SerializeField] SwimmerFish bigBlueFish;
    [SerializeField] GameObject bluePathBefore2;
    [SerializeField] GameObject bluePathAfter2;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
  
    //Trigger meetup blue on fishbowl pt1
    public void TriggerBlueMeetupPt1()
    {
        PlayerInput pInput = GameManager.Instance.playerRef.playerInput;
        //disable move input action
        pInput.actions.FindAction("Move").Disable();
        GameManager.Instance.playerRef.StopMovement();
        UIManager.Instance.PrepareBlueMeetupCinematic();
        
        
        pInput.actions.FindAction("Call").Enable();
        pInput.actions.FindAction("Call").performed += FirstTimeCallInput;
        
        GameManager.Instance.LoadLevelSection(2);

    }
    
    public void CinematicBlueMeetupPt2()
    {
        
        Transform[] bluePathTransforms = pathBlueMeetup1.GetComponentsInChildren<Transform>();
        Vector3[] bluePathPos1 = new Vector3[bluePathTransforms.Length-1];
        for (int i = 1; i < bluePathTransforms.Length; i++)
        {
            bluePathPos1[i-1] = bluePathTransforms[i].position;
        }
        
        Transform[] bluePathTransforms2 = pathBlueMeetup2.GetComponentsInChildren<Transform>();
        Vector3[] bluePathPos2 = new Vector3[bluePathTransforms2.Length-1];
        for (int i = 1; i < bluePathTransforms2.Length; i++)
        {
            bluePathPos2[i-1] = bluePathTransforms2[i].position;
        }
        
        Transform[] playerPathTransforms = pathPlayerMeetup1.GetComponentsInChildren<Transform>();
        Vector3[] playerPathPos1 = new Vector3[playerPathTransforms.Length-1];
        for (int i = 1; i < playerPathTransforms.Length; i++)
        {
            playerPathPos1[i-1] = playerPathTransforms[i].position;
        }
        
        Transform[] playerPathTransforms2 = pathPlayerMeetup2.GetComponentsInChildren<Transform>();
        Vector3[] playerPathPos2 = new Vector3[playerPathTransforms2.Length-1];
        for (int i = 1; i < playerPathTransforms2.Length; i++)
        {
            playerPathPos2[i-1] = playerPathTransforms2[i].position;
        }
        
        //ok hide the call icons
        UIManager.Instance.ToggleCallIcons(false);

        BlueNPC blueNpc = GameManager.Instance.blueNpcRef;
        Sequence cinematic = DOTween.Sequence();
        cinematic.Append(GameManager.Instance.playerRef.transform.DOPath(playerPathPos1, 3, PathType.CatmullRom, PathMode.Sidescroller2D)
                .SetEase(Ease.InOutSine)
                .SetLookAt(0.001f, transform.forward, Vector3.right))
            .Join(blueNpc.transform.DOPath(bluePathPos1, 4f, PathType.CatmullRom, PathMode.Sidescroller2D)
                .SetEase(Ease.InOutSine)
                .SetLookAt(0.001f, transform.forward, Vector3.right).OnWaypointChange(
                    (int waypointIndex) =>
                    {
                        if (waypointIndex == 1)
                        {
                            GameManager.Instance.playerRef.ToggleEyeFollowTarget(true,blueNpc.transform);
                            blueNpc.ToggleEyeFollowTarget(true,GameManager.Instance.playerRef.transform);
                            
                        }
                    }).SetDelay(1))
            
        .AppendCallback(() =>
            {
                AudioManager.Instance.ChangeBackgroundMusic(2);
                StartCoroutine(blueNpc.PlayCallSfx());
                //change to friend music
            })
            .AppendInterval(1)
            .Append(blueNpc.transform.DOPath(bluePathPos2, 5, PathType.CatmullRom, PathMode.Sidescroller2D)
                .SetEase(Ease.InOutSine)
                .SetLookAt(0.001f, transform.forward, Vector3.right))
            .Join(GameManager.Instance.playerRef.transform.DOPath(playerPathPos2, 4.5f, PathType.CatmullRom, PathMode.Sidescroller2D)
                .SetEase(Ease.InOutSine)
                .SetLookAt(0.001f, transform.forward, Vector3.right).SetDelay(1.5f)
                .OnWaypointChange(
                    (int waypointIndex) =>
                    {
                        if (waypointIndex == 3)
                            StartCoroutine(GameManager.Instance.playerRef.PlayCallSfx(false));
                    }))
            .AppendCallback(() => { StartCoroutine(blueNpc.PlayCallSfx()); })
            .OnComplete(
                () =>
                {
                    
                    GameManager.Instance.playerRef.playerInput.actions.FindAction("Move").Enable();
                    CinematicsManager.Instance.AfterCinematicEnds();
                    blueNpc.ToggleFollow(true);
                    GameManager.Instance.playerRef.SetBlueReference(blueNpc);
                    GameManager.Instance.LoadLevelSection(2);
                    GameManager.Instance.playerRef.ToggleEyeFollowTarget(false);
                    CameraManager.Instance.UpdatePlayerRadius(4,true);
                    CameraManager.Instance.AddObjectToCameraView(GameManager.Instance.blueNpcRef.transform,false,false,CameraManager.Instance.camZoomPlayer,1);
                    GameManager.Instance?.blueNpcRef.ToggleReactToCall(true);
                }
            );

    }
    
    private void FirstTimeCallInput(InputAction.CallbackContext ctx)
    {
        //does the call normally
        CinematicBlueMeetupPt2();
        GameManager.Instance.playerRef.playerInput.actions.FindAction("Call").performed -= FirstTimeCallInput;
        
    }

    public void TriggerFisbowlBlueFishesPt1(bool fullCinematic = false)
    {
        //if(fullCinematic)
          //  CinematicsManager.Instance?.BeforeCinematicStarts(true, true);

        
        //positions
        Transform[] bluePathTransforms = bluePathBefore.GetComponentsInChildren<Transform>();
        Vector3[] bluePathPos = new Vector3[bluePathTransforms.Length-1];
        for (int i = 1; i < bluePathTransforms.Length; i++)
        {
            bluePathPos[i-1] = bluePathTransforms[i].position;
        }
        

        GameManager.Instance?.blueNpcRef.ToggleReactToCall(false);
        StartCoroutine(GameManager.Instance.blueNpcRef.PlayCallSfx());
        GameManager.Instance.blueNpcRef.ChangeFollowTarget(newFollowTmp, 3, false);
        GameManager.Instance.blueNpcRef.ToggleFireReachedDestinationEvent(true);
        BlueMovement.OnBlueReachedDestination += TriggerFisbowlBlueFishesPt2;
        
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
                flockFishes.ToggleContainment(false);
                flockFishes.ToggleAvoidPlayer(true);
                flockFishes.ToggleAvoidBlue(true);
                
                GameManager.Instance?.blueNpcRef.ToggleFollow(false);
            })
            .Append(GameManager.Instance.blueNpcRef.transform
                .DOPath(bluePathPos2, 4, PathType.CatmullRom, PathMode.Sidescroller2D)
                .SetEase(Ease.InOutSine)
                .SetLookAt(0.001f, transform.forward, Vector3.right))
            .AppendInterval(0.25f)
            .AppendCallback(() =>
            {
                GameManager.Instance.blueNpcRef.ChangeFollowTarget(GameManager.Instance.playerRef.transform, -1,true);
                //CinematicsManager.Instance?.AfterCinematicEnds();
            });
    }
    
    public void TriggerFisbowlBigBlue()
    {
        //positions
        Transform[] bluePathTransforms = bluePathBefore2.GetComponentsInChildren<Transform>();
        Vector3[] bluePathPos = new Vector3[bluePathTransforms.Length-1];
        for (int i = 1; i < bluePathTransforms.Length; i++)
        {
            bluePathPos[i-1] = bluePathTransforms[i].position;
        }
        bluePathTransforms = bluePathAfter2.GetComponentsInChildren<Transform>();
        Vector3[] bluePathPos2 = new Vector3[bluePathTransforms.Length-1];
        for (int i = 1; i < bluePathTransforms.Length; i++)
        {
            bluePathPos2[i-1] = bluePathTransforms[i].position;
        }

        Sequence seq = DOTween.Sequence()
            .AppendCallback(() =>
            {
                StartCoroutine(GameManager.Instance.blueNpcRef.PlayCallSfx());
                GameManager.Instance.blueNpcRef.ToggleFollow(false);
                GameManager.Instance.blueNpcRef.ToggleEyeFollowTarget(true, bigBlueFish.transform);
            })
            .AppendInterval(0.2f)
            .Append(GameManager.Instance.blueNpcRef.transform
                .DOPath(bluePathPos, 4, PathType.CatmullRom, PathMode.Sidescroller2D)
                .SetEase(Ease.InOutSine)
                .SetLookAt(0.001f, transform.forward, Vector3.right))
            .AppendCallback(() =>
            {
                StartCoroutine(GameManager.Instance.blueNpcRef.PlayCallSfx());
            })
            .AppendInterval(0.1f)
            .AppendCallback(() =>
            {
                bigBlueFish.StarTree();
            })
            .AppendInterval(0.1f)
            .AppendCallback(() =>
            {
                StartCoroutine(bigBlueFish.PlayCallSfx());

            })
            .AppendInterval(0.2f)
            .AppendCallback(() =>
            {
                StartCoroutine(GameManager.Instance.blueNpcRef.PlayCallSfx());
                GameManager.Instance.blueNpcRef.ChangeFollowTarget(bigBlueFish.transform, 1);
                GameManager.Instance.blueNpcRef.ToggleFollow(true);
            });


    }



    // Update is called once per frame
    void Update()
    {
        
    }

    public void PrepareBlueForMeetup()
    {
        Transform[] tBluePathMonster = pathBlueMeetup1.GetComponentsInChildren<Transform>();
        GameManager.Instance.blueNpcRef.transform.position = tBluePathMonster[1].position;
    }
}
