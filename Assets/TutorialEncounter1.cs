using System.Collections;

using System.Collections.Generic;

using UnityEngine;

using DG.Tweening;

using FMOD.Studio;

using FMODUnity;
using UnityEngine.Serialization;


public class TutorialEncounter1 : MonoBehaviour

{

    [SerializeField] private float distanceToCheck;
    [FormerlySerializedAs("SFXMonsterDistance")] [SerializeField] FMODUnity.EventReference sfxMonsterDistance;
    [SerializeField] private GameManager gameManager;
    private GameObject player;
    private FMOD.Studio.EventInstance _instanceMonsterDistance;
    private FMOD.Studio.EventInstance _instanceMusicFriend;
    private CameraManager _cameraManager;

    // Start is called before the first frame update

    void Start()
    {
        _instanceMusicFriend = gameManager.GetBlueMusicInstance();
        player = GameObject.Find("Player");
        _cameraManager = GameObject.FindFirstObjectByType<CameraManager>();
    }

    private void Awake()
    {
        _instanceMonsterDistance = FMODUnity.RuntimeManager.CreateInstance(sfxMonsterDistance.Guid);
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(_instanceMonsterDistance, transform);
        _instanceMonsterDistance.start();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            _cameraManager.ChangePlayerRadius(40);
            float distance, vol = 0;
            Sequence seq = DOTween.Sequence();
            seq.Append(
                DOTween.To(() =>
                {
                    _instanceMonsterDistance.getParameterByName("Monster Distance", out distance);
                    return distance;

                },

                x => { _instanceMonsterDistance.setParameterByName("Monster Distance", x); }, 1, 7)
                )
            .Join(
                DOTween.To(() =>
                {
                    _instanceMusicFriend.getVolume(out vol);
                    return vol;

                },

                x => { _instanceMusicFriend.setVolume(x); }, 0, 7)
                );
                
            
        }
    }



    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _cameraManager.ChangePlayerRadius(28);
            float distance, vol = 0;
            Sequence seq = DOTween.Sequence();
            seq.Append(
                DOTween.To(() =>
                {
                    _instanceMonsterDistance.getParameterByName("Monster Distance", out distance);
                    return distance;

                },

                x => { _instanceMonsterDistance.setParameterByName("Monster Distance", x); }, 0, 7)
                )
            .Join(
                DOTween.To(() =>
                {
                    _instanceMusicFriend.getVolume(out vol);
                    return vol;

                },

                x => { _instanceMusicFriend.setVolume(x); }, 1, 7)
                );
        }
    }

}

