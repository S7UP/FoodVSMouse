using System;
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
        int timeLeft = Mathf.FloorToInt(GetParamValue("t", 0) * 60);
        List<BaseUnit> allyList = GameController.Instance.GetEachAlly();
        // �Ե�ǰÿ���Ѿ�ʩ��ʱЧ�ԵĻ��������ٶ�Ч��
        foreach (var u in allyList)
        {
            StatusManager.AddBaseAttackBuff(u, GetParamValue("attackPercent", 0), timeLeft);
        }

        Action<BaseCardBuilder> action = (c) =>
        {
            FoodUnit u = c.mProduct;
            StatusManager.AddBaseAttackBuff(u, GetParamValue("attackPercent", 0), timeLeft);
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
