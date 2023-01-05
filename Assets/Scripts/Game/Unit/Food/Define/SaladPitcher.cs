using UnityEngine;
using System;
/// <summary>
/// 色拉投手
/// </summary>
public class SaladPitcher : FoodUnit
{
    private static RuntimeAnimatorController BulletRuntimeAnimatorController;

    private int bounceCount; // 子弹弹跳次数
    private float bounceDamageRate; // 后续弹跳伤害比率
    private Vector2 targetPosition;

    public override void Awake()
    {
        base.Awake();
        if(BulletRuntimeAnimatorController == null)
            BulletRuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/34/Bullet");
    }

    public override void MInit()
    {
        base.MInit();
        // 根据转职情况来确定弹跳次数与后续弹跳伤害比率
        switch (mShape)
        {
            case 1:
                bounceCount = 1;
                bounceDamageRate = 1;
                break;
            case 2:
                bounceCount = 2;
                bounceDamageRate = 1;
                break;
            default:
                bounceCount = 1;
                bounceDamageRate = 0.5f;
                break;
        }
        targetPosition = Vector2.zero;
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
        BaseUnit targetUnit = PitcherManager.FindTargetByPitcher(this, transform.position.x, GetRowIndex());
        if (targetUnit != null)
        {
            targetPosition = targetUnit.transform.position;
        }
        return targetUnit!=null;
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
            mAttackFlag = false;
            ExecuteDamage();
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
        mAttackFlag = true;
        SetActionState(new IdleState(this));
    }

    /// <summary>
    /// 是否为伤害判定时刻（近战攻击为打出实际伤害，远程攻击为确定发射弹体）
    /// </summary>
    /// <returns></returns>
    public override bool IsDamageJudgment()
    {
        return (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= attackPercent && mAttackFlag);
    }

    /// <summary>
    /// 执行具体的攻击，位于伤害判定为真之后
    /// </summary>
    public override void ExecuteDamage()
    {
        // 选择目标
        BaseUnit target = PitcherManager.FindTargetByPitcher(this, transform.position.x, GetRowIndex());

        CreateBullet(transform.position, mCurrentAttack, target, bounceCount);
    }

    /// <summary>
    /// 创建一发弹射子弹（注意这是递归方法）
    /// </summary>
    /// <param name="startPosition"></param>
    /// <param name="ori_dmg"></param>
    /// <param name="target"></param>
    /// <param name="bounceLeft"></param>
    private BaseBullet CreateBullet(Vector2 startPosition, float ori_dmg, BaseUnit target, int bounceLeft)
    {
        // 计算当前子弹的伤害
        float real_dmg = ori_dmg;
        if(mShape == 2)
        {
            if (bounceLeft == 2)
                real_dmg = ori_dmg;
            else
                real_dmg = ori_dmg * bounceDamageRate;
        }
        else if(mShape < 2)
        {
            if (bounceLeft == 1)
                real_dmg = ori_dmg;
            else
                real_dmg = ori_dmg * bounceDamageRate;
        }
        AllyBullet b = AllyBullet.GetInstance(BulletRuntimeAnimatorController, this, real_dmg);
        b.AddSpriteOffsetY(new FloatModifier(0.5f*MapManager.gridHeight));
        b.isnDelOutOfBound = true; // 出屏不自删

        // 定义击中后的弹跳效果
        Action<BaseBullet, BaseUnit> hitEnemyAction = null;
        {
            hitEnemyAction = (b, u) =>
            {
                if (u == null)
                {
                    // 如果没砸到东西是不会弹跳的
                    return;
                }

                BaseUnit target = null;
                // 根据不同的情况选择不同的索敌算法
                if (bounceLeft == 1)
                {
                    if (mShape < 2)
                    {
                        // 非二转，去找最右的单位
                        target = MouseManager.FindTheMostRightTarget(this, u.transform.position.x, float.MaxValue, b.GetRowIndex());
                    }
                    else
                    {
                        // 二转，去找最左的单位
                        target = MouseManager.FindTheMostLeftTarget(this, float.MinValue, u.transform.position.x, b.GetRowIndex());
                    }
                }
                else if (bounceLeft == 2)
                {
                    // 这是只有二转会出现的情况，等效于一转最后一次索敌
                    target = MouseManager.FindTheMostRightTarget(this, u.transform.position.x, float.MaxValue, b.GetRowIndex());
                }

                if (target != null)
                {
                    // 若还有目标就进行弹射（本质是创建一个新的子弹），否则自毁（啥也不干）
                    CreateBullet(u.transform.position, ori_dmg, target, bounceLeft - 1);
                }
            };
        }

        // 这里判断目标是友方还是敌方，还是没有，根据这些情况来制定逻辑
        if (target != null && target is FoodUnit && target.mType == (int)FoodNameTypeMap.CherryPudding)
        {
            // 目标是友方布丁
            CherryPuddingFoodUnit pudding = target.GetComponent<CherryPuddingFoodUnit>();

            // 添加目标布丁存活监听
            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate
            {
                return !pudding.IsAlive();
            });
            t.OnExitFunc = delegate
            {
                pudding = null; // 如果目标布丁不存活，则取消其引用
            };
            b.AddTask(t);

            // 定义与添加重定向任务
            b.AddHitAction((b, u) => {
                // 重定向
                if (pudding != null)
                {
                    // pudding.RedirectThrowingObject();
                    BaseUnit next_target = pudding.FindRedirectThrowingObjectTarget(b);
                    if(next_target != null)
                    {
                        b.isnUseHitEffect = true; // 当前子弹不采用击中动画，直接消失
                        CreateBullet(b.transform.position, ori_dmg, next_target, bounceCount);
                    }   
                }
            });

            // 添加抛物线运动
            PitcherManager.AddDefaultFlyTask(b, startPosition, target, true, false);
        }
        else
        {
            // 目标是敌方，直接添加即可
            b.AddHitAction(hitEnemyAction);

            // 确定好参数后添加抛物线运动
            if (target != null)
                PitcherManager.AddDefaultFlyTask(b, startPosition, target, true, false);
            else
                PitcherManager.AddDefaultFlyTask(b, startPosition, targetPosition, true, false);
        }
        

        GameController.Instance.AddBullet(b);

        return b;
    }
}
