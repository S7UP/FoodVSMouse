using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;
/// <summary>
/// ������ȡ
/// </summary>
public class StealFireEnergySkillAbiliby : SkillAbility
{
    private int totalCastTime = 240; // ����ȡʱ��
    private int currentTime = 0;
    private BoolModifier boolModifier = new BoolModifier(true);
    private bool isCasting;
    private bool isEndCasting;

    public StealFireEnergySkillAbiliby(BaseUnit pmaster) : base(pmaster)
    {

    }

    public StealFireEnergySkillAbiliby(BaseUnit pmaster, SkillAbilityInfo info) : base(pmaster, info)
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
        // ������
        master.SetActionState(new CastState(master));
    }

    /// <summary>
    /// �ڼ����ڼ�ľ���ʵ��
    /// </summary>
    public override void OnSpelling()
    {
        if (isCasting)
        {
            currentTime++;
            if (currentTime >= totalCastTime)
            {
                isEndCasting = true;
                // ����͵��
                GameController.Instance.mCostController.RemoveShieldModifier("Fire", boolModifier);
            }
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
        return false; // ��������н���
    }

    public override void AfterSpell()
    {
        currentTime = 0;
        isCasting = false;
        isEndCasting = false;
        master.SetActionState(new MoveState(master));
    }

    /// <summary>
    /// ��ʼ͵��
    /// </summary>
    public void StartCasting()
    {
        GameController.Instance.mCostController.AddShieldModifier("Fire", boolModifier);
        isCasting = true;
    }

    /// <summary>
    /// �Ƿ���ʩ����
    /// </summary>
    /// <returns></returns>
    public bool IsCasting()
    {
        return isCasting;
    }

    public bool IsEndCasting()
    {
        return isEndCasting;
    }

    public void EndCasting()
    {
        isCasting = false;
    }
}
