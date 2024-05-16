using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LightShaftMover : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private float offsetX = 1;
    [SerializeField] private float offsetTime = 2.5f;
    void Start()
    {
        float x = transform.position.x;
        transform.DOMoveX(x + offsetX, offsetTime).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
