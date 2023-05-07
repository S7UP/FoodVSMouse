using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 巧克力面包
/// </summary>
public class ChocolateBread : FoodUnit
{
    private const string ShieldEffectKey = "ChocolateBreadShield";
    private static Sprite Shield_Sprite; 

    private int mHertIndex; // 受伤阶段 0：正常 1：小伤 2：重伤
    private List<float> mHertRateList = new List<float>()
    {
        0.67f, 0.33f
    };
    private int MaxShieldCount; // 圣盾层数上限
    private int CurrentShieldCount; // 当前圣盾层数
    private const int GetShieldTime = 600; // 获取一层圣盾所需要的时间
    private int nextShieldTimeLeft; // 获取下一个圣盾所需要的时间
    private int decTimeWhenHit; // 受伤时减少的获取下一个圣盾的时间

    public override void Awake()
    {
        if (Shield_Sprite == null)
            Shield_Sprite = GameManager.Instance.GetSprite("Effect/Shield");
        base.Awake();
    }

    public override void MInit()
    {
        mHertIndex = 0;
        nextShieldTimeLeft = GetShieldTime;
        CurrentShieldCount = 0;
        base.MInit();
        // 根据转职情况来初始化数值
        switch (mShape)
        {
            case 1:
                MaxShieldCount = 2;
                decTimeWhenHit = 180;
                AddShield();
                break;
            default:
                MaxShieldCount = 1;
                decTimeWhenHit = 120;
                break;
        }
        // 在受到伤害结算之前，尝试消耗护盾把本次伤害值无效化
        AddActionPointListener(ActionPointType.PreReceiveDamage, (action) => { TryToResistDamage(action); });
        // 在受到伤害结算之后，更新受伤贴图状态，并且根据伤害来决定是否会减少护盾的CD
        AddActionPointListener(ActionPointType.PostReceiveDamage, (action) => { UpdateHertMap(); OnHit(action);  });
        // 在接收治疗结算之后，更新受伤贴图状态
        AddActionPointListener(ActionPointType.PostReceiveCure, delegate { UpdateHertMap(); });
    }

    public override void MUpdate()
    {
        // 圣盾恢复计算
        if(CurrentShieldCount < MaxShieldCount)
        {
            nextShieldTimeLeft--;
            if (nextShieldTimeLeft <= 0)
            {
                nextShieldTimeLeft += GetShieldTime;
                AddShield();
            }
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

    /// <summary>
    /// 增加一层圣盾
    /// </summary>
    public void AddShield()
    {
        if(CurrentShieldCount < MaxShieldCount)
        {
            CurrentShieldCount++;
        }
        // 如果是从0加到1
        if(CurrentShieldCount == 1)
        {
            BaseEffect e = BaseEffect.CreateInstance(Shield_Sprite);
            GameController.Instance.AddEffect(e);
            AddEffectToDict(ShieldEffectKey, e, Vector2.zero);
        }
    }

    /// <summary>
    /// 减少一层圣盾
    /// </summary>
    public void DecShield()
    {
        if (CurrentShieldCount > 0)
        {
            CurrentShieldCount--;
        }
        // 如果减到0
        if(CurrentShieldCount == 0)
        {
            RemoveEffectFromDict(ShieldEffectKey);
        }
    }

    /// <summary>
    /// 挨打会减少圣盾时间
    /// </summary>
    /// <param name="action"></param>
    private void OnHit(CombatAction action)
    {
        // 满层圣盾是不会减时间的（但是这种情况真的可能发生吗）
        if (CurrentShieldCount == MaxShieldCount)
            return;

        if (action is DamageAction)
        {
            DamageAction dmgAction = action as DamageAction;
            if(dmgAction.DamageValue >= 10)
            {
                nextShieldTimeLeft -= decTimeWhenHit;
            }
        }
    }

    /// <summary>
    /// 尝试抵御一次伤害
    /// </summary>
    private void TryToResistDamage(CombatAction action)
    {
        if (CurrentShieldCount <= 0)
            return;

        if(action is DamageAction)
        {
            DamageAction dmgAction = action as DamageAction;
            dmgAction.DamageValue = 0; // 强制这次伤害为0
            DecShield();
            // 对5*5所有友方单位进行一次生命回复效果
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, 5, 5, "ItemCollideAlly");
            r.SetInstantaneous();
            r.isAffectFood = true;
            r.SetOnFoodEnterAction((unit) => {
                EffectManager.AddHealEffectToUnit(unit);
                new CureAction(CombatAction.ActionType.GiveCure, this, unit, mCurrentAttack).ApplyAction();
            });
            GameController.Instance.AddAreaEffectExecution(r);
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
