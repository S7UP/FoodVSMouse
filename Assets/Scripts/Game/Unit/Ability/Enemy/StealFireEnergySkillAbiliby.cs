using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;
/// <summary>
/// 光能窃取
/// </summary>
public class StealFireEnergySkillAbiliby : SkillAbility
{
    private int totalCastTime = 240; // 总窃取时间
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
        master.SetActionState(new CastState(master));
    }

    /// <summary>
    /// 在技能期间的具体实现
    /// </summary>
    public override void OnSpelling()
    {
        if (isCasting)
        {
            currentTime++;
            if (currentTime >= totalCastTime)
            {
                isEndCasting = true;
                // 结束偷窃
                GameController.Instance.mCostController.RemoveShieldModifier("Fire", boolModifier);
            }
        }
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
        return false; // 由外界自行结束
    }

    public override void AfterSpell()
    {
        currentTime = 0;
        isCasting = false;
        isEndCasting = false;
        master.SetActionState(new MoveState(master));
    }

    /// <summary>
    /// 开始偷窃
    /// </summary>
    public void StartCasting()
    {
        GameController.Instance.mCostController.AddShieldModifier("Fire", boolModifier);
        isCasting = true;
    }

    /// <summary>
    /// 是否在施法中
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
