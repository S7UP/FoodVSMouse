using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 双向水管具体实现
/// </summary>
public class WaterPipeFoodUnit : FoodUnit
{
    private static RuntimeAnimatorController Bullet_RuntimeAnimatorController;

    private static List<int> maxFrontAttackCountList = new List<int>{ 1, 2, 3 };
    private static List<int> maxBackAttackCountList = new List<int> { 2, 2, 3 };

    private int maxFrontAttackCount;
    private int maxBackAttackCount;
    private int maxAttackCount;
    private int currentAttackCount; // 当前攻击计数器
    private float endAttackPercent; // 发射最后一发子弹时的动画播放百分比
    private List<float> attackPercentList;

    public override void Awake()
    {
        if (Bullet_RuntimeAnimatorController == null)
            Bullet_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/6/Bullet");
        base.Awake();
    }

    public override void MInit()
    {
        base.MInit();
        // 根据转职情况来计算前后发射数
        maxFrontAttackCount = maxFrontAttackCountList[mShape];
        maxBackAttackCount = maxBackAttackCountList[mShape];
        maxAttackCount = Mathf.Max(maxFrontAttackCount, maxBackAttackCount);
        endAttackPercent = 0.90f;
        attackPercentList = new List<float>();
        for (int i = 0; i < maxAttackCount; i++)
        {
            attackPercentList.Add(attackPercent + (endAttackPercent - attackPercent)*i/(maxAttackCount-1));
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
        // 前攻击
        if (currentAttackCount < maxFrontAttackCount)
        {
            AllyBullet b = AllyBullet.GetInstance(BulletStyle.Normal, Bullet_RuntimeAnimatorController, this, mCurrentAttack);
            b.transform.position = transform.position + 0.25f*MapManager.gridWidth*Vector3.left;
            b.SetSpriteLocalPosition(GetSpriteLocalPosition() + Vector2.up * 0.1f);
            b.SetStandardVelocity(24);
            b.SetRotate(Vector2.right);
            b.SetHitSoundEffect("Splat" + GameManager.Instance.rand.Next(0, 3));
            GameController.Instance.AddBullet(b);
        }
        // 后攻击
        if(currentAttackCount < maxBackAttackCount)
        {
            AllyBullet b = AllyBullet.GetInstance(BulletStyle.Normal, Bullet_RuntimeAnimatorController, this, mCurrentAttack);
            b.transform.position = transform.position + 0.25f * MapManager.gridWidth * Vector3.right;
            b.SetSpriteLocalPosition(GetSpriteLocalPosition());
            b.SetStandardVelocity(24);
            b.SetRotate(Vector2.left);
            b.SetHitSoundEffect("Splat" + GameManager.Instance.rand.Next(0, 3));
            GameController.Instance.AddBullet(b);
            GameManager.Instance.audioSourceController.PlayEffectMusic("Throw"+GameManager.Instance.rand.Next(0, 2));
        }
    }
}
