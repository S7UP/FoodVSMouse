using UnityEngine;
/// <summary>
/// ��۴�
/// </summary>
public class FlourBag : FoodUnit
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
    /// ���ݵȼ���͵ȼ������¶�Ӧ����
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        //NumericBox.Attack.SetBase((float)(attr.baseAttrbute.baseAttack + attr.valueList[mLevel]));
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
        // �˺��ж�֡Ӧ��ִ���ж�
        if (IsDamageJudgment())
        {
            mAttackFlag = false;
            ExecuteDamage();
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
        return (mAttackFlag && animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime()>attackPercent);
    }

    public override void AfterGeneralAttack()
    {
        base.AfterGeneralAttack();
        ExecuteDeath();
    }

    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public override void ExecuteDamage()
    {
        BaseCardBuilder builder = GetCardBuilder();

        GameManager.Instance.audioSourceManager.PlayEffectMusic("Thump");
        // ��Ӷ�Ӧ���ж������
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, 1, 1, "ItemCollideEnemy");
            r.isAffectMouse = true;
            r.SetInstantaneous();
            r.SetAffectHeight(0);
            int count = 0;
            r.SetOnEnemyEnterAction((m) => {
                count++;
                if(mShape >= 1)
                    m.AddStatusAbility(new FrozenSlowStatusAbility(-90, m, 240));

                if (m.NumericBox.GetBoolNumericValue(StringManager.IgnoreBombInstantKill))
                {
                    new DamageAction(CombatAction.ActionType.RealDamage, this, m, 90 * mCurrentAttack).ApplyAction();
                    UnitManager.TriggerRecordDamage(m);
                }
                else
                {
                    UnitManager.Execute(this, m);
                }
            });
            r.AddBeforeDestoryAction(delegate {
                if (builder != null)
                {
                    Debug.Log("count="+count);
                    float percent = Mathf.Min(0.75f, 0.15f*count);
                    builder.AddLeftCDPercent(-percent);
                }
            });
            GameController.Instance.AddAreaEffectExecution(r);
        }
    }
}
