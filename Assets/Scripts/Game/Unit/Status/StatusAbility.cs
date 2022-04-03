using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 状态效果能力，挂在单位上执行特定的持续能力效果（BUFF类型）
/// </summary>
public class StatusAbility : AbilityEntity
{
    public StatusAbilityManager statusAbilityManager { get; set; }
    private bool isDisableEffect { get; set; } // 是否禁用此BUFF产生的效果（不影响持续时间的流逝）
    public bool canActiveInDeathState; // 在死亡状态下能否继续触发
    public FloatNumeric totalTime = new FloatNumeric(); // 总持续时间
    public float leftTime; // 当前剩余时间

    public StatusAbility()
    {

    }

    public StatusAbility(BaseUnit pmaster, float time) : base(pmaster)
    {
        totalTime.SetBase(time);
        leftTime = totalTime.Value;
    }

    public void SetEffectEnable(bool isEnable)
    {
        isDisableEffect = !isEnable;
        if (isEnable)
            OnEnableEffect();
        else
            OnDisableEffect();
    }

    /// <summary>
    /// 检测能否在死亡时使用
    /// </summary>
    /// <returns></returns>
    private bool IsActiveInDeath()
    {
        return master.IsAlive() || canActiveInDeathState;
    }

    // 以下为需要子类实现细节时Override的方法
    /// <summary>
    /// 在效果生效前（BUFF挂上一瞬间最早做的事）
    /// </summary>
    public virtual void BeforeEffect()
    {

    }


    /// <summary>
    /// 在触发禁用效果时的事件
    /// </summary>
    public virtual void OnDisableEffect()
    {
        
    }

    /// <summary>
    /// 在触发启用效果时的事件
    /// </summary>
    public virtual void OnEnableEffect()
    {

    }

    /// <summary>
    /// 在启用效果期间
    /// </summary>
    public virtual void OnEffecting()
    {

    }

    /// <summary>
    /// 在非启用效果期间
    /// </summary>
    public virtual void OnNotEffecting()
    {

    }

    /// <summary>
    /// 结束的条件，与持续时间是或关系！
    /// </summary>
    /// <returns></returns>
    public virtual bool IsMeetingEndCondition()
    {
        return false;
    }

    /// <summary>
    /// BUFF结束时要做的事
    /// </summary>
    public virtual void AfterEffect()
    {

    }

    // 以下为继承并实现父类的方法，行使主要逻辑！
    /// <summary>
    /// 启用该效果
    /// </summary>
    public override void ActivateAbility()
    {
        // 死亡检测，如果目标在死亡状态且这个技能不能在死亡期间释放则直接退出
        if (!IsActiveInDeath())
            return;
        BeforeEffect();
        SetEffectEnable(true);
    }

    /// <summary>
    /// 每帧随着主人更新
    /// </summary>
    public override void Update()
    {
        // 死亡检测，如果目标在死亡状态且这个技能不能在死亡期间释放则直接退出
        if (!IsActiveInDeath())
            return;

        if(leftTime <= 0 || IsMeetingEndCondition())
        {
            EndActivate();
            return;
        }

        if (!isDisableEffect)
            OnEffecting();
        else
            OnNotEffecting();
        leftTime -= 1;
    }

    /// <summary>
    /// 结束效果意味着BUFF的消失，所以应当通知其依附的管理器移除BUFF
    /// </summary>
    public override void EndActivate()
    {
        AfterEffect();
        statusAbilityManager.RemoveStatusAbility(this);
    }

    /// <summary>
    /// 是否达到了激活的条件
    /// </summary>
    /// <returns></returns>
    public override bool CanActive()
    {
        return !isDisableEffect;
    }

    //创建能力执行体
    public override AbilityExecution CreateAbilityExecution()
    {
        return null;
    }

    //应用能力效果
    public override void ApplyAbilityEffect(BaseUnit targetEntity)
    {
        //应用能力效果
    }
}
