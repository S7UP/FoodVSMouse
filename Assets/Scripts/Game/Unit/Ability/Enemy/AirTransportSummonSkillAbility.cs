using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;

using UnityEngine;

using static UnityEditorInternal.VersionControl.ListControl;
/// <summary>
/// 航母召唤飞行单位技能
/// </summary>
public class AirTransportSummonSkillAbility : SkillAbility
{
    private int leftUseCount = 1; // 剩余使用次数
    private int castState = 0; // 施法阶段（0：正在打开舱门；1：舱门打开并且召唤怪中；2：召唤怪完毕后关闭舱门； 3：舱门已关闭完成准备结束技能切换回正常态）
    private Action SummonAction; // 召唤事件
    public AirTransportSummonSkillAbility(BaseUnit pmaster) : base(pmaster)
    {

    }

    public AirTransportSummonSkillAbility(BaseUnit pmaster, SkillAbilityInfo info) : base(pmaster, info)
    {

    }

    /// <summary>
    /// 满足释放的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetSkillCondition()
    {
        // 能量足够，且有剩余使用次数
        return (leftUseCount>0);
    }

    public override void BeforeSpell()
    {
        // 停下来，切状态，播动画
        master.SetActionState(new CastState(master));
    }

    /// <summary>
    /// 在技能期间的具体实现
    /// </summary>
    public override void OnSpelling()
    {

    }

    /// <summary>
    /// 在非技能期间
    /// </summary>
    public override void OnNoSpelling()
    {

    }

    /// <summary>
    /// 在技能期间，结束技能所需要的条件
    /// </summary>
    public override bool IsMeetCloseSpellingCondition()
    {
        return castState == 3;
    }

    public override void AfterSpell()
    {
        castState = 0;
        leftUseCount--;
        master.SetActionState(new MoveState(master));
    }

    /// <summary>
    /// 由外部调用，完成打开门阶段
    /// </summary>
    /// <returns></returns>
    public void SetFinishPreCast()
    {
        castState = 1;
        // 再来一次动作切换，这次会切成持续开门悬停召怪动作
        master.SetActionState(new CastState(master));
        // 召怪
        if(SummonAction!=null)
            SummonAction();
    }

    /// <summary>
    /// 由外部调用，完成开门召怪阶段
    /// </summary>
    public void SetFinishCast()
    {
        castState = 2;
        // 再来一次动作切换，这次会切成关门动作
        master.SetActionState(new CastState(master));
    }

    /// <summary>
    /// 由外部调用，完成关门阶段并准备结束技能
    /// </summary>
    public void SetFinishPostCast()
    {
        castState = 3;
    }

    /// <summary>
    /// 设置召怪事件
    /// </summary>
    public void SetSummonActon(Action action)
    {
        SummonAction = action;
    }

    /// <summary>
    /// 获取动作阶段
    /// </summary>
    /// <returns></returns>
    public int GetCastState()
    {
        return castState;
    }
}
