using System.Collections;
using System.Collections.Generic;
using TMPro;

using UnityEngine;

/// <summary>
/// 老鼠技能模板
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
    /// 满足释放的条件
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
    /// 在技能期间的具体实现
    /// </summary>
    public override void OnSpelling()
    {
    }

    /// <summary>
    /// 在非技能期间
    /// </summary>
    public override void OnNoSpelling()
    {

    }

    /// <summary>
    /// 在技能期间，结束技能所需要的条件
    /// </summary>
    public override bool IsMeetCloseSpellingCondition()
    {
        return true;
    }

    public override void AfterSpell()
    {
    }
}
