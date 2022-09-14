using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 瓜皮护罩
/// </summary>
public class MelonShield : FoodUnit
{
    private int mHertIndex; // 受伤阶段 0：正常 1：小伤 2：重伤
    private SpriteRenderer Spr_Inside;
    private List<float> mHertRateList = new List<float>()
    {
        0.67f, 0.33f
    };

    public override void Awake()
    {
        base.Awake();
        Spr_Inside = transform.Find("Ani_Food").Find("Spr_Inside").GetComponent<SpriteRenderer>();
        typeAndShapeToLayer = 1; // 渲染层级应该要大一点s
    }

    // 单位被对象池回收时触发
    public override void OnDisable()
    {
        base.OnDisable();
        Spr_Inside.material = defaultMaterial; // 换回来
    }

    public override void MInit()
    {
        base.MInit();

        // 在受到伤害结算之后，更新受伤贴图状态
        AddActionPointListener(ActionPointType.PostReceiveDamage, delegate { UpdateHertMap(); });
        // 一转后在受到伤害结算之前计算反伤
        if(mShape>0)
            AddActionPointListener(ActionPointType.PreReceiveDamage, ReBoundDamage);
        // 在接收治疗结算之后，更新受伤贴图状态
        AddActionPointListener(ActionPointType.PostReceiveCure, delegate { UpdateHertMap(); });
        Spr_Inside.sprite = GameManager.Instance.GetSprite("Food/" + mType + "/inside/" + mHertIndex);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        mHertIndex = 0;
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
        animatorController.Play("Idle"+ mHertIndex);
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
            Spr_Inside.sprite = GameManager.Instance.GetSprite("Food/" + mType + "/inside/" + mHertIndex);
        }
    }

    public override void OnBurnStateEnter()
    {
        // 后面也要装上烧毁材质
        Spr_Inside.material = GameManager.Instance.GetMaterial("Dissolve2");
        base.OnBurnStateEnter();
    }

    public override void DuringBurn(float _Threshold)
    {
        Spr_Inside.material.SetFloat("_Threshold", _Threshold);
        base.DuringBurn(_Threshold);
    }

    /// <summary>
    /// 反弹伤害
    /// </summary>
    private void ReBoundDamage(CombatAction action)
    {
        DamageAction dmgAction = action as DamageAction;
        // 至少一转才能反伤
        if (mShape > 0)
        {
            // 添加对应的判定检测器
            {
                GameObject instance = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "AreaEffect/DamageAreaEffect");
                DamageAreaEffectExecution dmgEffect = instance.GetComponent<DamageAreaEffectExecution>();
                // 反弹伤害不应该超过自身当前生命值
                dmgEffect.Init(this, CombatAction.ActionType.ReboundDamage, Mathf.Min(dmgAction.DamageValue, mCurrentHp), GetRowIndex(), 3, 3, 0, 0, false, true);
                dmgEffect.transform.position = this.GetPosition();
                GameController.Instance.AddAreaEffectExecution(dmgEffect);
            }
        }
    }

    /// <summary>
    /// 更新特殊组件层级，在父类中该方法会被调用
    /// </summary>
    /// <param name="arrayIndex"></param>
    public override void UpdateSpecialRenderLayer()
    {
        Spr_Inside.sortingOrder = LayerManager.CalculateSortingLayer(LayerManager.UnitType.Ally, GetRowIndex(), -25, 0);
    }

    /// <summary>
    /// 一转前提下死亡时若剩余生命值大于0则对范围内所有敌人造成一次剩余生命值的伤害
    /// 此方法用于一些即死机制、强制移除时触发
    /// </summary>
    public override void AfterDeath()
    {
        if (mShape>0 && mCurrentHp > 0)
        {
            // 添加对应的判定检测器
            {
                GameObject instance = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "AreaEffect/DamageAreaEffect");
                DamageAreaEffectExecution dmgEffect = instance.GetComponent<DamageAreaEffectExecution>();
                dmgEffect.Init(this, CombatAction.ActionType.ReboundDamage, mCurrentHp, GetRowIndex(), 3, 3, 0, 0, false, true);
                dmgEffect.transform.position = this.GetPosition();
                GameController.Instance.AddAreaEffectExecution(dmgEffect);
            }
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

    public override List<SpriteRenderer> GetSpriteRendererList()
    {
        List<SpriteRenderer> l = new List<SpriteRenderer>();
        l.Add(GetSpriteRenderer());
        l.Add(Spr_Inside);
        return l;
    }
}
