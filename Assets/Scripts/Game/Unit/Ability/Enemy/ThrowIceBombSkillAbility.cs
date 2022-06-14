using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 投掷冰炸弹
/// </summary>
public class ThrowIceBombSkillAbility : SkillAbility
{
    public bool isThrow; // 能否再次释放的flag，当置为true时该技能无法再次施放
    private bool canSkill; // 是否可以施放，由该技能持有者决定
    private bool canThrowEntity; // 是否可以投掷出实体，由该技能持有者决定
    private Vector3 targetPosition;
    private bool canClose; // 是否需要关闭，由该技能持有者决定

    public ThrowIceBombSkillAbility(BaseUnit pmaster) : base(pmaster)
    {

    }

    public ThrowIceBombSkillAbility(BaseUnit pmaster, SkillAbilityInfo info) : base(pmaster, info)
    {

    }

    /// <summary>
    /// 设置技能为可施放
    /// </summary>
    public void SetSkillConditionEnable()
    {
        canSkill = true;
    }

    public void ThrowEntity(Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
        canThrowEntity = true;
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
        return (!isThrow && canSkill);
    }

    public override void BeforeSpell()
    {
        isThrow = true;
        // 停下来，切状态，播动画
        master.SetActionState(new CastState(master));
    }

    /// <summary>
    /// 在技能期间的具体实现
    /// </summary>
    public override void OnSpelling()
    {
        if (canThrowEntity)
        {
            // 投掷出实体
            canThrowEntity = false;
            IceBombBullet iceBullet = GameController.Instance.CreateBullet(master, master.transform.position, Vector2.right, BulletStyle.IceBomb) as IceBombBullet;
            iceBullet.SetAttribute(24.0f, true, 1.5f, iceBullet.transform.position, targetPosition, master.GetRowIndex());
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
        return canClose;
    }

    public override void AfterSpell()
    {
        master.SetActionState(new MoveState(master));
    }
}
