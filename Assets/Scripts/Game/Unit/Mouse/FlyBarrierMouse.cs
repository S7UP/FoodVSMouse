/// <summary>
/// ����·����
/// </summary>
public class FlyBarrierMouse : MouseUnit, IFlyUnit
{
    private bool isDrop; // �Ƿ񱻻���
    private int dropColumn; // ������
    private bool isDropBarrier;
    private int minDropBarrierColumn; // �����ϰ���
    private int maxDropBarrierColumn;
    private FloatModifier speedRateModifier = new FloatModifier(100.0f); // ����״̬��1.5����

    public override void MInit()
    {
        base.MInit();
        isDrop = false;
        dropColumn = 0; // ������Ĭ��Ϊ0������һ��
        isDropBarrier = false;
        minDropBarrierColumn = 4; // ������
        maxDropBarrierColumn = 6; // ������
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
    /// �Ƿ���������ϰ�����
    /// </summary>
    /// <returns></returns>
    private bool IsMeetDropBarrierCondition()
    {
        if (isDrop || isDropBarrier)
            return false;
        int index = GetColumnIndex();
        // ���ڿɵ����ϰ���Χ�ڣ��ҵ�ǰ�������пɹ���Ŀ��ʱ�������ϰ�
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
            // �ﵽ��Զ����ǿ�Ƶ���
            return true;
        }
        return false;
    }

    /// <summary>
    /// ִ�е����ϰ�����һ��
    /// </summary>
    private void ExcuteDropBarrier()
    {
        if (!isDropBarrier)
        {
            isDropBarrier = true;
            // ��������ֵΪ��һ�׶ΰٷֱ�
            mCurrentHp = (float)(mMaxHp * mHertRateList[0]);
            mHertRateList[0] = double.MaxValue;
            UpdateHertMap(); // ͨ��ǿ�Ƹı�HertRateListȻ��ǿ�Ƹ��£�ת��׶�
            // �����ϰ�
            InvincibilityBarrier b = GameController.Instance.CreateItem(GetColumnIndex(), GetRowIndex(), (int)ItemInGridType.TimelinessBarrier, 0) as InvincibilityBarrier;
            b.SetLeftTime(900); // 15s
            // �Ƴ��ϰ�����ʳ
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
    /// ִ������ϰ�����һ�Σ��������ڻ����¼�������ֻ�ܳ���һ�����¼����������������¼�����ǰѪ���ȵ��ڵ�һ�׶�
    /// </summary>
    private void ExcuteRemoveBarrier()
    {
        if (!isDropBarrier)
        {
            isDropBarrier = true;
            // ��������ֵΪ��һ�׶ΰٷֱ�
            mCurrentHp = (float)(mMaxHp * mHertRateList[0]);
            mHertRateList[0] = double.MaxValue;
            UpdateHertMap(); // ͨ��ǿ�Ƹı�HertRateListȻ��ǿ�Ƹ��£�ת��׶�
        }
    }

    /// <summary>
    /// ����Ƿ����㽵������
    /// </summary>
    private bool IsMeetDropCondition()
    {
        return (!isDrop && GetColumnIndex() <= dropColumn);
    }

    /// <summary>
    /// ִ�н��䣬��һ��
    /// </summary>
    public void ExecuteDrop()
    {
        if (!isDrop)
        {
            isDrop = true;
            isDropBarrier = true;
            // ����ǰ����ֵ���ڷ���״̬�ٽ�㣬����Ҫǿ��ͬ������ֵ���ٽ��
            if (mCurrentHp > mHertRateList[1] * mMaxHp)
            {
                mCurrentHp = (float)mHertRateList[1] * mMaxHp;
            }
            mHertRateList[0] = double.MaxValue;
            mHertRateList[1] = double.MaxValue;
            UpdateHertMap(); // ͨ��ǿ�Ƹı�HertRateListȻ��ǿ�Ƹ��£�ת��׶�
            NumericBox.MoveSpeed.RemoveFinalPctAddModifier(speedRateModifier); // �ָ�������·�ٶ�
            // ��Ϊת��״̬����״̬�µľ���ʵ�����¼�������
            SetActionState(new TransitionState(this));
            // �����λ�����һ�н���ģ���ֱ�Ӵ���èè
            if (GetColumnIndex() <= 0)
            {
                BaseCat cat = GameController.Instance.mItemController.GetSpecificRowCat(GetRowIndex());
                cat.OnTriggerEvent(); // ����������Ѿ��������Ƿ񴥷����ж������õ��ķ�������������
            }
        }
    }


    /// <summary>
    /// ����������״̬ʱ��Ӧ����ȫ�����ӵ�����
    /// </summary>
    /// <param name="bullet"></param>
    /// <returns></returns>
    public override bool CanHit(BaseBullet bullet)
    {
        return !(mCurrentActionState is TransitionState) && base.CanHit(bullet);
    }

    /// <summary>
    /// ����ͼ����ʱҪ������
    /// </summary>
    public override void OnUpdateRuntimeAnimatorController()
    {
        // ����һ���л����ǵ�0�׶ε���ͼʱ�����˳�����״̬��������0�׶ε�Ѫ���ٷֱ���Ϊ����1.0������֮����Զ�ﲻ������Ȼ�󲥷�׹������
        // ��ǰȡֵ��ΧΪ1~3ʱ����
        // 0 ����Я���ϰ�
        // 1 ���е���Я���ϰ�
        // 2 �������->�����ƶ�
        // 3 �����ƶ�
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
    /// ����ת��״̬ʱҪ�����£�������ָ����ձ�����ʱҪ����
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
        // ����������һ�κ�תΪ�ƶ�״̬
        if (AnimatorManager.GetCurrentFrame(animator) == AnimatorManager.GetTotalFrame(animator))
        {
            SetActionState(new MoveState(this));
        }
    }

    public override void OnTransitionStateExit()
    {
        mHeight = 0; // �߶Ƚ���Ϊ����߶�
    }
}
