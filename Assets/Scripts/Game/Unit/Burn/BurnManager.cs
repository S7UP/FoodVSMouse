public class BurnManager
{
    /// <summary>
    /// 获取目标灰烬抗性
    /// </summary>
    /// <param name="u"></param>
    public static float GetBurnDefence(BaseUnit u)
    {
        return 1 - u.NumericBox.BurnRate.TotalValue;
    }

    /// <summary>
    /// 对目标造成一次灰烬效果
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
