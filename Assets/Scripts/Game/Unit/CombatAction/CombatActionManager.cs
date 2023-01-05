using UnityEngine;

public class CombatActionManager
{
    /// <summary>
    /// ��Ŀ�����һ�λҽ������˺�
    /// </summary>
    /// <param name="unit"></param>
    /// <returns>��Ŀ����ɵ�ʵ���˺�</returns>
    public static float BombBurnDamageUnit(BaseUnit creator, BaseUnit unit, float damage)
    {
        float hp0 = unit.GetCurrentHp();
        // ���Ŀ���Ƿ��ֹը����ɱЧ������������ܵ��ض��Ļҽ��˺����������Ŀ�굱ǰ����ֵ�Ļҽ��˺�
        if (unit.NumericBox.GetBoolNumericValue(StringManager.IgnoreBombInstantKill))
            new BombDamageAction(CombatAction.ActionType.CauseDamage, creator, unit, damage).ApplyAction();
        else
            new BombDamageAction(CombatAction.ActionType.CauseDamage, creator, unit, unit.mCurrentHp).ApplyAction();
        float hp1 = Mathf.Max(0, unit.GetCurrentHp());
        return hp0 - hp1;
    }
}
