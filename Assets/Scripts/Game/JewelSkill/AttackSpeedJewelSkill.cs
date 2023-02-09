using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 疾风宝石
/// </summary>
public class AttackSpeedJewelSkill : BaseJewelSkill
{
    public AttackSpeedJewelSkill(float maxEnergy, float startEnergy, float deltaEnergy, Dictionary<string, float[]> paramDict) : base(maxEnergy, startEnergy, deltaEnergy, paramDict)
    {

    }

    protected override void OnExecute()
    {
        List<BaseUnit> allyList = GameController.Instance.GetEachAlly();
        // 对当前每个友军施加时效性的基础攻击速度效果
        foreach (var u in allyList)
        {
            StatusManager.AddBaseAttackSpeedBuff(u, GetParamValue("attackSpeedPercent", 0), Mathf.FloorToInt(GetParamValue("t", 0) * 60));
        }
    }
}
