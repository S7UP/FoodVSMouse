using S7P.Numeric;

using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 小号鼠
/// </summary>
public class TrumpetMouse : MouseUnit
{
    private static BoolModifier IgnoreModifier = new BoolModifier(true);
    private CustomizationSkillAbility skill;

    public override void MInit()
    {
        base.MInit();
        typeAndShapeValue = 1;
    }

    public override void MUpdate()
    {
        base.MUpdate();
        // 检查自己有没有被控，若被控则移除控制效果并且获得一次吹号的机会
        if (GetNoCountUniqueStatus(StringManager.Stun) != null)
        {
            StatusManager.RemoveAllSettleDownDebuff(this);
            // 清空冰冻损伤
            ITask task = Environment.EnvironmentFacade.GetIceDebuff(this);
            if (task != null)
            {
                Environment.IceTask ice_task = task as Environment.IceTask;
                ice_task.AddValue(-ice_task.GetValue());
            }
            if (skill != null)
                skill.FullCurrentEnergy();
        }
    }

    public override void LoadSkillAbility()
    {
        List<SkillAbility.SkillAbilityInfo> infoList = AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape);
        // 普通攻击
        if (infoList.Count > 0)
        {
            GeneralAttackSkillAbility s = new GeneralAttackSkillAbility(this, infoList[0]);
            skillAbilityManager.AddSkillAbility(s);
        }

        // 吹小号技能
        SkillAbility.SkillAbilityInfo info = new SkillAbility.SkillAbilityInfo()
        {
            name = "激励",
            needEnergy = 540,
            startEnergy = 0,
            energyRegeneration = 1,
            skillType = SkillAbility.Type.SpecialAbility,
            isExclusive = true,
            canActiveInDeathState = false,
            priority = 1
        };
        {
            CustomizationSkillAbility s = new CustomizationSkillAbility(this, info);
            s.noClearEnergyWhenStart = false;
            s.noClearEnergyWhenEnd = true;
            s.IsMeetSkillConditionFunc = delegate { return true; };
            s.BeforeSpellFunc = delegate {
                SetActionState(new CastState(this));
            };
            s.OnSpellingFunc = delegate { };
            s.IsMeetCloseSpellingConditionFunc = delegate { return true; };
            s.AfterSpellFunc = delegate { };
            skillAbilityManager.AddSkillAbility(s);

            // 与技能速率挂钩
            {
                FloatModifier skillSpeedMod = new FloatModifier((NumericBox.SkillSpeed.TotalValue - 1) * 100);
                s.energyRegeneration.AddPctAddModifier(skillSpeedMod);

                NumericBox.SkillSpeed.AddAfterValueChangeAction((val) => {
                    s.energyRegeneration.RemovePctAddModifier(skillSpeedMod);
                    skillSpeedMod.Value = (NumericBox.SkillSpeed.TotalValue - 1) * 100;
                    s.energyRegeneration.AddPctAddModifier(skillSpeedMod);
                });
            }
            skill = s;
        }
    }


    public override void OnCastStateEnter()
    {
        animatorController.Play("Cast");
    }

    public override void OnCastState()
    {
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            SetActionState(new MoveState(this));
    }

    public override void OnCastStateExit()
    {
        // 为本行老鼠解控
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(new Vector2(MapManager.GetColumnX(4), transform.position.y), new Vector2(11 * MapManager.gridWidth, 0.5f * MapManager.gridHeight), "ItemCollideEnemy");
        r.isAffectMouse = true;
        r.SetInstantaneous();
        r.SetOnEnemyEnterAction((m) => {
            StatusManager.RemoveAllSettleDownDebuff(m);
            // 清空冰冻损伤
            ITask task = Environment.EnvironmentFacade.GetIceDebuff(m);
            if (task != null)
            {
                Environment.IceTask ice_task = task as Environment.IceTask;
                ice_task.AddValue(-ice_task.GetValue());
            }
        });
        GameController.Instance.AddAreaEffectExecution(r);
    }
}
