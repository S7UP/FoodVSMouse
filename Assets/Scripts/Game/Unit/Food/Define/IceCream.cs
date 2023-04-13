using UnityEngine;
using S7P.Numeric;
/// <summary>
/// �����
/// </summary>
public class IceCream : FoodUnit
{
    public override void MInit()
    {
        base.MInit();
        // ��ȡ100%���ˣ��ӽ����޵�����ֵ���Լ����߻ҽ���ɱЧ��
        NumericBox.Defense.SetBase(1);
        NumericBox.AddDecideModifierToBoolDict(StringManager.Invincibility, new BoolModifier(true));
        // ����ѡȡ
        CloseCollision();
    }

    public override void OnCastStateEnter()
    {
        animatorController.Play("Cast");
    }

    /// <summary>
    /// �ж��Ƿ�����Ч�Ĺ���Ŀ��
    /// </summary>
    /// <returns></returns>
    protected override bool IsHasTarget()
    {
        return false;
    }

    /// <summary>
    /// �Ƿ�������ͨ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        return true;
    }

    /// <summary>
    /// ������ͨ��������״̬
    /// </summary>
    public override void BeforeGeneralAttack()
    {
        // �л�Ϊ����������ͼ
        SetActionState(new CastState(this));
    }

    /// <summary>
    /// ��ͨ�����ڼ�
    /// </summary>
    public override void OnGeneralAttack()
    {
        // �˺��ж�֡Ӧ��ִ���ж�
        if (IsDamageJudgment())
        {
            mAttackFlag = false;
            ExecuteDamage();
        }
    }

    /// <summary>
    /// �Ƿ�Ϊ�˺��ж�ʱ��
    /// </summary>
    /// <returns></returns>
    public override bool IsDamageJudgment()
    {
        BaseGrid g = GetGrid();
        if (g == null)
            return false;
        bool flag = false;
        foreach (var f in g.GetAttackableFoodUnitList())
        {
            if(f.GetCardBuilder()!=null && !f.GetCardBuilder().IsColdDown())
            {
                flag = true;
                break;
            }
        }
        return (mAttackFlag && flag);
    }

    /// <summary>
    /// �˳���ͨ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetEndGeneralAttackCondition()
    {
        return (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce());
    }

    /// <summary>
    /// �˳���ͨ������Ҫ������
    /// </summary>
    public override void AfterGeneralAttack()
    {
        // �����С�Ŀշ��˱���ܻ�������CD
        if (mAttackFlag)
            GetCardBuilder().ResetCD();
        // �ҽ��Ϳ�Ƭֱ����������
        ExecuteDeath();
    }

    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public override void ExecuteDamage()
    {
        int totalCD=0; // ����ȴ��Ƭʣ��CD֮��ͳ��
        // ʹ��ǰ�����п�ƬCD����
        foreach (var unit in GetGrid().GetFoodUnitList())
        {
            if(unit.mType != mType)
            {
                BaseCardBuilder builder = unit.GetCardBuilder();
                totalCD += builder.mCDLeft;
                builder.ResetCD();
            }
        }

        // ����Ƕ�ת�����Ը��ݱ���ȴ�Ŀ�Ƭ�ڱ���ȴǰ��ʣ��CD����������CD
        if(mShape >= 2)
        {
            BaseCardBuilder builder = GetCardBuilder();
            int returnCD = Mathf.Max(builder.mCDLeft - totalCD, 0);
            builder.mCDLeft -= returnCD;
        }
    }
}
