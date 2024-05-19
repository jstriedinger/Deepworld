using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using DG.Tweening;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using STOP_MODE = FMOD.Studio.STOP_MODE;

//this class conrols everythign that happens when you are chased by monsters
public class AudioManager : MonoBehaviour
{
    [Header("Audio sfx")]
    [SerializeField] private EventReference sfxFirstMonsterAppear;
    private StudioEventEmitter _sfxMonsterChaseLoop;
    
    [Header("Music")]
    [SerializeField] private EventReference musicAmbientIntro;
    [SerializeField] private EventReference musicAmbientDanger;
    [SerializeField] private EventReference musicBlue;
    [SerializeField] private EventReference musicCloseDanger;
    private FMOD.Studio.EventInstance _instanceAmbientIntro, _instanceAmbientDanger, _instanceFriend, _currentInstancePlaying,
        _instanceCloseDanger;
    // order of above 0 = ambient intro 1= danger 2=friend
    private int _currentMusicIndex;
    private Transform _playerRef;
    
    [HideInInspector]
    public int numMonstersChasing;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (numMonstersChasing > 0)
        {
            Collider2D col = Physics2D.OverlapCircle(_playerRef.position,40,LayerMask.GetMask("Monster"));
            Debug.Log("Closest enemy is: "+col.gameObject.name);
            float d = Vector3.Distance(_playerRef.position, col.transform.position) / 40;
            Debug.Log("Normalized Distance from player: "+d);
            _instanceCloseDanger.setParameterByName("Monster Distance", 1 - d);
        }
    }

    public void Initialize(Transform playerRef)
    {
        _playerRef = playerRef;
        _instanceAmbientIntro = RuntimeManager.CreateInstance(musicAmbientIntro.Guid);
        _instanceAmbientDanger = RuntimeManager.CreateInstance(musicAmbientDanger.Guid);
        _instanceAmbientDanger.setVolume(0.5f);
        _instanceFriend = RuntimeManager.CreateInstance(musicBlue.Guid);
        _instanceCloseDanger = RuntimeManager.CreateInstance(musicCloseDanger.Guid);
        
        RuntimeManager.AttachInstanceToGameObject(_instanceAmbientIntro, _playerRef);
        RuntimeManager.AttachInstanceToGameObject(_instanceAmbientDanger, _playerRef);
        RuntimeManager.AttachInstanceToGameObject(_instanceFriend, _playerRef);
        RuntimeManager.AttachInstanceToGameObject(_instanceCloseDanger, _playerRef);
    }

    private void Awake()
    {
        _sfxMonsterChaseLoop = GetComponent<StudioEventEmitter>();
        _currentMusicIndex = -1;
    }
    
    //Stop al background music instances
    public void StopAllFMODInstances()
    {
        _instanceAmbientIntro.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _instanceAmbientDanger.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _instanceFriend.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _instanceCloseDanger.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

   
    
    //Change bg music. 0 = intro, 1=danger 2= friends
    public void ChangeBackgroundMusic(int newMusic)
    {
        switch (_currentMusicIndex)
        {
            case 0:
                _instanceAmbientIntro.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                break;
            case 1:
                _instanceAmbientDanger.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                break;
            case 2:
                _instanceFriend.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                break;
        }
        switch (newMusic)
        {
            case 0:
                Debug.Log("Intro music");
                _instanceAmbientIntro.start();
                break;
            case 1: 
                Debug.Log("Danger music");
                _instanceAmbientDanger.start();
                break;
            case 2:
                Debug.Log("blue music");
                _instanceFriend.start();
                break;
            
        }

        _currentMusicIndex = newMusic;
    }
    
    #region Monsters

    //for level 1 tutorial encounter. Fade in danger/friend music
    public void ToggleCloseDangerAndFriendMusic(bool dir)
    {
        if (dir)
        {
            _instanceCloseDanger.start();
            float distance, vol = 0;
            Sequence seq = DOTween.Sequence();
            seq.Append(
                    DOTween.To(() =>
                        {
                            _instanceCloseDanger.getParameterByName("Monster Distance", out distance);
                            return distance;

                        },

                        x => { _instanceCloseDanger.setParameterByName("Monster Distance", x); }, 1, 7)
                )
                .Join(
                    DOTween.To(() =>
                        {
                            _instanceFriend.getVolume(out vol);
                            return vol;

                        },

                        x => { _instanceFriend.setVolume(x); }, 0, 7)
                );
        }
        else
        {
            float distance, vol = 0;
            Sequence seq = DOTween.Sequence();
            seq.Append(
                    DOTween.To(() =>
                        {
                            _instanceCloseDanger.getParameterByName("Monster Distance", out distance);
                            return distance;

                        },

                        x => { _instanceCloseDanger.setParameterByName("Monster Distance", x); }, 0, 7)
                )
                .Join(
                    DOTween.To(() =>
                        {
                            _instanceFriend.getVolume(out vol);
                            return vol;

                        },

                        x => { _instanceFriend.setVolume(x); }, 1, 7)
                )
                .OnComplete(() =>
                {
                    _instanceCloseDanger.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                })
                ;
        }
    }
    
    //Handles when to play the audio of monsters appearing
    public void PlayMonsterAppearSfx()
    {
        if(!sfxFirstMonsterAppear.IsNull)
        {
            RuntimeManager.PlayOneShot(sfxFirstMonsterAppear.Guid, transform.position);
        }
    }
    
    //updates mosnter chasing and plays audio if needed
    public void UpdateMonstersChasing(bool _newMonster, bool reset = false)
    {
        if (reset)
        {
            numMonstersChasing = 0;
            _sfxMonsterChaseLoop.Stop();
        }
        else
        {
            
            if (_newMonster)
            {
                numMonstersChasing++;
                if (numMonstersChasing == 1)
                {
                    _sfxMonsterChaseLoop.Play();
                    _instanceCloseDanger.start();
                }
                Gamepad.current?.SetMotorSpeeds(0.0625f, .15f);
            }
            else
            {
                numMonstersChasing--;
                if (numMonstersChasing <= 0)
                {
                    Gamepad.current?.SetMotorSpeeds(0, 0f);
                    numMonstersChasing = 0;
                    _sfxMonsterChaseLoop.Stop();
                    _instanceCloseDanger.stop(STOP_MODE.ALLOWFADEOUT);
                    _instanceCloseDanger.setParameterByName("Monster Distance", 0);
                }
            }
        }
    }

    public void StopChaseMusic()
    {
        _sfxMonsterChaseLoop.Stop();
        _instanceCloseDanger.stop(STOP_MODE.ALLOWFADEOUT);
    }
    #endregion

    
}
