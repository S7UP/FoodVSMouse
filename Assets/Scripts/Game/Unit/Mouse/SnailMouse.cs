using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ��ţ����
/// </summary>
public class SnailMouse : MouseUnit
{
    private GeneralAttackSkillAbility generalAttackSkillAbility;
    private ReleaseMucusSkillAbility releaseMucusSkillAbility;


    /// <summary>
    /// ���ؼ��ܣ�������ͨ�����뼼��
    /// </summary>
    public override void LoadSkillAbility()
    {
        List<SkillAbility.SkillAbilityInfo> infoList = AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape);
        // ��ͨ����
        if (infoList.Count > 0)
        {
            generalAttackSkillAbility = new GeneralAttackSkillAbility(this, infoList[0]);
            skillAbilityManager.AddSkillAbility(generalAttackSkillAbility);
        }
        // ʩ��ճҺ
        if (infoList.Count > 1)
        {
            releaseMucusSkillAbility = new ReleaseMucusSkillAbility(this, infoList[1]);
            skillAbilityManager.AddSkillAbility(releaseMucusSkillAbility);
        }
    }

    public override void OnCastStateEnter()
    {
        animator.Play("Cast");
    }

    public override void OnCastState()
    {
        if (currentStateTimer == 0)
            return;
        // ������������л�ȥ
        if (AnimatorManager.GetNormalizedTime(animator) > 1.0)
        {
            releaseMucusSkillAbility.EndActivate();
        }
    }
}
