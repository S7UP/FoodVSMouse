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
            BaseEffect eff = BaseEffect.GetInstance("HealEffect");
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
            BaseEffect eff = BaseEffect.GetInstance("WaterWaveEffect");
            eff.SetSpriteRendererSorting("Grid", 100);
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

    /// <summary>
    /// 添加毒菌感染特效
    /// </summary>
    /// <param name="u"></param>
    public static void AddMushroomEffectToUnit(BaseUnit u)
    {
        if (!u.IsContainEffect(EffectType.MushroomEffect))
        {
            BaseEffect eff = BaseEffect.GetInstance("MushroomEffect");
            GameController.Instance.AddEffect(eff);
            u.AddEffectToDict(EffectType.MushroomEffect, eff, Vector2.zero);
        }
    }

    /// <summary>
    /// 移除毒菌感染特效
    /// </summary>
    /// <param name="u"></param>
    public static void RemoveMushroomEffectFromUnit(BaseUnit u)
    {
        if (u.IsContainEffect(EffectType.MushroomEffect))
        {
            u.RemoveEffectFromDict(EffectType.MushroomEffect);
        }
    }
}