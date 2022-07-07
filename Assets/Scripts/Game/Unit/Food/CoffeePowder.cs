using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

using static UnityEngine.UI.CanvasScaler;
/// <summary>
/// ���ȷ�
/// </summary>
public class CoffeePowder : FoodUnit
{
    public override void MInit()
    {
        base.MInit();
        // ��ȡ100%���ˣ��ӽ����޵�����ֵ���Լ����߻ҽ���ɱЧ��
        NumericBox.Defense.SetBase(1);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, new BoolModifier(true));
        NumericBox.AddDecideModifierToBoolDict(StringManager.Invincibility, new BoolModifier(true));
        SetMaxHpAndCurrentHp(float.MaxValue);
        // ����ѡȡ
        CloseCollision();
    }

    public override void OnCastStateEnter()
    {
        animatorController.Play("Cast");
    }

    /// <summary>
    /// �Ƿ�������ͨ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        return true;
    }

    /// <summary>
    /// ������ͨ��������״̬
    /// </summary>
    public override void BeforeGeneralAttack()
    {
        // �л�Ϊ����������ͼ
        SetActionState(new CastState(this));
    }

    /// <summary>
    /// ��ͨ�����ڼ�
    /// </summary>
    public override void OnGeneralAttack()
    {

    }

    /// <summary>
    /// �˳���ͨ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetEndGeneralAttackCondition()
    {
        //return (AnimatorManager.GetNormalizedTime(animator) > 1.0 && !mAttackFlag);
        AnimatorStateRecorder a = animatorController.GetCurrentAnimatorStateRecorder();
        if (a != null)
        {
            return a.IsFinishOnce();
        }
        return false;
    }

    /// <summary>
    /// �˳���ͨ������Ҫ������
    /// </summary>
    public override void AfterGeneralAttack()
    {
        // �Ե�ǰ��Ƭʩ��Ч��
        ExecuteDamage();
        // ֱ����������
        ExecuteDeath();
    }

    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public override void ExecuteDamage()
    {
        // ʹ��ǰ��ӵ������ܻ����ȼ��Ŀ�ƬCD����
        BaseGrid g = GetGrid();
        if (g != null)
        {
            foreach (var item in g.GetAttackableFoodUnitList())
            {
                // �Ƴ���Щ���ĸ������Ч��
                StatusManager.RemoveAllSettleDownDebuff(item);
                // ���ӹ���Ч��
                item.AddStatusAbility(new AttackSpeedStatusAbility(item, 5 * 60, attr.valueList[mLevel]));
            }
        }
    }
}
