using System.Collections;

using System.Collections.Generic;

using UnityEngine;

using Cinemachine;

using static Cinemachine.CinemachineTargetGroup;
using System;
using DG.Tweening;

public class CameraManager : MonoBehaviour
{

    private GameManager _gameManager;
    private AudioManager _audioManager;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] CinemachineTargetGroup targetGroup;
    [SerializeField] float lerpDuration = 1f;
    [SerializeField] int camZoomPlayer = 28;
    [SerializeField] int camZoomEnemy = 28;


    private int _numMonstersOnScreen = 0;
    private float _defaultNoiseAmplitude;
    private CinemachineBasicMultiChannelPerlin _cbmcp;
    private GameObject _tmpExtraFollowedObject;
    private Tween _cameraTween;

    // Start is called before the first frame update
    private void Awake()
    {
        _cbmcp = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        _defaultNoiseAmplitude = _cbmcp.m_AmplitudeGain;
    }
    void Start()
    {
        
        targetGroup.m_Targets[0].radius = camZoomPlayer;
        _gameManager = GameObject.FindFirstObjectByType<GameManager>();
        _audioManager = GameObject.FindFirstObjectByType<AudioManager>();
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
    public void AddTempFollowedObj(GameObject tmpObj)
    {
        _tmpExtraFollowedObject = tmpObj;
        targetGroup.AddMember(_tmpExtraFollowedObject.transform, 0, camZoomPlayer);
        int memberIndex = targetGroup.FindMember(tmpObj.transform);
        DOTween.To(() => targetGroup.m_Targets[memberIndex].weight, x => targetGroup.m_Targets[memberIndex].weight = x, 1.8f, 2);
    }

    public void RemoveTempFollowedObj()
    {
        int memberIndex = targetGroup.FindMember(_tmpExtraFollowedObject.transform);
        DOTween.To(() => targetGroup.m_Targets[memberIndex].weight, x => targetGroup.m_Targets[memberIndex].weight = x, 0f, 2)
            .OnComplete(() =>
            {
                targetGroup.RemoveMember(_tmpExtraFollowedObject.transform);
                _tmpExtraFollowedObject = null;
            });
    }

    //Get back into the default noise amplitude
    public void ToggleDefaultNoise(bool toggle)
    {
        
        if (toggle)
        {
            DOTween.To(() => _cbmcp.m_AmplitudeGain,
                        x => _cbmcp.m_AmplitudeGain = x,
                        _defaultNoiseAmplitude, 6);
            
        }
        else
        {
            _cbmcp.m_AmplitudeGain = 0;
        }
        
    }
    
    public void ShakeCamera(float duration, int freq = 15)
    {
        float beforeFreq = _cbmcp.m_FrequencyGain;

        _cbmcp.m_AmplitudeGain = .5f;
        Sequence seq = DOTween.Sequence();
        seq.Append(DOTween.To(() => _cbmcp.m_FrequencyGain,
                x => _cbmcp.m_FrequencyGain = x, freq, duration))
            .AppendInterval(1)
            .Append(DOTween.To(() => _cbmcp.m_FrequencyGain,
                x => _cbmcp.m_FrequencyGain = x, beforeFreq, duration))
            .OnComplete(() => { 
                _cbmcp.m_AmplitudeGain = _defaultNoiseAmplitude;
            });
    }

    //makes all other target with radius 0 so that it focus on the monster that just eat oyr player
    public void OnGameOver(GameObject monster)
    {
        //reset targetgroup
        int i = targetGroup.FindMember(monster.transform);
        if (i > 0)
        {
            //all other members will lerp to zero
            Target monsterTarget = targetGroup.m_Targets[i];
            monsterTarget.radius = camZoomPlayer;
            for (int j = 0; j < targetGroup.m_Targets.Length; j++)
            {
                if (j != i)
                {
                    targetGroup.m_Targets[j].weight = 0;
                    if (j > 0)
                    {
                        EnemyMonster enemyMonster = targetGroup.m_Targets[j].target.GetComponent<EnemyMonster>();
                        enemyMonster.inCamera = false;
                        
                    }
                }
            }
        }
    }



    //reset to focus on player again
    public void ResetTargetGroup()
    {
        targetGroup.m_Targets[0].weight = 1.25f;
        targetGroup.m_Targets[0].radius = camZoomPlayer;
    }



    private void UpdateTargetGroupRadius()
    {
        int numMonsters = targetGroup.m_Targets.Length - 1;
        int newRadius = camZoomEnemy;// + (camZoomMultiplier * _numMonstersOnScreen);
        if (numMonsters >= 1)
        {
            for (int i = 1; i < targetGroup.m_Targets.Length; i++)
            {
                targetGroup.m_Targets[i].radius = newRadius;
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
        _cameraTween = DOTween.To(() => targetGroup.m_Targets[memberIndex].weight, x => targetGroup.m_Targets[memberIndex].weight = x, finalWeight, lerpDuration)
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
                    targetGroup.m_Targets[memberIndex].weight = 0;
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
            while (timeElapsed < duration && isDecreasing /*&targetGroup.m_Targets[memberIndex].radius == 21*/)
            {
                weightLerped = Mathf.Lerp(start, end, timeElapsed / duration);
                //change weight
                targetGroup.m_Targets[memberIndex].weight = weightLerped;
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            if (isDecreasing)
            {
                weightLerped = end;
                targetGroup.m_Targets[memberIndex].weight = weightLerped;
                isDecreasing = false;
                if(isMonster)
                    _numMonstersOnScreen--;
                //MetricManagerScript.instance?.LogString("EnemiesOnScreen", gameManager.numMonstersOnScreen.ToString());

            }
        }
        else
        {
            //increasing
            //targetGroup.m_Targets[memberIndex].radius = 19;
            bool isIncreasing = true;
            while (timeElapsed < duration && isIncreasing /*targetGroup.m_Targets[memberIndex].radius==19*/ )
            {
                weightLerped = Mathf.Lerp(start, end, timeElapsed / duration);
                //change weight
                targetGroup.m_Targets[memberIndex].weight = weightLerped;
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            if (isIncreasing)
            {
                weightLerped = end;
                targetGroup.m_Targets[memberIndex].weight = weightLerped;
                isIncreasing = false;
                if(isMonster)
                    _numMonstersOnScreen++;
                //MetricManagerScript.instance?.LogString("EnemiesOnScreen", gameManager.numMonstersOnScreen.ToString());

            }
        }
        //UpdateTargetGroupRadius();


    }



    public void ChangePlayerRadius(float newRadius, bool snap = false)
    {
        if (snap)
            targetGroup.m_Targets[0].radius = newRadius;
        else
        {
            DOTween.To(() => targetGroup.m_Targets[0].radius, x => targetGroup.m_Targets[0].radius = x, newRadius, 5);
        }

    }
    



    public void RemoveddObjectToTargetGroup(GameObject obj)
    {
        targetGroup.RemoveMember(obj.transform);
    }




}

