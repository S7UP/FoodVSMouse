public class BoiledWaterBoom : FoodUnit
{
    public override void MInit()
    {
        base.MInit();
        // ���ը������������
        FoodManager.AddBombModifier(this);
    }

    /// <summary>
    /// ը�������Ч����������
    /// </summary>
    public override void BeforeDrop()
    {
        base.BeforeDeath();
    }

    /// <summary>
    /// �ж��Ƿ�����Ч�Ĺ���Ŀ��
    /// </summary>
    /// <returns></returns>
    protected override bool IsHasTarget()
    {
        // ��ʱ��ը������Ҫ
        return false;
    }

    /// <summary>
    /// �Ƿ�������ͨ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        // ��ʱ��ը������Ҫ
        return true;
    }

    /// <summary>
    /// ������ͨ��������״̬
    /// </summary>
    public override void BeforeGeneralAttack()
    {
        // �л�Ϊ����������ͼ
        SetActionState(new AttackState(this));
    }

    /// <summary>
    /// ��ͨ�����ڼ�
    /// </summary>
    public override void OnGeneralAttack()
    {
        if (IsDamageJudgment())
        {
            // �ҽ��Ϳ�Ƭֱ����������
            ExecuteDeath();
        }
    }

    /// <summary>
    /// �˳���ͨ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetEndGeneralAttackCondition()
    {
        return animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce();
    }

    /// <summary>
    /// �Ƿ�Ϊ�˺��ж�ʱ�̣���ս����Ϊ���ʵ���˺���Զ�̹���Ϊȷ�����䵯�壩
    /// </summary>
    /// <returns></returns>
    public override bool IsDamageJudgment()
    {
        return (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce());
    }

    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public override void ExecuteDamage()
    {
        // ԭ�ز���һ����ըЧ��
        {
            BaseEffect e = BaseEffect.GetInstance("BoomEffect");
            e.animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/" + mType + "/BoomEffect");
            e.transform.position = transform.position;
            e.MInit();
            GameController.Instance.AddEffect(e);
        }
        // ��Ӷ�Ӧ���ж������
        {
            BombAreaEffectExecution bombEffect = BombAreaEffectExecution.GetInstance();
            bombEffect.Init(this, 900, GetRowIndex(), 5, 5, 0, 0, false, true);
            bombEffect.transform.position = this.GetPosition();
            GameController.Instance.AddAreaEffectExecution(bombEffect);
        }
    }

    /// <summary>
    /// ����
    /// </summary>
    public override void AfterDeath()
    {
        base.AfterDeath();
        // �˺��ж�Ϊ��ʧʱ����
        ExecuteDamage();
    }
}
