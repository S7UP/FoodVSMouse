using UnityEngine;

public class IceBucketFoodUnit : FoodUnit
{
    public override void MInit()
    {
        base.MInit();
        // ��ȡ100%���ˣ��ӽ����޵�����ֵ���Լ����߻ҽ���ɱЧ��
        NumericBox.Defense.SetBase(1);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, new BoolModifier(true));
        NumericBox.AddDecideModifierToBoolDict(StringManager.Invincibility, new BoolModifier(true));
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, new BoolModifier(true));
        SetMaxHpAndCurrentHp(float.MaxValue);
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
        return animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f; // ����������������һ�κ���Ϊ���ܽ���
        //return AnimatorManager.GetCurrentFrame(animator) == AnimatorManager.GetTotalFrame(animator);
    }

    /// <summary>
    /// �˳���ͨ������Ҫ������
    /// </summary>
    public override void AfterGeneralAttack()
    {
        // ִ���˺�Ч��
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
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        return (info.normalizedTime - Mathf.FloorToInt(info.normalizedTime) >= attackPercent && mAttackFlag);
    }

    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public override void ExecuteDamage()
    {
        // ԭ�ز���һ����ըЧ��
        {
            GameObject instance = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Effect/BoomEffect");
            BaseEffect effect = instance.GetComponent<BaseEffect>();
            effect.MInit();
            effect.animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/"+mType+"/BoomEffect");
            effect.transform.position = transform.position;
            GameController.Instance.AddEffect(effect);
        }
        // �����ез���λʩ�ӱ���Ч��
        foreach (var item in GameController.Instance.GetEachEnemy())
        {
            item.AddNoCountUniqueStatusAbility(StringManager.Frozen, new FrozenStatusAbility(item, 240));
            item.AddNoCountUniqueStatusAbility(StringManager.FrozenSlowDown, new FrozenSlowStatusAbility(item, 600));
        }
    }
}
