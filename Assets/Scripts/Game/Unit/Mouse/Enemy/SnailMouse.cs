using S7P.Numeric;

using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 蜗牛车鼠
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
        // 免疫减速、晕眩、冰冻效果
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, IgnoreModifier);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, IgnoreModifier);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreSlowDown, IgnoreModifier);
    }

    /// <summary>
    /// 加载技能，加载普通攻击与技能
    /// </summary>
    public override void LoadSkillAbility()
    {
        List<SkillAbility.SkillAbilityInfo> infoList = AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape);
        // 普通攻击
        if (infoList.Count > 0)
        {
            GeneralAttackSkillAbility s = new GeneralAttackSkillAbility(this, infoList[0]);
            skillAbilityManager.AddSkillAbility(s);
        }

        // 放粘液技能
        SkillAbility.SkillAbilityInfo info = new SkillAbility.SkillAbilityInfo()
        {
            name = "粘液",
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
        // 拉一坨

        // 判定部分
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
        // 特效部分
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
        // 动画播放完就切回去
        if(animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            SetActionState(new MoveState(this));
    }

    public override void OnDieStateEnter()
    {
        GameManager.Instance.audioSourceController.PlayEffectMusic("Explosion");
        base.OnDieStateEnter();
    }
}
