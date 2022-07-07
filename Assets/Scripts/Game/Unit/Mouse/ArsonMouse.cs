using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 纵火鼠
/// </summary>
public class ArsonMouse : MouseUnit
{
    private GeneralAttackSkillAbility generalAttackSkillAbility; // 平A技能
    private ThrowBombSkillAbility throwBombSkillAbility; // 投掷炸弹技能
    private bool isFindTarget;
    private Vector3 targetPosition;
    public int preTime; // 预警时间

    public override void MInit()
    {
        base.MInit();
        isFindTarget = false;
    }

    public override void MUpdate()
    {
        base.MUpdate();
        // 超过右二列时自动扔炸弹
        if (!throwBombSkillAbility.isThrow && GetColumnIndex() < 7)
        {
            SearchTargetPosition();
            if (isFindTarget)
            {
                ExcuteThrow();
            }
        }
    }

    /// <summary>
    /// 加载技能，加载普通攻击与技能
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
        // 投掷炸弹技能
        if (infoList.Count > 1)
        {
            throwBombSkillAbility = new ThrowBombSkillAbility(this, infoList[1]);
            skillAbilityManager.AddSkillAbility(throwBombSkillAbility);
            throwBombSkillAbility.master = this;
        }

    }

    /// <summary>
    /// 第一次进入攻击状态时，使用投掷攻击取代普通攻击
    /// </summary>
    public override void OnAttackStateEnter()
    {
        if (!throwBombSkillAbility.isThrow)
        {
            SearchTargetPosition();
            if (isFindTarget)
            {
                ExcuteThrow();
            }
            // 打断当前攻击
            generalAttackSkillAbility.EndActivate();
        }
        else
        {
            base.OnAttackStateEnter();
            Debug.Log("base.OnAttackStateEnter()");
        }
    }

    /// <summary>
    /// 执行投掷，仅一次
    /// </summary>
    private void ExcuteThrow()
    {
        throwBombSkillAbility.SetSkillConditionEnable();
    }

    /// <summary>
    /// 需要根据投掷情况来调整移动贴图
    /// </summary>
    public override void OnMoveStateEnter()
    {
        if (throwBombSkillAbility.isThrow)
        {
            animatorController.Play("Move1", true);
        }
        else
        {
            animatorController.Play("Move0", true);
        }
    }

    public override void OnCastStateEnter()
    {
        if (throwBombSkillAbility.IsFinishPreCast())
        {
            animatorController.Play("Cast");
        }
        else
        {
            animatorController.Play("PreCast");
        }
    }

    public override void OnCastState()
    {
        if (currentStateTimer <= 0)
        {
            return;
        }
        // 先等待技能是否有完成预警动作，完成后才能执行后面的投掷时机判定逻辑
        if (!throwBombSkillAbility.IsFinishPreCast())
            return;

        // 动画播放完一次后，退出技能状态
        if (AnimatorManager.GetCurrentFrame(animator) == AnimatorManager.GetTotalFrame(animator))
        {
            if (throwBombSkillAbility != null)
            {
                throwBombSkillAbility.CloseSkill();
            }
        }else if (AnimatorManager.GetNormalizedTime(animator) > 0.5 && mAttackFlag)
        {
            mAttackFlag = false;
            if (throwBombSkillAbility != null)
            {
                SearchTargetPosition();  // 投出弹体前再检测一次
                throwBombSkillAbility.ThrowEntity(targetPosition);
            }
        }
    }

    public override void OnCastStateExit()
    {
        mAttackFlag = true;
    }

    /// <summary>
    /// 寻找投掷落点
    /// </summary>
    private void SearchTargetPosition()
    {
        int rowIndex = GetRowIndex();
        List<BaseUnit> list = GameController.Instance.GetAllyList()[rowIndex];
        // 找当前行靠最后的单位
        bool flag = false;
        int minColumnIndex = GetColumnIndex() + 1; // 设置初始索引为自己所在列再右移一格，以表明检索范围为从最左边到自己所在格子
        if (list.Count > 0)
        {
            foreach (var item in list)
            {
                if (!item.CanBeSelectedAsTarget())
                    continue;
                int temp = item.GetColumnIndex();
                if (temp < minColumnIndex)
                {
                    minColumnIndex = temp;
                    flag = true;
                }
            }
        }
        if (flag)
        {
            isFindTarget = true;
            targetPosition = MapManager.GetGridLocalPosition(minColumnIndex, rowIndex);
            return;
        }

        // 如果没有，就找上下两行最靠后的单位
        // 上行
        if (rowIndex > 0)
        {
            list = GameController.Instance.GetAllyList()[rowIndex-1];
            if (list.Count > 0)
            {
                foreach (var item in list)
                {
                    if (!item.CanBeSelectedAsTarget())
                        continue;
                    int temp = item.GetColumnIndex();
                    if (temp < minColumnIndex)
                    {
                        minColumnIndex = temp;
                        flag = true;
                    }
                }
            }
        }
        // 下行
        if (rowIndex < 6)
        {
            list = GameController.Instance.GetAllyList()[rowIndex + 1];
            if (list.Count > 0)
            {
                foreach (var item in list)
                {
                    if (!item.CanBeSelectedAsTarget())
                        continue;
                    int temp = item.GetColumnIndex();
                    if (temp < minColumnIndex)
                    {
                        minColumnIndex = temp;
                        flag = true;
                    }
                }
            }
        }
        // 
        if (flag)
        {
            isFindTarget = true;
            targetPosition = MapManager.GetGridLocalPosition(minColumnIndex, rowIndex);
            return;
        }
        else
        {
            // 你tm不放卡的吗？（三行一张卡都没有）
            // 这种情况下target不会赋值，单位保持继续前进直至有放卡
        }
    }
}
