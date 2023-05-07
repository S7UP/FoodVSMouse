using System;
using System.Collections.Generic;
/// <summary>
/// 处理游戏单位的管理者
/// </summary>
public class UnitManager
{
    /// <summary>
    /// 检测双方单位能否被互相阻挡
    /// </summary>
    /// <param name="u1"></param>
    /// <param name="u2"></param>
    /// <returns></returns>
    public static bool CanBlock(BaseUnit u1, BaseUnit u2)
    {
        if (u1 == null || u2 == null)
            return false;

        foreach (var func in u1.CanBlockFuncList)
        {
            if (!func(u1, u2))
                return false;
        }
        foreach (var func in u2.CanBlockFuncList)
        {
            if (!func(u2, u1))
                return false;
        }
        return u1.CanBlock(u2) && u2.CanBlock(u1);
    }

    /// <summary>
    /// 检测单位与子弹能否发生被弹判定
    /// </summary>
    /// <returns></returns>
    public static bool CanBulletHit(BaseUnit u, BaseBullet b)
    {
        if (u == null || b == null)
            return false;

        // 只要有一个不满足条件就返回false
        foreach (var func in u.CanHitFuncList)
        {
            if (!func(u, b))
                return false;
        }
        foreach (var func in b.CanHitFuncList)
        {
            if (!func(b, u))
                return false;
        }

        return u.CanHit(b) && b.CanHit(u);
    }


    /// <summary>
    /// u1能否把u2作为攻击目标选择
    /// </summary>
    /// <param name="u1">可以为空，若为空则仅用来判定能否作为攻击目标</param>
    /// <param name="u2">不能为空，若为空一定返回false</param>
    /// <returns></returns>
    public static bool CanBeSelectedAsTarget(BaseUnit u1, BaseUnit u2)
    {
        if (u2 == null)
            return false;

        foreach (var func in u2.CanBeSelectedAsTargetFuncList)
        {
            if (!func(u2, u1))
                return false;
        }
        return u2.CanBeSelectedAsTarget(u1);
    }

    /// <summary>
    /// 获取经一定条件筛选后的单位表
    /// </summary>
    /// <param name="unitList"></param>
    /// <param name="ConditionFunc"></param>
    /// <returns></returns>
    public static List<BaseUnit> GetList(List<BaseUnit> unitList, Func<BaseUnit, bool> ConditionFunc)
    {
        List<BaseUnit> list = new List<BaseUnit>();
        if (unitList == null)
            return list;
        foreach (var unit in unitList)
        {
            if (ConditionFunc == null || ConditionFunc(unit))
                list.Add(unit);
        }
        return list;
    }


    /// <summary>
    /// 目标是否滞空（弹起）
    /// </summary>
    /// <returns></returns>
    public static bool IsFlying(BaseUnit unit)
    {
        return (unit.NumericBox.IntDict.ContainsKey(StringManager.Flying) && unit.NumericBox.IntDict[StringManager.Flying].Value > 0);
    }

    /// <summary>
    /// 处决某个单位
    /// </summary>
    /// <param name="unit"></param>
    public static DamageAction Execute(BaseUnit master, BaseUnit unit)
    {
        DamageAction action = new DamageAction(CombatAction.ActionType.RealDamage, master, unit, unit.mCurrentHp);
        action.ApplyAction();
        return action;
    }

    /// <summary>
    /// 触发单位全部内伤
    /// </summary>
    /// <param name="unit"></param>
    public static void TriggerRecordDamage(BaseUnit unit)
    {
        new DamageAction(CombatAction.ActionType.RecordDamage, null, unit, unit.mRecordDamageComponent.recordDamage).ApplyAction();
    }

    /// <summary>
    /// 使目标接收伤害
    /// </summary>
    /// <param name="combatAction"></param>
    public static void ReceiveDamageAction(BaseUnit u, DamageAction damageAction)
    {
        // 根据伤害类型来接收伤害
        if (damageAction.mActionType == CombatAction.ActionType.BurnDamage)
        {
            if (damageAction.IsDamageType(DamageAction.DamageType.BombBurn))
                damageAction.RealCauseValue = u.OnBombBurnDamage(damageAction.DamageValue);
            else
                damageAction.RealCauseValue = u.OnBurnDamage(damageAction.DamageValue);
        }
        else if (damageAction.mActionType == CombatAction.ActionType.RealDamage)
            damageAction.RealCauseValue = u.OnRealDamage(damageAction.DamageValue);
        else if (damageAction.mActionType == CombatAction.ActionType.RecordDamage)
            damageAction.RealCauseValue = u.OnRecordDamage(damageAction.DamageValue);
        else
        {
            if (damageAction.IsDamageType(DamageAction.DamageType.AOE))
                damageAction.RealCauseValue = u.OnAoeDamage(damageAction.DamageValue);
            else
                damageAction.RealCauseValue = u.OnDamage(damageAction.DamageValue);
        }
    }

    /// <summary>
    /// 获取满足特殊条件并按X升序排列的单位表（靠左的在表前面）
    /// </summary>
    /// <param name="ConditionFunc">特殊条件</param>
    /// <param name="ori_list">原表</param>
    /// <returns></returns>
    public static List<BaseUnit> GetSortedListByXPos(Func<BaseUnit, bool> ConditionFunc, List<BaseUnit> ori_list)
    {
        List<BaseUnit> list = new List<BaseUnit>();
        foreach (var u in ori_list)
        {
            if (ConditionFunc(u))
                list.Add(u);
        }
        list.Sort((u1, u2) => { return u1.transform.position.x.CompareTo(u2.transform.position.x); });
        return list;
    }
}
