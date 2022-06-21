using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 忍者鼠随从
/// </summary>
public class NinjaRetinueMouse : MouseUnit
{

    /// <summary>
    /// 播放拉取动画
    /// </summary>
    public void PlayRectifyClip()
    {
        animator.Play("Rectify");
    }

    public override void OnTransitionStateEnter()
    {
        animator.Play("Appear");
        CloseCollision();
    }

    public override void OnTransitionState()
    {
        if (currentStateTimer == 0)
            return;

        if (AnimatorManager.GetNormalizedTime(animator) > 1.0f)
            SetActionState(new MoveState(this));
    }

    public override void OnTransitionStateExit()
    {
        OpenCollision();
    }
}
