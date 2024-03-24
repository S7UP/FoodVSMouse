using S7P.Numeric;

using System;
/// <summary>
/// �����վ�
/// </summary>
public class FlyMouse : MouseUnit, IFlyUnit
{
    private bool isDrop; // �Ƿ񱻻���
    private int dropColumn; // ������
    private FloatModifier floatModifier = new FloatModifier(100);
    private BoolModifier boolMod = new BoolModifier(true);
    private static Func<BaseUnit, BaseUnit, bool> noBeSelectFunc = delegate { return false; };
    private static Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate { return false; };

    public override void MInit()
    {
        base.MInit();
        typeAndShapeValue = -1;
        mHeight = 1;
        isDrop = false;
        dropColumn = 0; // ������Ĭ��Ϊ0������һ��
        // ����״̬�»�ȡ100%���ټӳ�
        NumericBox.MoveSpeed.AddPctAddModifier(floatModifier);
        if(mShape == 6)
            Environment.WaterTask.AddUnitWaterRate(this, new S7P.Numeric.FloatModifier(2.0f));
    }

    public override void MUpdate()
    {
        base.MUpdate();
        if (IsMeetDropCondition())
        {
            OnShootDown();
        }
    }

    /// <summary>
    /// ����Ƿ����㽵������
    /// </summary>
    private bool IsMeetDropCondition()
    {
        return (transform.position.x <= MapManager.GetColumnX(dropColumn) && !isDrop);
    }

    /// <summary>
    /// ִ�н��䣬��һ��
    /// </summary>
    public void OnShootDown()
    {
        if (!isDrop)
        {
            isDrop = true;
            // ǿ��ͬ������ֵ���ٽ��
            // if (mCurrentHp > mHertRateList[0] * mMaxHp)
                mCurrentHp = (float)mHertRateList[0] * mMaxHp;
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
            OnShootDown();
        }
    }

    /// <summary>
    /// ����ת��״̬ʱҪ�����£�������ָ����ձ�����ʱҪ����
    /// </summary>
    public override void OnTransitionStateEnter()
    {
        // �������ڼ��Ƴ����ж�����Ч�������߶�����Ч�������޷���ѡȡΪ����Ŀ��
        StatusManager.RemoveAllSettleDownDebuff(this);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, boolMod);
        animatorController.Play("Drop");
        AddCanBeSelectedAsTargetFunc(noBeSelectFunc);
        AddCanHitFunc(noHitFunc);
    }

    public override void OnTransitionState()
    {
        if(!animatorController.GetCurrentAnimatorStateRecorder().aniName.Equals("Drop") || animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            SetActionState(new MoveState(this));
            AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(this, 60, false));
        }
    }

    public override void OnTransitionStateExit()
    {
        NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreStun, boolMod);
        RemoveCanBeSelectedAsTargetFunc(noBeSelectFunc);
        RemoveCanHitFunc(noHitFunc);
        mHeight = 0; // �߶Ƚ���Ϊ����߶�
    }
}
