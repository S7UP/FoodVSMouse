using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// �澳��������
/// </summary>
public class WonderLandNormalMouse : MouseUnit
{
    private bool isInRainArea; // �Ƿ�����������
    private bool canBlock; // �ɷ��赲
    private int slideState; // ���н׶Σ���������������ʱ������ 0 ǰҡ 1 �н��� 2��ҡ

    private static FloatModifier rainModifier = new FloatModifier(100); // ����+100%����BUFF

    public override void MInit()
    {
        isInRainArea = false;
        canBlock = true;
        slideState = 0;
        base.MInit();
        // �������������߼��ٺͱ�������
        if (mShape == 1)
        {
            NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, new BoolModifier(true));
            NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreSlowDown, new BoolModifier(true));
        }
    }

    public override void OnMoveStateEnter()
    {
        if (isInRainArea)
        {
            // ���л���ʱ���赲
            canBlock = false;
            if (slideState == 0)
                animatorController.Play("PreSlide");
            else if (slideState == 1)
                animatorController.Play("Slide", true);
            else if (slideState == 2)
                animatorController.Play("PostSlide");
        }
        else
        {
            // �������л���ʱ�Ϳ����赲
            canBlock = true;
            base.OnMoveStateEnter();
        }
    }

    public override bool CanBlock(BaseUnit unit)
    {
        return canBlock && base.CanBlock(unit);
    }

    public override void AfterGeneralAttack()
    {
        base.AfterGeneralAttack();
        // ���һ�ι�����󣬸õ�λ����������ȡ���赲�߲��ҿ�ʼ����
        if (isInRainArea)
        {
            SetNoCollideAllyUnit();
            SetActionState(new MoveState(this));
        }
    }

    public override void OnMoveState()
    {
        base.OnMoveState();
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            if (slideState == 0)
            {
                slideState = 1;
                SetActionState(new MoveState(this));
            }
            // ����1�н��е���������������ת��2��ҡ
            else if (slideState == 2)
            {
                slideState = 0;
                SetActionState(new MoveState(this));
            }
        }
    }

    /// <summary>
    /// ����������
    /// </summary>
    /// <param name="isIn"></param>
    public void SetInRainArea()
    {
        isInRainArea = true;
        // �ڷǹ���״̬ʱ��ǿ��ת�ɻ���״̬������״̬����ҪA�������ٻ���������ʵ�֣�
        if(!(mCurrentActionState is AttackState))
            SetActionState(new MoveState(this)); 
        NumericBox.MoveSpeed.RemovePctAddModifier(rainModifier);
        NumericBox.MoveSpeed.AddPctAddModifier(rainModifier);
    }

    /// <summary>
    /// ���ò�������
    /// </summary>
    public void SetOutOfRainArea()
    {
        // ����Ϊ�׶�2
        slideState = 2;
        SetActionState(new MoveState(this));
        // ȡ�������ж�һ��Ҫ������״̬֮���������ܴ������к�ҡ������ͨ�ƶ�
        isInRainArea = false;
        NumericBox.MoveSpeed.RemovePctAddModifier(rainModifier);
    }
}
