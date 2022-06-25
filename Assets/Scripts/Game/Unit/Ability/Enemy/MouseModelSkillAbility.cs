using System.Collections;
using System.Collections.Generic;
using TMPro;

using UnityEngine;

/// <summary>
/// ������ģ��
/// </summary>
public class MouseModelSkillAbility : SkillAbility
{

    public MouseModelSkillAbility(BaseUnit pmaster) : base(pmaster)
    {
    }

    public MouseModelSkillAbility(BaseUnit pmaster, SkillAbilityInfo info) : base(pmaster, info)
    {
    }

    /// <summary>
    /// �����ͷŵ�����
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetSkillCondition()
    {
        return true;
    }

    public override void BeforeSpell()
    {
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
        return true;
    }

    public override void AfterSpell()
    {
    }
}
