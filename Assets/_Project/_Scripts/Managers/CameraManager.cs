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
    
    //set camera to follow specific target
    public void ChangeCameraTracking(GameObject newTarget = null)
    {
        if (newTarget)
            virtualCamera.Follow = newTarget.transform;
        else
            virtualCamera.Follow = targetGroup.transform;
    }

    //makes all other target with radius 0 so that it focus on the monster that just eat oyr player
    public void OnGameOver(GameObject monster)
    {
        //reset targetgroup
        int i = targetGroup.FindMember(monster.transform);
        if (i > 0)
        {
            Debug.Log("Lerping the rest to zero");
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

