using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 土司面包
/// </summary>
public class ToastBread : FoodUnit
{
    private const string ToastBreadHealEffectKey = "ToastBreadHealEffectKey";
    private static Sprite HealEffect_Sprite;
    private int mHertIndex; // 受伤阶段 0：正常 1：小伤 2：重伤
    private int noHealTimeLeft; // 非自然回复倒计时
    private List<float> mHertRateList = new List<float>()
    {
        0.67f, 0.33f
    };

    public override void Awake()
    {
        if (HealEffect_Sprite == null)
            HealEffect_Sprite = GameManager.Instance.GetSprite("Food/38/HealEffect");
        base.Awake();
    }

    public override void MInit()
    {
        mHertIndex = 0;
        noHealTimeLeft = 0;
        base.MInit();

        // 在受到伤害结算之后，更新受伤贴图状态
        AddActionPointListener(ActionPointType.PostReceiveDamage, delegate { UpdateHertMap(); noHealTimeLeft = 300; });
        AddActionPointListener(ActionPointType.PostReceiveReboundDamage, delegate { UpdateHertMap(); noHealTimeLeft = 300; });
        // 在接收治疗结算之后，更新受伤贴图状态
        AddActionPointListener(ActionPointType.PostReceiveCure, delegate { UpdateHertMap(); });
    }

    public override void MUpdate()
    {
        // 自然回复功能
        if (noHealTimeLeft > 0)
        {
            noHealTimeLeft--;
            if (IsContainEffect(ToastBreadHealEffectKey))
            {
                RemoveEffectFromDict(ToastBreadHealEffectKey);
            }
        }
        else
        {
            if (!IsContainEffect(ToastBreadHealEffectKey))
            {
                BaseEffect e = BaseEffect.CreateInstance(HealEffect_Sprite);
                GameController.Instance.AddEffect(e);
                AddEffectToDict(ToastBreadHealEffectKey, e, Vector2.zero);
            }
            new CureAction(CombatAction.ActionType.GiveCure, this, this, mCurrentAttack / 60).ApplyAction();
        }
        base.MUpdate();
    }

    /// <summary>
    /// 根据等级表和等级来更新对应数据
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        SetMaxHpAndCurrentHp((float)(attr.baseAttrbute.baseHP + attr.valueList[mLevel]));
    }


    public override void OnIdleStateEnter()
    {
        animatorController.Play("Idle" + mHertIndex);
    }

    /// <summary>
    /// 当受伤或者被治疗时，更新单位贴图状态
    /// </summary>
    protected void UpdateHertMap()
    {
        // 要是死了的话就免了吧
        if (isDeathState)
            return;

        // 是否要切换控制器的flag
        bool flag = false;
        // 恢复到上一个受伤贴图检测
        while (mHertIndex > 0 && GetHeathPercent() > mHertRateList[mHertIndex - 1])
        {
            mHertIndex--;
            flag = true;
        }
        // 下一个受伤贴图的检测
        while (mHertIndex < mHertRateList.Count && GetHeathPercent() <= mHertRateList[mHertIndex])
        {
            mHertIndex++;
            flag = true;
        }
        // 有切换通知时才切换
        if (flag)
        {
            animatorController.Play("Idle" + mHertIndex);
        }
    }


    /////////////////////////////////以下功能均失效，不需要往下翻看/////////////////////////////////////

    /// <summary>
    /// 判断是否有有效的攻击目标
    /// </summary>
    /// <returns></returns>
    protected override bool IsHasTarget()
    {
        // 功能型卡片不需要
        return false;
    }

    /// <summary>
    /// 是否满足普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        // 功能型卡片不需要
        return true;
    }

    /// <summary>
    /// 进入普通攻击动画状态
    /// </summary>
    public override void BeforeGeneralAttack()
    {
        // 功能型卡片无攻击状态
    }

    /// <summary>
    /// 普通攻击期间
    /// </summary>
    public override void OnGeneralAttack()
    {
        // 功能型卡片无
    }

    /// <summary>
    /// 退出普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetEndGeneralAttackCondition()
    {
        // 功能型卡片无
        return true;
    }

    /// <summary>
    /// 退出普通攻击后要做的事
    /// </summary>
    public override void AfterGeneralAttack()
    {
        // 功能型卡片无
    }

    /// <summary>
    /// 是否为伤害判定时刻（近战攻击为打出实际伤害，远程攻击为确定发射弹体）
    /// </summary>
    /// <returns></returns>
    public override bool IsDamageJudgment()
    {
        // 功能型卡片无
        return false;
    }

    /// <summary>
    /// 执行具体的攻击，位于伤害判定为真之后
    /// </summary>
    public override void ExecuteDamage()
    {
        // 功能型卡片无
    }
}
