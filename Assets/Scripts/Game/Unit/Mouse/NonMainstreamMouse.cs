using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 非主流鼠
/// </summary>
public class NonMainstreamMouse : MouseUnit
{
    private GeneralAttackSkillAbility generalAttackSkillAbility;
    private DodgeSpikesSkillAbility dodgeSpikesSkillAbility;
    private FloatModifier floatModifier = new FloatModifier(200); // 加速修饰器
    private FloatModifier defenceModifier; // 减伤修饰器
    private FloatModifier attackModifier; // 百分比攻击力加成修饰器
    private int currentActionCount; // 当前动作计数器
    private float defenceValue; // 减伤幅度
    private float addAttackPercentValue; // 攻击力加成幅度

    public override void MInit()
    {
        base.MInit();
        defenceValue = 0.9f;
        addAttackPercentValue = 0;
        NumericBox.MoveSpeed.AddPctAddModifier(floatModifier);  // 直接获得移动速度加成
        defenceModifier = new FloatModifier(defenceValue); // 初始获得90%减伤
        NumericBox.Defense.AddAddModifier(defenceModifier);
        attackModifier = new FloatModifier(addAttackPercentValue);
        NumericBox.Attack.AddPctAddModifier(attackModifier);
        currentActionCount = 0;
    }

    /// <summary>
    /// 加载技能
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

        // 闪避突进技能
        if (infoList.Count > 1)
        {
            dodgeSpikesSkillAbility = new DodgeSpikesSkillAbility(this, infoList[1]);
            skillAbilityManager.AddSkillAbility(dodgeSpikesSkillAbility);
            dodgeSpikesSkillAbility.SetEndEvent(SkillEndEvent); // 设置技能结束事件
        }
    }

    /// <summary>
    /// 当与子弹单位发生刚体碰撞判定时
    /// </summary>
    public override void OnBulletCollision(BaseBullet bullet)
    {
        if (bullet.GetRowIndex() != GetRowIndex())
            return;

        // 在移动状态下 且 没有被阻挡时 触发闪避突进技能
        if(mCurrentActionState!=null && mCurrentActionState is MoveState && !IsHasTarget())
        {
            dodgeSpikesSkillAbility.SetSkillEnable();
        }else if (UnitManager.CanBulletHit(this, bullet))
        {
            // 否则正常检测子弹碰撞逻辑
            bullet.TakeDamage(this);
        }
    }

    /// <summary>
    /// 重写对子弹碰撞的判定：在闪避技能期间不受子弹判定，其他情况则受
    /// </summary>
    /// <param name="baseBullet"></param>
    /// <returns></returns>
    public override bool CanHit(BaseBullet baseBullet)
    {
        return !dodgeSpikesSkillAbility.isSpelling;
    }

    public override void OnCastStateEnter()
    {
        currentActionCount++;
        animator.Play("Dodge"+(currentActionCount%3));
    }

    public override void OnCastState()
    {
        if (currentStateTimer == 0)
            return;
        // 动作播放完 或 被阻挡了则停止躲避技能
        if (AnimatorManager.GetNormalizedTime(animator) > 1.0 || IsHasTarget())
            dodgeSpikesSkillAbility.EndSkill();
    }

    /// <summary>
    /// 普通攻击结束后，攻击力加成标签重置
    /// </summary>
    public override void AfterGeneralAttack()
    {
        base.AfterGeneralAttack();
        addAttackPercentValue = 0;
        // 攻击力加成数值标签替换
        NumericBox.Attack.RemovePctAddModifier(attackModifier);
        attackModifier = new FloatModifier(addAttackPercentValue);
        NumericBox.Attack.AddPctAddModifier(attackModifier);
    }

    /// <summary>
    /// 当闪避技能结束时的事件
    /// </summary>
    private void SkillEndEvent()
    {
        defenceValue = Mathf.Max(0, defenceValue - 0.1f);
        addAttackPercentValue = Mathf.Min(1900, addAttackPercentValue + 200);
        // 减伤数值标签替换
        NumericBox.Defense.RemoveAddModifier(defenceModifier);
        if (defenceValue > 0)
        {
            defenceModifier = new FloatModifier(defenceValue);
            NumericBox.Defense.AddAddModifier(defenceModifier);
        }
        // 攻击力加成数值标签替换
        NumericBox.Attack.RemovePctAddModifier(attackModifier);
        attackModifier = new FloatModifier(addAttackPercentValue);
        NumericBox.Attack.AddPctAddModifier(attackModifier);
    }

}
