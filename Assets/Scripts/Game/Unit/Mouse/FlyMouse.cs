/// <summary>
/// 基础空军
/// </summary>
public class FlyMouse : MouseUnit, IFlyUnit
{
    private bool isDrop; // 是否被击落
    private int dropColumn; // 降落列
    private FloatModifier floatModifier = new FloatModifier(100);

    public override void MInit()
    {
        base.MInit();
        isDrop = false;
        dropColumn = 0; // 降落列默认为0，即左一列
        // 飞行状态下获取100%移速加成
        NumericBox.MoveSpeed.AddPctAddModifier(floatModifier);
        // 6号是机械举旗鼠，免疫炸弹直接秒杀效果
        if (mShape == 6)
            NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, new BoolModifier(true));
    }

    public override void MUpdate()
    {
        base.MUpdate();
        if (IsMeetDropCondition())
        {
            ExecuteDrop();
        }
    }

    /// <summary>
    /// 检测是否满足降落条件
    /// </summary>
    private bool IsMeetDropCondition()
    {
        return (GetColumnIndex() <= dropColumn && !isDrop);
    }

    /// <summary>
    /// 执行降落，仅一次
    /// </summary>
    public void ExecuteDrop()
    {
        if (!isDrop)
        {
            isDrop = true;
            // 若当前生命值大于飞行状态临界点，则需要强制同步生命值至临界点
            if (mCurrentHp > mHertRateList[0] * mMaxHp)
            {
                mCurrentHp = (float)mHertRateList[0] * mMaxHp;
            }
            mHertRateList[0] = double.MaxValue;
            UpdateHertMap(); // 强制更新一次贴图
            // 设为转场状态，该状态下的具体实下如下几个方法
            SetActionState(new TransitionState(this));
            // 取消飞行状态移除移速加成
            NumericBox.MoveSpeed.RemovePctAddModifier(floatModifier);
            // 如果是位于最后一列降落的，则直接触发猫猫
            if(GetColumnIndex() <= 0)
            {
                BaseCat cat = GameController.Instance.mItemController.GetSpecificRowCat(GetRowIndex());
                cat.OnTriggerEvent(); // 这个方法里已经包括了是否触发的判定，不用担心反复触发的问题
            }
        }
    }


    /// <summary>
    /// 当处于下落状态时，应当完全不被子弹击中
    /// </summary>
    /// <param name="bullet"></param>
    /// <returns></returns>
    public override bool CanHit(BaseBullet bullet)
    {
        return !(mCurrentActionState is TransitionState) && base.CanHit(bullet);
    }

    /// <summary>
    /// 当贴图更新时要做的事
    /// </summary>
    public override void OnUpdateRuntimeAnimatorController()
    {
        // 当第一次切换到非第0阶段的贴图时（即退出飞行状态），将第0阶段的血量百分比设为超过1.0（即这之后永远达不到），然后播放坠机动画
        // 当前取值范围为1~3时触发
        // 0 飞行
        // 1 击落过程->正常移动
        // 2 受伤移动
        if (mHertIndex > 0 && mHertIndex <= 2 && !isDrop)
        {
            ExecuteDrop();
        }
    }

    /// <summary>
    /// 进入转场状态时要做的事，这里特指进入刚被击落时要做的
    /// </summary>
    public override void OnTransitionStateEnter()
    {
        animatorController.Play("Drop");
    }

    public override void OnTransitionState()
    {
        if (currentStateTimer <= 0)
        {
            return;
        }
        // 动画播放完一次后，转为移动状态
        //if (AnimatorManager.GetCurrentFrame(animator) == AnimatorManager.GetTotalFrame(animator)) 
        if(animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            SetActionState(new MoveState(this));
        }
    }

    public override void OnTransitionStateExit()
    {
        mHeight = 0; // 高度降低为地面高度
    }
}
