using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ��Ƥ����
/// </summary>
public class MelonShield : FoodUnit
{
    private int mHertIndex; // ���˽׶� 0������ 1��С�� 2������
    private SpriteRenderer Spr_Inside;
    private List<float> mHertRateList = new List<float>()
    {
        0.67f, 0.33f
    };

    public override void Awake()
    {
        base.Awake();
        Spr_Inside = transform.Find("Ani_Food").Find("Spr_Inside").GetComponent<SpriteRenderer>();
        typeAndShapeToLayer = 1; // ��Ⱦ�㼶Ӧ��Ҫ��һ��s
    }

    public override void MInit()
    {
        mHertIndex = 0;
        base.MInit();

        // ���ܵ��˺�����֮�󣬸���������ͼ״̬
        AddActionPointListener(ActionPointType.PostReceiveDamage, delegate { UpdateHertMap(); });
        // һת�����ܵ��˺�����֮ǰ���㷴��
        if (mShape > 0)
            AddActionPointListener(ActionPointType.PreReceiveDamage, ReBoundDamage);
        // �ڽ������ƽ���֮�󣬸���������ͼ״̬
        AddActionPointListener(ActionPointType.PostReceiveCure, delegate { UpdateHertMap(); });
        Spr_Inside.sprite = GameManager.Instance.GetSprite("Food/" + mType + "/inside/" + mHertIndex);
    }

    public override void SetCollider2DParam()
    {
        mBoxCollider2D.offset = new Vector2(0, 0);
        mBoxCollider2D.size = new Vector2(0.65f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
    }

    /// <summary>
    /// ���ݵȼ���͵ȼ������¶�Ӧ����
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        SetMaxHpAndCurrentHp((float)(attr.baseAttrbute.baseHP + attr.valueList[mLevel]));
    }

    
    public override void OnIdleStateEnter()
    {
        animatorController.Play("Idle"+ mHertIndex);
    }

    /// <summary>
    /// �����˻��߱�����ʱ�����µ�λ��ͼ״̬
    /// </summary>
    protected void UpdateHertMap()
    {
        // Ҫ�����˵Ļ������˰�
        if (isDeathState)
            return;

        // �Ƿ�Ҫ�л���������flag
        bool flag = false;
        // �ָ�����һ��������ͼ���
        while (mHertIndex > 0 && GetHeathPercent() > mHertRateList[mHertIndex - 1])
        {
            mHertIndex--;
            flag = true;
        }
        // ��һ��������ͼ�ļ��
        while (mHertIndex < mHertRateList.Count && GetHeathPercent() <= mHertRateList[mHertIndex])
        {
            mHertIndex++;
            flag = true;
        }
        // ���л�֪ͨʱ���л�
        if (flag)
        {
            animatorController.Play("Idle" + mHertIndex);
            Spr_Inside.sprite = GameManager.Instance.GetSprite("Food/" + mType + "/inside/" + mHertIndex);
        }
    }

    /// <summary>
    /// �����˺�
    /// </summary>
    private void ReBoundDamage(CombatAction action)
    {
        DamageAction dmgAction = action as DamageAction;
        // ����һת���ܷ��� ���� �˺���Դ�����Ƿ�����Դ�����������������з����߼���Ŀ������޵ݹ飩
        if (mShape > 0 && !dmgAction.IsDamageType(DamageAction.DamageType.Rebound) && !NumericBox.GetBoolNumericValue(StringManager.Invincibility))
        {
            // ��Ӷ�Ӧ���ж������
            {
                float dmg = Mathf.Min(mCurrentHp, dmgAction.DamageValue * GetFinalDamageRate()) * mCurrentAttack / 10;
                RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, 3, 3, "ItemCollideEnemy");
                r.SetInstantaneous();
                r.isAffectMouse = true;
                r.SetOnEnemyEnterAction((u) => {
                    DamageAction d = new DamageAction(CombatAction.ActionType.RealDamage, this, u, dmg);
                    d.AddDamageType(DamageAction.DamageType.Rebound);
                    d.ApplyAction();
                });
                GameController.Instance.AddAreaEffectExecution(r);
            }
        }
    }

    /// <summary>
    /// ������������㼶���ڸ����и÷����ᱻ����
    /// </summary>
    /// <param name="arrayIndex"></param>
    public override void UpdateSpecialRenderLayer()
    {
        Spr_Inside.sortingOrder = LayerManager.CalculateSortingLayer(LayerManager.UnitType.Ally, GetRowIndex(), -25, 0);
    }

    /// <summary>
    /// һתǰ��������ʱ��ʣ������ֵ����0��Է�Χ�����е������һ��ʣ������ֵ���˺�
    /// �˷�������һЩ�������ơ�ǿ���Ƴ�ʱ����
    /// </summary>
    public override void AfterDeath()
    {
        if (mShape>0 && mCurrentHp > 0)
        {
            float dmg = Mathf.Max(0, mCurrentHp) * mCurrentAttack / 10;
            // ��Ӷ�Ӧ���ж������
            {
                RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, 3, 3, "ItemCollideEnemy");
                r.SetInstantaneous();
                r.isAffectMouse = true;
                r.SetOnEnemyEnterAction((u) => {
                    DamageAction d = new DamageAction(CombatAction.ActionType.RealDamage, this, u, dmg);
                    d.AddDamageType(DamageAction.DamageType.Rebound);
                    d.ApplyAction();
                });
                GameController.Instance.AddAreaEffectExecution(r);
            }
        }
    }


    /////////////////////////////////���¹��ܾ�ʧЧ������Ҫ���·���/////////////////////////////////////

    /// <summary>
    /// �ж��Ƿ�����Ч�Ĺ���Ŀ��
    /// </summary>
    /// <returns></returns>
    protected override bool IsHasTarget()
    {
        // �����Ϳ�Ƭ����Ҫ
        return false;
    }

    /// <summary>
    /// �Ƿ�������ͨ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        // �����Ϳ�Ƭ����Ҫ
        return true;
    }

    /// <summary>
    /// ������ͨ��������״̬
    /// </summary>
    public override void BeforeGeneralAttack()
    {
        // �����Ϳ�Ƭ�޹���״̬
    }

    /// <summary>
    /// ��ͨ�����ڼ�
    /// </summary>
    public override void OnGeneralAttack()
    {
        // �����Ϳ�Ƭ��
    }

    /// <summary>
    /// �˳���ͨ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetEndGeneralAttackCondition()
    {
        // �����Ϳ�Ƭ��
        return true;
    }

    /// <summary>
    /// �˳���ͨ������Ҫ������
    /// </summary>
    public override void AfterGeneralAttack()
    {
        // �����Ϳ�Ƭ��
    }

    /// <summary>
    /// �Ƿ�Ϊ�˺��ж�ʱ�̣���ս����Ϊ���ʵ���˺���Զ�̹���Ϊȷ�����䵯�壩
    /// </summary>
    /// <returns></returns>
    public override bool IsDamageJudgment()
    {
        // �����Ϳ�Ƭ��
        return false;
    }

    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public override void ExecuteDamage()
    {
        // �����Ϳ�Ƭ��
    }

    public override List<SpriteRenderer> GetSpriteRendererList()
    {
        List<SpriteRenderer> l = new List<SpriteRenderer>();
        l.Add(GetSpriteRenderer());
        l.Add(Spr_Inside);
        return l;
    }
}
