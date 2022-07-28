using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 熊猫类
/// </summary>
public class PandaMouse : MouseUnit
{
    private GeneralAttackSkillAbility generalAttackSkillAbility; // 平A技能
    private ThrowLittleMouseSkillAbility throwLittleMouseSkillAbility; // 投掷二哥技能

    private float throwPercent; // 扔二哥所要到达的血线百分比

    public override void MInit()
    {
        base.MInit();
        // 防爆
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, new BoolModifier(true));
        throwPercent = 0.5f;
    }

    public override void MUpdate()
    {
        base.MUpdate();
        if (IsMeetingThrowCondition())
        {
            ExcuteThrow();
        }
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

        // 投掷小老鼠技能
        if (infoList.Count > 1)
        {
            throwLittleMouseSkillAbility = new ThrowLittleMouseSkillAbility(this, infoList[1]);
            skillAbilityManager.AddSkillAbility(throwLittleMouseSkillAbility);
            throwLittleMouseSkillAbility.master = this;
        }
    }

    /// <summary>
    /// 投掷二哥条件
    /// </summary>
    /// <returns></returns>
    private bool IsMeetingThrowCondition()
    {
        return (GetHeathPercent() < throwPercent && !throwLittleMouseSkillAbility.isThrow && GetColumnIndex()>2);
    }

    /// <summary>
    /// 执行投掷，仅一次
    /// </summary>
    private void ExcuteThrow()
    {
        throwLittleMouseSkillAbility.SetSkillConditionEnable();
    }

    public override void OnCastState()
    {
        if (currentStateTimer <= 0)
        {
            return;
        }
        // 动画播放完一次后，退出技能状态
        if (AnimatorManager.GetNormalizedTime(animator)>1.0)
        {
            if (throwLittleMouseSkillAbility != null)
            {
                throwLittleMouseSkillAbility.CloseSkill();
            }
        }
        else if (AnimatorManager.GetNormalizedTime(animator) > 0.4 && mAttackFlag)
        {
            mAttackFlag = false;
            if (throwLittleMouseSkillAbility != null)
            {
                // 投掷落点X坐标为  自身X坐标再向左走四格  与  左二列中心点X 坐标中的较小值，即最远也只能扔到左二列
                float targetX = Mathf.Max( transform.position.x - 4 * GameController.Instance.mMapController.gridWidth, MapManager.GetColumnX(1));
                throwLittleMouseSkillAbility.ThrowEntity(new Vector3(targetX, MapManager.GetRowY(GetRowIndex()), 0));
            }
        }
    }

    public override void OnCastStateExit()
    {
        mAttackFlag = true;
    }

    /// <summary>
    /// 执行具体的攻击，位于伤害判定为真之后
    /// </summary>
    public override void ExecuteDamage()
    {
        if (IsHasTarget())
        {
            // 单格内群伤
            BaseGrid grid = GetCurrentTarget().GetGrid();
            if (grid != null)
            {
                foreach (var item in grid.GetAttackableFoodUnitList())
                {
                    TakeDamage(item);
                }
            }
            else
            {
                TakeDamage(GetCurrentTarget());
            }
        }
            
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
        // 切换时的第一帧直接不执行update()，因为下述的info.normalizedTime的值还停留在上一个状态，逻辑会出问题！
        if (currentStateTimer <= 0)
        {
            return;
        }
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
        return animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f; // 攻击动画播放完整一次后视为技能结束
    }


    public override void OnIdleStateEnter()
    {
        if (throwLittleMouseSkillAbility.isThrow)
        {
            animatorController.Play("Idle1", true);
        }
        else
        {
            animatorController.Play("Idle0", true);
        }
        
    }

    public override void OnMoveStateEnter()
    {
        if (throwLittleMouseSkillAbility.isThrow)
        {
            animatorController.Play("Move1", true);
        }
        else
        {
            animatorController.Play("Move0", true);
        }
    }

    public override void OnAttackStateEnter()
    {
        if (throwLittleMouseSkillAbility.isThrow)
        {
            animatorController.Play("Attack1");
        }
        else
        {
            animatorController.Play("Attack0");
        }
    }

    public override void OnCastStateEnter()
    {
        animatorController.Play("Cast");
    }
}
