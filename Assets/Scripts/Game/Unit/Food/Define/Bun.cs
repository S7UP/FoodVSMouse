using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 小笼包
/// </summary>
public class Bun : FoodUnit
{
    private static RuntimeAnimatorController Bullet_RuntimeAnimatorController;
    private static int[] attackCountArray = new int[] { 1, 2, 4 };
    private static float[] speed_rate = new float[] { 1, 1, 2f };

    private int maxAttackCount;
    private int currentAttackCount; // 当前攻击计数器
    private List<float> attackPercentList;

    public override void Awake()
    {
        if (Bullet_RuntimeAnimatorController == null)
            Bullet_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/21/Bullet");
        base.Awake();
    }

    public override void MInit()
    {
        base.MInit();
        // 根据转职情况来计算前后发射数
        maxAttackCount = attackCountArray[mShape];

        attackPercentList = new List<float>();
        if (mShape == 0)
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
    }

    /// <summary>
    /// 根据等级表和等级来更新对应数据
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        NumericBox.Attack.SetBase((float)(attr.baseAttrbute.baseAttack + attr.valueList[mLevel]));
    }

    /// <summary>
    /// 判断是否有有效的攻击目标
    /// </summary>
    /// <returns></returns>
    protected override bool IsHasTarget()
    {
        return GameController.Instance.CheckRowCanAttack(this, GetRowIndex());
    }

    /// <summary>
    /// 是否满足普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        // 发现目标即可
        return IsHasTarget();
    }

    /// <summary>
    /// 进入普通攻击动画状态
    /// </summary>
    public override void BeforeGeneralAttack()
    {
        // 切换为攻击动画贴图
        SetActionState(new AttackState(this));
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
        SetActionState(new IdleState(this));
    }

    /// <summary>
    /// 是否为伤害判定时刻（近战攻击为打出实际伤害，远程攻击为确定发射弹体）
    /// </summary>
    /// <returns></returns>
    public override bool IsDamageJudgment()
    {
        return currentAttackCount<attackPercentList.Count && animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= attackPercentList[currentAttackCount];
    }

    /// <summary>
    /// 执行具体的攻击，位于伤害判定为真之后
    /// </summary>
    public override void ExecuteDamage()
    {
        AllyBullet b = AllyBullet.GetInstance(BulletStyle.Normal, Bullet_RuntimeAnimatorController, this, mCurrentAttack);
        b.transform.position = transform.position;
        b.SetSpriteLocalPosition(GetSpriteLocalPosition() + Vector2.up * 0.2f);
        b.SetStandardVelocity(24 * speed_rate[mShape]);
        b.SetRotate(Vector2.right);
        b.SetHitSoundEffect("Splat" + GameManager.Instance.rand.Next(0, 3));
        GameController.Instance.AddBullet(b);
    }
}
