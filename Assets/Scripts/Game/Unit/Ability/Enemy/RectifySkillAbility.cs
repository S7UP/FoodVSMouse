using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ���������ټ���
/// </summary>
public class RectifySkillAbility : SkillAbility
{
    // ����������õļ���
    private NinjaMouse mouseMaster;

    public RectifySkillAbility(BaseUnit pmaster) : base(pmaster)
    {
        mouseMaster = pmaster as NinjaMouse;
    }

    public RectifySkillAbility(BaseUnit pmaster, SkillAbilityInfo info) : base(pmaster, info)
    {
        mouseMaster = pmaster as NinjaMouse;
    }

    /// <summary>
    /// �����ͷŵ�����
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetSkillCondition()
    {
        // �����㹻����
        return true;
    }

    public override void BeforeSpell()
    {
        // ������
        BaseActionState s = new BaseActionState(master);
        s.SetEnterAction(delegate { mouseMaster.PlayRectifyClip(); });
        master.SetActionState(s);
        // ��ȡС��
        mouseMaster.PullRetinue();
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
        return mouseMaster.currentStateTimer > 0 && mouseMaster.IsCurrentClipEnd();
    }

    public override void AfterSpell()
    {
        master.SetActionState(new MoveState(master));
    }
}
