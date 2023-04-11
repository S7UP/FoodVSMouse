using System.Collections.Generic;

using UnityEngine;

public class V5Gun : BaseWeapons
{
    private static RuntimeAnimatorController Bullet_RuntimeAnimatorController;
    private const int maxAttackCount = 4;
    private int currentAttackCount; // 当前攻击计数器
    private static List<float> attackPercentList = new List<float>();

    public override void Awake()
    {
        if (Bullet_RuntimeAnimatorController == null)
        {
            Bullet_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Weapons/2/Bullet");
            for (int i = 0; i < 4; i++)
            {
                attackPercentList.Add(0.5f + 0.4f*((float)i/3));
            }
        }
            
        base.Awake();
    }

    /// <summary>
    /// 加载技能，此处仅加载普通攻击，具体技能加载实现请在子类中重写
    /// </summary>
    public override void LoadSkillAbility()
    {
        // 普通攻击
        weaponsGeneralAttackSkillAbility = new WeaponsGeneralAttackSkillAbility(this, new SkillAbility.SkillAbilityInfo()
        {
            name = "普通攻击",
            needEnergy = 57,
            startEnergy = 57,
            energyRegeneration = 1.0f,
            skillType = SkillAbility.Type.GeneralAttack,
            isExclusive = true,
            canActiveInDeathState = false,
            priority = 0
        });
        skillAbilityManager.AddSkillAbility(weaponsGeneralAttackSkillAbility);
    }
    public override void MInit()
    {
        base.MInit();
        currentAttackCount = 0;
    }

    /// <summary>
    /// 判断是否有有效的攻击目标
    /// </summary>
    /// <returns></returns>
    public override bool IsHasTarget()
    {
        return GameController.Instance.CheckRowCanAttack(master, GetRowIndex());
    }

    /// <summary>
    /// 退出普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetEndGeneralAttackCondition()
    {
        return animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce();
    }

    /// <summary>
    /// 退出普通攻击后要做的事
    /// </summary>
    public override void AfterGeneralAttack()
    {
        base.AfterGeneralAttack();
        currentAttackCount = 0;
    }

    /// <summary>
    /// 普通攻击期间
    /// </summary>
    public override void OnGeneralAttack()
    {
        // 伤害判定帧应当执行判定
        if (IsDamageJudgment())
        {
            ExecuteDamage();
            currentAttackCount++;
        }
    }

    /// <summary>
    /// 是否为伤害判定时刻（近战攻击为打出实际伤害，远程攻击为确定发射弹体）
    /// </summary>
    /// <returns></returns>
    public override bool IsDamageJudgment()
    {
        return currentAttackCount < attackPercentList.Count && animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= attackPercentList[currentAttackCount];
    }

    /// <summary>
    /// 执行具体的攻击，位于伤害判定为真之后
    /// </summary>
    public override void ExecuteDamage()
    {
        GameManager.Instance.audioSourceManager.PlayEffectMusic("Throw" + GameManager.Instance.rand.Next(0, 2));
        if (currentAttackCount < maxAttackCount)
        {
            AllyBullet b = AllyBullet.GetInstance(BulletStyle.NoStrengthenNormal, Bullet_RuntimeAnimatorController, master, 0);
            b.SetHitSoundEffect("Splat" + GameManager.Instance.rand.Next(0, 3));
            float ori_dmg = master.mCurrentAttack;
            b.SetStandardVelocity(36);
            b.SetRotate(Vector2.right);
            b.AddHitAction((b, u) => { 
                if(u is MouseUnit)
                {
                    MouseUnit m = u as MouseUnit;
                    float dmg = ori_dmg;
                    if (!m.IsBoss())
                    {
                        dmg += 0.03f * u.GetCurrentHp();
                    }
                    new DamageAction(CombatAction.ActionType.RealDamage, master, u, dmg).ApplyAction();
                }
            });
            GameController.Instance.AddBullet(b);
        }
    }
}
