using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ������ʯ
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
        // �������
        for (int i = 0; i < 7; i++)
        {
            CreateIceShooter(new Vector2(MapManager.GetColumnX(-1), MapManager.GetRowY(i)));
        }
    }

    /// <summary>
    /// ����һ�����ӷ�����
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
                // �������ε��ӵ�������ӵ��ܸ�����DEBUFF
                AllyBullet b = AllyBullet.GetInstance(BulletStyle.Laser, null, null, 0);
                b.transform.position = item.transform.position;
                b.isIgnoreHeight = true;
                b.Hide();
                b.SetStandardVelocity(48);
                b.SetRotate(Vector2.right);
                b.isnKillSelf = true; // ���в��Ի�
                // ��ÿ�������е�Ŀ��ʩ�Ӽ����������Ч��
                b.AddHitAction((b, u) => {
                    u.AddStatusAbility(new FrozenSlowStatusAbility(-GetParamValue("decMovePercent", 0), u, Mathf.FloorToInt(GetParamValue("t", 0) * 60)));
                    StatusManager.AddFinalAttackSpeedDeBuff(u, -GetParamValue("decAttackSpeedPercent", 0), Mathf.FloorToInt(GetParamValue("t", 0) * 60));
                });
                GameController.Instance.AddBullet(b);
                // ������Ч
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
