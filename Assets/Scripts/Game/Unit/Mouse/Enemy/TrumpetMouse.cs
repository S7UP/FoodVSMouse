using S7P.Numeric;

using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// С����
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
        // ����Լ���û�б��أ����������Ƴ�����Ч�����һ��һ�δ��ŵĻ���
        if (GetNoCountUniqueStatus(StringManager.Stun) != null)
        {
            StatusManager.RemoveAllSettleDownDebuff(this);
            // ��ձ�������
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
        // ��ͨ����
        if (infoList.Count > 0)
        {
            GeneralAttackSkillAbility s = new GeneralAttackSkillAbility(this, infoList[0]);
            skillAbilityManager.AddSkillAbility(s);
        }

        // ��С�ż���
        SkillAbility.SkillAbilityInfo info = new SkillAbility.SkillAbilityInfo()
        {
            name = "����",
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

            // �뼼�����ʹҹ�
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
        // Ϊ����������
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(new Vector2(MapManager.GetColumnX(4), transform.position.y), new Vector2(11 * MapManager.gridWidth, 0.5f * MapManager.gridHeight), "ItemCollideEnemy");
        r.isAffectMouse = true;
        r.SetInstantaneous();
        r.SetOnEnemyEnterAction((m) => {
            StatusManager.RemoveAllSettleDownDebuff(m);
            // ��ձ�������
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
