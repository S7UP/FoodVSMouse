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
        // 判定部分
        List<BaseUnit> allyList = GameController.Instance.GetEachAlly();
        // 对当前每个友军施加时效性的无敌、减伤、回复效果。
        foreach (var u in allyList)
        {
            StatusManager.AddInvincibilityBuff(u, Mathf.FloorToInt(GetParamValue("t", 0) * 60));
            StatusManager.AddDamageRateBuff(u, 1 - GetParamValue("defence", 0)/100, Mathf.FloorToInt(GetParamValue("t", 0) * 60));
            StatusManager.AddCureBuff(null, u, GetParamValue("healPercent", 0)/100 *u.mMaxHp, 60, Mathf.FloorToInt(GetParamValue("t", 0) * 60));
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
    }
}
