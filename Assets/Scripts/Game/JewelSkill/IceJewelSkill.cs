using Environment;

using System;
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
        int timeLeft = Mathf.FloorToInt(GetParamValue("t", 0) * 60);
        int totalTime = timeLeft;

        // 发射冰河
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
                        // 施加冰冻损伤
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
            // 1.125秒（也就是冰锥弹消失后）每出新生成的敌方单位也会附加等量<冰冻损伤>
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
                    // 施加冰冻损伤
                    EnvironmentFacade.AddIceDebuff(m, GetParamValue("iceValue0", 0));
                    time += 60;
                }
            }, null);
            m.AddTask(t);
        };

        // 后续判定
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
                // 对每个被击中的目标施加冰冻损伤
                b.AddHitAction((b, u) => {
                    if(u is MouseUnit)
                        EnvironmentFacade.AddIceDebuff(u, GetParamValue("iceValue1", 0));
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
