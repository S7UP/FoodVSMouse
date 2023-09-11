using System;
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
        int timeLeft = Mathf.FloorToInt(GetParamValue("t", 0) * 60);

        List<BaseUnit> allyList = GameController.Instance.GetEachAlly();
        // �Ե�ǰÿ���Ѿ�ʩ��ʱЧ�ԵĻ��������ٶ�Ч�����������Ч��
        foreach (var u in allyList)
        {
            StatusManager.AddBaseAttackSpeedBuff(u, GetParamValue("attackSpeedPercent", 0), timeLeft);
            StatusManager.AddIgnoreSettleDownBuff(u, timeLeft);
        }

        Action<BaseCardBuilder> action = (c) =>
        {
            FoodUnit u = c.mProduct;
            StatusManager.AddBaseAttackSpeedBuff(u, GetParamValue("attackSpeedPercent", 0), timeLeft);
            StatusManager.AddIgnoreSettleDownBuff(u, timeLeft);
        };

        // �����ж�
        GameController.Instance.AddTasker(
            //Action InitAction, 
            delegate {
                foreach (var c in GameController.Instance.mCardController.mCardBuilderList)
                    c.AddAfterBuildAction(action);
            },
            //Action UpdateAction, 
            delegate {
                timeLeft--;
            },
            //Func<bool> EndCondition, 
            delegate { return timeLeft <= 0; },
            //Action EndEvent
            delegate {
                foreach (var c in GameController.Instance.mCardController.mCardBuilderList)
                    c.RemoveAfterBuildAction(action);
            }
            );
    }
}
