using UnityEngine;
/// <summary>
/// 臭豆腐投手
/// </summary>
public class TofuPitcher : FoodUnit
{
    private static RuntimeAnimatorController[] GreenBulletRuntimeAnimatorControllerArray;
    private static RuntimeAnimatorController[] RedBulletRuntimeAnimatorControllerArray;
    private static RuntimeAnimatorController PoisonEffectRuntimeAnimatorController;
    private static string DebuffName = "臭豆腐中毒";

    private Vector2 targetPosition;
    private int GreenBulletAttackCount; // 投掷臭豆腐所需要的攻击次数
    private int greenLeft; // 投掷臭豆腐前还需要的攻击次数
    private float GreenDamageRate; // 臭豆腐的伤害倍率
    private float poisonDamageRate; // 每秒中毒伤害倍率
    private float addDamageRate; // 臭豆腐的增伤效果
    private int PoisonTime; // 中毒时间

    public override void Awake()
    {
        base.Awake();
        if (GreenBulletRuntimeAnimatorControllerArray == null)
        {
            GreenBulletRuntimeAnimatorControllerArray = new RuntimeAnimatorController[3];
            RedBulletRuntimeAnimatorControllerArray = new RuntimeAnimatorController[3];
            for (int i = 0; i < 3; i++)
            {
                GreenBulletRuntimeAnimatorControllerArray[i] = GameManager.Instance.GetRuntimeAnimatorController("Food/36/" + i + "/GreenBullet");
                RedBulletRuntimeAnimatorControllerArray[i] = GameManager.Instance.GetRuntimeAnimatorController("Food/36/" + i + "/RedBullet");
            }
            PoisonEffectRuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/36/Poison");
        }
    }

    public override void MInit()
    {
        base.MInit();
        // 根据转职情况来确定参数
        switch (mShape)
        {
            case 1:
                GreenBulletAttackCount = 2;
                PoisonTime = 240;
                greenLeft = 0;
                break;
            case 2:
                GreenBulletAttackCount = 1;
                PoisonTime = 240;
                greenLeft = 0;
                break;
            default:
                GreenBulletAttackCount = 2;
                PoisonTime = 180;
                greenLeft = GreenBulletAttackCount;
                break;
        }
        
        GreenDamageRate = 0.25f;
        addDamageRate = 1.12f;
        poisonDamageRate = 0.1f;
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
        if (greenLeft > 0)
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
        {
            targetPosition = targetUnit.transform.position;
        }
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
        // 选择目标
        BaseUnit target = PitcherManager.FindTargetByPitcher(this, transform.position.x, GetRowIndex());


        if (greenLeft > 0)
        {
            greenLeft--;
            CreateRedBullet(transform.position, target, mCurrentAttack);
        }
        else
        {
            greenLeft = GreenBulletAttackCount;
            CreateGreenBullet(transform.position, target, mCurrentAttack);
        }

    }

    /// <summary>
    /// 投掷红豆腐
    /// </summary>
    /// <param name="startPosition"></param>
    /// <param name="target"></param>
    private BaseBullet CreateRedBullet(Vector2 startPosition, BaseUnit target, float ori_dmg)
    {
        AllyBullet b = AllyBullet.GetInstance(RedBulletRuntimeAnimatorControllerArray[mShape], this, ori_dmg);
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
                        CreateRedBullet(b.transform.position, next_target, ori_dmg);
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
    private BaseBullet CreateGreenBullet(Vector2 startPosition, BaseUnit target, float ori_dmg)
    {
        AllyBullet b = AllyBullet.GetInstance(GreenBulletRuntimeAnimatorControllerArray[mShape], this, GreenDamageRate * ori_dmg);
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
                        CreateGreenBullet(b.transform.position, next_target, ori_dmg);
                    }
                    else
                    {
                        // 原地破裂，但还是会触发范围效果
                        CreateDebuffArea(b.transform.position, ori_dmg);
                    }
                }
            });
        }
        else
        {
            // 添加击中后的事件
            b.AddHitAction((b, u) => {
                if (u == null || !u.IsAlive())
                    CreateDebuffArea(b.transform.position, ori_dmg);
                else
                    CreateDebuffArea(u.transform.position, ori_dmg);
            });
        }

        GameController.Instance.AddBullet(b);
        return b;
    }


    /// <summary>
    /// 为一定范围内的敌人附上中毒
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="ori_dmg"></param>
    private void CreateDebuffArea(Vector2 pos, float ori_dmg)
    {
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, 1, 3, "ItemCollideEnemy");
        r.isAffectMouse = true;
        r.SetAffectHeight(0);
        r.SetInstantaneous();

        IntModifier countModifier = new IntModifier(1);
        FloatModifier addDmgModifier = new FloatModifier(addDamageRate);

        r.SetOnEnemyEnterAction((u) => {
            // 变量定义
            int timeLeft = PoisonTime; // 中毒剩余时间
            int triggerPoisonDamageTimeLeft = 60; // 触发毒伤判定剩余时间
            // 添加伤害增加效果 和 持续真实伤害效果
            CustomizationTask t = new CustomizationTask();
            t.OnEnterFunc = delegate {
                if (!u.NumericBox.IntDict.ContainsKey(DebuffName))
                {
                    u.NumericBox.IntDict.Add(DebuffName, new IntNumeric());
                }
                if(u.NumericBox.IntDict[DebuffName].Value == 0)
                {
                    // 添加中毒特效
                    BaseEffect e = BaseEffect.CreateInstance(PoisonEffectRuntimeAnimatorController, null, "Idle", null, true);
                    GameController.Instance.AddEffect(e);
                    u.AddEffectToDict(DebuffName, e, Vector2.zero);
                }
                u.NumericBox.IntDict[DebuffName].AddAddModifier(countModifier);
                // 添加增伤效果
                u.NumericBox.DamageRate.AddModifier(addDmgModifier);
            };

            t.AddTaskFunc(delegate {
                triggerPoisonDamageTimeLeft--;
                if (triggerPoisonDamageTimeLeft == 0)
                {
                    triggerPoisonDamageTimeLeft += 60;
                    // 造成一次真实伤害
                    new DamageAction(CombatAction.ActionType.RealDamage, null, u, poisonDamageRate*ori_dmg).ApplyAction();
                }
                
                if(timeLeft > 0)
                {
                    timeLeft--;
                    return false;
                }
                else
                {
                    return true;
                }

            });

            t.OnExitFunc = delegate {
                u.NumericBox.IntDict[DebuffName].RemoveAddModifier(countModifier);
                // 移除增伤效果
                u.NumericBox.DamageRate.RemoveModifier(addDmgModifier);
                if (u.NumericBox.IntDict[DebuffName].Value == 0)
                {
                    // 移除中毒特效
                    u.RemoveEffectFromDict(DebuffName);
                }
            };

            u.AddTask(t);

        });

        GameController.Instance.AddAreaEffectExecution(r);
    }
}
