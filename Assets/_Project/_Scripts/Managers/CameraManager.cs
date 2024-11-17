using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using System;
using DG.Tweening;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{

    private GameManager _gameManager;
    private AudioManager _audioManager;
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] CinemachineTargetGroup targetGroup;
    [SerializeField] float lerpDuration = 1f;
    [SerializeField] int camZoomPlayer = 28;
    [SerializeField] int camZoomEnemy = 28;

    [Header("Final chase cam")]
    [SerializeField] private Transform chaseCamAnchor;
    [SerializeField] private int chaseCameDistance;
    [SerializeField] private int maxAddRadiusChase;
    private bool _checkChaseCamDistance;
    private float _cameraStep;
    private float _tmpCurrentZoomPlayer;
    


    private int _numMonstersOnScreen = 0;
    private float _defaultNoiseAmplitude;
    private CinemachineBasicMultiChannelPerlin _cbmcp;
    private Transform _tmpExtraFollowedObject;
    private Tween _cameraTween;

    // Start is called before the first frame update
    private void Awake()
    {
        _cbmcp = virtualCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
        _defaultNoiseAmplitude = _cbmcp.AmplitudeGain;
    }
    void Start()
    {
        
        targetGroup.Targets[0].Radius = camZoomPlayer;
        _gameManager = GameObject.FindFirstObjectByType<GameManager>();
        _audioManager = GameObject.FindFirstObjectByType<AudioManager>();
        _cameraStep = (float) maxAddRadiusChase / chaseCameDistance;



    }

    private void LateUpdate()
    {
        //for final chase cam
        if (_checkChaseCamDistance)
        {
            
            UpdateCameraZoomForFinalChase();
        }
    }

    private void UpdateCameraZoomForFinalChase()
    {
        if (_gameManager.playerRef.transform.position.y < chaseCamAnchor.position.y)
        {
            float distanceY = Math.Abs(chaseCamAnchor.position.y - _gameManager.playerRef.transform.position.y);
            if (distanceY <= chaseCameDistance)
            {
                float addedRadius = distanceY * _cameraStep;
                Debug.Log("Adding radius: "+addedRadius);
                targetGroup.Targets[0].Radius =  camZoomPlayer + ( maxAddRadiusChase - addedRadius);
                    
            }
        }
    }

    //set camera to follow specific target
    public void ChangeCameraTracking(GameObject newTarget = null)
    {
        if (newTarget)
            virtualCamera.Follow = newTarget.transform;
        else
            virtualCamera.Follow = targetGroup.transform;
    }

    //Changes from previous followed objet to the targetGroup
    public void StartFollowingTargetGroup(Transform lastFollow)
    {
        targetGroup.transform.position = lastFollow.position;
        virtualCamera.Follow = targetGroup.transform;
    }

    public void ChangeFollowedObject(Transform newFollow)
    {
        virtualCamera.Follow = newFollow;
    }

    //Add an extra obj to be followed. Used for camera changes on cinemachine target group
    public void AddTempFollowedObj(Transform tmpObj, bool replace = false, int extraRadius = 0)
    {
        _tmpExtraFollowedObject = tmpObj;
        targetGroup.AddMember(_tmpExtraFollowedObject, 0, camZoomPlayer+extraRadius);
        int memberIndex = targetGroup.FindMember(tmpObj.transform);
        if (replace)
        {
            //inmediate and reduce player
            targetGroup.Targets[0].Weight = 0;
            targetGroup.Targets[memberIndex].Weight = 1.25f;
        }
        else
        {
            DOTween.To(() => targetGroup.Targets[memberIndex].Weight, x => targetGroup.Targets[memberIndex].Weight = x, 1.8f, 2);
        }
    }

    public void RemoveTempFollowedObj()
    {
        int memberIndex = targetGroup.FindMember(_tmpExtraFollowedObject);
        targetGroup.Targets[0].Weight = 1.25f;
        DOTween.To(() => targetGroup.Targets[memberIndex].Weight, x => targetGroup.Targets[memberIndex].Weight = x, 0f, 2)
            .OnComplete(() =>
            {
                targetGroup.RemoveMember(_tmpExtraFollowedObject);
                _tmpExtraFollowedObject = null;
            });
    }

    //Get back into the default noise amplitude
    public void ToggleDefaultNoise(bool toggle)
    {
        
        if (toggle)
        {
            DOTween.To(() => _cbmcp.AmplitudeGain,
                        x => _cbmcp.AmplitudeGain = x,
                        _defaultNoiseAmplitude, 6);
            
        }
        else
        {
            _cbmcp.AmplitudeGain = 0;
        }
        
    }
    
    public void ShakeCamera(float duration, int freq = 15)
    {
        float beforeFreq = _cbmcp.FrequencyGain;

        _cbmcp.AmplitudeGain = .5f;
        Sequence seq = DOTween.Sequence();
        seq.Append(DOTween.To(() => _cbmcp.FrequencyGain,
                x => _cbmcp.FrequencyGain = x, freq, duration))
            .AppendInterval(1)
            .Append(DOTween.To(() => _cbmcp.FrequencyGain,
                x => _cbmcp.FrequencyGain = x, beforeFreq, duration))
            .OnComplete(() => { 
                _cbmcp.AmplitudeGain = _defaultNoiseAmplitude;
            });
    }

    //makes all other target with radius 0 so that it focus on the monster that just eat oyr playerCharacter
    public void OnGameOver(GameObject monster)
    {
        if (monster)
        {
            //reset targetgroup
            int i = targetGroup.FindMember(monster.transform);
            if (i > 0)
            {
                //all other members will lerp to zero
                CinemachineTargetGroup.Target monsterTarget = targetGroup.Targets[i];
                monsterTarget.Radius = camZoomPlayer;
                for (int j = 0; j < targetGroup.Targets.Count; j++)
                {
                    if (j != i)
                    {
                        targetGroup.Targets[j].Weight = 0;
                        if (j > 0)
                        {
                            MonsterReactive monsterReactive = targetGroup.Targets[j].Object.GetComponent<MonsterReactive>();
                            if(monsterReactive)
                                monsterReactive.inCamera = false;
                            
                        }
                    }
                }
            }
            
        }
    }



    //reset to focus on playerCharacter again
    public void ResetTargetGroup()
    {
        targetGroup.Targets[0].Weight = 1.25f;
        targetGroup.Targets[0].Radius = camZoomPlayer;
    }



    private void UpdateTargetGroupRadius()
    {
        int numMonsters = targetGroup.Targets.Count - 1;
        int newRadius = camZoomEnemy;// + (camZoomMultiplier * _numMonstersOnScreen);
        if (numMonsters >= 1)
        {
            for (int i = 1; i < targetGroup.Targets.Count; i++)
            {
                targetGroup.Targets[i].Radius = newRadius;
            }

        }

    }



    //adds enemy to camera view
    public void AddObjectToCameraView(Transform objectToAdd, bool isMonster, bool makeNoise, float radius = 0, float weight = 1)
    {
        if(_numMonstersOnScreen == 0 && (isMonster || makeNoise) )
            StartCoroutine(_audioManager.PlayMonsterAppearSfx());
        
        //if not added already
        if (targetGroup.FindMember(objectToAdd) == -1)
            targetGroup.AddMember(objectToAdd, 0, isMonster? camZoomEnemy :  radius);
        
        ToggleCameraTween(objectToAdd.transform,weight, true,isMonster);
        //StartCoroutine(LerpWeightinTargetGroup(objectToAdd.transform, lerpDuration, 0, isMonster? 1 : 1.2f, isMonster));
    }





    //remove enemy from camera view

    public void RemoveObjectFromCameraView(Transform objectToRemove, bool isMonster)
    {
        if (targetGroup.FindMember(objectToRemove) > 0)
        {
            ToggleCameraTween(objectToRemove, 0,false, isMonster);
            //StartCoroutine(LerpWeightinTargetGroup(objectToRemove.transform, lerpDuration, 1, 0, isMonster));
        }
    }

    private void ToggleCameraTween(Transform member, float finalWeight, bool isAdding, bool isMonster)
    {
        //always stop the tween before using it again
        _cameraTween.Kill();
        int memberIndex = targetGroup.FindMember(member);
        _cameraTween = DOTween.To(() => targetGroup.Targets[memberIndex].Weight, x => targetGroup.Targets[memberIndex].Weight = x, finalWeight, lerpDuration)
            .SetAutoKill(false)
            .OnComplete(() =>
            {
                if (isMonster)
                {
                    if (isAdding)
                        _numMonstersOnScreen++;
                    else
                        _numMonstersOnScreen--;
                }
            })
            .OnKill(() =>
            {
                //if it was removing and ended unexpectedly, go to zero always
                if (!isAdding)
                    targetGroup.Targets[memberIndex].Weight = 0;
            });
        
    }

    //Lerps the weight of the target so the camera size change is smooth
    private IEnumerator LerpWeightinTargetGroup(Transform member, float duration, float start, float end, bool isMonster)
    {
        float timeElapsed = 0;
        float weightLerped = 0;
        int memberIndex = targetGroup.FindMember(member);

        if (start > end)
        {
            //decreasing
            bool isDecreasing = true;
            while (timeElapsed < duration && isDecreasing /*&targetGroup.Targets[memberIndex].radius == 21*/)
            {
                weightLerped = Mathf.Lerp(start, end, timeElapsed / duration);
                //change weight
                targetGroup.Targets[memberIndex].Weight = weightLerped;
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            if (isDecreasing)
            {
                weightLerped = end;
                targetGroup.Targets[memberIndex].Weight = weightLerped;
                isDecreasing = false;
                if(isMonster)
                    _numMonstersOnScreen--;
                //MetricManagerScript.instance?.LogString("EnemiesOnScreen", gameManager.numMonstersOnScreen.ToString());

            }
        }
        else
        {
            //increasing
            //targetGroup.Targets[memberIndex].radius = 19;
            bool isIncreasing = true;
            while (timeElapsed < duration && isIncreasing /*targetGroup.Targets[memberIndex].radius==19*/ )
            {
                weightLerped = Mathf.Lerp(start, end, timeElapsed / duration);
                //change weight
                targetGroup.Targets[memberIndex].Weight = weightLerped;
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            if (isIncreasing)
            {
                weightLerped = end;
                targetGroup.Targets[memberIndex].Weight = weightLerped;
                isIncreasing = false;
                if(isMonster)
                    _numMonstersOnScreen++;
                //MetricManagerScript.instance?.LogString("EnemiesOnScreen", gameManager.numMonstersOnScreen.ToString());

            }
        }
        //UpdateTargetGroupRadius();


    }



    public void ChangePlayerRadius(int newRadius)
    {
        DOTween.To(() => targetGroup.Targets[0].Radius, x => targetGroup.Targets[0].Radius = x, newRadius, 2)
            .OnComplete(() =>
            {
                camZoomPlayer = newRadius;
            });
    }
    



    public void RemoveddObjectToTargetGroup(GameObject obj)
    {
        targetGroup.RemoveMember(obj.transform);
    }

    /**
     * Increaese/decrease the radius by 10
     */
    public void TogglePlayerCameraRadius(bool toggle)
    {
        if (toggle)
        {
            
        }
    }


    /**
     * 1st final chase camera zoom change
     */
    public void ToggleFinalChaseCam(bool toggle)
    {
        _checkChaseCamDistance = toggle;
        if (!toggle)
        {
            _tmpCurrentZoomPlayer = camZoomPlayer + maxAddRadiusChase;
            targetGroup.Targets[0].Radius = camZoomPlayer + maxAddRadiusChase;

        }
    }
    

    public void FollowFocusBlue(bool toggle)
    {
        if (toggle)
        {
            targetGroup.AddMember(_gameManager.blueNpcRef.transform,1.25f, camZoomPlayer);
            targetGroup.Targets[0].Weight = 0;
        }
        else
        {
            Sequence seq = DOTween.Sequence();
            seq.Append(
                    DOTween.To(() => targetGroup.Targets[0].Weight, x => targetGroup.Targets[0].Weight = x, 1.25f, 2f)
                )
                .Join(
                    DOTween.To(() => targetGroup.Targets[1].Weight, x => targetGroup.Targets[1].Weight = x, 0f, 2f)
                )
                .OnComplete(() =>
                {
                    targetGroup.RemoveMember(_gameManager.blueNpcRef.transform);
                    targetGroup.Targets[0].Weight = 1.25f;
                });


        }
    }
}

