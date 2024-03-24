using System.Collections.Generic;

using S7P.Numeric;

using UnityEngine;
/// <summary>
/// 巧克力投手
/// </summary>
public class ChocolatePitcher : FoodUnit
{
    private static RuntimeAnimatorController BigBulletRuntimeAnimatorController;
    private static RuntimeAnimatorController SmallBulletRuntimeAnimatorController;
    private static Sprite Debuff_Spr;
    private const string DebuffName = "粘稠";

    private const int DecMoveSpeedTime = 720; // 巧克力块的减速时间

    private const int chargeTime = 180; // 充能一次需要的时间
    private const int maxChargeCount = 2; // 最大充能数

    private int chargeCount; // 当前充能数
    private int chargeTimeLeft; // 剩余充能时间
    private float moveSpeedRate; // 被上了debuff的移速比率

    public override void Awake()
    {
        base.Awake();
        if (BigBulletRuntimeAnimatorController == null)
        {
            BigBulletRuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/35/BigBullet");
            SmallBulletRuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/35/SmallBullet");
            Debuff_Spr = GameManager.Instance.GetSprite("Food/35/Debuff");
        }
    }

    public override void MInit()
    {
        base.MInit();
        chargeCount = 1;
        chargeTimeLeft = chargeTime;
        switch (mShape)
        {
            case 1:
                moveSpeedRate = 0.33f;
                break;
            case 2:
                moveSpeedRate = 0.22f;
                break;
            default:
                moveSpeedRate = 0.33f;
                break;
        }
    }

    public override void MUpdate()
    {
        // 充能机制
        if (chargeCount < maxChargeCount)
        {
            chargeTimeLeft--;
            if (chargeTimeLeft <= 0)
            {
                chargeTimeLeft += chargeTime;
                chargeCount++;
            }
        }

        base.MUpdate();
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
        animatorController.Play("Attack0");
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
        BaseUnit targetUnit = PitcherManager.FindTargetByPitcher(this, transform.position.x - MapManager.gridWidth / 2, GetRowIndex());
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
        GameManager.Instance.audioSourceController.PlayEffectMusic("Throw" + GameManager.Instance.rand.Next(0, 2));
        // 选择巧克力粒目标
        {
            BaseUnit target = PitcherManager.FindTargetByPitcher(this, transform.position.x - MapManager.gridWidth / 2, rowIndex);
            CreateSmallBullet(transform.position, target);
        }

        // 选择巧克力块目标
        {
            BaseUnit target;
            if (chargeCount > 0 && TryFindBigBulletTarget(out target))
            {
                chargeCount--;
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
        b.SetHitSoundEffect("Splat" + GameManager.Instance.rand.Next(0, 3));
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
            t.AddOnExitAction(delegate
            {
                pudding = null; // 如果目标布丁不存活，则取消其引用
            });
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
            PitcherManager.AddDefaultFlyTask(b, startPosition, new Vector2(MapManager.GetColumnX(8.5f), startPosition.y), true, false);

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
        AllyBullet b = AllyBullet.GetInstance(BulletStyle.Throwing, BigBulletRuntimeAnimatorController, this, 0);
        b.SetHitSoundEffect("Butter");
        b.AddSpriteOffsetY(new FloatModifier(0.5f * MapManager.gridHeight));
        b.isnDelOutOfBound = true; // 出屏不自删

        // 确定好参数后添加抛物线运动
        if (target != null)
            PitcherManager.AddDefaultFlyTask(b, startPosition, target, true, false);
        else
            PitcherManager.AddDefaultFlyTask(b, startPosition, new Vector2(MapManager.GetColumnX(8.5f), startPosition.y), true, false);

        // 添加击中后的事件
        b.AddHitAction((b, u) => {
            if (u != null && u.IsAlive())
            {
                DebuffTask t;
                if (u.GetTask(DebuffName) == null)
                {
                    t = new DebuffTask(u);
                    u.taskController.AddUniqueTask(DebuffName, t);
                }
                else
                {
                    t = u.GetTask(DebuffName) as DebuffTask;
                }
                t.AddBuff(DecMoveSpeedTime, moveSpeedRate);
            }
        });

        GameController.Instance.AddBullet(b);
        return b;
    }

    /// <summary>
    /// 尝试去找巧克力块的攻击目标
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public bool TryFindBigBulletTarget(out BaseUnit target)
    {
        // 单行索敌
        List<BaseUnit> list = new List<BaseUnit>();
        List<BaseUnit> debuffList = new List<BaseUnit>(); // 粘稠的单位
        // 筛选出合适的单位
        float minX = transform.position.x - MapManager.gridWidth / 2;
        foreach (var u in GameController.Instance.GetSpecificRowEnemyList(GetRowIndex()).ToArray())
        {
            bool canSelect = UnitManager.CanBeSelectedAsTarget(this, u);
            // 高度为0，可被选为攻击目标，在范围内，且不粘稠不免疫减速
            if (u.GetHeight() == 0 && canSelect && u.transform.position.x >= minX)
            {
                if (u.GetTask(DebuffName) != null || u.NumericBox.GetBoolNumericValue(StringManager.IgnoreSlowDown))
                {
                    debuffList.Add(u);
                }
                else
                {
                    list.Add(u);
                }
            }
        }

        // 如果都是粘稠单位
        if (list.Count == 0 && debuffList.Count > 0)
        {
            list = debuffList;
        }

        target = null;
        // 去找坐标最小的单位
        if (list.Count > 0)
        {
            foreach (var u in list)
            {
                if (target == null || u.transform.position.x < target.transform.position.x)
                {
                    target = u;
                }
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// 粘稠Debuff任务
    /// </summary>
    private class DebuffTask : ITask
    {
        private int timeLeft; // 剩余时间
        // private FloatModifier decMoveSpeedMod = new FloatModifier(0);
        private BaseUnit unit;

        public DebuffTask(BaseUnit unit)
        {
            this.unit = unit;
            // decMoveSpeedMod.Value = 0;
            Initial();
        }

        private void Initial()
        {

        }

        public void OnEnter()
        {
            // 添加粘稠特效
            BaseEffect e = BaseEffect.CreateInstance(Debuff_Spr);
            string name;
            int order;
            if (unit.TryGetSpriteRenternerSorting(out name, out order))
            {
                e.SetSpriteRendererSorting(name, order + 1);
            }
            GameController.Instance.AddEffect(e);
            unit.mEffectController.AddEffectToDict(DebuffName, e, 0.35f * Vector2.up + 0.15f * Vector2.left);    
        }

        public void OnUpdate()
        {
            timeLeft--;
        }

        public bool IsMeetingExitCondition()
        {
            return timeLeft <= 0;
        }

        public void OnExit()
        {
            // 移除粘稠特效
            unit.mEffectController.RemoveEffectFromDict(DebuffName);

            // unit.NumericBox.MoveSpeed.RemoveFinalPctAddModifier(decMoveSpeedMod);
        }

        // 叠加一层效果
        public void AddBuff(int timeLeft, float move_speed_rate)
        {
            // 对无视减速的单位无效
            if (unit.NumericBox.GetBoolNumericValue(StringManager.IgnoreSlowDown))
                timeLeft = 1;
            //move_speed_rate = 0.5f || = 0.33f;
            this.timeLeft = timeLeft;
            //unit.NumericBox.MoveSpeed.RemoveFinalPctAddModifier(decMoveSpeedMod);
            //decMoveSpeedMod.Value = -(1 - move_speed_rate)*100;
            //unit.NumericBox.MoveSpeed.AddFinalPctAddModifier(decMoveSpeedMod);
            unit.AddStatusAbility(new SlowStatusAbility(-(1-move_speed_rate)*100, unit, timeLeft));
        }

        public void ShutDown()
        {

        }

        public bool IsClearWhenDie()
        {
            return true;
        }
    }
}
