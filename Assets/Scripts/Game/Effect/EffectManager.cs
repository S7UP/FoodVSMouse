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
        if (!u.mEffectController.IsContainEffect("HealEffect"))
        {
            BaseEffect eff = BaseEffect.CreateInstance(GameManager.Instance.GetRuntimeAnimatorController("Mouse/5/HealEffect"), null, "Heal", null, false);
            string name;
            int order;
            if (u.TryGetSpriteRenternerSorting(out name, out order))
            {
                eff.SetSpriteRendererSorting(name, order + 4);
            }
            GameController.Instance.AddEffect(eff);
            u.mEffectController.AddEffectToDict("HealEffect", eff, Vector2.zero);
        }
    }

    /// <summary>
    /// 水波特效给予对象（不重复）
    /// </summary>
    /// <param name="u"></param>
    public static void AddWaterWaveEffectToUnit(BaseUnit u, Vector2 pos)
    {
        if (!u.mEffectController.IsContainEffect("WaterWave"))
        {
            BaseEffect eff = BaseEffect.CreateInstance(GameManager.Instance.GetRuntimeAnimatorController("Effect/WaterWaveEffect"), "Appear", "Idle", "Disappear", true);
            eff.SetSpriteRendererSorting("Grid", 100);
            GameController.Instance.AddEffect(eff);
            u.mEffectController.AddEffectToDict("WaterWave", eff, pos);
        }
    }

    /// <summary>
    /// 移除水波特效
    /// </summary>
    /// <param name="u"></param>
    public static void RemoveWaterWaveEffectFromUnit(BaseUnit u)
    {
        if (u.mEffectController.IsContainEffect("WaterWave"))
        {
            // u.mEffectController.RemoveEffectFromDict("WaterWave");
            u.mEffectController.GetEffect("WaterWave").ExecuteDeath();
        }
    }

    /// <summary>
    /// 添加毒菌感染特效
    /// </summary>
    /// <param name="u"></param>
    public static void AddMushroomEffectToUnit(BaseUnit u)
    {
        if (!u.mEffectController.IsContainEffect("MushroomEffect"))
        {
            BaseEffect eff = BaseEffect.GetInstance("MushroomEffect");
            GameController.Instance.AddEffect(eff);
            u.mEffectController.AddEffectToDict("MushroomEffect", eff, Vector2.zero);
        }
    }

    /// <summary>
    /// 移除毒菌感染特效
    /// </summary>
    /// <param name="u"></param>
    public static void RemoveMushroomEffectFromUnit(BaseUnit u)
    {
        if (u.mEffectController.IsContainEffect("MushroomEffect"))
        {
            u.mEffectController.RemoveEffectFromDict("MushroomEffect");
        }
    }

    /// <summary>
    /// 添加岩浆炙烤特效
    /// </summary>
    /// <param name="u"></param>
    public static void AddLavaEffectToUnit(BaseUnit u)
    {
        if (!u.mEffectController.IsContainEffect("Lava"))
        {
            BaseEffect eff = BaseEffect.CreateInstance(GameManager.Instance.GetRuntimeAnimatorController("Effect/LavaEffect"), null, "Idle", null, true);
            string name;
            int order;
            if (u.TryGetSpriteRenternerSorting(out name, out order))
            {
                eff.SetSpriteRendererSorting(name, order + 3);
            }
            GameController.Instance.AddEffect(eff);
            u.mEffectController.AddEffectToDict("Lava", eff, 0.075f * MapManager.gridHeight * Vector2.down);
        }
    }

    /// <summary>
    /// 移除岩浆炙烤特效
    /// </summary>
    /// <param name="u"></param>
    public static void RemoveLavaEffectFromUnit(BaseUnit u)
    {
        if (u.mEffectController.IsContainEffect("Lava"))
        {
            u.mEffectController.RemoveEffectFromDict("Lava");
        }
    }

    public static BaseEffect GetFireEffect(bool isCycle)
    {
        BaseEffect e = BaseEffect.CreateInstance(GameManager.Instance.GetRuntimeAnimatorController("Effect/Fire"), "Appear", "Idle", "Disappear", isCycle);
        return e;
    }
}