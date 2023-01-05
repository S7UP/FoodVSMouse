using UnityEngine;
using System;
using System.Collections.Generic;
/// <summary>
/// 章鱼烧
/// </summary>
public class Takoyaki : FoodUnit
{
    private static RuntimeAnimatorController Bullet_RuntimeAnimatorController;
    private float hpRate;
    private float addDamage;
    private int maxAttackCount;
    private int currentAttackCount; // 当前攻击计数器
    private float endAttackPercent; // 发射最后一发子弹时的动画播放百分比
    private List<float> attackPercentList;


    public override void Awake()
    {
        if(Bullet_RuntimeAnimatorController == null)
            Bullet_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/13/Bullet");
        base.Awake();
    }

    public override void MInit()
    {
        base.MInit();
        hpRate = 0.75f; // 受到伤害增幅度时的血线
        if(mShape == 1)
        {
            addDamage = 0.5f;
        }else if(mShape == 2)
        {
            addDamage = 1.5f;
        }
        else
        {
            addDamage = 0;
        }

        maxAttackCount = 2;
        endAttackPercent = 0.769f;
        attackPercentList = new List<float>();
        for (int i = 0; i < maxAttackCount; i++)
        {
            attackPercentList.Add(attackPercent + (endAttackPercent - attackPercent) * i / (maxAttackCount - 1));
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
        foreach (var unit in GameController.Instance.GetEachEnemy())
        {
            if (UnitManager.CanBeSelectedAsTarget(this, unit))
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
        return currentAttackCount < attackPercentList.Count && animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= attackPercentList[currentAttackCount];
    }

    /// <summary>
    /// 执行具体的攻击，位于伤害判定为真之后
    /// </summary>
    public override void ExecuteDamage()
    {
        CreateBullet();
    }

    /// <summary>
    /// 产生一个跟踪回旋镖
    /// </summary>
    private void CreateBullet()
    {
        float dmg = mCurrentAttack;
        AllyBullet b = AllyBullet.GetInstance(Bullet_RuntimeAnimatorController, this, dmg);
        b.isNavi = false;
        b.isIgnoreHeight = true; // 攻击时无视高度
        b.transform.position = transform.position;
        b.SetStandardVelocity(36);
        b.SetRotate(Vector2.right);
        b.AddHitAction((b, u) => {
            if(mShape > 0 && u.GetHeathPercent() <= hpRate)
                new DamageAction(CombatAction.ActionType.CauseDamage, this, u, addDamage*dmg).ApplyAction();
        });
        GameController.Instance.AddBullet(b);
        // 以下任务的变量
        Func<BaseBullet, BaseUnit, bool> canHitFunc = null; // 只允许击中目标的引用
        float angleDeltaRate = 0;
        int aliveTime = 3 * 60;
        // 添加追踪任务
        TaskManager.AddTrackAbility(b,
                // Func < BaseBullet, BaseUnit > FindTargetFunc
                (bullet) =>
                {
                    // 找生命值最低的可被选取的敌人
                    float minHp = float.MaxValue;
                    BaseUnit targetUnit = null;
                    foreach (var unit in GameController.Instance.GetEachEnemy())
                    {
                        if (UnitManager.CanBeSelectedAsTarget(this, unit) && UnitManager.CanBulletHit(unit, b))
                        {
                            if (unit.mCurrentHp < minHp)
                            {
                                minHp = unit.mCurrentHp;
                                targetUnit = unit;
                            }
                        }
                    }
                    // 添加只能击中被选取目标的效果
                    if (targetUnit != null)
                    {
                        canHitFunc = (b1, u) => { return targetUnit == u; };
                        bullet.AddCanHitFunc(canHitFunc);
                    }
                    return targetUnit;
                },
                // Func<BaseBullet, BaseUnit, bool> InValidFunc,
                (bullet, unit) =>
                {
                    bool isInValid = !unit.IsValid() || !UnitManager.CanBeSelectedAsTarget(this, unit) || !UnitManager.CanBulletHit(unit, b);
                    if (isInValid)
                    {
                        bullet.RemoveCanHitFunc(canHitFunc);
                    }
                    return isInValid;
                },
                // Action < BaseBullet, BaseUnit > TrackAction,
                (bullet, unit) =>
                {
                    angleDeltaRate = Mathf.Min(1, angleDeltaRate + 0.0025f);
                    Vector2 currentRotate = bullet.GetRotate();
                    Vector2 targetRotate = (unit.transform.position - bullet.transform.position).normalized; // 计算得出最终需要达到的方向
                    float dAngle = Vector2.Angle(currentRotate, targetRotate); // 算出二者的夹角（一定是正的，取值范围是0~180）
                    float sign = Mathf.Sign(currentRotate.x * targetRotate.y - targetRotate.x * currentRotate.y); // 通过叉乘来确定角度差是正的还是负的
                    dAngle = dAngle * sign;
                    bullet.SetRotate(bullet.mAngle + angleDeltaRate * dAngle);
                    aliveTime--;
                },
                // Action < BaseBullet > NoTargetAction,
                (bullet) =>
                {
                    angleDeltaRate = 0;
                },
                // Func<BaseBullet, BaseUnit, bool> ExitConditionFunc);
                (bullet, unit) =>
                {
                    // 自毁
                    bool isDestory = (aliveTime <= 0);
                    if (isDestory) 
                    {
                        bullet.KillThis();
                    }
                    return isDestory;
                });
        // 添加一个自旋任务
        TaskManager.AddSpinAbility(b, 6);
    }
}
