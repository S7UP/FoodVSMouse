using UnityEngine;
/// <summary>
/// ըը��
/// </summary>
public class MushroomDestroyer : FoodUnit
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
            RetangleAreaEffectExecution e = RetangleAreaEffectExecution.GetInstance(transform.position, 3, 3, "ItemCollideAlly");
            e.SetInstantaneous(); // ���ó�������Ч
            e.isAffectFood = true;
            int count = 0;
            e.SetOnFoodEnterAction((unit)=> 
            {
                if (LuminescentMold.CureUnit(unit))
                    count++;
            });
            // ��ը����ʱ����
            e.AddBeforeDestoryAction((e) => 
            {
                count = Mathf.Min(count, 9); // count���Ϊ9
                // ����10%*���ù������CD
                mBuilder.AddLeftCDPercent(-count*0.1f);
                // ����75*(9-�����ù����)����
                SmallStove.CreateAddFireEffect(e.transform.position, 75*(9-count));
            });
            GameController.Instance.AddAreaEffectExecution(e);
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
