using UnityEngine;

public class RandemIdleBehavior : StateMachineBehaviour
{
    [SerializeField] private int numAnims;
     //OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    // override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    // {
    //     
    // }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.normalizedTime % 1 > 0.98f)
        {
            int idleAnim = Random.Range(1, numAnims + 1);
            animator.SetFloat("idleSeed", idleAnim);
        }
    }

    
}
