using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 蜗牛车鼠
/// </summary>
public class SnailMouse : MouseUnit
{
    private GeneralAttackSkillAbility generalAttackSkillAbility;
    private ReleaseMucusSkillAbility releaseMucusSkillAbility;


    /// <summary>
    /// 加载技能，加载普通攻击与技能
    /// </summary>
    public override void LoadSkillAbility()
    {
        List<SkillAbility.SkillAbilityInfo> infoList = AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape);
        // 普通攻击
        if (infoList.Count > 0)
        {
            generalAttackSkillAbility = new GeneralAttackSkillAbility(this, infoList[0]);
            skillAbilityManager.AddSkillAbility(generalAttackSkillAbility);
        }
        // 施放粘液
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
        // 动画播放完就切回去
        if (AnimatorManager.GetNormalizedTime(animator) > 1.0)
        {
            releaseMucusSkillAbility.EndActivate();
        }
    }
}
