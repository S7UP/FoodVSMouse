using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D.Path;

using UnityEngine;
/// <summary>
/// 扑克护罩
/// </summary>
public class PokerShield : FoodUnit
{
    private int mHertIndex; // 受伤阶段 0：正常 1：小伤 2：重伤
    private SpriteRenderer Spr_Inside;
    private Animator InsideAnimator;
    private AnimatorController InsideAnimatorController = new AnimatorController();
    private List<float> mHertRateList = new List<float>()
    {
        0.67f, 0.33f
    };

    public override void Awake()
    {
        base.Awake();
        Spr_Inside = transform.Find("Ani_Food").Find("Spr_Inside").GetComponent<SpriteRenderer>();
        InsideAnimator = Spr_Inside.GetComponent<Animator>();
        typeAndShapeToLayer = 1; // 渲染层级应该要大一点
        // 全程免疫毒菌感染
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBacterialInfection, new BoolModifier(true));
    }

    // 单位被对象池回收时触发
    public override void OnDisable()
    {
        base.OnDisable();
        Spr_Inside.material = defaultMaterial; // 换回来
    }

    public override void MInit()
    {
        mHertIndex = 0;
        base.MInit();
        InsideAnimatorController.ChangeAnimator(InsideAnimator);
        // 在受到伤害结算之后，更新受伤贴图状态
        AddActionPointListener(ActionPointType.PostReceiveDamage, delegate { UpdateHertMap(); });
        // 在接收治疗结算之后，更新受伤贴图状态
        AddActionPointListener(ActionPointType.PostReceiveCure, delegate { UpdateHertMap(); });
        // Spr_Inside.sprite = GameManager.Instance.GetSprite("Food/" + mType + "/inside/" + mHertIndex);
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
        InsideAnimatorController.Play("Idle" + mHertIndex);
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
            InsideAnimatorController.Play("Idle" + mHertIndex);
            // Spr_Inside.sprite = GameManager.Instance.GetSprite("Food/" + mType + "/inside/" + mHertIndex);
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
    /// 更新特殊组件层级，在父类中该方法会被调用
    /// </summary>
    /// <param name="arrayIndex"></param>
    public override void UpdateSpecialRenderLayer()
    {
        Spr_Inside.sortingOrder = LayerManager.CalculateSortingLayer(LayerManager.UnitType.Ally, GetRowIndex(), -25, 0);
    }

    public override void MPause()
    {
        base.MPause();
        InsideAnimatorController.Pause();
    }

    public override void MResume()
    {
        base.MPause();
        InsideAnimatorController.Resume();
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
