using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 忍者鼠整顿技能
/// </summary>
public class RectifySkillAbility : SkillAbility
{
    // 仅忍者鼠可用的技能
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
    /// 满足释放的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetSkillCondition()
    {
        // 能量足够即可
        return true;
    }

    public override void BeforeSpell()
    {
        // 播动画
        BaseActionState s = new BaseActionState(master);
        s.SetEnterAction(delegate { mouseMaster.PlayRectifyClip(); });
        master.SetActionState(s);
        // 拉取小弟
        mouseMaster.PullRetinue();
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
        return mouseMaster.currentStateTimer > 0 && mouseMaster.IsCurrentClipEnd();
    }

    public override void AfterSpell()
    {
        master.SetActionState(new MoveState(master));
    }
}
