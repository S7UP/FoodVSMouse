using S7P.Numeric;

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// ħ����
/// </summary>
public class MagicMirrorMouse : MouseUnit
{
    private GeneralAttackSkillAbility generalAttackSkillAbility;
    private StealFireEnergySkillAbiliby stealFireEnergySkillAbiliby;

    private float stealFire; // ͵���Ļ�
    private int timeLeft; // ʩ��ʣ��ʱ��
    private CustomizationTask castTask;
    private TaskController castTaskController = new TaskController();

    public override void MInit()
    {
        stealFire = 0;
        timeLeft = 0;
        castTask = null;
        castTaskController.Initial();
        base.MInit();

        // ��е���ܵ�ˮʴ�˺�����
        Environment.WaterTask.AddUnitWaterRate(this, new S7P.Numeric.FloatModifier(2.0f));
    }

    public override void AfterDeath()
    {
        if(stealFire > 0)
            SmallStove.CreateAddFireEffect(transform.position, stealFire*0.75f);
        base.AfterDeath();
    }

    private CustomizationTask GetCastTask()
    {
        int stealTimeLeft = 60;
        CustomizationTask task = new CustomizationTask();
        task.AddOnEnterAction(delegate {
            animatorController.Play("PreCast");
        });
        task.AddTaskFunc(delegate {
            if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                timeLeft += 240;
                animatorController.Play("Cast", true);
                return true;
            }
            return false;
        });
        task.AddTaskFunc(delegate {
            timeLeft--;
            stealTimeLeft--;
            if(stealTimeLeft <= 0)
            {
                stealTimeLeft += 60;
                stealFire += 50;
                CreateDecFire(50);
            }
            if (timeLeft <= 0)
            {
                animatorController.Play("PostCast");
                return true;
            }
            return false;
        });
        task.AddTaskFunc(delegate {
            return animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce();
        });
        return task;
    }

    private void CreateDecFire(float decCount)
    {
        // ��ʾ�ظ���������Ч
        BaseEffect e = BaseEffect.GetInstance("Emp_AddFireEffect");
        GameController.Instance.AddEffect(e);
        e.transform.SetParent(GameManager.Instance.GetUICanvas().transform);
        e.transform.localScale = Vector3.one;
        e.transform.position = transform.position;
        Text text = e.transform.Find("Text").GetComponent<Text>();
        text.text = "-" + (int)decCount;
        text.color = new Color(1, 0, 0);
        // ʵ�ʻظ�
        GameController.Instance.AddFireResource(-decCount);
    }

    /// <summary>
    /// ���ؼ���
    /// </summary>
    public override void LoadSkillAbility()
    {
        List<SkillAbility.SkillAbilityInfo> infoList = AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape);
        // ��ͨ����
        if (infoList.Count > 0)
        {
            generalAttackSkillAbility = new GeneralAttackSkillAbility(this, infoList[0]);
            skillAbilityManager.AddSkillAbility(generalAttackSkillAbility);
        }

        {
            CustomizationSkillAbility skill = new CustomizationSkillAbility(this, new SkillAbility.SkillAbilityInfo()
            {
                name = "������ȡ",
                needEnergy = 900,
                startEnergy = 360,
                energyRegeneration = 1,
                skillType = SkillAbility.Type.SpecialAbility,
                isExclusive = true,
                canActiveInDeathState = false,
                priority = 10,
            });
            skill.BeforeSpellFunc = delegate {
                castTask = GetCastTask();
                castTaskController.AddTask(castTask);
                SetActionState(new CastState(this));
            };
            skill.IsMeetSkillConditionFunc = delegate { return true; };
            skill.IsMeetCloseSpellingConditionFunc = delegate { return castTask.IsEnd(); };
            skill.AfterSpellFunc = delegate {
                SetActionState(new MoveState(this));
                castTask = null;
            };
            skillAbilityManager.AddSkillAbility(skill);

            // �뼼�����ʹҹ�
            FloatModifier skillSpeedMod = new FloatModifier((NumericBox.SkillSpeed.TotalValue - 1) * 100);
            skill.energyRegeneration.AddPctAddModifier(skillSpeedMod);

            NumericBox.SkillSpeed.AddAfterValueChangeAction((val) => {
                skill.energyRegeneration.RemovePctAddModifier(skillSpeedMod);
                skillSpeedMod.Value = (NumericBox.SkillSpeed.TotalValue - 1) * 100;
                skill.energyRegeneration.AddPctAddModifier(skillSpeedMod);
            });
        }
    }

    /// <summary>
    /// ���뼼��
    /// </summary>
    public override void OnCastStateEnter()
    {

    }

    public override void OnCastState()
    {
        castTaskController.Update();
    }
}
