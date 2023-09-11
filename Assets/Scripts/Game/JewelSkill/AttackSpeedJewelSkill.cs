using System;
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
        int timeLeft = Mathf.FloorToInt(GetParamValue("t", 0) * 60);

        List<BaseUnit> allyList = GameController.Instance.GetEachAlly();
        // 对当前每个友军施加时效性的基础攻击速度效果与控制免疫效果
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

        // 后续判定
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
