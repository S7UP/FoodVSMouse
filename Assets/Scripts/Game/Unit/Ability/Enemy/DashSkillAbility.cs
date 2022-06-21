using System.Collections;
using System.Collections.Generic;
using TMPro;

using UnityEngine;

/// <summary>
/// �������漼��
/// </summary>
public class DashSkillAbility : SkillAbility
{
    private FloatModifier addMoveSpeedModifier = new FloatModifier(300);
    private bool isDashed; // �Ƿ��Ѿ�����ˣ�����
    private MouseUnit mouseMaster;
    private int maxTime = 270; // �����ʱ��
    private int currentTime = 0;

    public DashSkillAbility(BaseUnit pmaster) : base(pmaster)
    {
        mouseMaster = pmaster as MouseUnit;
    }

    public DashSkillAbility(BaseUnit pmaster, SkillAbilityInfo info) : base(pmaster, info)
    {
        mouseMaster = pmaster as MouseUnit;
    }

    /// <summary>
    /// �����ͷŵ�����
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetSkillCondition()
    {
        // ��ǰû������������㹻����
        return !isDashed;
    }

    public override void BeforeSpell()
    {
        // ������
        master.SetActionState(new MoveState(master));
        // ���300%���ƶ��ٶȼӳ�
        master.NumericBox.MoveSpeed.AddPctAddModifier(addMoveSpeedModifier);
    }

    /// <summary>
    /// �ڼ����ڼ�ľ���ʵ��
    /// </summary>
    public override void OnSpelling()
    {
        currentTime++;
        // �������У�����������赲����ô��������Ŀ����� {������*�����ƶ��ٶȵı�׼ֵ*25%} ���˺�
        // ��Ŀ�����ڱ����˺�����������棬����������
        if (mouseMaster.IsHasTarget())
        {
            BaseUnit target = mouseMaster.GetCurrentTarget();
            new DamageAction(CombatAction.ActionType.CauseDamage, mouseMaster, target, 
                mouseMaster.mCurrentAttack*TransManager.TranToStandardVelocity(mouseMaster.mCurrentMoveSpeed)*0.25f).ApplyAction();
            if (target.IsAlive())
                EndActivate();
        }
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
        return currentTime >= maxTime;
    }

    public override void AfterSpell()
    {
        isDashed = true;
        master.SetActionState(new MoveState(master));
        // �Ƴ����ټӳ�Ч��
        master.NumericBox.MoveSpeed.RemovePctAddModifier(addMoveSpeedModifier);
    }

    /// <summary>
    /// �������ã���ȡ�����Ƿ�ʩ�Ź�һ�ε���Ϣ
    /// </summary>
    /// <returns></returns>
    public bool IsDashed()
    {
        return isDashed;
    }
}
