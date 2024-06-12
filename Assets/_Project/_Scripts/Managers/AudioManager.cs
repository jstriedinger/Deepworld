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
    public EventReference sfxMonsterScream;
    [SerializeField] private EventReference sfxBlueCall;
    [SerializeField] private EventReference sfxBlueScream;
    
    private StudioEventEmitter _sfxMonsterChaseLoop;
    
    [FormerlySerializedAs("musicAmbientIntro")]
    [Header("Music")]
    [SerializeField] private EventReference musicIntro;
    [SerializeField] private EventReference musicBlue;
    [SerializeField] private EventReference musicMystery;
    [SerializeField] private EventReference musicCloseDanger;
    [SerializeField] private EventReference musicUnderworld;
    private FMOD.Studio.EventInstance _instanceMusicIntro, _instanceMusicUnderworld, _instanceMusicBlue, _currentInstancePlaying,
        _instanceCloseDanger, _instanceMusicMystery;
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
            if (col)
            {
                float d = Vector3.Distance(_playerRef.position, col.transform.position) / 40;
                _instanceCloseDanger.setParameterByName("Monster Distance", 1 - d);
                
            }
        }
    }

    public void Initialize(Transform playerRef)
    {
        _playerRef = playerRef;
        _instanceMusicIntro = RuntimeManager.CreateInstance(musicIntro.Guid);
        _instanceMusicBlue = RuntimeManager.CreateInstance(musicBlue.Guid);
        _instanceMusicMystery = RuntimeManager.CreateInstance(musicMystery.Guid);
        _instanceCloseDanger = RuntimeManager.CreateInstance(musicCloseDanger.Guid);
        _instanceMusicUnderworld = RuntimeManager.CreateInstance(musicUnderworld.Guid);
        
        //all volume starts in zero
        _instanceMusicIntro.setVolume(0);
        _instanceMusicBlue.setVolume(0);
        _instanceMusicMystery.setVolume(0);
        _instanceMusicUnderworld.setVolume(0);
        
        RuntimeManager.AttachInstanceToGameObject(_instanceMusicIntro, _playerRef);
        RuntimeManager.AttachInstanceToGameObject(_instanceMusicUnderworld, _playerRef);
        RuntimeManager.AttachInstanceToGameObject(_instanceMusicBlue, _playerRef);
        RuntimeManager.AttachInstanceToGameObject(_instanceCloseDanger, _playerRef);
        RuntimeManager.AttachInstanceToGameObject(_instanceMusicMystery, _playerRef);
    }

    private void Awake()
    {
        _sfxMonsterChaseLoop = GetComponent<StudioEventEmitter>();
        _currentMusicIndex = -1;
    }
    
    //Stop al background music instances
    public void StopAllFMODInstances()
    {
        _instanceMusicIntro.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _instanceMusicUnderworld.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _instanceMusicBlue.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _instanceCloseDanger.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

   
    
    //Change background music smoothly.
    //1 = intro, 2=Blue 3=mystery 4=closeDanger 5=underworld
    public void ChangeBackgroundMusic(int newMusic)
    {
        float vol = 0;
        Sequence seq = DOTween.Sequence();
        
        switch (_currentMusicIndex)
        {
            case 1:
                seq.Append(DOTween.To(() =>
                    {
                        _instanceMusicIntro.getVolume(out vol);
                        return vol;
                    },
                    x => { _instanceMusicIntro.setVolume(x); }, 0, 2));
                seq.AppendCallback(() => { _instanceMusicIntro.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); });
                
                break;
            case 2:
                seq.Append(DOTween.To(() =>
                    {
                        _instanceMusicBlue.getVolume(out vol);
                        return vol;
                    },
                    x => { _instanceMusicBlue.setVolume(x); }, 0, 3));
                seq.AppendCallback(() => { _instanceMusicBlue.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); });
                break;
            case 3:
                seq.Append(DOTween.To(() =>
                    {
                        _instanceMusicMystery.getVolume(out vol);
                        return vol;
                    },
                    x => { _instanceMusicMystery.setVolume(x); }, 0, 3));
                seq.AppendCallback(() => { _instanceMusicMystery.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); });
                break;
            case 4:
                seq.Append(DOTween.To(() =>
                    {
                        _instanceCloseDanger.getVolume(out vol);
                        return vol;
                    },
                    x => { _instanceCloseDanger.setVolume(x); }, 0, 3));
                seq.AppendCallback(() => { _instanceCloseDanger.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); });
                
                break;
            case 5:
                seq.Append(DOTween.To(() =>
                    {
                        _instanceMusicUnderworld.getVolume(out vol);
                        return vol;
                    },
                    x => { _instanceMusicUnderworld.setVolume(x); }, 0, 3));
                seq.AppendCallback(() => { _instanceMusicUnderworld.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); });
                break;
        }
        switch (newMusic)
        {
            case 1:
                seq.AppendCallback(() =>    
                {
                    _instanceMusicIntro.setVolume(1);
                    _instanceMusicIntro.start();
                    
                });
                break;
            case 2: 
                seq.AppendCallback(() =>    
                {
                    _instanceMusicBlue.setVolume(1);
                    _instanceMusicBlue.start();
                });
                break;
            case 3:
                seq.AppendCallback(() =>    
                {
                    
                    _instanceMusicMystery.setVolume(1);
                    _instanceMusicMystery.start();
                    
                });
                break;
            case 4:
                seq.AppendCallback(() =>    
                {
                    
                    _instanceCloseDanger.setVolume(1);
                    _instanceCloseDanger.start();
                    
                });
                break;
            case 5:
                seq.AppendCallback(() =>    
                {
                    
                    _instanceMusicUnderworld.setVolume(1);
                    _instanceMusicUnderworld.start(); 
                    
                });
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
                            _instanceMusicBlue.getVolume(out vol);
                            return vol;

                        },

                        x => { _instanceMusicBlue.setVolume(x); }, 0, 7)
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
                            _instanceMusicBlue.getVolume(out vol);
                            return vol;

                        },

                        x => { _instanceMusicBlue.setVolume(x); }, 1, 7)
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

    //reduces background music volume to better hear something
    //for now it uses the friend music right away
    public void ToggleMusicVolume(bool off)
    {
        float vol = 0;
        if (off)
        {
            DOTween.To(() =>
                {
                    _instanceMusicBlue.getVolume(out vol);
                    return vol;
                }, x => { _instanceMusicBlue.setVolume(x); }, 0.3f, 3);
        }
        else
        {
            DOTween.To(() =>
            {
                _instanceMusicBlue.getVolume(out vol);
                return vol;
            }, x => { _instanceMusicBlue.setVolume(x); }, 1, 3);
        }

    }
    
    //Used on first level. Fading out smoothly since there is option added on FMOD and
    //Im too lazy to open that up
    public void FadeOutFriendMusic()
    {
        float vol = 0;
        DOTween.To(() =>
        {
            _instanceMusicBlue.getVolume(out vol);
            return vol;
        }, x => { _instanceMusicBlue.setVolume(x); }, 0, 5)
            .OnComplete(() =>
            {
                _instanceMusicBlue.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            });
    }

    //Do the cinematic screams of Blue and the monster, as changing the background music
    public void DoCinematicScreams()
    {
        Sequence seq = DOTween.Sequence()
            .AppendCallback(() =>
            {
                RuntimeManager.PlayOneShot(sfxMonsterScream, transform.position);
            })
            .AppendInterval(1f)
            .AppendCallback(() =>
            {
                RuntimeManager.PlayOneShot(sfxMonsterScream, transform.position);
                ChangeBackgroundMusic(3);
            })
            .AppendInterval(2f)
            .AppendCallback(() =>
            {
                RuntimeManager.PlayOneShot(sfxBlueCall, transform.position);
            });
    }

    public void DoCinematicScreamsInsideTunnel()
    {
        
    }
}
