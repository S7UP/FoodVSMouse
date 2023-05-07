using System.Collections.Generic;
/// <summary>
/// 伤害行动
/// </summary>
public class DamageAction : CombatAction
{
    public enum DamageType
    {
        Default, // 默认伤害类型
        BombBurn, // 爆破灰烬来源
        AOE, // 范围伤害来源
        Rebound, // 反弹伤害来源
    }

    private List<DamageType> DamageTypeList = new List<DamageType>(); // 自身伤害类型的标签，若为空则默认为Default
    public float DamageValue { get; set; }
    public float RealCauseValue; // 实际造成伤害（在对目标造成伤害后获取，否则为0）


    public DamageAction(ActionType actionType, BaseUnit creator, BaseUnit target, float damageValue) : base(actionType, creator, target)
    {
        DamageValue = damageValue;
        RealCauseValue = 0;
    }

    /// <summary>
    /// 为自身添加一种伤害类型的标签
    /// </summary>
    /// <param name="type"></param>
    public void AddDamageType(DamageType type)
    {
        if (!DamageTypeList.Contains(type))
            DamageTypeList.Add(type);
    }

    /// <summary>
    /// 自身是否是某种类型的伤害
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool IsDamageType(DamageType type)
    {
        if (type is DamageType.Default && (DamageTypeList.Count == 0 || DamageTypeList.Contains(DamageType.Default)))
            return true;
        else
            return DamageTypeList.Contains(type);
    }

    public List<DamageType> GetDamageTypeList()
    {
        return DamageTypeList;
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
        }
    }

    //应用伤害
    public override void ApplyAction()
    {
        // 如果伤害小于等于0那么它不会生效
        if (DamageValue <= 0)
            return;
        PreProcess();
        if (Creator != null)
            Creator.TriggerActionPoint(ActionPointType.WhenCauseDamage, this);
        if (Target != null)
        {
            Target.TriggerActionPoint(ActionPointType.WhenReceiveDamage, this);
            // 默认的接收伤害的方法
            if(Target.IsUseDefaultRecieveDamageActionMethod())
                UnitManager.ReceiveDamageAction(Target, this);
        }
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
        }
    }
}