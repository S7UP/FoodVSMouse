
using System.Collections.Generic;

using UnityEngine;

public class BunGun : BaseWeapons
{
    private static RuntimeAnimatorController Bullet_RuntimeAnimatorController;
    private static int[] attackCountArray = new int[] { 1, 2, 4 };
    private static float[] speed_rate = new float[] { 1, 1, 2f };

    private int maxAttackCount;
    private int currentAttackCount; // 当前攻击计数器
    private List<float> attackPercentList;
    private float dmgValue;

    private int shape;

    public override void Awake()
    {
        if (Bullet_RuntimeAnimatorController == null)
        {
            Bullet_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Weapons/5/Bullet");
        }
            
        base.Awake();
    }

    public override void MInit()
    {
        shape = 0;
        base.MInit();
        // 根据携带小笼包卡片的转职情况来计算发射数
        int level = 0;
        BaseCardBuilder builder = GameController.Instance.mCardController.GetCardBuilderByType(FoodNameTypeMap.Bun);
        if (builder != null)
        {
            shape = builder.mShape;
            level = builder.mLevel;
        }
        // 根据转职情况来计算前后发射数
        maxAttackCount = attackCountArray[shape];

        attackPercentList = new List<float>();
        if (shape == 0)
        {
            attackPercentList.Add(attackPercent);
        }
        else
        {
            float end = 1.0f;
            for (int i = 0; i < maxAttackCount; i++)
                attackPercentList.Add(attackPercent + (end - attackPercent) * i / (maxAttackCount - 1));
        }

        currentAttackCount = 0;
        // 获取单发子弹伤害
        dmgValue = FoodManager.GetAttack(FoodNameTypeMap.Bun, level, shape);
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
        currentAttackCount = 0;
        base.AfterGeneralAttack();
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
        AllyBullet b = AllyBullet.GetInstance(BulletStyle.Normal, Bullet_RuntimeAnimatorController, master, master.mCurrentAttack/10 * dmgValue);
        b.transform.position = transform.position;
        b.SetStandardVelocity(24 * speed_rate[shape]);
        b.SetRotate(Vector2.right);
        b.SetHitSoundEffect("Splat" + GameManager.Instance.rand.Next(0, 3));
        b.AddHitAction((b, u) => {
            if (u is MouseUnit)
            {
                MouseUnit m = u as MouseUnit;
                if (!m.IsBoss())
                {
                    new DamageAction(CombatAction.ActionType.CauseDamage, master, u, 0.02f * u.GetCurrentHp()).ApplyAction();
                }
            }
        });
        GameController.Instance.AddBullet(b);
    }
}
