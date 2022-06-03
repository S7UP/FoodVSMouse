using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnDamageAction : CombatAction
{
    //伤害数值
    public float DamageValue { get; set; }

    public BurnDamageAction(ActionType actionType, BaseUnit creator, BaseUnit target, float damageValue) : base(actionType, creator, target)
    {
        DamageValue = damageValue;
    }

    //前置处理
    private void PreProcess()
    {
        //触发 造成伤害前 行动点
        if (Creator != null)
        {
            Creator.TriggerActionPoint(ActionPointType.PreCauseDamage, this);
        }
        //触发 承受伤害前 行动点
        if (Target != null)
        {
            Target.TriggerActionPoint(ActionPointType.PreReceiveDamage, this);
            Target.TriggerActionPoint(ActionPointType.PreReceiveBurnDamage, this);
        }
    }

    //应用伤害
    public override void ApplyAction()
    {
        PreProcess();
        Target.ReceiveBurnDamage(this);
        PostProcess();
    }

    //后置处理
    private void PostProcess()
    {
        //触发 造成伤害后 行动点
        if (Creator != null)
        {
            Creator.TriggerActionPoint(ActionPointType.PostCauseDamage, this);
        }
        //触发 承受伤害后 行动点
        if (Target != null)
        {
            Target.TriggerActionPoint(ActionPointType.PostReceiveDamage, this);
            Target.TriggerActionPoint(ActionPointType.PostReceiveBurnDamage, this);
        }
    }
}
