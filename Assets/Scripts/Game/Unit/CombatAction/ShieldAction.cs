using System.Collections;
using System.Collections.Generic;

using UnityEngine;


/// <summary>
/// 回复行动
/// </summary>
public class ShieldAction : CombatAction
{
    //治疗数值数值
    public float Value { get; set; }

    public ShieldAction(ActionType actionType, BaseUnit creator, BaseUnit target, float shieldValue) : base(actionType, creator, target)
    {
        Value = shieldValue;
    }


    //前置处理
    private void PreProcess()
    {
        //触发 输出护盾前 行动点
        Creator.TriggerActionPoint(ActionPointType.PreCauseShield, this);
        //触发 接受护盾前 行动点
        Target.TriggerActionPoint(ActionPointType.PreReceiveShield, this);
    }

    //应用治疗
    public override void ApplyAction()
    {
        PreProcess();
        Target.ReceiveShield(this);
        PostProcess();
    }

    //后置处理
    private void PostProcess()
    {
        //触发 输出护盾后 行动点
        Creator.TriggerActionPoint(ActionPointType.PostCauseShield, this);
        //触发 接受护盾后 行动点
        Target.TriggerActionPoint(ActionPointType.PostReceiveShield, this);
    }
}