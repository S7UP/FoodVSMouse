public class EnemyHealSkillAbility : SkillAbility
{
    public EnemyHealSkillAbility(BaseUnit pmaster) : base(pmaster)
    {

    }

    public EnemyHealSkillAbility(BaseUnit pmaster, SkillAbilityInfo info) : base(pmaster, info)
    {

    }

    /// <summary>
    /// �����ͷŵ�����
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetSkillCondition()
    {
        // ֻҪ�����㹻�Ϳ����ͷ�
        return true;
    }

    public override void BeforeSpell()
    {
        enableEnergyRegeneration = true;
        // ͣ��������״̬��������
        master.SetActionState(new CastState(master));
    }

    /// <summary>
    /// �ڼ����ڼ�ľ���ʵ��
    /// </summary>
    public override void OnSpelling()
    {
        
    }

    /// <summary>
    /// �ڷǼ����ڼ�
    /// </summary>
    public override void OnNoSpelling()
    {

    }

    /// <summary>
    /// �ڼ����ڼ䣬������������Ҫ������
    /// </summary>
    public override bool IsMeetCloseSpellingCondition()
    {
        // ����Ч���ڽ���ʱ��Ч�������ڼ���õ����������ۻ��ƣ��۵�0ʱ�����˳�
        return currentEnergy<=0;
    }

    public override void OnMeetCloseSpellingCondition()
    {
        // ��Ѫ��
        float add = (master as HealMouse).GetHealValue();
        foreach (var unit in GameController.Instance.GetEachEnemy())
        {
            // ��ҪĿ����
            if (!unit.IsAlive())
                continue;
            // ���������û�г�������
            float realAdd = unit.GetRealCureValue(add);
            float leftAdd = realAdd - unit.mMaxHp + unit.mCurrentHp;
            if (leftAdd > 0)
            {
                // ���������ʱ��ʣ���������ת��Ϊ����
                new CureAction(CombatAction.ActionType.GiveCure, master, unit, unit.mMaxHp - unit.mCurrentHp).ApplyAction();
                new ShieldAction(CombatAction.ActionType.GiveShield, master, unit, leftAdd).ApplyAction();
                // Debug.Log("��ǰĿ������ʧ����ֵ��"+(unit.mMaxHp - unit.mCurrentHp)+", �����ظ�����ֵ��"+(unit.mMaxHp - unit.mCurrentHp)+", ������û���ֵ��"+(leftAdd));
            }
            else
            {
                // δ���ʱȫ��ת��Ϊ����
                new CureAction(CombatAction.ActionType.GiveCure, master, unit, realAdd).ApplyAction();
                // Debug.Log("��ǰĿ������ʧ����ֵ��" + (unit.mMaxHp - unit.mCurrentHp) + ", �����ظ�����ֵ��" + realAdd);
            }
            // �ظ���Ч��Ӹ���λ��
            EffectManager.AddHealEffectToUnit(unit);
        }
    }

    public override void AfterSpell()
    {
        enableEnergyRegeneration = false;
        master.SetActionState(new MoveState(master));
    }
}
