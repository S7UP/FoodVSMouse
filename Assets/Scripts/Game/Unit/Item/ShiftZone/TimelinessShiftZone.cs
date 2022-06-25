using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 基础变速地带
/// </summary>
public class TimelinessShiftZone : BaseShiftZone
{
    private int leftTime; // 剩余时间

    public override void MInit()
    {
        base.MInit();
        leftTime = 0;
    }

    /// <summary>
    /// 设置剩余时间
    /// </summary>
    public void SetLeftTime(int t)
    {
        leftTime = t;
    }

    public override void MUpdate()
    {
        base.MUpdate();
        if(leftTime<=0)
        {
            ExecuteDeath();
        }
        leftTime--;
    }

    public override void OnIdleStateEnter()
    {
        animator.Play("Appear");
    }
}
