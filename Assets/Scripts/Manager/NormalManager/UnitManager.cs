using System;
using System.Collections.Generic;
/// <summary>
/// ������Ϸ��λ�Ĺ�����
/// </summary>
public class UnitManager
{
    /// <summary>
    /// ���˫����λ�ܷ񱻻����赲
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
    /// ��ⵥλ���ӵ��ܷ��������ж�
    /// </summary>
    /// <returns></returns>
    public static bool CanBulletHit(BaseUnit u, BaseBullet b)
    {
        if (u == null || b == null)
            return false;

        // ֻҪ��һ�������������ͷ���false
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
    /// u1�ܷ��u2��Ϊ����Ŀ��ѡ��
    /// </summary>
    /// <param name="u1">����Ϊ�գ���Ϊ����������ж��ܷ���Ϊ����Ŀ��</param>
    /// <param name="u2">����Ϊ�գ���Ϊ��һ������false</param>
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
    /// ��ȡ��һ������ɸѡ��ĵ�λ��
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
    /// Ŀ���Ƿ��Ϳգ�����
    /// </summary>
    /// <returns></returns>
    public static bool IsFlying(BaseUnit unit)
    {
        return (unit.NumericBox.IntDict.ContainsKey(StringManager.Flying) && unit.NumericBox.IntDict[StringManager.Flying].Value > 0);
    }

    /// <summary>
    /// ����ĳ����λ
    /// </summary>
    /// <param name="unit"></param>
    public static DamageAction Execute(BaseUnit master, BaseUnit unit)
    {
        DamageAction action = new DamageAction(CombatAction.ActionType.RealDamage, master, unit, unit.mCurrentHp);
        action.ApplyAction();
        return action;
    }

    /// <summary>
    /// ������λȫ������
    /// </summary>
    /// <param name="unit"></param>
    public static void TriggerRecordDamage(BaseUnit unit)
    {
        new DamageAction(CombatAction.ActionType.RecordDamage, null, unit, unit.mRecordDamageComponent.recordDamage).ApplyAction();
    }

    /// <summary>
    /// ʹĿ������˺�
    /// </summary>
    /// <param name="combatAction"></param>
    public static void ReceiveDamageAction(BaseUnit u, DamageAction damageAction)
    {
        // �����˺������������˺�
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
    /// ��ȡ����������������X�������еĵ�λ��������ڱ�ǰ�棩
    /// </summary>
    /// <param name="ConditionFunc">��������</param>
    /// <param name="ori_list">ԭ��</param>
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
