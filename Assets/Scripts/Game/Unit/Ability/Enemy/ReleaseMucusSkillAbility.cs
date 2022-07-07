using UnityEngine;
/// <summary>
/// ʩ��ճҺ����
/// </summary>
public class ReleaseMucusSkillAbility : SkillAbility
{
    public ReleaseMucusSkillAbility(BaseUnit pmaster) : base(pmaster)
    {
    }

    public ReleaseMucusSkillAbility(BaseUnit pmaster, SkillAbilityInfo info) : base(pmaster, info)
    {
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
        master.SetActionState(new CastState(master));
        // ����ճҺʵ��
        TimelinessShiftZone t = (TimelinessShiftZone)GameController.Instance.CreateItem((Vector2)master.transform.position - master.moveRotate*MapManager.gridWidth, (int)ItemInGridType.ShiftZone, 0);
        t.SetLeftTime(1800); // ����30s
        t.SetChangePercent(100.0f); // ���ӵ�ǰ100%��������
        t.SetActionState(new IdleState(t));
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
        // ��������EndActivate��������
        return false;
    }

    public override void AfterSpell()
    {
        master.SetActionState(new MoveState(master));
    }
}
