using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 冰冻宝石
/// </summary>
public class IceJewelSkill : BaseJewelSkill
{
    private static RuntimeAnimatorController Shooter_RuntimeAnimatorController;
    private static RuntimeAnimatorController IceEffect_RuntimeAnimatorController;

    public IceJewelSkill(float maxEnergy, float startEnergy, float deltaEnergy, Dictionary<string, float[]> paramDict) : base(maxEnergy, startEnergy, deltaEnergy, paramDict)
    {
        if (Shooter_RuntimeAnimatorController == null)
        {
            Shooter_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Jewel/0/Shooter");
            IceEffect_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Jewel/0/IceEffect");
        }
    }

    protected override void OnExecute()
    {
        // 发射冰河
        for (int i = 0; i < 7; i++)
        {
            CreateIceShooter(new Vector2(MapManager.GetColumnX(-1), MapManager.GetRowY(i)));
        }
    }

    /// <summary>
    /// 产生一个冰河发射器
    /// </summary>
    private void CreateIceShooter(Vector2 pos)
    {
        CustomizationItem item = CustomizationItem.GetInstance(pos, Shooter_RuntimeAnimatorController);
        CustomizationTask t = new CustomizationTask();
        t.AddOnEnterAction(delegate {
            item.animatorController.Play("Execute");
        });
        t.AddTaskFunc(delegate {
            if (item.animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= 0.6f)
            {
                // 产生隐形的子弹，这个子弹能给怪上DEBUFF
                AllyBullet b = AllyBullet.GetInstance(BulletStyle.Laser, null, null, 0);
                b.transform.position = item.transform.position;
                b.isIgnoreHeight = true;
                b.Hide();
                b.SetStandardVelocity(48);
                b.SetRotate(Vector2.right);
                b.isnKillSelf = true; // 击中不自毁
                // 对每个被击中的目标施加减速与减攻速效果
                b.AddHitAction((b, u) => {
                    u.AddStatusAbility(new FrozenSlowStatusAbility(-GetParamValue("decMovePercent", 0), u, Mathf.FloorToInt(GetParamValue("t", 0) * 60)));
                    StatusManager.AddFinalAttackSpeedDeBuff(u, -GetParamValue("decAttackSpeedPercent", 0), Mathf.FloorToInt(GetParamValue("t", 0) * 60));
                });
                GameController.Instance.AddBullet(b);
                // 产生特效
                BaseEffect e = BaseEffect.CreateInstance(IceEffect_RuntimeAnimatorController, null, "Execute", null, false);
                e.transform.position = item.transform.position + Vector3.right*MapManager.gridWidth;
                GameController.Instance.AddEffect(e);
                return true;
            }
            return false;
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
