using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D.Path;

using UnityEngine;
/// <summary>
/// �˿˻���
/// </summary>
public class PokerShield : FoodUnit
{
    private int mHertIndex; // ���˽׶� 0������ 1��С�� 2������
    private SpriteRenderer Spr_Inside;
    private Animator InsideAnimator;
    private AnimatorController InsideAnimatorController = new AnimatorController();
    private List<float> mHertRateList = new List<float>()
    {
        0.67f, 0.33f
    };

    public override void Awake()
    {
        base.Awake();
        Spr_Inside = transform.Find("Ani_Food").Find("Spr_Inside").GetComponent<SpriteRenderer>();
        InsideAnimator = Spr_Inside.GetComponent<Animator>();
        typeAndShapeToLayer = 1; // ��Ⱦ�㼶Ӧ��Ҫ��һ��
        // ȫ�����߶�����Ⱦ
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBacterialInfection, new BoolModifier(true));
    }

    // ��λ������ػ���ʱ����
    public override void OnDisable()
    {
        base.OnDisable();
        Spr_Inside.material = defaultMaterial; // ������
    }

    public override void MInit()
    {
        mHertIndex = 0;
        base.MInit();
        InsideAnimatorController.ChangeAnimator(InsideAnimator);
        // ���ܵ��˺�����֮�󣬸���������ͼ״̬
        AddActionPointListener(ActionPointType.PostReceiveDamage, delegate { UpdateHertMap(); });
        // �ڽ������ƽ���֮�󣬸���������ͼ״̬
        AddActionPointListener(ActionPointType.PostReceiveCure, delegate { UpdateHertMap(); });
        // Spr_Inside.sprite = GameManager.Instance.GetSprite("Food/" + mType + "/inside/" + mHertIndex);
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
        animatorController.Play("Idle" + mHertIndex);
        InsideAnimatorController.Play("Idle" + mHertIndex);
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
            InsideAnimatorController.Play("Idle" + mHertIndex);
            // Spr_Inside.sprite = GameManager.Instance.GetSprite("Food/" + mType + "/inside/" + mHertIndex);
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
    /// ������������㼶���ڸ����и÷����ᱻ����
    /// </summary>
    /// <param name="arrayIndex"></param>
    public override void UpdateSpecialRenderLayer()
    {
        Spr_Inside.sortingOrder = LayerManager.CalculateSortingLayer(LayerManager.UnitType.Ally, GetRowIndex(), -25, 0);
    }

    public override void MPause()
    {
        base.MPause();
        InsideAnimatorController.Pause();
    }

    public override void MResume()
    {
        base.MPause();
        InsideAnimatorController.Resume();
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
