public class BurnManager
{
    /// <summary>
    /// ��ȡĿ��ҽ�����
    /// </summary>
    /// <param name="u"></param>
    public static float GetBurnDefence(BaseUnit u)
    {
        return 1 - u.mBurnRate;
    }

    /// <summary>
    /// ��Ŀ�����һ�λҽ�Ч��
    /// </summary>
    /// <param name="creator"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static DamageAction BurnDamage(BaseUnit creator, BaseUnit target)
    {
        return BurnDamage(creator, target, 1);
    }

    /// <summary>
    /// ��Ŀ�����һ��rate���ʵĻҽ�Ч��
    /// </summary>
    /// <param name="creator"></param>
    /// <param name="target"></param>
    /// <param name="rate"></param>
    /// <returns></returns>
    public static DamageAction BurnDamage(BaseUnit creator, BaseUnit target, float rate)
    {
        DamageAction action = new DamageAction(CombatAction.ActionType.BurnDamage, creator, target, target.mMaxHp*rate);
        action.AddDamageType(DamageAction.DamageType.BombBurn); // ���Ϊ�ҽ���Դ
        action.ApplyAction();
        return action;
    }

    /// <summary>
    /// �ҽ�����
    /// </summary>
    /// <param name="creator"></param>
    /// <param name="target"></param>
    /// <param name="rate"></param>
    /// <returns></returns>
    public static DamageAction BurnInstanceKill(BaseUnit creator, BaseUnit target)
    {
        DamageAction action = new DamageAction(CombatAction.ActionType.BurnDamage, creator, target, target.mMaxHp);
        // action.AddDamageType(DamageAction.DamageType.BombBurn); // ���Ϊ�ҽ���Դ
        action.ApplyAction();
        return action;
    }
}
