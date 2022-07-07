using System.Collections.Generic;
/// <summary>
/// ħ����
/// </summary>
public class MagicMirrorMouse : MouseUnit
{
    private GeneralAttackSkillAbility generalAttackSkillAbility;
    private StealFireEnergySkillAbiliby stealFireEnergySkillAbiliby;
    private bool isFirstAttack;
    private int castState; // ʩ��״̬�� 0ǰҡ 1ʩ���� 2��ҡ

    public override void MInit()
    {
        base.MInit();
        isFirstAttack = true;
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

        // ͵�Թ��ܼ���
        if (infoList.Count > 1)
        {
            stealFireEnergySkillAbiliby = new StealFireEnergySkillAbiliby(this, infoList[1]);
            skillAbilityManager.AddSkillAbility(stealFireEnergySkillAbiliby);
        }
    }

    /// <summary>
    /// ��һ�ν��빥��״̬ʱ�������ָ�����������ϱ��ι����Դ�������
    /// </summary>
    public override void OnAttackStateEnter()
    {
        if (isFirstAttack)
        {
            isFirstAttack = false;
            // ��ϵ�ǰ����
            generalAttackSkillAbility.EndActivate();
            // �ָ�������
            stealFireEnergySkillAbiliby.FullCurrentEnergy();
        }
        else
        {
            base.OnAttackStateEnter();
        }
    }

    /// <summary>
    /// ���뼼��
    /// </summary>
    public override void OnCastStateEnter()
    {
        isFirstAttack = false; // ���뼼�ܵ�һ�ι����ָ���������Ч��ʧЧ
        animatorController.Play("PreCast");
    }

    public override void OnCastState()
    {
        if (currentStateTimer == 0)
            return;

        // ÿ������������ʱ����鼼��ʩ�ŵ�״̬���жϵ�ǰ����ʲô�׶Σ� ʩ��ǰҡ ʩ���� ʩ����ҡ
        if (castState==1) // ��ʩ���ڼ䣬�ȴ�ʩ������
        {
            if (stealFireEnergySkillAbiliby.IsEndCasting())
            {
                castState = 2;
                animatorController.Play("PostCast");
                currentStateTimer = 0;
                stealFireEnergySkillAbiliby.EndCasting();
            }
        }
        else if (AnimatorManager.GetNormalizedTime(animator) > 1.0)
        {
            if (castState == 0)
            {
                // ǰҡ����
                stealFireEnergySkillAbiliby.StartCasting(); // ��ʼʩ��
                animatorController.Play("Cast", true);
                currentStateTimer = 0;
                castState = 1;
            }else if(castState == 2)
            {
                // ��ҡ����
                castState = 0;
                stealFireEnergySkillAbiliby.EndActivate();
            }
        }
    }
}
