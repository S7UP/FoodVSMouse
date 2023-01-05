using UnityEngine;

public class CombatActionManager
{
    /// <summary>
    /// 对目标进行一次灰烬爆破伤害
    /// </summary>
    /// <param name="unit"></param>
    /// <returns>对目标造成的实际伤害</returns>
    public static float BombBurnDamageUnit(BaseUnit creator, BaseUnit unit, float damage)
    {
        float hp0 = unit.GetCurrentHp();
        // 检测目标是否防止炸弹秒杀效果，如果防则受到特定的灰烬伤害，否则造成目标当前生命值的灰烬伤害
        if (unit.NumericBox.GetBoolNumericValue(StringManager.IgnoreBombInstantKill))
            new BombDamageAction(CombatAction.ActionType.CauseDamage, creator, unit, damage).ApplyAction();
        else
            new BombDamageAction(CombatAction.ActionType.CauseDamage, creator, unit, unit.mCurrentHp).ApplyAction();
        float hp1 = Mathf.Max(0, unit.GetCurrentHp());
        return hp0 - hp1;
    }
}
