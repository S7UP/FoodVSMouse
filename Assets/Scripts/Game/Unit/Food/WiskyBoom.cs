using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WiskyBoom : FoodUnit
{
    public override void MInit()
    {
        base.MInit();
        SetLevel(12);
    }

    /// <summary>
    /// ���ݵȼ���͵ȼ������¶�Ӧ����
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        //NumericBox.Attack.SetBase((float)(attr.baseAttrbute.baseAttack + attr.valueList[mLevel]));
    }

    /// <summary>
    /// �ж��Ƿ�����Ч�Ĺ���Ŀ��
    /// </summary>
    /// <returns></returns>
    protected override bool IsHasTarget()
    {
        // ��ʱ��ը������Ҫ
        return false;
    }

    /// <summary>
    /// �Ƿ�������ͨ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        // ��ʱ��ը������Ҫ
        return true;
    }

    /// <summary>
    /// ������ͨ��������״̬
    /// </summary>
    public override void BeforeGeneralAttack()
    {
        // �л�Ϊ����������ͼ
        SetActionState(new AttackState(this));
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
        return (AnimatorManager.GetCurrentFrame(animator) == AnimatorManager.GetTotalFrame(animator)-1); // �����ŵ����һ֡ʱ�˳�
        //return animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f; // ����������������һ�κ���Ϊ���ܽ���
    }

    /// <summary>
    /// �˳���ͨ������Ҫ������
    /// </summary>
    public override void AfterGeneralAttack()
    {
        // �˺��ж�Ϊ��ʧʱ����
        ExecuteDamage();
        // �ҽ��Ϳ�Ƭֱ����������
        ExecuteDeath();
    }

    /// <summary>
    /// �Ƿ�Ϊ�˺��ж�ʱ�̣���ս����Ϊ���ʵ���˺���Զ�̹���Ϊȷ�����䵯�壩
    /// </summary>
    /// <returns></returns>
    public override bool IsDamageJudgment()
    {
        return false;
    }

    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public override void ExecuteDamage()
    {
        // ԭ�ز���һ����ըЧ��
        {
            GameObject instance = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Effect/FireVertical");
            BaseEffect effect = instance.GetComponent<BaseEffect>();
            effect.InIt();
            effect.transform.position = new Vector3(transform.position.x, MapManager.GetRowY(3), transform.position.z);
            GameController.Instance.AddEffect(effect);
        }
        // ��Ӷ�Ӧ���ж������
        {

            GameObject instance = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "AreaEffect/BombAreaEffect");
            BombAreaEffectExecution bombEffect = instance.GetComponent<BombAreaEffectExecution>();
            bombEffect.Init(this, 900, GetRowIndex(), 1, int.MaxValue, 0, 0, false, true);
            bombEffect.transform.position = this.GetPosition();
            GameController.Instance.AddAreaEffectExecution(bombEffect);
        }

    }
}
