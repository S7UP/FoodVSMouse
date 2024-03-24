using S7P.Numeric;

using UnityEngine;

public class ChocolateCannon : FoodUnit
{
    private static RuntimeAnimatorController Bullet_Run;

    public override void Awake()
    {
        if (Bullet_Run == null)
            Bullet_Run = GameManager.Instance.GetRuntimeAnimatorController("Food/24/Bullet");
        base.Awake();
    }

    public override void MInit()
    {
        base.MInit();
    }

    /// <summary>
    /// 根据等级表和等级来更新对应数据
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        NumericBox.Attack.SetBase((float)(attr.baseAttrbute.baseAttack + attr.valueList[mLevel]));
    }

    /// <summary>
    /// 加载技能
    /// </summary>
    public override void LoadSkillAbility()
    {
        float need = 1080;
        if (mShape >= 1)
            need = 720;

        SkillAbility.SkillAbilityInfo info = new SkillAbility.SkillAbilityInfo()
        {
            needEnergy = need,
            startEnergy = 0,
            energyRegeneration = 1.0f
        };

        CustomizationSkillAbility s = new CustomizationSkillAbility(this, info);
        // 定义
        {
            s.IsMeetSkillConditionFunc = delegate {
                return true;
            };
            s.BeforeSpellFunc = delegate
            {
                animatorController.Play("Attack");
                mAttackFlag = true;
            };
            s.OnSpellingFunc = delegate 
            {
                if(animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() > attackPercent && mAttackFlag)
                {
                    mAttackFlag = false;
                    // 产生弹体
                    GameManager.Instance.audioSourceController.PlayEffectMusic("Coblaunch");
                    CreateBullet();

                    // 二转额外再发一发
                    if (mShape >= 2)
                    {
                        CustomizationTask task = new CustomizationTask();
                        task.AddTimeTaskFunc(15);
                        task.AddOnExitAction(delegate {
                            GameManager.Instance.audioSourceController.PlayEffectMusic("Coblaunch");
                            CreateBullet();
                        });
                        taskController.AddTask(task);
                    }
                }
            };
            s.IsMeetCloseSpellingConditionFunc = delegate {
                return animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce();
            };
            s.AfterSpellFunc = delegate {
                animatorController.Play("Idle", true);
            };
        }
        skillAbilityManager.AddSkillAbility(s);
        // 与攻击速率挂钩
        {
            FloatModifier skillSpeedMod = new FloatModifier(NumericBox.AttackSpeed.Value * 100);
            s.energyRegeneration.AddPctAddModifier(skillSpeedMod);

            NumericBox.SkillSpeed.AddAfterValueChangeAction((val) => {
                s.energyRegeneration.RemovePctAddModifier(skillSpeedMod);
                skillSpeedMod.Value = NumericBox.AttackSpeed.Value * 100;
                s.energyRegeneration.AddPctAddModifier(skillSpeedMod);
            });
        }
    }

    private void CreateBullet()
    {
        float dmg = mCurrentAttack;
        AllyBullet b = AllyBullet.GetInstance(BulletStyle.Throwing, Bullet_Run, this, 0);
        b.isnDelOutOfBound = true; // 出屏不自删
        b.SetHitSoundEffect("Boom");
        b.AddHitAction(delegate {
            CreateDamageArea(b.transform.position, dmg);
        });
        PitcherManager.AddDefaultFlyTask(b, transform.position, new Vector2(MapManager.GetColumnX(7), transform.position.y), true, false);
        GameController.Instance.AddBullet(b);
    }

    private void CreateDamageArea(Vector2 pos, float dmg)
    {
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, 3, 3, "ItemCollideEnemy");
        r.isAffectMouse = true;
        r.SetInstantaneous();
        r.SetOnEnemyEnterAction((u) => {
            DamageAction d = new DamageAction(CombatAction.ActionType.BurnDamage, this, u, Mathf.Max(0.5f * u.mMaxHp * u.mBurnRate, dmg * u.NumericBox.AoeRate.TotalValue));
            // d.AddDamageType(DamageAction.DamageType.AOE);
            d.ApplyAction();
        });
        GameController.Instance.AddAreaEffectExecution(r);
    }
}
