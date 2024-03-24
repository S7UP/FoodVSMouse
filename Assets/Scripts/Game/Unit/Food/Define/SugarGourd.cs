
using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 糖葫芦炮弹
/// </summary>
public class SugarGourd : FoodUnit
{
    private static readonly int[] countArray = { 1, 1, 2 }; // 根据转职情况来确定发射几颗子弹
    private int currentAttackCount;
    private int maxAttackCount;
    private List<float> attackPercentList;
    private float endAttackPercent; // 发射最后一发子弹时的动画播放百分比

    public override void MInit()
    {
        base.MInit();
        currentAttackCount = 0;
        maxAttackCount = countArray[mShape];
        endAttackPercent = 0.7f;
        attackPercentList = new List<float>();
        if (maxAttackCount <= 1)
        {
            attackPercentList.Add(endAttackPercent);
        }
        else
        {
            for (int i = 0; i < maxAttackCount; i++)
            {
                attackPercentList.Add(attackPercent + (endAttackPercent - attackPercent) * i / (maxAttackCount - 1));
            }
        }
        currentAttackCount = 0;
    }

    public override void MDestory()
    {
        base.MDestory();
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
        // 只要有空中单位并且可以被选取即可攻击
        foreach (var item in GameController.Instance.GetEachEnemy())
        {
            if (item.GetHeight() == 1 && UnitManager.CanBeSelectedAsTarget(this, item) && item.IsAlive())
                return true;
        }
        return false;
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
        return (currentAttackCount < maxAttackCount && animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= attackPercentList[currentAttackCount]);
    }

    /// <summary>
    /// 执行具体的攻击，位于伤害判定为真之后
    /// </summary>
    public override void ExecuteDamage()
    {
        GameManager.Instance.audioSourceController.PlayEffectMusic("Swing");
        TrackingBullets b = (TrackingBullets)GameController.Instance.CreateBullet(this, transform.position + Vector3.up * MapManager.gridHeight/2, Vector2.up, BulletStyle.SugarGourd);
        b.SetHitSoundEffect("Splat"+GameManager.Instance.rand.Next(0, 3));
        b.animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Bullet/"+ ((int)BulletStyle.SugarGourd)+"/"+mShape);
        b.SetHeight(1); // 设置高度为对空高度
        b.SetSearchEnemyEnable(true); // 开启索敌模式
        b.SetCompareFunc(BulletCompareFunc);
        b.SetVelocityChangeEvent(TransManager.TranToVelocity(12), TransManager.TranToVelocity(48), 90);
        b.SetDamage(mCurrentAttack);
        // b.AddHitAction(BulletHitAction); // 设置击中后的事件
    }

    ////////////////////////////////////////////////////////////以下是私有方法///////////////////////////////////////////////////////////////////////

    /// <summary>
    /// 子弹索敌比较逻辑
    /// </summary>
    /// <param name="currentTarget">当前目标</param>
    /// <param name="compareTarget">被比较的目标</param>
    /// <returns>若为true则compareTarget会取代currentTarget成为当前target</returns>
    private bool BulletCompareFunc(BaseUnit currentTarget, BaseUnit compareTarget)
    {
        if (compareTarget == null || !compareTarget.IsAlive() || !UnitManager.CanBeSelectedAsTarget(this, compareTarget) || compareTarget.GetHeight()!=1)
            return false;
        if (currentTarget == null || !currentTarget.IsAlive() || !UnitManager.CanBeSelectedAsTarget(this, compareTarget) || currentTarget.GetHeight() != 1)
            return true;

        if (compareTarget.mMaxHp == currentTarget.mMaxHp)
            return (compareTarget.transform.position.x < currentTarget.transform.position.x);
        else
            return compareTarget.mMaxHp > currentTarget.mMaxHp;
    }
}
