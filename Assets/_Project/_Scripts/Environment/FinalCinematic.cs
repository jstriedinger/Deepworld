using System.Collections;

using System.Collections.Generic;

using DG.Tweening;
using UnityEngine;

using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class FinalCinematic : MonoBehaviour

{

    [SerializeField] Transform[] path;

    [SerializeField] GameObject blueObject;

    [SerializeField] MonsterPlayer player;

    [SerializeField] float cinematicTime;

    [SerializeField] BlueNPC blueNPC;

    [SerializeField] CanvasGroup fadeOut;



    [SerializeField] private RectTransform topCinematicBar;
    [SerializeField] private RectTransform bottomCinematicBar;



    PlayerInput playerInput;

    Rigidbody2D blueRb;

    // Start is called before the first frame update

    void Start()

    {
        blueRb = blueObject.GetComponent<Rigidbody2D>();

        playerInput = player.gameObject.GetComponent<PlayerInput>();

    }



    // Update is called once per frame

    void Update()

    {

        

    }



    void PlayFinalCinematic()
    {
        Vector3[] playerVectors = new Vector3[path.Length];
        for (int i = 0; i < path.Length; i++)
        {
            playerVectors[i] = path[i].position;
        }

        playerInput.enabled = false;
        player.StopMovement();

        ShowCinematicBars();
        GameManager gm = FindFirstObjectByType<GameManager>();
        

        Sequence finalCinematic = DOTween.Sequence()
            .Append(player.transform.DOPath(playerVectors, cinematicTime, PathType.CatmullRom, PathMode.Sidescroller2D)

                .SetEase(Ease.InOutSine)

                .SetLookAt(0.001f, transform.forward, Vector3.right)
                .SetDelay(1)
                .OnWaypointChange(
            (int waypointIndex) =>
            {
                if (waypointIndex == 4 )
                    StartCoroutine(player.PlayCallSFX());

            }))
            .AppendCallback(() => { StartCoroutine(blueNPC.PlayCallSFX()); })
            .AppendInterval(3)
            .Append(fadeOut.DOFade(1, 3))
            .OnComplete( () =>
            {
                gm.ResetGame();
            });
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
            Debug.Log("play final cinematic");
        if(collision.gameObject.CompareTag("Player"))
        {
            PlayFinalCinematic();
        }
    }



    private void ShowCinematicBars()
    {
        Sequence barsSeq = DOTween.Sequence()
            .Append(topCinematicBar.DOSizeDelta(new Vector2(0, 100), 1))
            .Join(bottomCinematicBar.DOSizeDelta(new Vector2(0, 100), 1));


    }

}

