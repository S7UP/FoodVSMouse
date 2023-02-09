using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ���籦ʯ
/// </summary>
public class AttackSpeedJewelSkill : BaseJewelSkill
{
    public AttackSpeedJewelSkill(float maxEnergy, float startEnergy, float deltaEnergy, Dictionary<string, float[]> paramDict) : base(maxEnergy, startEnergy, deltaEnergy, paramDict)
    {

    }

    protected override void OnExecute()
    {
        List<BaseUnit> allyList = GameController.Instance.GetEachAlly();
        // �Ե�ǰÿ���Ѿ�ʩ��ʱЧ�ԵĻ��������ٶ�Ч��
        foreach (var u in allyList)
        {
            StatusManager.AddBaseAttackSpeedBuff(u, GetParamValue("attackSpeedPercent", 0), Mathf.FloorToInt(GetParamValue("t", 0) * 60));
        }
    }
}
