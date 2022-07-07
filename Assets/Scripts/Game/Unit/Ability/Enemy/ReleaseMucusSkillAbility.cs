using UnityEngine;
/// <summary>
/// 施放粘液技能
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
        master.SetActionState(new CastState(master));
        // 产生粘液实体
        TimelinessShiftZone t = (TimelinessShiftZone)GameController.Instance.CreateItem((Vector2)master.transform.position - master.moveRotate*MapManager.gridWidth, (int)ItemInGridType.ShiftZone, 0);
        t.SetLeftTime(1800); // 持续30s
        t.SetChangePercent(100.0f); // 增加当前100%基础移速
        t.SetActionState(new IdleState(t));
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
        // 由外界调用EndActivate方法结束
        return false;
    }

    public override void AfterSpell()
    {
        master.SetActionState(new MoveState(master));
    }
}
