public class BurnManager
{
    /// <summary>
    /// 获取目标灰烬抗性
    /// </summary>
    /// <param name="u"></param>
    public static float GetBurnDefence(BaseUnit u)
    {
        return 1 - u.mBurnRate;
    }

    /// <summary>
    /// 对目标造成一次灰烬效果
    /// </summary>
    /// <param name="creator"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static DamageAction BurnDamage(BaseUnit creator, BaseUnit target)
    {
        return BurnDamage(creator, target, 1);
    }

    /// <summary>
    /// 对目标造成一次rate比率的灰烬效果
    /// </summary>
    /// <param name="creator"></param>
    /// <param name="target"></param>
    /// <param name="rate"></param>
    /// <returns></returns>
    public static DamageAction BurnDamage(BaseUnit creator, BaseUnit target, float rate)
    {
        DamageAction action = new DamageAction(CombatAction.ActionType.BurnDamage, creator, target, target.mMaxHp*rate);
        action.AddDamageType(DamageAction.DamageType.BombBurn); // 标记为灰烬来源
        action.ApplyAction();
        return action;
    }

    /// <summary>
    /// 灰烬处决
    /// </summary>
    /// <param name="creator"></param>
    /// <param name="target"></param>
    /// <param name="rate"></param>
    /// <returns></returns>
    public static DamageAction BurnInstanceKill(BaseUnit creator, BaseUnit target)
    {
        DamageAction action = new DamageAction(CombatAction.ActionType.BurnDamage, creator, target, target.mMaxHp);
        // action.AddDamageType(DamageAction.DamageType.BombBurn); // 标记为灰烬来源
        action.ApplyAction();
        return action;
    }
}
