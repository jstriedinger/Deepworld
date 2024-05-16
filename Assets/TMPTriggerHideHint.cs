using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class TMPTriggerHideHint : MonoBehaviour
{
    [SerializeField] private CanvasGroup HideHint;
    [SerializeField] private TextMeshProUGUI textField;
    [SerializeField] private string hideText;
    bool once = false;
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
        if(collision.gameObject.CompareTag("Player") && !once)
        {
            once = true;
            textField.SetText(hideText);
            Sequence seq = DOTween.Sequence()
            .Append(HideHint.DOFade(1, 2))
            .Append(HideHint.DOFade(0, 2).SetDelay(3).OnComplete(
                () =>
                {

                }
            ));

        }
    }
}
