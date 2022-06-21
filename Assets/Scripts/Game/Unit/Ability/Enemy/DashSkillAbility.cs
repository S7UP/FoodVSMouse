using System.Collections;
using System.Collections.Generic;
using TMPro;

using UnityEngine;

/// <summary>
/// 忍者鼠冲锋技能
/// </summary>
public class DashSkillAbility : SkillAbility
{
    private FloatModifier addMoveSpeedModifier = new FloatModifier(300);
    private bool isDashed; // 是否已经冲过了（？）
    private MouseUnit mouseMaster;
    private int maxTime = 270; // 最大冲锋时间
    private int currentTime = 0;

    public DashSkillAbility(BaseUnit pmaster) : base(pmaster)
    {
        mouseMaster = pmaster as MouseUnit;
    }

    public DashSkillAbility(BaseUnit pmaster, SkillAbilityInfo info) : base(pmaster, info)
    {
        mouseMaster = pmaster as MouseUnit;
    }

    /// <summary>
    /// 满足释放的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetSkillCondition()
    {
        // 此前没冲过，且能量足够即可
        return !isDashed;
    }

    public override void BeforeSpell()
    {
        // 播动画
        master.SetActionState(new MoveState(master));
        // 获得300%的移动速度加成
        master.NumericBox.MoveSpeed.AddPctAddModifier(addMoveSpeedModifier);
    }

    /// <summary>
    /// 在技能期间的具体实现
    /// </summary>
    public override void OnSpelling()
    {
        currentTime++;
        // 冲锋过程中，如果即将被阻挡，那么会立即对目标造成 {攻击力*自身移动速度的标准值*25%} 的伤害
        // 若目标死于本次伤害，则会继续冲锋，否则结束冲锋
        if (mouseMaster.IsHasTarget())
        {
            BaseUnit target = mouseMaster.GetCurrentTarget();
            new DamageAction(CombatAction.ActionType.CauseDamage, mouseMaster, target, 
                mouseMaster.mCurrentAttack*TransManager.TranToStandardVelocity(mouseMaster.mCurrentMoveSpeed)*0.25f).ApplyAction();
            if (target.IsAlive())
                EndActivate();
        }
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
        return currentTime >= maxTime;
    }

    public override void AfterSpell()
    {
        isDashed = true;
        master.SetActionState(new MoveState(master));
        // 移除移速加成效果
        master.NumericBox.MoveSpeed.RemovePctAddModifier(addMoveSpeedModifier);
    }

    /// <summary>
    /// 由外界调用，获取技能是否施放过一次的信息
    /// </summary>
    /// <returns></returns>
    public bool IsDashed()
    {
        return isDashed;
    }
}
