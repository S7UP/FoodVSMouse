using S7P.Numeric;

using UnityEngine;
/// <summary>
/// ��ת�������
/// </summary>
public class SpinCoffee : FoodUnit
{
    private FloatModifier costMod = new FloatModifier(-50f / 7 / 60);

    public override void MInit()
    {
        base.MInit();
        // һתʩ�Ӳ��ᱻѡΪ����Ŀ��Ͳ��ɱ��赲��Ч��
        if(mShape >= 1)
        {
            AddCanBeSelectedAsTargetFunc(delegate { return false; });
            AddCanBlockFunc(delegate { return false; });
            // ���������Ч
            BaseEffect e = BaseEffect.CreateInstance(GameManager.Instance.GetRuntimeAnimatorController("Effect/HiddenEffect"), "Appear", "Idle", "Disappear", true);
            e.SetSpriteRendererSorting("Effect", 2);
            GameController.Instance.AddEffect(e);
            mEffectController.AddEffectToDict("SpinCoffeeHidden", e, new Vector2(0, 0 * 0.5f * MapManager.gridWidth));
        }
        // ÿ7��50��
        GameController.Instance.AddCostResourceModifier("Fire", costMod);
    }

    public override void MDestory()
    {
        GameController.Instance.RemoveCostResourceModifier("Fire", costMod);
        base.MDestory();
    }

    /// <summary>
    /// ���ݵȼ���͵ȼ������¶�Ӧ����
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        NumericBox.Attack.SetBase((float)(attr.baseAttrbute.baseAttack + attr.valueList[mLevel]));
    }

    /// <summary>
    /// �ж��Ƿ�����Ч�Ĺ���Ŀ��
    /// </summary>
    /// <returns></returns>
    protected override bool IsHasTarget()
    {
        if (GameController.Instance.isEnableNoTargetAttackMode)
            return true;

        float left = transform.position.x - 1.5f * MapManager.gridWidth;
        float right = transform.position.x + 1.5f * MapManager.gridWidth;
        float bottom = transform.position.y - 1.5f * MapManager.gridHeight;
        float top = transform.position.y + 1.5f * MapManager.gridHeight;
        foreach (var unit in GameController.Instance.GetEachEnemy())
        {
            if (unit.transform.position.x >= left && unit.transform.position.x <= right && unit.transform.position.y >= bottom && unit.transform.position.y <= top)
                return true;
        }
        return false;
    }

    /// <summary>
    /// �Ƿ�������ͨ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        // ����Ŀ�꼴��
        return IsHasTarget();
    }

    /// <summary>
    /// ������ͨ��������״̬
    /// </summary>
    public override void BeforeGeneralAttack()
    {
        // �л�Ϊ����������ͼ
        SetActionState(new AttackState(this));
        GameManager.Instance.audioSourceManager.PlayEffectMusic("Fume");
    }

    /// <summary>
    /// ��ͨ�����ڼ�
    /// </summary>
    public override void OnGeneralAttack()
    {
        // �˺��ж�֡Ӧ��ִ���ж�
        if (IsDamageJudgment())
        {
            mAttackFlag = false;
            ExecuteDamage();
        }
    }

    /// <summary>
    /// �˳���ͨ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetEndGeneralAttackCondition()
    {
        return animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce();
    }

    /// <summary>
    /// �˳���ͨ������Ҫ������
    /// </summary>
    public override void AfterGeneralAttack()
    {
        mAttackFlag = true;
        SetActionState(new IdleState(this));
    }

    /// <summary>
    /// �Ƿ�Ϊ�˺��ж�ʱ�̣���ս����Ϊ���ʵ���˺���Զ�̹���Ϊȷ�����䵯�壩
    /// </summary>
    /// <returns></returns>
    public override bool IsDamageJudgment()
    {
        return (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= attackPercent && mAttackFlag);
    }

    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public override void ExecuteDamage()
    {
        float dmg = mCurrentAttack;
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, 3, 3, "ItemCollideEnemy");
        r.isAffectMouse = true;
        r.SetOnEnemyEnterAction((u) => {
            DamageAction d = new DamageAction(CombatAction.ActionType.CauseDamage, this, u, dmg);
            d.AddDamageType(DamageAction.DamageType.AOE);
            d.ApplyAction();
        });
        r.SetInstantaneous();
        GameController.Instance.AddAreaEffectExecution(r);
    }
}
