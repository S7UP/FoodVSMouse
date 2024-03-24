using System.Collections.Generic;

using S7P.Numeric;

using UnityEngine;
/// <summary>
/// 臭豆腐投手
/// </summary>
public class TofuPitcher : FoodUnit
{
    private static RuntimeAnimatorController GreenBulletRuntimeAnimatorController;
    private static RuntimeAnimatorController RedBulletRuntimeAnimatorController;
    private static RuntimeAnimatorController PoisonEffectRuntimeAnimatorController;
    private static RuntimeAnimatorController PoisonAreaEffectRuntimeAnimatorController;
    private const string DebuffName = "臭豆腐中毒";

    private float poisonDamageRate; // 每秒中毒伤害倍率
    private int PoisonTime; // 中毒时间

    private int chargeTime; // 充能一次需要的时间
    private int chargeCount; // 当前充能数
    private const int maxChargeCount = 2; // 最大充能数
    private int chargeTimeLeft; // 剩余充能时间

    public override void Awake()
    {
        base.Awake();
        if (PoisonEffectRuntimeAnimatorController == null)
        {
            PoisonEffectRuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/36/Poison");
            PoisonAreaEffectRuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/36/PoisonEffect");
        }
    }

    public override void MInit()
    {
        base.MInit();

        if(GreenBulletRuntimeAnimatorController == null)
        {
            GreenBulletRuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/36/" + mShape + "/GreenBullet");
            RedBulletRuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/36/" + mShape + "/RedBullet");
        }
        // 根据转职情况来确定参数
        switch (mShape)
        {
            case 1:
                poisonDamageRate = 0.5f;
                PoisonTime = 720;
                chargeTime = 180;
                break;
            case 2:
                poisonDamageRate = 1f;
                PoisonTime = 360;
                chargeTime = 102;
                break;
            default:
                poisonDamageRate = 0.5f;
                PoisonTime = 720;
                chargeTime = 180;
                break;
        }

        chargeCount = 1;
        chargeTimeLeft = chargeTime;
    }

    public override void MUpdate()
    {
        // 充能机制
        if(chargeCount < maxChargeCount)
        {
            chargeTimeLeft--;
            if(chargeTimeLeft <= 0)
            {
                chargeTimeLeft += chargeTime;
                chargeCount++;
            }
        }

        base.MUpdate();
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
        // 选择红豆腐目标
        {
            BaseUnit target = PitcherManager.FindTargetByPitcher(this, transform.position.x - MapManager.gridWidth / 2, rowIndex);
            CreateRedBullet(transform.position, target, mCurrentAttack);
        }

        // 选择臭豆腐目标
        {
            BaseUnit target;
            if (chargeCount > 0 && TryFindGreenBulletTarget(out target))
            {
                chargeCount--;
                CreateGreenBullet(transform.position, target, mCurrentAttack);
            }
        }

    }

    /// <summary>
    /// 投掷红豆腐
    /// </summary>
    /// <param name="startPosition"></param>
    /// <param name="target"></param>
    private BaseBullet CreateRedBullet(Vector2 startPosition, BaseUnit target, float ori_dmg)
    {
        AllyBullet b = AllyBullet.GetInstance(BulletStyle.Throwing, RedBulletRuntimeAnimatorController, this, ori_dmg);
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
                        CreateRedBullet(b.transform.position, next_target, ori_dmg);
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
    /// 投掷臭豆腐
    /// </summary>
    /// <param name="startPosition"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    private BaseBullet CreateGreenBullet(Vector2 startPosition, BaseUnit target, float ori_dmg)
    {
        AllyBullet b = AllyBullet.GetInstance(BulletStyle.Throwing, GreenBulletRuntimeAnimatorController, this, 0);
        b.SetHitSoundEffect("Splat" + GameManager.Instance.rand.Next(0, 3));
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
                TofuTask t;
                if (u.GetTask(DebuffName) == null)
                {
                    t = new TofuTask(u);
                    u.taskController.AddUniqueTask(DebuffName, t);
                }
                else
                {
                    t = u.GetTask(DebuffName) as TofuTask;
                }
                t.AddBuff(poisonDamageRate * ori_dmg, PoisonTime);
            }
        });
        GameController.Instance.AddBullet(b);
        return b;
    }

    /// <summary>
    /// 尝试去找臭豆腐的攻击目标
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public bool TryFindGreenBulletTarget(out BaseUnit target)
    {
        float minX = transform.position.x - MapManager.gridWidth / 2;
        // 单行索敌
        List<BaseUnit> list = new List<BaseUnit>();
        List<BaseUnit> debuffList = new List<BaseUnit>(); // 中毒的单位
        // 筛选出合适的单位
        float maxHp = float.MinValue;
        foreach (var u in GameController.Instance.GetSpecificRowEnemyList(GetRowIndex()).ToArray())
        {
            bool canSelect = UnitManager.CanBeSelectedAsTarget(this, u);

            // 高度为0，可被选为攻击目标，在范围内，且不中毒
            if (u.GetHeight() == 0 && canSelect && u.transform.position.x >= minX)
            {
                if (u.GetTask(DebuffName) != null)
                {
                    debuffList.Add(u);
                    continue;
                }

                // 找血最多的
                if (u.mCurrentHp > maxHp)
                {
                    maxHp = u.mCurrentHp;
                    list.Clear();
                    list.Add(u);
                }
                else if (u.mCurrentHp == maxHp)
                    list.Add(u);
            }
        }

        // 如果都是中毒单位
        if (list.Count == 0 && debuffList.Count > 0)
        {
            foreach (var u in debuffList)
            {
                // 找血最多的
                if (u.mCurrentHp > maxHp)
                {
                    maxHp = u.mCurrentHp;
                    list.Clear();
                    list.Add(u);
                }
                else if (u.mCurrentHp == maxHp)
                    list.Add(u);
            }
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
    /// 臭豆腐BUFF任务
    /// </summary>
    private class TofuTask : ITask
    {
        // 每半秒伤害系列
        private const int interval = 15;
        private int triggerDamageTimeLeft; // 触发伤害剩余时间
        private float dmg; // 每跳伤害
        private int timeLeft; // 剩余时间
        private BaseUnit unit;

        public TofuTask(BaseUnit unit)
        {
            this.unit = unit;
            Initial();
        }

        private void Initial()
        {
            triggerDamageTimeLeft = interval;
        }

        public void OnEnter()
        {
            // 添加中毒特效
            BaseEffect e = BaseEffect.CreateInstance(PoisonAreaEffectRuntimeAnimatorController, "Appear", "Idle", "Disappear", true);
            string name;
            int order;
            if (unit.TryGetSpriteRenternerSorting(out name, out order))
            {
                e.SetSpriteRendererSorting(name, order + 2);
            }
            GameController.Instance.AddEffect(e);
            unit.mEffectController.AddEffectToDict(DebuffName, e, 0.35f * Vector2.up + 0.15f * Vector2.left);
        }

        public void OnUpdate()
        {
            timeLeft--;
            triggerDamageTimeLeft--;
            if (triggerDamageTimeLeft <= 0)
            {
                triggerDamageTimeLeft += interval;
                new DamageAction(CombatAction.ActionType.CauseDamage, null, unit, dmg).ApplyAction();
            }
        }

        public bool IsMeetingExitCondition()
        {
            return timeLeft <= 0;
        }

        public void OnExit()
        {
            // 移除中毒特效
            unit.mEffectController.RemoveEffectFromDict(DebuffName);
        }

        // 叠加一层效果
        public void AddBuff(float dmg, int timeLeft)
        {
            this.dmg = dmg / (60f/interval);
            this.timeLeft = timeLeft;
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
