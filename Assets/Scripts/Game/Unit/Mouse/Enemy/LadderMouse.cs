using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 梯子类
/// </summary>
public class LadderMouse : MouseUnit
{
    private GeneralAttackSkillAbility generalAttackSkillAbility;
    private PutLadderSkillAbility putLadderSkillAbility;
    private FloatModifier velocityBuffModifier; // 加速buff
    private BaseGrid targetGrid;
    private float old_P2_HpRate; // 阶段点2原始血量比率

    public override void MInit()
    {
        base.MInit();
        // 变种之弹簧鼠防爆
        if (mShape == 1)
            NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, new BoolModifier(true));
        old_P2_HpRate = (float)mHertRateList[1];
        velocityBuffModifier = new FloatModifier(100);
        NumericBox.MoveSpeed.AddPctAddModifier(velocityBuffModifier); // 初始获得100%移速

    }

    /// <summary>
    /// 默认放置物品的方法
    /// </summary>
    public void DefaultPutEvent()
    {
        BaseLadder l = (BaseLadder)GameController.Instance.CreateItem(targetGrid.GetColumnIndex(), targetGrid.GetRowIndex(), (int)ItemNameTypeMap.Ladder, mShape);
        // 设置贴图向右偏移
        //l.SetSpriteLocalPosition(new Vector2(0.5f*MapManager.gridWidth, 0));
        l.AddSpriteOffsetX(new FloatModifier(0.5f * MapManager.gridWidth));
        l.SetMoveDistance(MapManager.gridWidth*3);
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

        // 架梯子技能
        if (infoList.Count > 1)
        {
            putLadderSkillAbility = new PutLadderSkillAbility(this, infoList[1]);
            skillAbilityManager.AddSkillAbility(putLadderSkillAbility);
            putLadderSkillAbility.SetEvent(DefaultPutEvent);
        }
    }


    public override void OnMoveStateEnter()
    {
        animatorController.Play("Move", true);
    }

    public override void OnAttackStateEnter()
    {
        // 进入攻击状态时检测一下梯子是否放了，
        // 若没放，则检测当前攻击目标所在格是否有包含防护型卡片，如果有，取消当次攻击并且效果转化为放梯子技能，如果无，同下
        // 若已放，则进入正常攻击流程
        if (!putLadderSkillAbility.IsSkilled())
        {
            BaseUnit u = GetCurrentTarget();
            if (u!=null)
            {
                BaseGrid g = u.GetGrid();
                if (g != null)
                {
                    if (g.IsContainTag(FoodInGridType.Shield) || g.IsContainTag(FoodInGridType.Defence))
                    {
                        targetGrid = g;
                        generalAttackSkillAbility.EndActivate(); // 取消平A
                        putLadderSkillAbility.TriggerSkill(); // 触发梯子技能
                        return; // 就此打住
                    }
                }
            }
        }
        base.OnAttackStateEnter();
    }

    // CastState为放置动作
    public override void OnCastStateEnter()
    {
        animatorController.Play("Put");
        Debug.Log("t = " + animatorController.GetCurrentAnimatorStateRecorder().aniTime);
    }

    public override void OnCastState()
    {
        if (currentStateTimer == 0)
            return;
        Debug.Log("r = "+ animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime());
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            Debug.Log("putLadderSkillAbility.TriggerEvent()");
            putLadderSkillAbility.TriggerEvent(); // 触发实际事件
            putLadderSkillAbility.SetEndSkill();
        }
    }

    public override void OnCastStateExit()
    {
        // 若血量高于阶段点2，则强制血量为阶段点2的血量
        mCurrentHp = Mathf.Min(mCurrentHp, mMaxHp * old_P2_HpRate-1);
        putLadderSkillAbility.SetSkilled();
        UpdateHertMap();
    }

    /// <summary>
    /// 当贴图更新时要做的事
    /// </summary>
    public override void OnUpdateRuntimeAnimatorController()
    {
        // 0 携带完整弹簧冲锋
        // 1 携带破损弹簧冲锋
        // 2 无弹簧正常
        // 3 无弹簧负伤
        if (mHertIndex > 0)
        {
            mHertRateList[0] = double.MaxValue;
        }
        if (mHertIndex > 1)
        {
            mHertRateList[0] = double.MaxValue;
            mHertRateList[1] = double.MaxValue;
            // 如果是没有放梯子就被打到这个血线以下，那么播放一个梯子脱落的动画同时设置对应技能为已施放（即此后不能再重复释放）
            if (!putLadderSkillAbility.IsSkilled())
            {
                animatorController.Play("Drop");
                putLadderSkillAbility.SetSkilled();
            }
            // 移除移速buff
            NumericBox.MoveSpeed.RemovePctAddModifier(velocityBuffModifier);
        }
    }

}
