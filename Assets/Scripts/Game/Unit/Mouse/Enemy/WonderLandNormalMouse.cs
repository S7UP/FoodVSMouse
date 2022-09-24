using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 奇境基本老鼠
/// </summary>
public class WonderLandNormalMouse : MouseUnit
{
    private bool isInRainArea; // 是否在下雨区域
    private bool canBlock; // 可否被阻挡
    private int slideState; // 滑行阶段，仅当在下雨区域时有意义 0 前摇 1 行进中 2后摇

    private static FloatModifier rainModifier = new FloatModifier(100); // 下雨+100%移速BUFF

    public override void MInit()
    {
        isInRainArea = false;
        canBlock = true;
        slideState = 0;
        base.MInit();
        // 公爵夫人鼠免疫减速和冰冻减速
        if (mShape == 1)
        {
            NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, new BoolModifier(true));
            NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreSlowDown, new BoolModifier(true));
        }
    }

    public override void OnMoveStateEnter()
    {
        if (isInRainArea)
        {
            // 雨中滑行时不阻挡
            canBlock = false;
            if (slideState == 0)
                animatorController.Play("PreSlide");
            else if (slideState == 1)
                animatorController.Play("Slide", true);
            else if (slideState == 2)
                animatorController.Play("PostSlide");
        }
        else
        {
            // 不在雨中滑行时就可以阻挡
            canBlock = true;
            base.OnMoveStateEnter();
        }
    }

    public override bool CanBlock(BaseUnit unit)
    {
        return canBlock && base.CanBlock(unit);
    }

    public override void AfterGeneralAttack()
    {
        base.AfterGeneralAttack();
        // 如果一次攻击完后，该单位已在雨中则取消阻挡者并且开始滑行
        if (isInRainArea)
        {
            SetNoCollideAllyUnit();
            SetActionState(new MoveState(this));
        }
    }

    public override void OnMoveState()
    {
        base.OnMoveState();
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            if (slideState == 0)
            {
                slideState = 1;
                SetActionState(new MoveState(this));
            }
            // 对于1行进中的情况，由外界解除并转向2后摇
            else if (slideState == 2)
            {
                slideState = 0;
                SetActionState(new MoveState(this));
            }
        }
    }

    /// <summary>
    /// 设置在雨中
    /// </summary>
    /// <param name="isIn"></param>
    public void SetInRainArea()
    {
        isInRainArea = true;
        // 在非攻击状态时都强制转成滑行状态，攻击状态则需要A完那下再滑（在上面实现）
        if(!(mCurrentActionState is AttackState))
            SetActionState(new MoveState(this)); 
        NumericBox.MoveSpeed.RemovePctAddModifier(rainModifier);
        NumericBox.MoveSpeed.AddPctAddModifier(rainModifier);
    }

    /// <summary>
    /// 设置不在雨中
    /// </summary>
    public void SetOutOfRainArea()
    {
        // 设置为阶段2
        slideState = 2;
        SetActionState(new MoveState(this));
        // 取消雨中判定一定要在设置状态之后，这样才能触发滑行后摇而非普通移动
        isInRainArea = false;
        NumericBox.MoveSpeed.RemovePctAddModifier(rainModifier);
    }
}
