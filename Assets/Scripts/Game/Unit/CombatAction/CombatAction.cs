/// <summary>
/// 战斗行动概念，造成伤害、治疗单位、赋给效果等属于战斗行动，需要继承自CombatAction
/// 战斗行动由战斗实体主动发起，包含本次行动所需要用到的所有数据，并且会触发一系列行动点事件
/// </summary>
public class CombatAction
{
    /// <summary>
    /// 动作类型
    /// </summary>
    public enum ActionType
    {
        SpellSkill,  // 技能效果
        CauseDamage, // 造成伤害
        ReboundDamage, // 反弹伤害
        RecordDamage, // 标记伤害
        GiveCure,    // 回复
        AssignEffect,// 提供效果
        GiveShield,  // 提供护盾
        BurnDamage,   // 灰烬伤害
        RealDamage, // 真实伤害
    }

    public CombatAction(ActionType actionType, BaseUnit creator, BaseUnit target)
    {
        mActionType = actionType;
        Creator = creator;
        Target = target;
    }

    public ActionType mActionType { get; set; }
    public BaseUnit Creator { get; set; }
    public BaseUnit Target { get; set; }

    public virtual void ApplyAction()
    {

    }
}