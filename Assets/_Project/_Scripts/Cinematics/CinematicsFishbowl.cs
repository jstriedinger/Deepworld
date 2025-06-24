using System;
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
    
    [Header("Monster approach")]
    [SerializeField] MonsterCinematic curiousMonster1;
    [SerializeField] Transform curiousMonsterCameraPoint;
    [SerializeField] GameObject curiousMonsterPath1;
    [SerializeField] GameObject curiousMonsterPath2;
    
    [Header("Event 3&4 - Big blue and green")]
    [SerializeField] SwimmerFish bigBlueFish;
    [SerializeField] SwimmerFish bigGreenFish;
    [SerializeField] Transform tempFollowForbigGreenFish;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Start()
    {
        if (bigGreenFish)
        {
            bigGreenFish.ToggleCanReactToPlayer(false);
        }
        if (bigBlueFish)
        {
            bigBlueFish.ToggleCanReactToPlayer(false);
        }
    }

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
        

    }

    public void TriggerCinematicCuriousMonster()
    {
        Transform[] bluePathTransforms = curiousMonsterPath1.GetComponentsInChildren<Transform>();
        Vector3[] bluePathPos1 = new Vector3[bluePathTransforms.Length-1];
        for (int i = 1; i < bluePathTransforms.Length; i++)
        {
            bluePathPos1[i-1] = bluePathTransforms[i].position;
        }
        
        bluePathTransforms = curiousMonsterPath2.GetComponentsInChildren<Transform>();
        Vector3[] bluePathPos2 = new Vector3[bluePathTransforms.Length-1];
        for (int i = 1; i < bluePathTransforms.Length; i++)
        {
            bluePathPos2[i-1] = bluePathTransforms[i].position;
        }
        curiousMonster1.gameObject.SetActive(true);
        curiousMonster1.ToggleTrackTarget(true, GameManager.Instance?.playerRef.gameObject);
        CameraManager.Instance.AddObjectToCameraView(curiousMonsterCameraPoint,false,false,CameraManager.Instance.camZoomPlayer,1);
        Sequence seq = DOTween.Sequence()
            .Append(curiousMonster1.transform.DOPath(bluePathPos1, 3f, PathType.CatmullRom, PathMode.Sidescroller2D)
                .SetEase(Ease.InOutSine)
                .SetLookAt(0.001f, transform.forward, Vector3.right))
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                StartCoroutine(curiousMonster1.PlayReactSfx(true, true));
            })
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                CameraManager.Instance?.RemoveObjectFromCameraView(curiousMonsterCameraPoint, false);
                curiousMonster1.ToggleTrackTarget(false);
            })
            .Append(curiousMonster1.transform.DOPath(bluePathPos2, 5f, PathType.CatmullRom, PathMode.Sidescroller2D)
                .SetEase(Ease.InOutSine)
                .SetLookAt(0.001f, transform.forward, Vector3.right))
            .OnComplete(() =>
            {
                curiousMonster1.gameObject.SetActive(false);

            });



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
        UIManager.Instance.TogglePlayerUIPrompt(false);

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
                            GameManager.Instance.playerRef.PlayCallSfx(false);
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
    
    
    public void TriggerFisbowlBigGreen()
    {
        Debug.Log("Trigger big green");
        //blue sound
        StartCoroutine(GameManager.Instance.blueNpcRef.PlayCallSfx());
        CameraManager.Instance.AddObjectToCameraView(bigGreenFish.transform,false,false,CameraManager.Instance.camZoomPlayer,1);
        GameManager.Instance?.blueNpcRef.ToggleReactToCall(false);
        //blue follow 
        GameManager.Instance.blueNpcRef.ChangeFollowTarget(bigGreenFish.transform, 1,true);
        GameManager.Instance.blueNpcRef.ToggleEyeFollowTarget(true,bigGreenFish.transform);
        //whale sound
        // wait couple of seconds
        // restart

        Sequence seq = DOTween.Sequence()
            .AppendInterval(1)
            .AppendCallback(() =>
            {
                bigGreenFish.PlayCallSfxSimple();
            })
            .AppendInterval(8f)
            .AppendCallback(() =>
            {
                StartCoroutine(GameManager.Instance.blueNpcRef.PlayCallSfx());
                GameManager.Instance?.blueNpcRef.ChangeFollowTarget(GameManager.Instance?.playerRef.transform);
                GameManager.Instance?.blueNpcRef.ToggleEyeFollowTarget(true,GameManager.Instance?.playerRef.transform);
                CameraManager.Instance.RemoveObjectFromCameraView(bigGreenFish.transform,false);

            })
            .AppendCallback(() =>
            {
                bigGreenFish.PlayCallSfxSimple();
                bigGreenFish.ToggleCanReactToPlayer(true);
                GameManager.Instance?.blueNpcRef.ToggleReactToCall(true);
            });


    }

    public void TriggerFisbowlBigBlue()
    {
        Debug.Log("Trigger big whale");
        //blue sound
        StartCoroutine(GameManager.Instance.blueNpcRef.PlayCallSfx());
        CameraManager.Instance.AddObjectToCameraView(bigBlueFish.transform,false,false,CameraManager.Instance.camZoomPlayer,1);
        GameManager.Instance?.blueNpcRef.ToggleReactToCall(false);
        //blue follow 
        GameManager.Instance.blueNpcRef.ChangeFollowTarget(bigBlueFish.transform, 1,true);
        GameManager.Instance.blueNpcRef.ToggleEyeFollowTarget(true,bigBlueFish.transform);
        //whale sound
        // wait couple of seconds
        // restart

        Sequence seq = DOTween.Sequence()
            .AppendInterval(1)
            .AppendCallback(() =>
            {
                bigBlueFish.PlayCallSfxSimple();
            })
            .AppendInterval(10f)
            .AppendCallback(() =>
            {
                StartCoroutine(GameManager.Instance.blueNpcRef.PlayCallSfx());
                GameManager.Instance?.blueNpcRef.ChangeFollowTarget(GameManager.Instance?.playerRef.transform);
                GameManager.Instance?.blueNpcRef.ToggleEyeFollowTarget(true,GameManager.Instance?.playerRef.transform);
                CameraManager.Instance.RemoveObjectFromCameraView(bigBlueFish.transform,false);

            })
            .AppendCallback(() =>
            {
                bigBlueFish.PlayCallSfxSimple();
                bigBlueFish.ToggleCanReactToPlayer(true);
                GameManager.Instance?.blueNpcRef.ToggleReactToCall(true);
            });


    }
    
    // Update is called once per frame
    

    public void PrepareBlueForMeetup()
    {
        Transform[] tBluePathMonster = pathBlueMeetup1.GetComponentsInChildren<Transform>();
        GameManager.Instance.blueNpcRef.transform.position = tBluePathMonster[1].position;
    }
}
