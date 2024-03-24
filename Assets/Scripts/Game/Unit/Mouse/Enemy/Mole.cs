using UnityEngine;
using System;
using S7P.Numeric;
/// <summary>
/// ������
/// </summary>
public class Mole : MouseUnit
{
    // ��Ϊ3P��P1�ڵ���,P2�ڵ�������̬��P3�ڵ�������̬
    private bool isAppear; // �Ƿ����
    private bool isMoveLeft; // �������Ƿ������ƶ�
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
        // ��ʼ�ڵ���ʱ���߱���Ч��
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, IgnoreFrozen);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreSlowDown, IgnoreFrozenSlowDown);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, IgnoreFrozen);
        // ��ʼ�ڵ���ʱ���ӻ�������300%
        speedModifier = new FloatModifier(300f);
        NumericBox.MoveSpeed.AddPctAddModifier(speedModifier);
        // ��ʼ�߶�Ϊ-1
        mHeight = -1;
        // ��ʼ������Ϊ����Ŀ�ꡢ���ɱ�������Ҳ���赲������
        AddCanBeSelectedAsTargetFunc(noBeSelectedAsTargetFunc);
        AddCanHitFunc(noHitFunc);
        AddCanBlockFunc(noBlockFunc);
        GameManager.Instance.audioSourceController.PlayEffectMusic("Dig");
    }

    /// <summary>
    /// ִ�г���
    /// </summary>
    private void ExecuteAppear(bool isMoveLeft)
    {
        if (!isAppear)
        {
            if (mHertIndex == 0)
                mHertIndex++;

            isAppear = true;
            mHertRateList[0] = 1.0f;
            OnHertStageChanged();
            this.isMoveLeft = isMoveLeft;
            // �ĳ���
            SetActionState(new TransitionState(this));
        }
    }

    /// <summary>
    /// ���ڵ���ʱѪ���½����ڶ�������׶�ʱǿ�Ƴ�����������
    /// </summary>
    public override void OnUpdateRuntimeAnimatorController()
    {
        if (mHertIndex > 0 && mHertIndex <= 2)
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
                // �ĳ���
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
        // ���ƶ�����һ�и��������Եʱ����������������
        if(transform.position.x < MapManager.GetColumnX(-0.5f))
        {
            ExecuteAppear(false);
        }
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
            // �������߶���״̬
            NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreFrozen, IgnoreFrozen);
            NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreSlowDown, IgnoreFrozenSlowDown);
            NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreStun, IgnoreFrozen);
            SetActionState(new CastState(this));
            // �Ƴ�����Ч��
            NumericBox.MoveSpeed.RemovePctAddModifier(speedModifier);
            // �߶���Ϊ�����߶�0
            mHeight = 0;
            // �Ƴ�������Ϊ����Ŀ�ꡢ���ɱ�����
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
            // ���ڿ��Ա��赲��
            RemoveCanBlockFunc(noBlockFunc);
            SetActionState(new MoveState(this));
        }
    }

    public override bool CanTriggerCat()
    {
        return isAppear && isMoveLeft;
    }
}
