using S7P.Numeric;

using System.Collections.Generic;
/// <summary>
/// ��Ѫ������ļ���
/// </summary>
public class HealMouse:MouseUnit
{
    private static List<float> healHpList = new List<float>{20, 40, 60, 60}; // ��̬��ظ�����ӳ������ڿ��Ե������xml�ļ��洢��ʵ���������߼�����
    private GeneralAttackSkillAbility generalAttackSkillAbility; // ƽA����
    private PreSkillAbility preSkillAbility; // ����ǰ��
    private EnemyHealSkillAbility enemyHealSkillAbility; // ���� 

    public override void MInit()
    {
        base.MInit();
        typeAndShapeValue = 1; // ͼ��Ȩ��+1
    }

    /// <summary>
    /// ���ؼ��ܣ�������ͨ�������Ѫ����
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
            
        // ��Ѫ����
        if (infoList.Count > 2)
        {
            preSkillAbility = new PreSkillAbility(this, infoList[1]);
            skillAbilityManager.AddSkillAbility(preSkillAbility);
            // �뼼�����ʹҹ�
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
            // �뼼�����ʹҹ�
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
    /// �ͷż��ܵ�����
    /// </summary>
    /// <returns></returns>
    private bool IsMeetingSkillCondition()
    {
        return true;
    }

    /// <summary>
    /// �ͷż���ǰ���¼�
    /// </summary>
    private void BeforeSpell()
    {
        // �л�Ϊʩ��״̬
        // SetActionState(new CastState(this));
    }

    public override void OnCastStateEnter()
    {
        animatorController.Play("Cast", true);
    }

    /// <summary>
    /// ��ȡ�ظ���
    /// </summary>
    /// <returns></returns>
    public float GetHealValue()
    {
        return healHpList[mShape]*mCurrentAttack;
    }
}
