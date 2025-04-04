using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseMonsterReset : MonoBehaviour
{
    public GameObject monster;
    [SerializeField] private GameManager gameManager;
    private Vector3 initialPos;
    private float countDown = 4f;
    private bool isCountingDown;
    // Start is called before the first frame update
    void Awake()
    {
        initialPos = transform.position;
        countDown = 4f;
    }

    // Update is called once per frame
    void Update()
    {
        if(!isCountingDown){
            if(GameManager.Instance.isPlayerDead){
                isCountingDown = true;
                return;
            }
        }
        else{
            countDown -= Time.deltaTime;
            if(countDown <= 0){
                monster.transform.position = initialPos;
                monster.SetActive(false);
                isCountingDown = false;
                countDown = 4f;
            }
        }
    }
}
