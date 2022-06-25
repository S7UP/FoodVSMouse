using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �������ٵش�
/// </summary>
public class TimelinessShiftZone : BaseShiftZone
{
    private int leftTime; // ʣ��ʱ��

    public override void MInit()
    {
        base.MInit();
        leftTime = 0;
    }

    /// <summary>
    /// ����ʣ��ʱ��
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
