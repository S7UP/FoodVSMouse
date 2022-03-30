using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ��ͨ����
/// </summary>
public class GeneralAttackSkillAbility : SkillAbility
{
    public GeneralAttackSkillAbility(BaseUnit pmaster) :base(pmaster)
    {
        // Ĭ��ƽA������յ�λ�Ĺ���������
        if (pmaster.mBaseAttackSpeed > 0)
            needEnergy.SetBase(ConfigManager.fps / pmaster.mBaseAttackSpeed);
    }

    public GeneralAttackSkillAbility(BaseUnit pmaster, SkillAbilityInfo info) :base(pmaster, info)
    {
        // Ĭ��ƽA������յ�λ�Ĺ�����������������info�����
        if(pmaster.mBaseAttackSpeed > 0)
            needEnergy.SetBase(ConfigManager.fps / pmaster.mBaseAttackSpeed);
    }

    /// <summary>
    /// �����ͷŵ�����
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetSkillCondition()
    {
        return master.IsMeetGeneralAttackCondition();
    }

    public override void BeforeSpell()
    {
        master.BeforeGeneralAttack();
    }

    /// <summary>
    /// �ڼ����ڼ�ľ���ʵ��
    /// </summary>
    public override void OnSpelling()
    {
        master.OnGeneralAttack();
    }

    /// <summary>
    /// �ڼ����ڼ䣬������������Ҫ������
    /// </summary>
    public override bool IsMeetCloseSpellingCondition()
    {
        return master.IsMeetEndGeneralAttackCondition();
    }

    public override void AfterSpell()
    {
        master.AfterGeneralAttack();
    }
}
