using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 无敌的障碍
/// </summary>
public class InvincibilityBarrier :　BaseBarrier 
{
    private int leftTime; // 持续时间

    public override void MInit()
    {
        base.MInit();
        mType = (int)ItemNameTypeMap.PigBarrier;
        // 添加无敌的标签
        NumericBox.AddDecideModifierToBoolDict(StringManager.Invincibility, new BoolModifier(true));
    }

    public override void OnEnable()
    {
        base.OnEnable();
        leftTime = -1;
    }

    /// <summary>
    /// 设置存活时间
    /// </summary>
    public void SetLeftTime(int time)
    {
        leftTime = time;
    }

    public override void MUpdate()
    {
        base.MUpdate();
        // 剩余时间减到0则回收该对象，因此一开始如果设置成-1则代表永远不消失
        leftTime--;
        if (leftTime == 0)
        {
            DeathEvent();
        }
    }
}
