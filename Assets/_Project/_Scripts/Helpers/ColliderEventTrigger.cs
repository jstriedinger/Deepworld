using System.Collections;

using System.Collections.Generic;

using UnityEngine;

using UnityEngine.Events;
using UnityEngine.Serialization;

public class ColliderEventTrigger : MonoBehaviour
{
    [Tooltip("If selected it will trigger every time. If not, it will only happen once")]
    [SerializeField] private UnityEvent cinematicTriggerEvent;

    [SerializeField] private bool triggerOnce = true;
    [SerializeField] private bool triggeredByBlue = false;
    private bool _triggered;

    // Start is called before the first frame update

    void Start()
    {
        _triggered = false;
    }

    public void RestartTrigger()
    {
        _triggered = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((triggerOnce && !_triggered) || !triggerOnce)
        {
            if((!triggeredByBlue && collision.gameObject.CompareTag("Player"))
               || (triggeredByBlue && collision.gameObject.CompareTag("Blue")) )
            {
                
                cinematicTriggerEvent.Invoke();
                _triggered = true;
            }
        }
    }

}

