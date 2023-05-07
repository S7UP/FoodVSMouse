using System;

using S7P.Numeric;

using UnityEngine;
/// <summary>
/// 冰煮蛋器投手
/// </summary>
public class IceEggPitcher : FoodUnit
{
    private static RuntimeAnimatorController[] BulletRuntimeAnimatorControllerArray;
    private FloatModifier costMod = new FloatModifier(-75f/7/60);

    private float mainDamageRate; // 主要目标伤害倍率
    private float aoeDamageRate; // 范围伤害倍率
    private Vector2 targetPosition;

    public override void Awake()
    {
        base.Awake();
        if (BulletRuntimeAnimatorControllerArray == null)
        {
            BulletRuntimeAnimatorControllerArray = new RuntimeAnimatorController[3];
            for (int i = 0; i < 3; i++)
            {
                BulletRuntimeAnimatorControllerArray[i] = GameManager.Instance.GetRuntimeAnimatorController("Bullet/7/0");
            }
        }
    }

    public override void MInit()
    {
        base.MInit();
        // 根据转职情况来确定首要目标与范围伤害的伤害比率
        switch (mShape)
        {
            case 1:
                mainDamageRate = 6f;
                aoeDamageRate = 1.2f;
                break;
            case 2:
                mainDamageRate = 6f;
                aoeDamageRate = 1.2f;
                break;
            default:
                mainDamageRate = 4.5f;
                aoeDamageRate = 0.9f;
                break;
        }
        targetPosition = Vector2.zero;
        GameController.Instance.AddCostResourceModifier("Fire", costMod);
    }

    public override void MDestory()
    {
        GameController.Instance.RemoveCostResourceModifier("Fire", costMod);
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
        GameManager.Instance.audioSourceManager.PlayEffectMusic("Throw" + GameManager.Instance.rand.Next(0, 2));
        // 选择目标
        BaseUnit target = PitcherManager.FindTargetByPitcher(this, transform.position.x, GetRowIndex());

        CreateBullet(transform.position, mCurrentAttack, target);
    }

    /// <summary>
    /// 创建一发弹射子弹
    /// </summary>
    /// <param name="startPosition"></param>
    /// <param name="ori_dmg"></param>
    /// <param name="target"></param>
    private BaseBullet CreateBullet(Vector2 startPosition, float ori_dmg, BaseUnit target)
    {
        AllyBullet b = AllyBullet.GetInstance(BulletStyle.Throwing, BulletRuntimeAnimatorControllerArray[mShape], this, mainDamageRate * ori_dmg);
        b.AddSpriteOffsetY(new FloatModifier(0.5f * MapManager.gridHeight));
        b.isnDelOutOfBound = true; // 出屏不自删
        b.SetHitSoundEffect("Eggimpact"+GameManager.Instance.rand.Next(0, 2));

        // 定义击中后的效果
        Action<BaseBullet, BaseUnit> hitEnemyAction = null;
        {
            hitEnemyAction = (b, u) =>
            {
                if (u != null)
                {
                    // 产生AOE
                    CreateDamageArea(u.transform.position, ori_dmg);
                }
                else
                {
                    CreateDamageArea(b.transform.position, ori_dmg);
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
                        CreateBullet(b.transform.position, ori_dmg, next_target);
                    }
                    else
                    {
                        // 原地破裂，但同样会触发范围效果
                        hitEnemyAction(b, null);
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

    /// <summary>
    /// AOE伤害
    /// </summary>
    private void CreateDamageArea(Vector2 pos, float ori_dmg)
    {
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, 3, 3, "ItemCollideEnemy");
        r.isAffectMouse = true;
        r.SetAffectHeight(0);
        r.SetInstantaneous();
        r.SetOnEnemyEnterAction((u) => {
            u.AddStatusAbility(new FrozenSlowStatusAbility(-50, u, 180));
            DamageAction d = new DamageAction(CombatAction.ActionType.CauseDamage, this, u, aoeDamageRate * ori_dmg);
            d.AddDamageType(DamageAction.DamageType.AOE);
            d.ApplyAction();
        });
        GameController.Instance.AddAreaEffectExecution(r);
    }
}
