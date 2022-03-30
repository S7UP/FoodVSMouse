using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealSkillAbility : SkillAbility
{
    public EnemyHealSkillAbility(BaseUnit pmaster) : base(pmaster)
    {

    }

    public EnemyHealSkillAbility(BaseUnit pmaster, SkillAbilityInfo info) : base(pmaster, info)
    {

    }

    /// <summary>
    /// 满足释放的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetSkillCondition()
    {
        // 只要能量足够就可以释放
        return true;
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
        // 加上能量回复的条件，表明在引导期间
        //if(enableEnergyRegeneration)
    }

    /// <summary>
    /// 在技能期间，结束技能所需要的条件
    /// </summary>
    public override bool IsMeetCloseSpellingCondition()
    {
        return true;
    }

    public override void AfterSpell()
    {
        // 加血！
        float add = (master as HealMouse).GetHealValue();
        foreach (var unit in GameController.Instance.GetEachEnemy())
        {
            // 检测治疗有没有超过上限
            float realAdd = unit.GetRealCureValue(add);
            float leftAdd = realAdd - unit.mMaxHp + unit.mCurrentHp;
            if (leftAdd > 0)
            {
                // 当治疗溢出时，剩余的治疗量转化为护盾
                new CureAction(CombatAction.ActionType.GiveCure, master, unit, unit.mMaxHp - unit.mCurrentHp).ApplyAction();
                new ShieldAction(CombatAction.ActionType.GiveShield, master, unit, leftAdd).ApplyAction();
                // Debug.Log("当前目标已损失生命值："+(unit.mMaxHp - unit.mCurrentHp)+", 即将回复生命值："+(unit.mMaxHp - unit.mCurrentHp)+", 即将获得护盾值："+(leftAdd));
            }
            else
            {
                // 未溢出时全额转化为治疗
                new CureAction(CombatAction.ActionType.GiveCure, master, unit, realAdd).ApplyAction();
                // Debug.Log("当前目标已损失生命值：" + (unit.mMaxHp - unit.mCurrentHp) + ", 即将回复生命值：" + realAdd);
            }
                

        }
        enableEnergyRegeneration = false;
        master.SetActionState(new MoveState(master));
    }
}
