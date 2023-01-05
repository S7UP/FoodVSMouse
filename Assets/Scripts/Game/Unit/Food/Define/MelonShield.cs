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

    // ��λ������ػ���ʱ����
    public override void OnDisable()
    {
        base.OnDisable();
    }

    public override void MInit()
    {
        mHertIndex = 0;
        base.MInit();

        // ���ܵ��˺�����֮�󣬸���������ͼ״̬
        AddActionPointListener(ActionPointType.PostReceiveDamage, delegate { UpdateHertMap(); });
        AddActionPointListener(ActionPointType.PostReceiveReboundDamage, delegate { UpdateHertMap(); });
        // һת�����ܵ��˺�����֮ǰ���㷴��
        if (mShape>0)
            AddActionPointListener(ActionPointType.PreReceiveDamage, ReBoundDamage);
        // �ڽ������ƽ���֮�󣬸���������ͼ״̬
        AddActionPointListener(ActionPointType.PostReceiveCure, delegate { UpdateHertMap(); });
        Spr_Inside.sprite = GameManager.Instance.GetSprite("Food/" + mType + "/inside/" + mHertIndex);
    }

    public override void SetCollider2DParam()
    {
        mBoxCollider2D.offset = new Vector2(0, -0.08f * MapManager.gridHeight);
        mBoxCollider2D.size = new Vector2(0.65f * MapManager.gridWidth, 0.33f * MapManager.gridHeight);
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

    public override void OnBurnStateEnter()
    {
        // ����ҲҪװ���ջٲ���
        Spr_Inside.material = GameManager.Instance.GetMaterial("Dissolve2");
        base.OnBurnStateEnter();
    }

    public override void DuringBurn(float _Threshold)
    {
        Spr_Inside.material.SetFloat("_Threshold", _Threshold);
        base.DuringBurn(_Threshold);
    }

    /// <summary>
    /// �����˺�
    /// </summary>
    private void ReBoundDamage(CombatAction action)
    {
        DamageAction dmgAction = action as DamageAction;
        // ����һת���ܷ���
        if (mShape > 0)
        {
            // ��Ӷ�Ӧ���ж������
            {
                DamageAreaEffectExecution dmgEffect = DamageAreaEffectExecution.GetInstance();
                // �����˺���Ӧ�ó�������ǰ����ֵ����ȥ��
                // ����û�ˣ������Ǹ�ע�ͱ������
                dmgEffect.Init(this, CombatAction.ActionType.ReboundDamage, dmgAction.DamageValue * mCurrentAttack/10, GetRowIndex(), 3, 3, 0, 0, false, true);
                dmgEffect.transform.position = this.GetPosition();
                GameController.Instance.AddAreaEffectExecution(dmgEffect);
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
            // ��Ӷ�Ӧ���ж������
            {
                DamageAreaEffectExecution dmgEffect = DamageAreaEffectExecution.GetInstance();
                dmgEffect.Init(this, CombatAction.ActionType.ReboundDamage, Mathf.Max(0, mCurrentHp) * mCurrentAttack / 10, GetRowIndex(), 3, 3, 0, 0, false, true);
                dmgEffect.transform.position = this.GetPosition();
                GameController.Instance.AddAreaEffectExecution(dmgEffect);
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
