using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ǿ����ʯ
/// </summary>
public class AttackJewelSkill : BaseJewelSkill
{
    public AttackJewelSkill(float maxEnergy, float startEnergy, float deltaEnergy, Dictionary<string, float[]> paramDict) :base(maxEnergy, startEnergy, deltaEnergy, paramDict)
    {

    }

    protected override void OnExecute()
    {
        List<BaseUnit> allyList = GameController.Instance.GetEachAlly();
        // �Ե�ǰÿ���Ѿ�ʩ��ʱЧ�ԵĻ��������ٶ�Ч��
        foreach (var u in allyList)
        {
            StatusManager.AddBaseAttackBuff(u, GetParamValue("attackPercent", 0), Mathf.FloorToInt(GetParamValue("t", 0)*60));
        }
    }
}
