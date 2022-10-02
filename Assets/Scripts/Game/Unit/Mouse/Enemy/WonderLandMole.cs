using UnityEngine;
/// <summary>
/// �澳���
/// </summary>
public class WonderLandMole : MouseUnit
{
    // ��Ϊ3P��P1�ڵ���,P2�ڵ�������̬��P3�ڵ�������̬
    private bool isAppear; // �Ƿ����
    private bool isMoveLeft; // �������Ƿ������ƶ�
    private bool canBeHited; // �ܷ񱻻���
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
        // ��ʼ�ڵ���ʱ���߱���Ч�� �Լ��ҽ���ɱЧ��
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, IgnoreFrozen);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, IgnoreFrozenSlowDown);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, IgnoreBombInstantKill);
        // ��ʼ�ڵ���ʱ���ӻ�������300%
        speedModifier = new FloatModifier(300f);
        NumericBox.MoveSpeed.AddPctAddModifier(speedModifier);
        // ��ʼ�߶�Ϊ-1
        mHeight = -1;
    }

    /// <summary>
    /// �����ڶݵ�״̬ʱ��ȫ�����ӵ�����
    /// </summary>
    /// <param name="bullet"></param>
    /// <returns></returns>
    public override bool CanHit(BaseBullet bullet)
    {
        return canBeHited && base.CanHit(bullet);
    }

    /// <summary>
    /// �����ڶݵ�״̬ʱ���ڲ���ѡȡ״̬
    /// </summary>
    /// <returns></returns>
    public override bool CanBeSelectedAsTarget()
    {
        return canBeHited && base.CanBeSelectedAsTarget();
    }

    /// <summary>
    /// �Ƿ�������ͨ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        // ���赲�� �� �赲��������Ч��
        return canBeHited && base.IsMeetGeneralAttackCondition();
    }

    /// <summary>
    /// ִ�г���
    /// </summary>
    public void ExecuteAppear(bool isMoveLeft)
    {
        // ����ʱ������������P1�׶Σ�����Ҫǿ��ת��P2�׶�
        if (mHertIndex < 1)
        {
            mHertRateList[mHertIndex] = 1.0f;
            UpdateHertMap();
        }

        isAppear = true;
        this.isMoveLeft = isMoveLeft;
        SetActionState(new TransitionState(this));
        // �������߶���״̬�ͻҽ���ɱ
        NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreFrozen, IgnoreFrozen);
        NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, IgnoreFrozenSlowDown);
        NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, IgnoreBombInstantKill);
        // �Ƴ�����Ч��
        NumericBox.MoveSpeed.RemovePctAddModifier(speedModifier);
        // �߶���Ϊ�����߶�0
        mHeight = 0;
    }

    /// <summary>
    /// ���ڵ���ʱѪ���½����ڶ�������׶�ʱǿ�Ƴ�����������
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
                // �ĳ���
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
        // ���ƶ�����һ�и��������Եʱ����������������
        if (!isAppear && transform.position.x < MapManager.GetColumnX(-0.4f))
            ExecuteAppear(false);
    }

    /// <summary>
    /// ����ת��״̬ʱҪ������
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
        // ����������һ�κ�תΪ�ƶ�״̬
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            SetActionState(new MoveState(this));
        }
        // �е��ж�
        if (!canBeHited && animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() > 0.5)
            canBeHited = true;
    }

    /// <summary>
    /// �ܵ�һ�λҽ��˺��ͳ���
    /// </summary>
    /// <param name="dmg"></param>
    public override void OnBombBurnDamage(float dmg)
    {
        base.OnBombBurnDamage(dmg);
        if (dmg > 0 && !isAppear)
            ExecuteAppear(true);
    }
}
