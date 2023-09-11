using S7P.Numeric;

using System.Collections.Generic;
/// <summary>
/// 加血类老鼠的技能
/// </summary>
public class HealMouse:MouseUnit
{
    private static List<float> healHpList = new List<float>{20, 40, 60, 60}; // 形态与回复量的映射表，后期可以单独拆成xml文件存储，实现数据与逻辑分离
    private GeneralAttackSkillAbility generalAttackSkillAbility; // 平A技能
    private PreSkillAbility preSkillAbility; // 演奏前置
    private EnemyHealSkillAbility enemyHealSkillAbility; // 演奏 

    public override void MInit()
    {
        base.MInit();
        typeAndShapeValue = 1; // 图层权重+1
    }

    /// <summary>
    /// 加载技能，加载普通攻击与回血技能
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
            
        // 回血技能
        if (infoList.Count > 2)
        {
            preSkillAbility = new PreSkillAbility(this, infoList[1]);
            skillAbilityManager.AddSkillAbility(preSkillAbility);
            // 与技能速率挂钩
            {
                FloatModifier skillSpeedMod = new FloatModifier((NumericBox.SkillSpeed.TotalValue - 1) * 100);
                preSkillAbility.energyRegeneration.AddPctAddModifier(skillSpeedMod);

                NumericBox.SkillSpeed.AddAfterValueChangeAction((val) => {
                    preSkillAbility.energyRegeneration.RemovePctAddModifier(skillSpeedMod);
                    skillSpeedMod.Value = (NumericBox.SkillSpeed.TotalValue - 1) * 100;
                    preSkillAbility.energyRegeneration.AddPctAddModifier(skillSpeedMod);
                });
            }

            enemyHealSkillAbility = new EnemyHealSkillAbility(this, infoList[2]);
            skillAbilityManager.AddSkillAbility(enemyHealSkillAbility);
            // 与技能速率挂钩
            {
                FloatModifier skillSpeedMod = new FloatModifier((NumericBox.SkillSpeed.TotalValue - 1) * 100);
                enemyHealSkillAbility.energyRegeneration.AddPctAddModifier(skillSpeedMod);

                NumericBox.SkillSpeed.AddAfterValueChangeAction((val) => {
                    enemyHealSkillAbility.energyRegeneration.RemovePctAddModifier(skillSpeedMod);
                    skillSpeedMod.Value = (NumericBox.SkillSpeed.TotalValue - 1) * 100;
                    enemyHealSkillAbility.energyRegeneration.AddPctAddModifier(skillSpeedMod);
                });
            }
        }
        preSkillAbility.SetTargetSkillAbility(enemyHealSkillAbility);
        preSkillAbility.SetSkillCondition(IsMeetingSkillCondition);
        preSkillAbility.SetBeforeSkillAction(BeforeSpell);
    }

    /// <summary>
    /// 释放技能的条件
    /// </summary>
    /// <returns></returns>
    private bool IsMeetingSkillCondition()
    {
        return true;
    }

    /// <summary>
    /// 释放技能前的事件
    /// </summary>
    private void BeforeSpell()
    {
        // 切换为施法状态
        // SetActionState(new CastState(this));
    }

    public override void OnCastStateEnter()
    {
        animatorController.Play("Cast", true);
    }

    /// <summary>
    /// 获取回复量
    /// </summary>
    /// <returns></returns>
    public float GetHealValue()
    {
        return healHpList[mShape]*mCurrentAttack;
    }
}
