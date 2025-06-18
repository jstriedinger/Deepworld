using BehaviorDesigner.Runtime;
using UnityEngine;

public class TunnelHideout : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if( collision.gameObject.CompareTag("Player") && !GameManager.Instance.isPlayerDead)
        {
            GameManager.Instance.playerRef.ToggleHidePlayer(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !GameManager.Instance.isPlayerDead)
        {
            GameManager.Instance.playerRef.ToggleHidePlayer(false);
        }
    }
}
