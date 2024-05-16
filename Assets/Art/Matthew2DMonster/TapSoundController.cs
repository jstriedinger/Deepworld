using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapSoundController : MonoBehaviour
{

    public AudioClip[] tap;
    public LayerMask layer;

        private float timer;
    

    void Start(){
        timer = 0.5f;
    }
    // Start is called before the first frame update
    void Update(){
        if(timer < 0.5f){
            timer -= Time.deltaTime;
        }
        else if(timer < 0f){
            timer = 0.5f;
        }
    }

    // Update is called once per frame
    void OnCollisionEnter2D()
    {
        Debug.Log("Collision detected!");
        if(timer == 0.5f){
            Debug.Log("Audio played!");
            int rand = Random.Range(0,3);
            GetComponent<AudioSource>().clip = tap[rand];

            GetComponent<AudioSource>().Play();
        }
        
        
    }
}
