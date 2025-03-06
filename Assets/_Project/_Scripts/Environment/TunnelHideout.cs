using BehaviorDesigner.Runtime;
using UnityEngine;

public class TunnelHideout : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if( collision.gameObject.CompareTag("Player") && !GameManager.Instance.isPlayerDead)
        {
            GlobalVariables.Instance.SetVariableValue("isPlayerHidden", true);
            GameManager.Instance.playerRef.isHidden = true;

            Collider2D[] monsterHits = Physics2D.OverlapCircleAll(collision.transform.position, 100, LayerMask.GetMask("Monster"));
            //lets tell the monster chasing that were near the shelter
            foreach (Collider2D monsterCollider in monsterHits)
            {
                MonsterReactive monsterReactive = monsterCollider.GetComponent<MonsterReactive>();
                monsterReactive?.OnPlayerHides();
                
            }

        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !GameManager.Instance.isPlayerDead)
        {
            GlobalVariables.Instance.SetVariableValue("isPlayerHidden", false);
            GameManager.Instance.playerRef.isHidden = false;
        }
    }
}
