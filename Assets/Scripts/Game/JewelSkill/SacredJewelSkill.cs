using System;
using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 神圣宝石
/// </summary>
public class SacredJewelSkill : BaseJewelSkill
{
    private static RuntimeAnimatorController Shooter_RuntimeAnimatorController;

    public SacredJewelSkill(float maxEnergy, float startEnergy, float deltaEnergy, Dictionary<string, float[]> paramDict) : base(maxEnergy, startEnergy, deltaEnergy, paramDict)
    {
        if (Shooter_RuntimeAnimatorController == null)
        {
            Shooter_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Jewel/1/Shooter");
        }
    }

    protected override void OnExecute()
    {
        int timeLeft = Mathf.FloorToInt(GetParamValue("t", 0) * 60);
        // 判定部分
        List<BaseUnit> allyList = GameController.Instance.GetEachAlly();
        // 对当前每个友军施加时效性的无敌、减伤、回复效果。
        foreach (var u in allyList)
        {
            StatusManager.AddInvincibilityBuff(u, timeLeft);
            StatusManager.AddDamageRateBuff(u, 1 - GetParamValue("defence", 0)/100, timeLeft);
            StatusManager.AddCureBuff(null, u, GetParamValue("healPercent", 0)/100 *u.mMaxHp, 60, timeLeft);
        }

        // 特效部分
        CustomizationItem item = CustomizationItem.GetInstance(new Vector2(MapManager.GetColumnX(4), MapManager.GetRowY(3)), Shooter_RuntimeAnimatorController);
        CustomizationTask t = new CustomizationTask();
        t.AddOnEnterAction(delegate {
            item.animatorController.Play("Execute");
        });
        t.AddTaskFunc(delegate {
            if (item.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                return true;
            }
            return false;
        });
        t.AddOnExitAction(delegate {
            item.ExecuteDeath();
        });
        item.AddTask(t);
        GameController.Instance.AddItem(item);

        Action<BaseCardBuilder> action = (c) =>
        {
            FoodUnit u = c.mProduct;
            StatusManager.AddInvincibilityBuff(u, timeLeft);
            StatusManager.AddDamageRateBuff(u, 1 - GetParamValue("defence", 0) / 100, timeLeft);
            StatusManager.AddCureBuff(null, u, GetParamValue("healPercent", 0) / 100 * u.mMaxHp, 60, timeLeft);
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
