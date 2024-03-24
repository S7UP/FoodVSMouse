using System.Collections.Generic;

using UnityEngine;

public class WaterPipeGun : BaseWeapons
{
    private static RuntimeAnimatorController Bullet_RuntimeAnimatorController;

    private static List<int> maxFrontAttackCountList = new List<int> { 1, 2, 3 };
    private static List<int> maxBackAttackCountList = new List<int> { 2, 2, 3 };

    private int maxFrontAttackCount;
    private int maxBackAttackCount;
    private int maxAttackCount;
    private int currentAttackCount; // 当前攻击计数器
    private float endAttackPercent; // 发射最后一发子弹时的动画播放百分比
    private List<float> attackPercentList = new List<float>();
    private float dmgValue; // 单发子弹伤害

    public override void Awake()
    {
        if (Bullet_RuntimeAnimatorController == null)
        {
            Bullet_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/6/Bullet");
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
        // 根据携带水管卡片的转职情况来计算前后发射数
        int shape = 0;
        int level = 0;
        BaseCardBuilder builder = GameController.Instance.mCardController.GetCardBuilderByType(FoodNameTypeMap.WaterPipe);
        if(builder != null)
        {
            shape = builder.mShape;
            level = builder.mLevel;
        }
        maxFrontAttackCount = maxFrontAttackCountList[shape];
        maxBackAttackCount = maxBackAttackCountList[shape];
        maxAttackCount = Mathf.Max(maxFrontAttackCount, maxBackAttackCount);
        endAttackPercent = 0.90f;
        attackPercentList.Clear();
        for (int i = 0; i < maxAttackCount; i++)
        {
            attackPercentList.Add(attackPercent + (endAttackPercent - attackPercent) * i / (maxAttackCount - 1));
        }
        currentAttackCount = 0;
        // 获取单发子弹伤害
        dmgValue = FoodManager.GetAttack(FoodNameTypeMap.WaterPipe, level, shape);
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
        // 前攻击
        if (currentAttackCount < maxFrontAttackCount)
        {
            AllyBullet b = AllyBullet.GetInstance(BulletStyle.Normal, Bullet_RuntimeAnimatorController, master, master.mCurrentAttack/10* dmgValue);
            b.SetHitSoundEffect("Splat" + GameManager.Instance.rand.Next(0, 3));
            b.transform.position = transform.position;
            b.SetSpriteLocalPosition(master.GetSpriteLocalPosition());
            b.SetStandardVelocity(24);
            b.SetRotate(Vector2.right);
            b.AddHitAction((b, u) => {
                if (u is MouseUnit)
                {
                    MouseUnit m = u as MouseUnit;
                    if (!m.IsBoss())
                    {
                        new DamageAction(CombatAction.ActionType.CauseDamage, master, u, 0.01f * u.GetCurrentHp()).ApplyAction();
                    }
                }
            });
            GameController.Instance.AddBullet(b);
        }
        // 后攻击
        if (currentAttackCount < maxBackAttackCount)
        {
            AllyBullet b = AllyBullet.GetInstance(BulletStyle.Normal, Bullet_RuntimeAnimatorController, master, master.mCurrentAttack/10* dmgValue);
            b.SetHitSoundEffect("Splat" + GameManager.Instance.rand.Next(0, 3));
            b.transform.position = transform.position;
            b.SetSpriteLocalPosition(master.GetSpriteLocalPosition());
            b.SetStandardVelocity(24);
            b.SetRotate(Vector2.left);
            b.AddHitAction((b, u) => {
                if (u is MouseUnit)
                {
                    MouseUnit m = u as MouseUnit;
                    if (!m.IsBoss())
                    {
                        new DamageAction(CombatAction.ActionType.RealDamage, master, u, 0.01f * u.GetCurrentHp()).ApplyAction();
                    }
                }
            });
            GameController.Instance.AddBullet(b);
            GameManager.Instance.audioSourceController.PlayEffectMusic("Throw" + GameManager.Instance.rand.Next(0, 2));
        }
    }
}
