using UnityEngine;
using System;
/// <summary>
/// ����ģ�͵Ĺ�����
/// </summary>
public class MouseModel : MouseUnit
{
    public bool canTriggerCat;
    public bool canTriggerLoseWhenEnterLoseLine;

    public override void MInit()
    {
        mType = -1;
        mShape = -1;
        canTriggerCat = true;
        canTriggerLoseWhenEnterLoseLine = true;
        base.MInit();

        spriteRenderer.sprite = null;
        animator.runtimeAnimatorController = null;
    }

    /// <summary>
    /// ���ؼ��ܣ��˴���������ͨ���������弼�ܼ���ʵ��������������д
    /// </summary>
    public override void LoadSkillAbility()
    {
        skillAbilityManager.AddSkillAbility(new GeneralAttackSkillAbility(this, new SkillAbility.SkillAbilityInfo()
        {
            name = "��ͨ����",
            needEnergy = 30,
            startEnergy = 0,
            energyRegeneration = 1,
            skillType = SkillAbility.Type.GeneralAttack,
            isExclusive = true,
            canActiveInDeathState = false,
            priority = 0,
        }));
    }

    public override void UpdateRuntimeAnimatorController()
    {

    }

    public override void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "Mouse/MouseModel", this.gameObject);
    }

    public static MouseModel GetInstance(RuntimeAnimatorController runtimeAnimatorController)
    {
        MouseModel m = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Mouse/MouseModel").GetComponent<MouseModel>();
        m.animator.runtimeAnimatorController = runtimeAnimatorController;
        m.MInit();
        m.animator.runtimeAnimatorController = runtimeAnimatorController;
        return m;
    }

    /// <summary>
    /// �ж��״̬��ͼʱ�����
    /// </summary>
    /// <param name="runtimeAnimatorControllerArray"></param>
    /// <param name="hertRateArray"></param>
    /// <returns></returns>
    public static MouseModel GetInstance(RuntimeAnimatorController[] runtimeAnimatorControllerArray, float[] hertRateArray)
    {
        MouseModel m = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Mouse/MouseModel").GetComponent<MouseModel>();
        m.animator.runtimeAnimatorController = runtimeAnimatorControllerArray[0];
        m.MInit();
        foreach (var item in hertRateArray)
        {
            m.mHertRateList.Add(item);
        }
        m.animator.runtimeAnimatorController = runtimeAnimatorControllerArray[0];
        Action<CombatAction> action = delegate
        {
            // Ҫ�����˵Ļ������˰�
            if (m.isDeathState)
                return;

            // �Ƿ�Ҫ�л���������flag
            bool flag = false;
            // �ָ�����һ��������ͼ���
            while (m.mHertIndex > 0 && m.GetHeathPercent() > m.mHertRateList[m.mHertIndex - 1])
            {
                m.mHertIndex--;
                flag = true;
            }
            // ��һ��������ͼ�ļ��
            while (m.mHertIndex < m.mHertRateList.Count && m.GetHeathPercent() <= m.mHertRateList[m.mHertIndex])
            {
                m.mHertIndex++;
                flag = true;
            }
            // ���л�֪ͨʱ���л�
            if (flag)
            {
                AnimatorStateRecorder a = m.animatorController.GetCurrentAnimatorStateRecorder(); // ��ȡ��ǰ�ڲ��ŵĶ���
                m.animator.runtimeAnimatorController = runtimeAnimatorControllerArray[m.mHertIndex];
                m.animatorController.ChangeAnimator(m.animator);
                // ���ֵ�ǰ��������
                if (a != null)
                {
                    m.animatorController.Play(a.aniName, a.isCycle, a.GetNormalizedTime());
                }
                m.OnUpdateRuntimeAnimatorController();
            }
        };
        m.AddActionPointListener(ActionPointType.PostReceiveDamage, action);
        m.AddActionPointListener(ActionPointType.PostReceiveReboundDamage, action);
        return m;
    }

    public override bool CanTriggerCat()
    {
        return canTriggerCat;
    }

    public override bool CanTriggerLoseWhenEnterLoseLine()
    {
        return canTriggerLoseWhenEnterLoseLine;
    }
}
