using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��Ч���������ṩһЩ��̬����
/// </summary>
public class EffectManager
{
    /// <summary>
    /// �������Ч��������󣨲��ظ���
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