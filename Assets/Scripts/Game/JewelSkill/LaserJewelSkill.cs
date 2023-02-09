using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 激光宝石
/// </summary>
public class LaserJewelSkill : BaseJewelSkill
{
    private static RuntimeAnimatorController Shooter_RuntimeAnimatorController;
    private static RuntimeAnimatorController LaserEffect_RuntimeAnimatorController;

    public LaserJewelSkill(float maxEnergy, float startEnergy, float deltaEnergy, Dictionary<string, float[]> paramDict) : base(maxEnergy, startEnergy, deltaEnergy, paramDict)
    {
        if (Shooter_RuntimeAnimatorController == null)
        {
            Shooter_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Jewel/3/Shooter");
            LaserEffect_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Jewel/3/LaserEffect");
        }
    }

    protected override void OnExecute()
    {
        List<BaseUnit> enemyList = GameController.Instance.GetEachEnemy();
        // 对当前每个敌军施加晕眩效果
        foreach (var u in enemyList)
        {
            u.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(u, Mathf.FloorToInt(GetParamValue("t", 0) * 60), false));
        }
        // 发射激光
        for (int i = 0; i < 7; i++)
        {
            CreateLaserShooter(new Vector2(MapManager.GetColumnX(-1), MapManager.GetRowY(i)));
        }
    }

    /// <summary>
    /// 产生一个激光发射器
    /// </summary>
    private void CreateLaserShooter(Vector2 pos)
    {
        CustomizationItem item = CustomizationItem.GetInstance(pos, Shooter_RuntimeAnimatorController);
        CustomizationTask t = new CustomizationTask();
        t.OnEnterFunc = delegate {
            item.animatorController.Play("Execute");
        };
        t.AddTaskFunc(delegate {
            if (item.animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= 0.6f)
            {
                // 产生激光子弹
                AllyBullet b = AllyBullet.GetInstance(BulletStyle.Laser, LaserEffect_RuntimeAnimatorController, null, 0);
                b.isIgnoreHeight = true;
                b.transform.position = item.transform.position;
                b.SetStandardVelocity(48);
                b.SetRotate(Vector2.right);
                b.isnKillSelf = true; // 击中不自毁
                GameController.Instance.AddBullet(b);
                // 添加灰烬判定区域
                RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(b.transform.position, 1, 1, "ItemCollideEnemy");
                r.isAffectMouse = true;
                r.SetOnEnemyEnterAction((u) => {
                    CombatActionManager.BombBurnDamageUnit(null, u, GetParamValue("dmg", 0));
                });
                {
                    CustomizationTask t = new CustomizationTask();
                    t.AddTaskFunc(delegate {
                        if (b.IsAlive())
                        {
                            r.transform.position = b.transform.position;
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    });
                    t.OnExitFunc = delegate
                    {
                        r.MDestory();
                    };
                    r.AddTask(t);
                }
                GameController.Instance.AddAreaEffectExecution(r);
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
        t.OnExitFunc = delegate {
            item.ExecuteDeath();
        };
        item.AddTask(t);
        GameController.Instance.AddItem(item);
    }
}
