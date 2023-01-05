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
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, new BoolModifier(true));
        NumericBox.AddDecideModifierToBoolDict(StringManager.Invincibility, new BoolModifier(true));
        // ����ѡȡ
        CloseCollision();
    }

    public override void OnCastStateEnter()
    {
        animatorController.Play("PreCast");
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
    /// �Ƿ�Ϊ�˺��ж�ʱ�̣���ս����Ϊ���ʵ���˺���Զ�̹���Ϊȷ�����䵯�壩
    /// </summary>
    /// <returns></returns>
    public override bool IsDamageJudgment()
    {
        // ����������һ����ұ����п�Ƭռ�и���
        BaseGrid g = GetGrid();
        if (g == null)
            return false;
        return (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= attackPercent && mAttackFlag && g.GetFoodUnitList().Count>1);
    }

    /// <summary>
    /// �˳���ͨ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetEndGeneralAttackCondition()
    {
        return (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce() && !mAttackFlag);
    }

    /// <summary>
    /// �˳���ͨ������Ҫ������
    /// </summary>
    public override void AfterGeneralAttack()
    {
        // �ҽ��Ϳ�Ƭֱ����������
        ExecuteDeath();
    }

    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public override void ExecuteDamage()
    {
        //u.GetCardBuilder().ResetCD();
        // �����ע���Ѿ���ʱ�ˣ������Ǵ����ϰ汾�趨���°汾�趨������
        // ʹ��ǰ�����п�ƬCD����
        foreach (var unit in GetGrid().GetFoodUnitList())
        {
            if(unit.mType != mType)
                unit.GetCardBuilder().ResetCD();
        }
    }
}
