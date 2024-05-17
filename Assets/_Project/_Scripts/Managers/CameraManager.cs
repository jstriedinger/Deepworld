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
    [SerializeField] int camZoomMultiplier = 5;


    private int _numMonstersOnScreen = 0;

    // Start is called before the first frame update
    private void Awake()
    {
        
    }

    void Start()
    {
        targetGroup.m_Targets[0].radius = camZoomPlayer;
        _gameManager = GameObject.FindFirstObjectByType<GameManager>();
        _audioManager = GameObject.FindFirstObjectByType<AudioManager>();
    }
    
    public void ChangeCameraTracking()
    {
        virtualCamera.Follow = targetGroup.transform;
    }



    // Update is called once per frame

    void Update()

    {

    }



    //reset to only have the player

    public void ResetTargetGroup()
    {
        var oldTargets = targetGroup.m_Targets;
        targetGroup.m_Targets = new Target[1];
        targetGroup.m_Targets[0] = oldTargets[0];
    }



    private void UpdateTargetGroupRadius()
    {
        int numMonsters = targetGroup.m_Targets.Length - 1;
        int newRadius = camZoomEnemy + (camZoomMultiplier * _numMonstersOnScreen);
        if (numMonsters >= 1)
        {
            for (int i = 1; i < targetGroup.m_Targets.Length; i++)
            {
                targetGroup.m_Targets[i].radius = newRadius;
            }

        }

    }



    //adds enemy to camera view

    public void AddMonsterToView(GameObject monsterToAdd)
    {
        if(_numMonstersOnScreen == 0)
            _audioManager.PlayMonsterAppearSfx();

        if (targetGroup.FindMember(monsterToAdd.transform) == -1)
        {

            targetGroup.AddMember(monsterToAdd.transform, 0, camZoomEnemy);

        }
        StartCoroutine(LerpWeightinTargetGroup(monsterToAdd.transform, lerpDuration, 0, 1));
    }





    //remove enemy from camera view

    public void RemoveEnemyFromCameraView(GameObject monsterToRemove)
    {
        if (targetGroup.FindMember(monsterToRemove.transform) > 0)
        {
            StartCoroutine(LerpWeightinTargetGroup(monsterToRemove.transform, lerpDuration, 1, 0));
        }
    }

    //Lerps the weight of the target so the camera size change is smooth
    private IEnumerator LerpWeightinTargetGroup(Transform member, float duration, float start, float end)
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
                _numMonstersOnScreen++;
                //MetricManagerScript.instance?.LogString("EnemiesOnScreen", gameManager.numMonstersOnScreen.ToString());

            }
        }
        UpdateTargetGroupRadius();


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



    public void AddObjectToTargetGroup(GameObject obj)

    {

        targetGroup.AddMember(obj.transform, 2, 45);

    }



    public void RemoveddObjectToTargetGroup(GameObject obj)
    {
        targetGroup.RemoveMember(obj.transform);
    }




}

