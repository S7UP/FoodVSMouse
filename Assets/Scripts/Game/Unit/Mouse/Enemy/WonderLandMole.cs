using UnityEngine;
/// <summary>
/// 奇境刺猬
/// </summary>
public class WonderLandMole : MouseUnit
{
    // 分为3P，P1在地下,P2在地面正常态，P3在地面受伤态
    private bool isAppear; // 是否出土
    private bool isMoveLeft; // 出土后是否向左移动
    private bool canBeHited; // 能否被击中
    private static BoolModifier IgnoreFrozen = new BoolModifier(true);
    private static BoolModifier IgnoreFrozenSlowDown = new BoolModifier(true);
    private static BoolModifier IgnoreBombInstantKill = new BoolModifier(true);
    private FloatModifier speedModifier;

    public override void MInit()
    {
        isAppear = false;
        isMoveLeft = false;
        canBeHited = false;
        speedModifier = null;
        base.MInit();
        // 初始在地下时免疫冰冻效果 以及灰烬秒杀效果
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, IgnoreFrozen);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, IgnoreFrozenSlowDown);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, IgnoreBombInstantKill);
        // 初始在地下时增加基础移速300%
        speedModifier = new FloatModifier(300f);
        NumericBox.MoveSpeed.AddPctAddModifier(speedModifier);
        // 初始高度为-1
        mHeight = -1;
    }

    /// <summary>
    /// 当处于遁地状态时完全不被子弹击中
    /// </summary>
    /// <param name="bullet"></param>
    /// <returns></returns>
    public override bool CanHit(BaseBullet bullet)
    {
        return canBeHited && base.CanHit(bullet);
    }

    /// <summary>
    /// 当处于遁地状态时处于不可选取状态
    /// </summary>
    /// <returns></returns>
    public override bool CanBeSelectedAsTarget()
    {
        return canBeHited && base.CanBeSelectedAsTarget();
    }

    /// <summary>
    /// 是否满足普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        // 被阻挡了 且 阻挡对象是有效的
        return canBeHited && base.IsMeetGeneralAttackCondition();
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
        SetActionState(new TransitionState(this));
        // 不再免疫冻结状态和灰烬秒杀
        NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreFrozen, IgnoreFrozen);
        NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, IgnoreFrozenSlowDown);
        NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, IgnoreBombInstantKill);
        // 移除加速效果
        NumericBox.MoveSpeed.RemovePctAddModifier(speedModifier);
        // 高度置为正常高度0
        mHeight = 0;
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
                SetMoveRoate(Vector2.left);
            }
            else
            {
                SetMoveRoate(Vector2.right);                
                // 改朝向
                SetLocalScale(new Vector2(-1, 1));
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
        if (!isAppear && transform.position.x < MapManager.GetColumnX(-0.4f))
            ExecuteAppear(false);
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
            SetActionState(new MoveState(this));
        }
        // 中弹判定
        if (!canBeHited && animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() > 0.5)
            canBeHited = true;
    }

    /// <summary>
    /// 受到一次灰烬伤害就出土
    /// </summary>
    /// <param name="dmg"></param>
    public override void OnBombBurnDamage(float dmg)
    {
        base.OnBombBurnDamage(dmg);
        if (dmg > 0 && !isAppear)
            ExecuteAppear(true);
    }
}
