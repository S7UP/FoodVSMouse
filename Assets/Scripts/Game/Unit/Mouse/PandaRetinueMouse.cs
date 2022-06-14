using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 熊猫鼠随从
/// </summary>
public class PandaRetinueMouse : MouseUnit
{
    /// <summary>
    /// 播放飞行动画
    /// </summary>
    public void PlayFlyClip()
    {
        animator.Play("Fly");
    }
}
