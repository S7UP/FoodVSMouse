using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ��ʥ��ʯ
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
        // �ж�����
        List<BaseUnit> allyList = GameController.Instance.GetEachAlly();
        // �Ե�ǰÿ���Ѿ�ʩ��ʱЧ�Ե��޵С����ˡ��ظ�Ч����
        foreach (var u in allyList)
        {
            StatusManager.AddInvincibilityBuff(u, Mathf.FloorToInt(GetParamValue("t", 0) * 60));
            StatusManager.AddDamageRateBuff(u, 1 - GetParamValue("defence", 0)/100, Mathf.FloorToInt(GetParamValue("t", 0) * 60));
            StatusManager.AddCureBuff(null, u, GetParamValue("healPercent", 0)/100 *u.mMaxHp, 60, Mathf.FloorToInt(GetParamValue("t", 0) * 60));
        }

        // ��Ч����
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
