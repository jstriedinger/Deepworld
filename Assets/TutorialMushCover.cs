using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialMushCover : MonoBehaviour
{
    [SerializeField] private SpriteRenderer iconHidden;
    [SerializeField] private SpriteRenderer iconOpen;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Color tmp = iconOpen.color;
            tmp.a = 0f;
            iconOpen.color = tmp;

            iconHidden.DOFade(1, 1);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Color tmp = iconHidden.color;
            tmp.a = 0f;
            iconHidden.color = tmp;

            iconOpen.DOFade(1, 1);
        }
    }

}
