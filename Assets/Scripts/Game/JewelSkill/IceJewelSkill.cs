using Environment;

using System;
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
        int timeLeft = Mathf.FloorToInt(GetParamValue("t", 0) * 60);
        int totalTime = timeLeft;

        // �������
        for (int i = 0; i < 7; i++)
        {
            CreateIceShooter(new Vector2(MapManager.GetColumnX(-1), MapManager.GetRowY(i)));
        }

        foreach (var u in GameController.Instance.GetEachEnemy())
        {
            if(u is MouseUnit)
            {
                MouseUnit m = u as MouseUnit;
                if (m.IsBoss())
                    continue;
                m.AddStatusAbility(new SlowStatusAbility(-GetParamValue("decMovePercent", 0), m, timeLeft));
                StatusManager.AddFinalAttackSpeedDeBuff(m, -GetParamValue("decAttackSpeedPercent", 0), timeLeft);
                StatusManager.AddFinalAttackDeBuff(m, -GetParamValue("decAttackSpeedPercent", 0), timeLeft);
                CustomizationTask t = new CustomizationTask();
                int time = 60;
                t.AddTimeTaskFunc(timeLeft, null, delegate {
                    time--;
                    if (time <= 0)
                    {
                        // ʩ�ӱ�������
                        EnvironmentFacade.AddIceDebuff(m, GetParamValue("iceValue0", 0));
                        time += 60;
                    }
                }, null);
                m.taskController.AddTask(t);
            }
        }

        Action<MouseUnit> action = (m) => {
            if (m.IsBoss())
                return;
            // 1.125�루Ҳ���Ǳ�׶����ʧ��ÿ�������ɵĵз���λҲ�ḽ�ӵ���<��������>
            if(timeLeft <= totalTime - 67)
            {
                EnvironmentFacade.AddIceDebuff(m, GetParamValue("iceValue1", 0));
            }

            m.AddStatusAbility(new SlowStatusAbility(-GetParamValue("decMovePercent", 0), m, timeLeft));
            StatusManager.AddFinalAttackSpeedDeBuff(m, -GetParamValue("decAttackSpeedPercent", 0), timeLeft);
            StatusManager.AddFinalAttackDeBuff(m, -GetParamValue("decAttackSpeedPercent", 0), timeLeft);
            CustomizationTask t = new CustomizationTask();
            int time = 60;
            t.AddTimeTaskFunc(timeLeft, null, delegate {
                time--;
                if(time <= 0)
                {
                    // ʩ�ӱ�������
                    EnvironmentFacade.AddIceDebuff(m, GetParamValue("iceValue0", 0));
                    time += 60;
                }
            }, null);
            m.AddTask(t);
        };

        // �����ж�
        GameController.Instance.AddTasker(
            //Action InitAction, 
            delegate {
                GameController.Instance.mMouseFactory.AddProcessAction(action);
            },
            //Action UpdateAction, 
            delegate {
                timeLeft--;
            },
            //Func<bool> EndCondition, 
            delegate { return timeLeft <= 0; },
            //Action EndEvent
            delegate {
                GameController.Instance.mMouseFactory.RemoveProcessAction(action);
            }
            );
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
                // ��ÿ�������е�Ŀ��ʩ�ӱ�������
                b.AddHitAction((b, u) => {
                    if(u is MouseUnit)
                        EnvironmentFacade.AddIceDebuff(u, GetParamValue("iceValue1", 0));
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
