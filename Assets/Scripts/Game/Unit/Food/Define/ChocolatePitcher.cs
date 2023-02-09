using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 巧克力投手
/// </summary>
public class ChocolatePitcher : FoodUnit
{
    private static RuntimeAnimatorController BigBulletRuntimeAnimatorController;
    private static RuntimeAnimatorController SmallBulletRuntimeAnimatorController;

    private Vector2 targetPosition;
    private int BigBulletAttackCount; // 投掷巧克力块所需要的攻击次数
    private int bigLeft; // 投掷巧克力块前还需要的攻击次数
    private float BigDamageRate; // 巧克力块造成的伤害倍率
    private int StunTime; // 巧克力块造成的晕眩效果

    public override void Awake()
    {
        base.Awake();
        if (BigBulletRuntimeAnimatorController == null)
        {
            BigBulletRuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/35/BigBullet");
            SmallBulletRuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/35/SmallBullet");
        }
    }

    public override void MInit()
    {
        base.MInit();
        // 根据转职情况来确定参数
        switch (mShape)
        {
            case 1:
                BigBulletAttackCount = 2;
                BigDamageRate = 1f;
                StunTime = 360;
                break;
            case 2:
                BigBulletAttackCount = 1;
                BigDamageRate = 1f;
                StunTime = 360;
                break;
            default:
                BigBulletAttackCount = 2;
                BigDamageRate = 1f;
                StunTime = 240;
                break;
        }
        bigLeft = BigBulletAttackCount;
        targetPosition = Vector2.zero;
    }

    /// <summary>
    /// 根据等级表和等级来更新对应数据
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        NumericBox.Attack.SetBase((float)(attr.baseAttrbute.baseAttack + attr.valueList[mLevel]));
    }

    public override void OnAttackStateEnter()
    {
        if(bigLeft > 0)
            animatorController.Play("Attack0");
        else
            animatorController.Play("Attack1");
    }

    protected override void UpdateAttackAnimationSpeed()
    {
        UpdateAnimationSpeedByAttackSpeed("Attack0");
        UpdateAnimationSpeedByAttackSpeed("Attack1");
    }

    /// <summary>
    /// 判断是否有有效的攻击目标
    /// </summary>
    /// <returns></returns>
    protected override bool IsHasTarget()
    {
        BaseUnit targetUnit = PitcherManager.FindTargetByPitcher(this, transform.position.x, GetRowIndex());
        if (targetUnit != null)
            targetPosition = targetUnit.transform.position;
        return targetUnit != null;
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
        int rowIndex = GetRowIndex();
        if(bigLeft > 0)
        {
            bigLeft--;
            BaseUnit target = PitcherManager.FindTargetByPitcher(this, transform.position.x, rowIndex);
            CreateSmallBullet(transform.position, target);
        }
        else
        {
            bigLeft = BigBulletAttackCount;
            // 对三行
            int startIndex = Mathf.Max(0, rowIndex - 1);
            int endIndex = Mathf.Min(6, rowIndex + 1);
            for (int i = startIndex; i <= endIndex; i++)
            {
                // 先找i行没有被定身的最左单位
                List<BaseUnit> unitList = new List<BaseUnit>();
                foreach (var u in GameController.Instance.GetSpecificRowEnemyList(i))
                {
                    unitList.Add(u);
                }
                // 寻找本行的友方布丁单位，也一并加入
                foreach (var u in GameController.Instance.GetSpecificRowAllyList(i))
                {
                    if (u.mType == (int)FoodNameTypeMap.CherryPudding)
                        unitList.Add(u);
                }
                List<BaseUnit> list = UnitManager.GetList(unitList, (u) => { return u.GetNoCountUniqueStatus(StringManager.Stun) == null; });
                BaseUnit target = PitcherManager.FindTargetByPitcher(this, transform.position.x, list, null);
                if(target == null)
                    target = PitcherManager.FindTargetByPitcher(this, transform.position.x, i);
                if(target != null || i == rowIndex)
                    CreateBigBullet(transform.position, target);
            }
        }
        
    }

    /// <summary>
    /// 投掷巧克力粒
    /// </summary>
    /// <param name="startPosition"></param>
    /// <param name="target"></param>
    private BaseBullet CreateSmallBullet(Vector2 startPosition, BaseUnit target)
    {
        AllyBullet b = AllyBullet.GetInstance(BulletStyle.Throwing, SmallBulletRuntimeAnimatorController, this, mCurrentAttack);
        b.AddSpriteOffsetY(new FloatModifier(0.5f * MapManager.gridHeight));
        b.isnDelOutOfBound = true; // 出屏不自删

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
                    BaseUnit next_target = pudding.FindRedirectThrowingObjectTarget(b);
                    if (next_target != null)
                    {
                        b.isnUseHitEffect = true; // 当前子弹不采用击中动画，直接消失
                        CreateSmallBullet(b.transform.position, next_target);
                    }
                }
            });
        }

        // 确定好参数后添加抛物线运动
        if (target != null)
            PitcherManager.AddDefaultFlyTask(b, startPosition, target, true, false);
        else
            PitcherManager.AddDefaultFlyTask(b, startPosition, targetPosition, true, false);

        GameController.Instance.AddBullet(b);
        return b;
    }

    /// <summary>
    /// 投掷巧克力块
    /// </summary>
    /// <param name="startPosition"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    private BaseBullet CreateBigBullet(Vector2 startPosition, BaseUnit target)
    {
        AllyBullet b = AllyBullet.GetInstance(BulletStyle.Throwing, BigBulletRuntimeAnimatorController, this, BigDamageRate*mCurrentAttack);
        b.AddSpriteOffsetY(new FloatModifier(0.5f * MapManager.gridHeight));
        b.isnDelOutOfBound = true; // 出屏不自删

        // 确定好参数后添加抛物线运动
        if (target != null)
            PitcherManager.AddDefaultFlyTask(b, startPosition, target, true, false);
        else
            PitcherManager.AddDefaultFlyTask(b, startPosition, targetPosition, true, false);


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
                    BaseUnit next_target = pudding.FindRedirectThrowingObjectTarget(b);
                    if (next_target != null)
                    {
                        b.isnUseHitEffect = true; // 当前子弹不采用击中动画，直接消失
                        CreateBigBullet(b.transform.position, next_target);
                    }
                }
            });
        }
        else
        {
            // 添加击中后的事件
            b.AddHitAction((b, u) => {
                if (u == null || !u.IsAlive())
                    return;

                // 如果目标已处于定身状态则效果会延长，但最多不超过15秒
                StatusAbility s = u.GetNoCountUniqueStatus(StringManager.Stun);
                if (s != null)
                    s.leftTime += Mathf.Min(60, Mathf.Max(0, 540 - s.leftTime));
                else
                    u.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(u, StunTime, false));
                //if (u.NumericBox.GetBoolNumericValue(StringManager.IgnoreStun))
                //{
                //    // 若目标免疫晕眩，则造成额外伤害
                //    new DamageAction(CombatAction.ActionType.CauseDamage, this, u, BigDamageRate * mCurrentAttack).ApplyAction();
                //}
            });
        }

        GameController.Instance.AddBullet(b);
        return b;
    }
}
