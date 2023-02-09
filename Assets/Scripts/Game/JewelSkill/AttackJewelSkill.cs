using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 强力宝石
/// </summary>
public class AttackJewelSkill : BaseJewelSkill
{
    public AttackJewelSkill(float maxEnergy, float startEnergy, float deltaEnergy, Dictionary<string, float[]> paramDict) :base(maxEnergy, startEnergy, deltaEnergy, paramDict)
    {

    }

    protected override void OnExecute()
    {
        List<BaseUnit> allyList = GameController.Instance.GetEachAlly();
        // 对当前每个友军施加时效性的基础攻击速度效果
        foreach (var u in allyList)
        {
            StatusManager.AddBaseAttackBuff(u, GetParamValue("attackPercent", 0), Mathf.FloorToInt(GetParamValue("t", 0)*60));
        }
    }
}
