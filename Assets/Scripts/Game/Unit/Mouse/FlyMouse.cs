using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyMouse : MouseUnit
{
    private bool isDrop; // �Ƿ񱻻���
    private int dropColumn; // ������

    public override void MInit()
    {
        base.MInit();
        isDrop = false;
        dropColumn = 0; // ������Ĭ��Ϊ0������һ��
    }

    public override void MUpdate()
    {
        base.MUpdate();
        if (IsMeetDropCondition())
        {
            ExcuteDrop();
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
    private void ExcuteDrop()
    {
        if (!isDrop)
        {
            isDrop = true;
            mHertRateList[0] = double.MaxValue;
            UpdateHertMap(); // ͨ��ǿ�Ƹı�HertRateListȻ��ǿ�Ƹ��£�ת��׶�
            // ��Ϊת��״̬����״̬�µľ���ʵ�����¼�������
            SetActionState(new TransitionState(this));
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
            ExcuteDrop();
        }
    }

    /// <summary>
    /// ����ת��״̬ʱҪ�����£�������ָ����ձ�����ʱҪ����
    /// </summary>
    public override void OnTransitionStateEnter()
    {
        animator.Play("Drop");
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
