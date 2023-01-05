using System;
public class IceBucketFoodUnit : FoodUnit
{
    private bool isTrigger;
    public override void MInit()
    {
        isTrigger = false;
        base.MInit();
        Action<CombatAction> hitedAction = (combatAction) => {
            isTrigger = true;
        };
        AddActionPointListener(ActionPointType.PostReceiveDamage, hitedAction);
        AddActionPointListener(ActionPointType.PostReceiveReboundDamage, hitedAction);
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
    /// ���ݵȼ���͵ȼ������¶�Ӧ����
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        //NumericBox.Attack.SetBase((float)(attr.baseAttrbute.baseAttack + attr.valueList[mLevel]));
    }

    /// <summary>
    /// �Ƿ�������ͨ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        // ����ֵ����50���ߵ�ǰ���ڸ�����ͨ���Ϳ�Ƭ�Ĵ���
        return isTrigger || GetGrid().IsContainTag(FoodInGridType.Default);
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
            e.animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/"+mType+"/BoomEffect");
            e.transform.position = transform.position;
            e.MInit();
            GameController.Instance.AddEffect(e);
        }
        // �����ез���λʩ�ӱ���Ч��
        foreach (var item in GameController.Instance.GetEachEnemy())
        {
            item.AddNoCountUniqueStatusAbility(StringManager.Frozen, new FrozenStatusAbility(item, 240, false));
            item.AddStatusAbility(new FrozenSlowStatusAbility(-50, item, 480));
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
