using System.Collections.Generic;
/// <summary>
/// ��Ѫ������ļ���
/// </summary>
public class HealMouse:MouseUnit
{
    private static List<float> healHpList = new List<float>{200, 400, 600, 800}; // ��̬��ظ�����ӳ������ڿ��Ե������xml�ļ��洢��ʵ���������߼�����
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
            enemyHealSkillAbility = new EnemyHealSkillAbility(this, infoList[2]);
            skillAbilityManager.AddSkillAbility(enemyHealSkillAbility);
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
        return healHpList[mShape];
    }
}
