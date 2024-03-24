using UnityEngine;

public class Fishbone : FoodUnit
{
    private RetangleAreaEffectExecution checkArea;

    public override void MInit()
    {
        base.MInit();
        // ���ᱻѡΪ����Ŀ��Ͳ��ɱ��赲��Ч��
        {
            AddCanBeSelectedAsTargetFunc(delegate { return false; });
            AddCanBlockFunc(delegate { return false; });
            // ���������Ч
            // EnvironmentFacade.AddFogBuff(this);
        }
        // ���ɼ������
        CreateCheckArea();
    }

    /// <summary>
    /// ���ݵȼ���͵ȼ������¶�Ӧ����
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        NumericBox.Attack.SetBase((float)(attr.baseAttrbute.baseAttack + attr.valueList[mLevel]));
    }

    /// <summary>
    /// ���ɼ������
    /// </summary>
    private void CreateCheckArea()
    {
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, new Vector2(MapManager.gridWidth, 0.5f * MapManager.gridHeight), "ItemCollideEnemy");
        r.isAffectMouse = true;
        r.SetAffectHeight(0);
        //r.AddEnemyEnterConditionFunc((m) => {
        //    return true;
        //});
        GameController.Instance.AddAreaEffectExecution(r);

        CustomizationTask task = new CustomizationTask();
        task.AddTaskFunc(delegate {
            r.transform.position = transform.position;
            return !IsAlive();
        });
        task.AddOnExitAction(delegate {
            r.MDestory();
        });
        r.taskController.AddTask(task);

        checkArea = r;
    }

    #region ����Ϊ��ͨ�������

    protected override bool IsHasTarget()
    {
        return checkArea != null && checkArea.mouseUnitList.Count > 0;
    }

    /// <summary>
    /// �Ƿ�������ͨ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        return IsHasTarget();
    }

    /// <summary>
    /// ������ͨ��������״̬
    /// </summary>
    public override void BeforeGeneralAttack()
    {
        // �л�Ϊ����������ͼ
        SetActionState(new AttackState(this));
        // GameManager.Instance.audioSourceController.PlayEffectMusic("Fume");
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
    /// �˳���ͨ������Ҫ������
    /// </summary>
    public override void AfterGeneralAttack()
    {
        mAttackFlag = true;
        SetActionState(new IdleState(this));
    }

    /// <summary>
    /// �Ƿ�Ϊ�˺��ж�ʱ�̣���ս����Ϊ���ʵ���˺���Զ�̹���Ϊȷ�����䵯�壩
    /// </summary>
    /// <returns></returns>
    public override bool IsDamageJudgment()
    {
        return (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= attackPercent && mAttackFlag);
    }

    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public override void ExecuteDamage()
    {
        foreach (var m in checkArea.mouseUnitList.ToArray())
        {
            DamageAction d = new DamageAction(CombatAction.ActionType.CauseDamage, this, m, mCurrentAttack);
            d.AddDamageType(DamageAction.DamageType.AOE);
            d.ApplyAction();
            if(mShape >= 1)
                m.AddStatusAbility(new SlowStatusAbility(-50, m, 120));
        }
    }
    #endregion
}
