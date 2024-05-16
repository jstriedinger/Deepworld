using System.Collections;

using System.Collections.Generic;

using UnityEngine;

using UnityEngine.Events;

public class CollideEventTrigger : MonoBehaviour
{

    [Tooltip("If selected it will trigger every time. If not, it will only happen once")]
    [SerializeField] private bool _continuous;
    [SerializeField] private UnityEvent cinematicTriggerEvent;
    private bool trigerred;

    // Start is called before the first frame update

    void Start()
    {
        trigerred = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            if (_continuous)
                cinematicTriggerEvent.Invoke();
            else if (!trigerred)
            {
                trigerred = true;
                cinematicTriggerEvent.Invoke();
            }
        }
    }

}

