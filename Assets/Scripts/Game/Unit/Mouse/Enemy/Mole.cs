using UnityEngine;
using System;
using S7P.Numeric;
/// <summary>
/// 鼹鼠类
/// </summary>
public class Mole : MouseUnit
{
    // 分为3P，P1在地下,P2在地面正常态，P3在地面受伤态
    private bool isAppear; // 是否出土
    private bool isMoveLeft; // 出土后是否向左移动
    private BoolModifier IgnoreFrozen = new BoolModifier(true);
    private BoolModifier IgnoreFrozenSlowDown = new BoolModifier(true);
    private FloatModifier speedModifier;
    private static Func<BaseUnit, BaseUnit, bool> noBeSelectedAsTargetFunc = delegate { return false; };
    private static Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate { return false; };
    private static Func<BaseUnit, BaseUnit, bool> noBlockFunc = delegate { return false; };
    private int stunTimeLeft;

    public override void MInit()
    {
        isAppear = false;
        isMoveLeft = false;
        speedModifier = null;
        stunTimeLeft = 120;
        base.MInit();
        // 初始在地下时免疫冰冻效果
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, IgnoreFrozen);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, IgnoreFrozenSlowDown);
        // 初始在地下时增加基础移速300%，锯刀狂鼠为500%
        if (mShape==2 || mShape == 5)
            speedModifier = new FloatModifier(500f);
        else
            speedModifier = new FloatModifier(300f);
        NumericBox.MoveSpeed.AddPctAddModifier(speedModifier);
        // 初始高度为-1
        mHeight = -1;
        // 初始不可作为攻击目标、不可被攻击、也不阻挡不攻击
        AddCanBeSelectedAsTargetFunc(noBeSelectedAsTargetFunc);
        AddCanHitFunc(noHitFunc);
        AddCanBlockFunc(noBlockFunc);
        GameManager.Instance.audioSourceManager.PlayEffectMusic("Dig");
    }

    /// <summary>
    /// 执行出土
    /// </summary>
    public void ExecuteAppear(bool isMoveLeft)
    {
        // 若此时控制器还处于P1阶段，则需要强制转成P2阶段
        if (mHertIndex < 1)
        {
            mHertRateList[mHertIndex] = 1.0f;
            UpdateHertMap();
        }

        isAppear = true;
        this.isMoveLeft = isMoveLeft;
        // 改朝向
        SetActionState(new TransitionState(this));
    }

    /// <summary>
    /// 当在地下时血量下降到第二或第三阶段时强制出土并且左行
    /// </summary>
    public override void OnUpdateRuntimeAnimatorController()
    {
        if (mHertIndex > 0 && mHertIndex <= 2 && !isAppear)
        {
            ExecuteAppear(true);
        }
    }

    public override void OnMoveStateEnter()
    {
        base.OnMoveStateEnter();
        if (isAppear)
        {
            if (isMoveLeft)
            {
                // 改朝向
                SetLocalScale(new Vector2(-1, 1));
                SetMoveRoate(Vector2.left);
            }
            else
            {
                SetMoveRoate(Vector2.right);
            }
        }
        else
        {
            SetMoveRoate(Vector2.left);
        }

    }

    public override void OnMoveState()
    {
        base.OnMoveState();
        // 当移动到左一列格子最左边缘时出土，但是是逆行
        if(!isAppear && transform.position.x < MapManager.GetColumnX(-0.4f))
        {
            ExecuteAppear(false);
        }
    }

    /// <summary>
    /// 进入转场状态时要做的事
    /// </summary>
    public override void OnTransitionStateEnter()
    {
        if (isMoveLeft)
        {
            animatorController.Play("Appear0");
        }
        else
        {
            animatorController.Play("Appear1");
        }
            
    }

    public override void OnTransitionState()
    {
        // 动画播放完一次后，转为移动状态
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            // 不再免疫冻结状态
            NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreFrozen, IgnoreFrozen);
            NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, IgnoreFrozenSlowDown);
            SetActionState(new CastState(this));
            // 移除加速效果
            NumericBox.MoveSpeed.RemovePctAddModifier(speedModifier);
            // 高度置为正常高度0
            mHeight = 0;
            // 移除不可作为攻击目标、不可被攻击
            RemoveCanBeSelectedAsTargetFunc(noBeSelectedAsTargetFunc);
            RemoveCanHitFunc(noHitFunc);
        }
    }

    public override void OnTransitionStateExit()
    {
        
    }

    public override void OnCastStateEnter()
    {
        animatorController.Play("Stun", true);
    }

    public override void OnCastState()
    {
        if (stunTimeLeft > 0)
        {
            stunTimeLeft--;
        }
        else
        {
            // 现在可以被阻挡了
            RemoveCanBlockFunc(noBlockFunc);
            SetActionState(new MoveState(this));
        }
    }
}
