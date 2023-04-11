using System;
using System.Collections.Generic;

using S7P.Numeric;

using UnityEngine;
/// <summary>
/// 热狗大炮
/// </summary>
public class HotDog : FoodUnit
{
    private static RuntimeAnimatorController BulletRuntimeAnimatorController;

    private float airUnitDamageRate; // 对空中目标伤害倍率
    private float airAoeDamageRate; // 对空中范围伤害倍率
    private float groundUnitDamageRate; // 对地面目标伤害倍率
    private int airSlowTime; // 对空减速时间
    private int groundSlowTime; // 对地减速时间
    private Vector2 targetPosition;
    private BaseUnit airTarget; // 当前被选为目标的空中单位
    private BaseUnit groundTarget; // 当前被选为目标的地面单位
    private int attackCount; // 连续攻击次数
    private int attackLeft; // 剩余攻击次数
    private static List<float> attackPercentList = new List<float>() { 0.5f, 0.75f };

    public override void Awake()
    {
        if (BulletRuntimeAnimatorController == null)
        {
            BulletRuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/41/Bullet");
        }
        base.Awake();
    }

    public override void MInit()
    {
        base.MInit();
        // 根据转职情况来确定炮弹伤害比率
        switch (mShape)
        {
            case 1:
                airUnitDamageRate = 2.66f;
                attackCount = 1;
                break;
            case 2:
                airUnitDamageRate = 2.66f;
                attackCount = 2;
                break;
            default:
                airUnitDamageRate = 2.0f;
                attackCount = 1;
                break;
        }
        attackLeft = attackCount;
        targetPosition = Vector2.zero;
        groundUnitDamageRate = 1.0f;
        airAoeDamageRate = 0.2f;
        airSlowTime = 180;
        groundSlowTime = 30;
        airTarget = null;
        groundTarget = null;
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
        FindTarget();
        return GetAirTarget()!=null || GetGroundTarget()!=null;
    }

    /// <summary>
    /// 索敌方式
    /// </summary>
    private void FindTarget()
    {
        // 清空当前索敌
        airTarget = null;
        groundTarget = null;

        // 先获取当前行的所有敌方单位,然后选择自身右侧的敌方单位
        List<BaseUnit> unitList = new List<BaseUnit>();
        foreach (var unit in GameController.Instance.GetSpecificRowEnemyList(GetRowIndex()))
        {
            if(unit.transform.position.x > transform.position.x && UnitManager.CanBeSelectedAsTarget(this, unit))
            {
                unitList.Add(unit);
            }
        }
        
        // 取集合A与B中的目标
        foreach (var m in unitList)
        {
            if(m.GetHeight() == 1 && (GetAirTarget() == null || m.transform.position.x < GetAirTarget().transform.position.x))
            {
                airTarget = m;
            }
            else if (m.GetHeight() == 0 && (GetGroundTarget() == null || m.transform.position.x < GetGroundTarget().transform.position.x))
            {
                groundTarget = m;
            }
        }
        if (airTarget != null)
            targetPosition = airTarget.transform.position;
        else if (groundTarget != null)
            targetPosition = groundTarget.transform.position;
    }

    /// <summary>
    /// 获取当前锁定的空中目标
    /// </summary>
    /// <returns></returns>
    public BaseUnit GetAirTarget()
    {
        if (airTarget != null && !airTarget.IsAlive())
            airTarget = null;
        return airTarget;
    }

    /// <summary>
    /// 获取当前锁定的地面目标
    /// </summary>
    /// <returns></returns>
    public BaseUnit GetGroundTarget()
    {
        if (groundTarget != null && !groundTarget.IsAlive())
            groundTarget = null;
        return groundTarget;
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
            attackLeft--;
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
        attackLeft = attackCount;
        SetActionState(new IdleState(this));
    }

    /// <summary>
    /// 是否为伤害判定时刻（近战攻击为打出实际伤害，远程攻击为确定发射弹体）
    /// </summary>
    /// <returns></returns>
    public override bool IsDamageJudgment()
    {
        return (attackLeft > 0 && animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= attackPercentList[attackCount - attackLeft]);
    }

    /// <summary>
    /// 执行具体的攻击，位于伤害判定为真之后
    /// </summary>
    public override void ExecuteDamage()
    {
        GameManager.Instance.audioSourceManager.PlayEffectMusic("Throw" + GameManager.Instance.rand.Next(0, 2));
        // 立即进行一次索敌更新
        FindTarget();
        int c = attackCount - attackLeft; // 计算当前是第几次攻击(0,1)
        if(c == 0)
        {
            // 优先对空索敌
            CreateBullet(transform.position, mCurrentAttack, (GetAirTarget() != null ? GetAirTarget() : GetGroundTarget()));
        }
        else if(c == 1)
        {
            // 优先对地索敌
            CreateBullet(transform.position, mCurrentAttack, (GetGroundTarget() != null ? GetGroundTarget() : GetAirTarget()));
        }
    }

    /// <summary>
    /// 创建一发弹射子弹
    /// </summary>
    /// <param name="startPosition"></param>
    /// <param name="ori_dmg"></param>
    /// <param name="target"></param>
    private BaseBullet CreateBullet(Vector2 startPosition, float ori_dmg, BaseUnit target)
    {
        bool isAirTarget = (target != null && target.GetHeight() == 1); // 是否为空中单位

        AllyBullet b = AllyBullet.GetInstance(BulletStyle.Throwing, BulletRuntimeAnimatorController, this, (isAirTarget ? airUnitDamageRate:groundUnitDamageRate) * ori_dmg);
        b.SetHitSoundEffect("Splat" + GameManager.Instance.rand.Next(0, 3));
        b.AddSpriteOffsetY(new FloatModifier(0.5f * MapManager.gridHeight));
        b.SetHeight(isAirTarget ? 1:0);
        b.isnDelOutOfBound = true; // 出屏不自删

        // 定义击中后的效果
        Action<BaseBullet, BaseUnit> hitEnemyAction = null;
        {
            hitEnemyAction = (b, u) =>
            {
                if (u != null)
                {
                    int timeLeft = (isAirTarget ? airSlowTime : groundSlowTime);
                    // 为目标施加减速
                    u.AddStatusAbility(new FrozenSlowStatusAbility(-50, u, timeLeft));
                    // 若目标为空中敌人，则产生对空AOE
                    if(isAirTarget)
                        CreateDamageArea(u.transform.position, ori_dmg);
                }
            };
        }

        // 目标是敌方，直接添加即可
        b.AddHitAction(hitEnemyAction);

        // 确定好参数后添加抛物线运动
        if (target != null)
            PitcherManager.AddDefaultFlyTask(b, startPosition, target, true, false);
        else
            PitcherManager.AddDefaultFlyTask(b, startPosition, targetPosition, true, false);

        GameController.Instance.AddBullet(b);
        return b;
    }


    /// <summary>
    /// 对空AOE伤害
    /// </summary>
    private void CreateDamageArea(Vector2 pos, float ori_dmg)
    {
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, 3, 3, "ItemCollideEnemy");
        r.isAffectMouse = true;
        r.SetAffectHeight(1);
        r.SetInstantaneous();
        r.SetOnEnemyEnterAction((u) => {
            new DamageAction(CombatAction.ActionType.CauseDamage, this, u, Mathf.Min(airAoeDamageRate * ori_dmg, u.mMaxHp * 0.1f)).ApplyAction();
        });
        GameController.Instance.AddAreaEffectExecution(r);
    }
}
