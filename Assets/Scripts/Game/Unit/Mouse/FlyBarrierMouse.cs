/// <summary>
/// 飞行路障鼠
/// </summary>
public class FlyBarrierMouse : MouseUnit, IFlyUnit
{
    private bool isDrop; // 是否被击落
    private int dropColumn; // 降落列
    private bool isDropBarrier;
    private int minDropBarrierColumn; // 降落障碍列
    private int maxDropBarrierColumn;
    private FloatModifier speedRateModifier = new FloatModifier(100.0f); // 飞行状态下1.5倍速

    public override void MInit()
    {
        base.MInit();
        isDrop = false;
        dropColumn = 0; // 降落列默认为0，即左一列
        isDropBarrier = false;
        minDropBarrierColumn = 4; // 右五列
        maxDropBarrierColumn = 6; // 右三列
        NumericBox.MoveSpeed.AddFinalPctAddModifier(speedRateModifier);
    }

    public override void MUpdate()
    {
        base.MUpdate();

    }

    public override void OnMoveState()
    {
        base.OnMoveState();
        if (IsMeetDropBarrierCondition())
        {
            SetActionState(new CastState(this));
        }

        if (IsMeetDropCondition())
        {
            ExecuteDrop();
        }
    }

    public override void OnCastStateEnter()
    {
        animatorController.Play("Move", true);
    }

    public override void OnCastState()
    {
        if (animatorController.GetCurrentAnimatorStateRecorder().timer > 60)
        {
            ExcuteDropBarrier();
            SetActionState(new MoveState(this));
        }
    }

    /// <summary>
    /// 是否满足掉落障碍条件
    /// </summary>
    /// <returns></returns>
    private bool IsMeetDropBarrierCondition()
    {
        if (isDrop || isDropBarrier)
            return false;
        int index = GetColumnIndex();
        // 当在可掉落障碍范围内，且当前格子下有可攻击目标时，掉落障碍
        if(index > minDropBarrierColumn && index <= maxDropBarrierColumn)
        {
            BaseGrid g = GameController.Instance.mMapController.GetGrid(GetColumnIndex(), GetRowIndex());
            if(g!=null)
            {
                foreach (var item in g.GetAttackableFoodUnitList())
                {
                    if (item.CanBeSelectedAsTarget())
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        else if(index == minDropBarrierColumn)
        {
            // 达到最远距离强制掉猪
            return true;
        }
        return false;
    }

    /// <summary>
    /// 执行掉落障碍，仅一次
    /// </summary>
    private void ExcuteDropBarrier()
    {
        if (!isDropBarrier)
        {
            isDropBarrier = true;
            // 设置生命值为第一阶段百分比
            mCurrentHp = (float)(mMaxHp * mHertRateList[0]);
            mHertRateList[0] = double.MaxValue;
            UpdateHertMap(); // 通过强制改变HertRateList然后强制更新，转变阶段
            // 掉落障碍
            InvincibilityBarrier b = GameController.Instance.CreateItem(GetColumnIndex(), GetRowIndex(), (int)ItemInGridType.TimelinessBarrier, 0) as InvincibilityBarrier;
            b.SetLeftTime(900); // 15s
            // 移除障碍处美食
            if (b.GetGrid() != null)
            {
                foreach (var item in b.GetGrid().GetAttackableFoodUnitList())
                {
                    if (!item.NumericBox.GetBoolNumericValue(StringManager.Invincibility) && !item.tag.Equals("Character") && item.CanBeSelectedAsTarget())
                    {
                        item.ExecuteDeath();
                    }
                }
            }
        }
    }

    /// <summary>
    /// 执行清除障碍，仅一次，与上属于互斥事件，二者只能出现一，该事件触发条件是上述事件触发前血线先低于第一阶段
    /// </summary>
    private void ExcuteRemoveBarrier()
    {
        if (!isDropBarrier)
        {
            isDropBarrier = true;
            // 设置生命值为第一阶段百分比
            mCurrentHp = (float)(mMaxHp * mHertRateList[0]);
            mHertRateList[0] = double.MaxValue;
            UpdateHertMap(); // 通过强制改变HertRateList然后强制更新，转变阶段
        }
    }

    /// <summary>
    /// 检测是否满足降落条件
    /// </summary>
    private bool IsMeetDropCondition()
    {
        return (!isDrop && GetColumnIndex() <= dropColumn);
    }

    /// <summary>
    /// 执行降落，仅一次
    /// </summary>
    public void ExecuteDrop()
    {
        if (!isDrop)
        {
            isDrop = true;
            isDropBarrier = true;
            // 若当前生命值大于飞行状态临界点，则需要强制同步生命值至临界点
            if (mCurrentHp > mHertRateList[1] * mMaxHp)
            {
                mCurrentHp = (float)mHertRateList[1] * mMaxHp;
            }
            mHertRateList[0] = double.MaxValue;
            mHertRateList[1] = double.MaxValue;
            UpdateHertMap(); // 通过强制改变HertRateList然后强制更新，转变阶段
            NumericBox.MoveSpeed.RemoveFinalPctAddModifier(speedRateModifier); // 恢复正常走路速度
            // 设为转场状态，该状态下的具体实下如下几个方法
            SetActionState(new TransitionState(this));
            // 如果是位于最后一列降落的，则直接触发猫猫
            if (GetColumnIndex() <= 0)
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
        // 0 飞行携带障碍
        // 1 飞行但不携带障碍
        // 2 击落过程->正常移动
        // 3 受伤移动
        if(mHertIndex > 0 && !isDropBarrier)
        {
            ExcuteRemoveBarrier();
        }

        if (mHertIndex > 1 && mHertIndex <= 2 && !isDrop)
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
        if (AnimatorManager.GetCurrentFrame(animator) == AnimatorManager.GetTotalFrame(animator))
        {
            SetActionState(new MoveState(this));
        }
    }

    public override void OnTransitionStateExit()
    {
        mHeight = 0; // 高度降低为地面高度
    }
}
