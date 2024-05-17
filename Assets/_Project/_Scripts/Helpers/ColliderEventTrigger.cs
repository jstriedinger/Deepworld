using System.Collections;

using System.Collections.Generic;

using UnityEngine;

using UnityEngine.Events;
using UnityEngine.Serialization;

public class ColliderEventTrigger : MonoBehaviour
{

    [Tooltip("If selected it will trigger every time. If not, it will only happen once")]
    [SerializeField] private UnityEvent cinematicTriggerEvent;
    private bool _trigerred;

    // Start is called before the first frame update

    void Start()
    {
        _trigerred = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player") && !_trigerred)
        {
            cinematicTriggerEvent.Invoke();
            _trigerred = true;
        }
    }

}

