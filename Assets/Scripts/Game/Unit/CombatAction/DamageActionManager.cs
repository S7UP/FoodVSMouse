using UnityEngine;

public class DamageActionManager
{
    /// <summary>
    /// 复制一个DamageAction，但是伤害来源与伤害目标不一样
    /// </summary>
    /// <param name="d"></param>
    /// <returns></returns>
    public static DamageAction Copy(DamageAction d, BaseUnit creator, BaseUnit target)
    {
        DamageAction d2 = new DamageAction(d.mActionType, creator, target, d.DamageValue);
        foreach (var type in d.GetDamageTypeList())
        {
            d2.AddDamageType(type);
        }
        return d2;
    }
}
