using S7P.Numeric;

using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ��ţ����
/// </summary>
public class SnailMouse : MouseUnit
{
    private static BoolModifier IgnoreModifier = new BoolModifier(true);
    private RuntimeAnimatorController Mucous_Run;

    public override void Awake()
    {
        if(Mucous_Run == null)
        {
            Mucous_Run = GameManager.Instance.GetRuntimeAnimatorController("Mouse/26/Mucous");
        }
        base.Awake();
    }

    public override void MInit()
    {
        base.MInit();
        // ���߼��١���ѣ������Ч��
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, IgnoreModifier);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, IgnoreModifier);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreSlowDown, IgnoreModifier);
    }

    /// <summary>
    /// ���ؼ��ܣ�������ͨ�����뼼��
    /// </summary>
    public override void LoadSkillAbility()
    {
        List<SkillAbility.SkillAbilityInfo> infoList = AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape);
        // ��ͨ����
        if (infoList.Count > 0)
        {
            GeneralAttackSkillAbility s = new GeneralAttackSkillAbility(this, infoList[0]);
            skillAbilityManager.AddSkillAbility(s);
        }

        // ��ճҺ����
        SkillAbility.SkillAbilityInfo info = new SkillAbility.SkillAbilityInfo()
        {
            name = "ճҺ",
            needEnergy = 720,
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
        }
    }

    public override void OnCastStateEnter()
    {
        animatorController.Play("Cast");

        Vector2 pos = (Vector2)transform.position - moveRotate * MapManager.gridWidth;
        // ��һ��

        // �ж�����
        RetangleAreaEffectExecution r = null;
        {
            FloatModifier mod = new FloatModifier(100);

            r = RetangleAreaEffectExecution.GetInstance(pos, new Vector2(MapManager.gridWidth, 0.5f * MapManager.gridHeight), "ItemCollideEnemy");
            r.SetAffectHeight(0);
            r.isAffectMouse = true;
            r.SetOnEnemyEnterAction((m) => {
                m.NumericBox.MoveSpeed.AddPctAddModifier(mod);
            });
            r.SetOnEnemyExitAction((m) => {
                m.NumericBox.MoveSpeed.RemovePctAddModifier(mod);
            });
            GameController.Instance.AddAreaEffectExecution(r);

            CustomizationTask task = new CustomizationTask();
            task.AddTimeTaskFunc(2160);
            task.AddOnExitAction(delegate {
                r.MDestory();
            });
            r.taskController.AddTask(task);
        }
        // ��Ч����
        {
            BaseEffect e = BaseEffect.CreateInstance(Mucous_Run, "Appear", "Idle", "Disappear", true);
            e.transform.position = pos;
            e.SetSpriteRendererSorting("Grid", 1);
            GameController.Instance.AddEffect(e);

            CustomizationTask task = new CustomizationTask();
            task.AddTaskFunc(delegate {
                return !r.IsValid();
            });
            task.AddOnExitAction(delegate {
                e.ExecuteDeath();
            });
            e.taskController.AddTask(task);
        }
    }

    public override void OnCastState()
    {
        // ������������л�ȥ
        if(animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            SetActionState(new MoveState(this));
    }

    public override void OnDieStateEnter()
    {
        GameManager.Instance.audioSourceController.PlayEffectMusic("Explosion");
        base.OnDieStateEnter();
    }
}
