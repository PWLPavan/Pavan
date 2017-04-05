using UnityEngine;
using System.Collections;
using FGUnity.Utils;

public class DisableAnimatorBehavior : StateMachineBehaviour
{
    public enum TriggerTime
    {
        Enter,
        Exit
    }

    public TriggerTime Trigger = TriggerTime.Exit;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (Trigger == TriggerTime.Enter)
        {
            CoroutineInstance.Run(null, WaitToDisable(animator));
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (Trigger == TriggerTime.Exit)
            animator.enabled = false;
    }

    private IEnumerator WaitToDisable(Animator inAnimator)
    {
        yield return null;
        inAnimator.enabled = false;
    }
}
