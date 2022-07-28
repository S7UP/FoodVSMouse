using UnityEngine;
/// <summary>
/// 特效管理器，提供一些静态方法
/// </summary>
public class EffectManager
{
    /// <summary>
    /// 治疗效果给予对象（不重复）
    /// </summary>
    /// <param name="u"></param>
    public static void AddHealEffectToUnit(BaseUnit u)
    {
        if (!u.IsContainEffect(EffectType.HealEffect))
        {
            BaseEffect eff = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Effect/HealEffect").GetComponent<BaseEffect>();
            GameController.Instance.AddEffect(eff);
            u.AddEffectToDict(EffectType.HealEffect, eff, Vector2.zero);
        }
    }

    /// <summary>
    /// 水波特效给予对象（不重复）
    /// </summary>
    /// <param name="u"></param>
    public static void AddWaterWaveEffectToUnit(BaseUnit u)
    {
        if (!u.IsContainEffect(EffectType.WaterWave))
        {
            BaseEffect eff = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Effect/WaterWaveEffect").GetComponent<BaseEffect>();
            GameController.Instance.AddEffect(eff);
            u.AddEffectToDict(EffectType.WaterWave, eff, 0.075f*MapManager.gridHeight*Vector2.down);
        }
    }

    /// <summary>
    /// 移除水波特效
    /// </summary>
    /// <param name="u"></param>
    public static void RemoveWaterWaveEffectFromUnit(BaseUnit u)
    {
        if (u.IsContainEffect(EffectType.WaterWave))
        {
            u.RemoveEffectFromDict(EffectType.WaterWave);
        }
    }
}