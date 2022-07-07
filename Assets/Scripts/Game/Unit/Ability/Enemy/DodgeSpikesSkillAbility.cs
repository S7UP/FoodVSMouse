
using UnityEngine;
using System;

/// <summary>
/// ����ͻ��
/// </summary>
public class DodgeSpikesSkillAbility : SkillAbility
{
    private bool isMeetSkillCondition; // �����������㼼�ܴ�������
    private bool isMeetCloseSpellingCondition; // �����������㼼�ܽ�������
    private Action EndEvent; // �����¼�

    public DodgeSpikesSkillAbility(BaseUnit pmaster) : base(pmaster)
    {
        noClearEnergyWhenEnd = true;
    }

    public DodgeSpikesSkillAbility(BaseUnit pmaster, SkillAbilityInfo info) : base(pmaster, info)
    {
        noClearEnergyWhenEnd = true;
    }

    /// <summary>
    /// �����ͷŵ�����
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetSkillCondition()
    {
        return isMeetSkillCondition;
    }

    public override void BeforeSpell()
    {
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
        return isMeetCloseSpellingCondition;
    }

    public override void AfterSpell()
    {
        // ���ñ�����Ȼ��ʵ��λ��Ч����λ��Ч������Ŀ���׼�ƶ��ٶ�
        // ��Ĭ��1.0��׼�ƶ��ٶȵĻ����£�ÿ���һ�μ���λ��1/12��
        // �����������һֱ��200%�����ټӳɣ���3.0��׼���٣�������������һ�μ���Ӧ��λ��1/4��
        master.transform.position += (Vector3)master.moveRotate * TransManager.TranToStandardVelocity(master.GetMoveSpeed()) * MapManager.gridWidth / 12;
        master.SetActionState(new MoveState(master));
        // �����������Ա��´�����ʹ��
        FullCurrentEnergy();
        isMeetSkillCondition = false;
        isMeetCloseSpellingCondition = false;
        if (EndEvent != null)
            EndEvent();
    }

    public void SetSkillEnable()
    {
        isMeetSkillCondition = true;
    }

    public void EndSkill()
    {
        isMeetCloseSpellingCondition = true;
    }

    public void SetEndEvent(Action action)
    {
        this.EndEvent = action;
    }
}
