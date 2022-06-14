using System.Collections;
using System.Collections.Generic;
using TMPro;

using UnityEngine;
/// <summary>
/// 空中直线投掷炸弹
/// </summary>
public class FlyThrowBombSkillAbility : SkillAbility
{
    private bool canSkill; // 是否可以施放，由该技能持有者决定
    private bool canClose; // 是否需要关闭，由该技能持有者决定

    public FlyThrowBombSkillAbility(BaseUnit pmaster) : base(pmaster)
    {

    }

    public FlyThrowBombSkillAbility(BaseUnit pmaster, SkillAbilityInfo info) : base(pmaster, info)
    {

    }

    /// <summary>
    /// 设置技能为可施放
    /// </summary>
    public void SetSkillConditionEnable()
    {
        canSkill = true;
    }


    /// <summary>
    /// 关闭技能
    /// </summary>
    public void CloseSkill()
    {
        canClose = true;
    }

    /// <summary>
    /// 满足释放的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetSkillCondition()
    {
        // 能量足够，非投掷过，允许施放
        return canSkill;
    }

    public override void BeforeSpell()
    {
        // 丢出炸弹实体
        FlyBombBullet flybombBullet = GameController.Instance.CreateBullet(master, master.transform.position + Vector3.left*0.225f + Vector3.up*0.3f, Vector2.down, BulletStyle.FlyBomb) as FlyBombBullet;
        flybombBullet.InitVelocity(0, 1.0f/ConfigManager.fps, master.transform.position.y, master.GetRowIndex());
        flybombBullet.UpdateRenderLayer(0); // 再更新一下图层吧
        flybombBullet.transform.right = Vector3.right;
        // 停下来，切状态，播动画
        master.SetActionState(new CastState(master));
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
        return canClose;
    }

    public override void AfterSpell()
    {
        master.SetActionState(new MoveState(master));
        canSkill = false;
    }
}
