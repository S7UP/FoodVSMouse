using UnityEngine;
/// <summary>
/// ��Ч���������ṩһЩ��̬����
/// </summary>
public class EffectManager
{
    /// <summary>
    /// ����Ч��������󣨲��ظ���
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
    /// ˮ����Ч������󣨲��ظ���
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
    /// �Ƴ�ˮ����Ч
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