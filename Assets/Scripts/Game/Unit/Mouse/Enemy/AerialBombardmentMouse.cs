using S7P.Numeric;

using System;
using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 机械投弹鼠
/// </summary>
public class AerialBombardmentMouse : MouseUnit, IFlyUnit
{
    private static RuntimeAnimatorController Bomb_Run;
    private static Vector3 offset = new Vector3(-0.2f, 0.25f);

    private GeneralAttackSkillAbility generalAttackSkillAbility;

    private bool isThrow;
    private bool isDrop; // 是否被击落
    private int dropColumn; // 降落列
    private BoolModifier boolMod = new BoolModifier(true);
    private FloatModifier moveSpeedMod = new FloatModifier(100);
    private static Func<BaseUnit, BaseUnit, bool> noBeSelectFunc = delegate { return false; };
    private static Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate { return false; };

    private void Awake()
    {
        base.Awake();
        if (Bomb_Run == null)
            Bomb_Run = GameManager.Instance.GetRuntimeAnimatorController("Mouse/9/Bomb");
    }

    public override void MInit()
    {
        dropColumn = 0; // 降落列默认为0，即左一列
        base.MInit();
        mHeight = 1;
        isThrow = false;
        isDrop = false;
        Environment.WaterTask.AddUnitWaterRate(this, new S7P.Numeric.FloatModifier(2.0f));
    }

    public override void MUpdate()
    {
        base.MUpdate();
        if (IsMeetDropCondition())
        {
            OnShootDown();
        }
    }

    /// <summary>
    /// 检测是否满足降落条件
    /// </summary>
    private bool IsMeetDropCondition()
    {
        return (transform.position.x <= MapManager.GetColumnX(dropColumn) && !isDrop);
    }

    /// <summary>
    /// 加载技能
    /// </summary>
    public override void LoadSkillAbility()
    {
        List<SkillAbility.SkillAbilityInfo> infoList = AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape);
        // 普通攻击
        if (infoList.Count > 0)
        {
            generalAttackSkillAbility = new GeneralAttackSkillAbility(this, infoList[0]);
            skillAbilityManager.AddSkillAbility(generalAttackSkillAbility);
        }

        // 投炸弹技能
        if (infoList.Count > 1)
        {
            RetangleAreaEffectExecution r = null;
            CustomizationSkillAbility s = new CustomizationSkillAbility(this, infoList[1]);
            // 定义
            {
                s.IsMeetSkillConditionFunc = delegate {
                    return !isDrop && !isThrow;
                };
                s.BeforeSpellFunc = delegate 
                {
                    // 检测器
                    r = RetangleAreaEffectExecution.GetInstance(transform.position + new Vector3(offset.x, 0), new Vector2(0.1f * MapManager.gridWidth, 0.1f * MapManager.gridHeight), "ItemCollideAlly");
                    r.SetAffectHeight(0);
                    r.AddFoodEnterConditionFunc((u) => {
                        return FoodManager.IsAttackableFoodType(u) && UnitManager.CanBeSelectedAsTarget(this, u);
                    });
                    r.isAffectFood = true;
                    GameController.Instance.AddAreaEffectExecution(r);

                    CustomizationTask task = new CustomizationTask();
                    task.AddTaskFunc(delegate {
                        r.transform.position = transform.position + new Vector3(offset.x, 0);
                        return isDrop;
                    });
                    task.AddOnExitAction(delegate {
                        r.MDestory();
                    });
                    r.taskController.AddTask(task);
                };
                s.IsMeetCloseSpellingConditionFunc = delegate {
                    return r.foodUnitList.Count > 0 || isDrop;
                };
                s.AfterSpellFunc = delegate {
                    if (!isDrop)
                    {
                        isThrow = true;
                        animatorController.Play("Idle1");
                        CreateBomb();
                        r.MDestory();
                        // 投完就加速
                        NumericBox.MoveSpeed.AddPctAddModifier(moveSpeedMod);
                    }
                };
            }
            skillAbilityManager.AddSkillAbility(s);
            // 与技能速率挂钩
            {
                FloatModifier skillSpeedMod = new FloatModifier((NumericBox.SkillSpeed.TotalValue - 1)*100);
                s.energyRegeneration.AddPctAddModifier(skillSpeedMod);

                NumericBox.SkillSpeed.AddAfterValueChangeAction((val) => {
                    s.energyRegeneration.RemovePctAddModifier(skillSpeedMod);
                    skillSpeedMod.Value = (NumericBox.SkillSpeed.TotalValue - 1) * 100;
                    s.energyRegeneration.AddPctAddModifier(skillSpeedMod);
                });
            }
        }
    }

    /// <summary>
    /// 添加一个炸弹
    /// </summary>
    private void CreateBomb()
    {
        Vector3 start = transform.position + offset;
        Vector3 end = transform.position + new Vector3(offset.x, 0);

        BaseEffect e = BaseEffect.CreateInstance(Bomb_Run, null, "Fly", "Hit", true);
        e.SetSpriteRendererSorting(GetSpriteRenderer().sortingLayerName, GetSpriteRenderer().sortingOrder + 1);
        e.transform.position = start;
        GameController.Instance.AddEffect(e);

        CustomizationTask task = new CustomizationTask();
        task.AddTimeTaskFunc(45, null, (leftTime, totalTime) => 
        {
            float rate = 1 - (float)leftTime / totalTime;
            e.transform.position = Vector3.Lerp(start, end, rate*rate);
        }, null);
        task.AddOnExitAction(delegate {
            // 对美食
            {
                RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, new Vector2(0.5f * MapManager.gridWidth, 0.5f * MapManager.gridHeight), "ItemCollideAlly");
                r.SetAffectHeight(0);
                r.isAffectFood = true;
                r.isAffectMouse = true;
                r.SetTotoalTimeAndTimeLeft(2);
                r.AddExcludeMouseUnit(this);
                r.AddBeforeDestoryAction(delegate
                {
                    // 所有美食单位分摊伤害
                    int count = r.foodUnitList.Count;
                    foreach (var u in r.foodUnitList.ToArray())
                        new DamageAction(CombatAction.ActionType.BurnDamage, this, u, Mathf.Max(50, u.mMaxHp / 2) / count).ApplyAction();
                });
                GameController.Instance.AddAreaEffectExecution(r);
            }
            // 对老鼠
            {
                RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, new Vector2(1f * MapManager.gridWidth, 1f * MapManager.gridHeight), "ItemCollideEnemy");
                r.SetAffectHeight(0);
                r.isAffectMouse = true;
                r.SetTotoalTimeAndTimeLeft(2);
                r.AddExcludeMouseUnit(this);
                r.AddBeforeDestoryAction(delegate
                {
                    // 所有老鼠单位受到一次灰烬效果
                    foreach (var u in r.mouseUnitList.ToArray())
                        BurnManager.BurnDamage(this, u);
                });
                GameController.Instance.AddAreaEffectExecution(r);
            }

            e.ExecuteDeath();
        });
        e.taskController.AddTask(task);
    }

    /// <summary>
    /// 执行降落，仅一次
    /// </summary>
    public void OnShootDown()
    {
        if (!isDrop)
        {
            isDrop = true;
            // 若当前生命值大于飞行状态临界点，则需要强制同步生命值至临界点
            // if(mCurrentHp> mHertRateList[0] * mMaxHp)
            {
                mCurrentHp = (float)mHertRateList[0]*mMaxHp;
            }
            mHertRateList[0] = double.MaxValue;
            UpdateHertMap(); // 强制更新一次贴图
            NumericBox.MoveSpeed.RemovePctAddModifier(moveSpeedMod); // 移除加速（可能）
            // 设为转场状态，该状态下的具体实下如下几个方法
            SetActionState(new TransitionState(this));
        }
    }

    /// <summary>
    /// 当贴图更新时要做的事
    /// </summary>
    public override void OnUpdateRuntimeAnimatorController()
    {
        // 当第一次切换到非第0阶段的贴图时（即退出飞行状态），将第0阶段的血量百分比设为超过1.0（即这之后永远达不到），然后播放坠落动画
        // 当前取值范围为1~3时触发
        // 0 飞行
        // 1 击落过程->正常移动
        // 2 受伤移动
        if (mHertIndex > 0 && mHertIndex <= 2 && !isDrop)
        {
            OnShootDown();
        }
    }

    /// <summary>
    /// 进入转场状态时要做的事，这里特指进入刚被击落时要做的
    /// </summary>
    public override void OnTransitionStateEnter()
    {
        // 被击落期间移除所有定身类效果且免疫定身类效果
        StatusManager.RemoveAllSettleDownDebuff(this);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, boolMod);
        animatorController.Play("Drop");
        AddCanBeSelectedAsTargetFunc(noBeSelectFunc);
        AddCanHitFunc(noHitFunc);
    }

    public override void OnTransitionState()
    {
        // 动画播放完一次后，转为移动状态
        if (!animatorController.GetCurrentAnimatorStateRecorder().aniName.Equals("Drop") || animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            SetActionState(new MoveState(this));
            AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(this, 60, false));
        }
    }

    public override void OnTransitionStateExit()
    {
        NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreStun, boolMod);
        mHeight = 0; // 高度降低为地面高度
        RemoveCanBeSelectedAsTargetFunc(noBeSelectFunc);
        RemoveCanHitFunc(noHitFunc);
    }

    public override void OnMoveStateEnter()
    {
        if(!isDrop && isThrow)
            animatorController.Play("Move1", true);
        else
            animatorController.Play("Move", true);
        //if (isDrop)
        //{
        //    animatorController.Play("Move", true);
        //}
        //else
        //{
        //    if (isThrow)
        //    {
        //        animatorController.Play("Move1", true);
        //    }else
        //        animatorController.Play("Move", true);
        //}
    }
}
