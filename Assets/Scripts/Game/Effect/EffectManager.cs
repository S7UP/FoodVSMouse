using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 特效管理器，提供一些静态方法
/// </summary>
public class EffectManager
{
    /// <summary>
    /// 添加治疗效果给予对象（不重复）
    /// </summary>
    /// <param name="u"></param>
    public static void AddHealEffectToUnit(BaseUnit u)
    {
        if (!u.IsContainEffect(EffectType.HealEffect))
        {
            BaseEffect eff = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Effect/HealEffect").GetComponent<BaseEffect>();
            GameController.Instance.AddEffect(eff);
            u.AddEffectToDict(EffectType.HealEffect, eff);
        }
    }
}