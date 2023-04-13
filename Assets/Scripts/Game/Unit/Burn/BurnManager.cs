public class BurnManager
{
    /// <summary>
    /// ��ȡĿ��ҽ�����
    /// </summary>
    /// <param name="u"></param>
    public static float GetBurnDefence(BaseUnit u)
    {
        return 1 - u.NumericBox.BurnRate.TotalValue;
    }

    /// <summary>
    /// ��Ŀ�����һ�λҽ�Ч��
    /// </summary>
    /// <param name="creator"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static DamageAction BurnDamage(BaseUnit creator, BaseUnit target)
    {
        DamageAction action = new DamageAction(CombatAction.ActionType.BurnDamage, creator, target, target.mMaxHp* target.NumericBox.BurnRate.TotalValue);
        action.ApplyAction();
        return action;
    }
}
