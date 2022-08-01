/// <summary>
/// �����վ�
/// </summary>
public class FlyMouse : MouseUnit, IFlyUnit
{
    private bool isDrop; // �Ƿ񱻻���
    private int dropColumn; // ������
    private FloatModifier floatModifier = new FloatModifier(100);

    public override void MInit()
    {
        base.MInit();
        isDrop = false;
        dropColumn = 0; // ������Ĭ��Ϊ0������һ��
        // ����״̬�»�ȡ100%���ټӳ�
        NumericBox.MoveSpeed.AddPctAddModifier(floatModifier);
        // 6���ǻ�е����������ը��ֱ����ɱЧ��
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
    /// ����Ƿ����㽵������
    /// </summary>
    private bool IsMeetDropCondition()
    {
        return (GetColumnIndex() <= dropColumn && !isDrop);
    }

    /// <summary>
    /// ִ�н��䣬��һ��
    /// </summary>
    public void ExecuteDrop()
    {
        if (!isDrop)
        {
            isDrop = true;
            // ����ǰ����ֵ���ڷ���״̬�ٽ�㣬����Ҫǿ��ͬ������ֵ���ٽ��
            if (mCurrentHp > mHertRateList[0] * mMaxHp)
            {
                mCurrentHp = (float)mHertRateList[0] * mMaxHp;
            }
            mHertRateList[0] = double.MaxValue;
            UpdateHertMap(); // ǿ�Ƹ���һ����ͼ
            // ��Ϊת��״̬����״̬�µľ���ʵ�����¼�������
            SetActionState(new TransitionState(this));
            // ȡ������״̬�Ƴ����ټӳ�
            NumericBox.MoveSpeed.RemovePctAddModifier(floatModifier);
            // �����λ�����һ�н���ģ���ֱ�Ӵ���èè
            if(GetColumnIndex() <= 0)
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
        // 0 ����
        // 1 �������->�����ƶ�
        // 2 �����ƶ�
        if (mHertIndex > 0 && mHertIndex <= 2 && !isDrop)
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
        //if (AnimatorManager.GetCurrentFrame(animator) == AnimatorManager.GetTotalFrame(animator)) 
        if(animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            SetActionState(new MoveState(this));
        }
    }

    public override void OnTransitionStateExit()
    {
        mHeight = 0; // �߶Ƚ���Ϊ����߶�
    }
}
