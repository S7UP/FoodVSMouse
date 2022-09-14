using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// �������ٵش�
/// </summary>
public class BaseShiftZone : BaseItem
{
    private float changePercent; // �������ٸı�ֵ
    private List<BaseUnit> unitList = new List<BaseUnit>(); // λ�ڸñ��ٴ���Ч��Χ�ڵĵ�λ��
    public int skinType; // Ƥ������

    public override void MInit()
    {
        skinType = 0;
        unitList.Clear();
        base.MInit();
        changePercent = 0;
        SetSkin(0); // Ĭ��Ϊ��ţճҺ
        animatorController.Play("Appear");
    }

    /// <summary>
    /// ����Ƥ�����
    /// </summary>
    public void SetSkin(int skinType)
    {
        this.skinType = skinType;
        animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Item/"+mType+"/"+mShape+"/"+ skinType);
    }

    /// <summary>
    /// �����ٶȸı����
    /// </summary>
    public void SetChangePercent(float per)
    {
        changePercent = per;
    }

    private void OnCollision(Collider2D collision)
    {
        // ��������ʱ�������κ���ײ�¼�
        if (isDeathState)
        {
            return;
        }
        if (collision.tag.Equals("Mouse"))
        {
            MouseUnit m = collision.GetComponent<MouseUnit>();
            if (unitList.Contains(m) || m.GetRowIndex() != GetRowIndex() || m.GetHeight() != GetHeight())
                return;
            // ��ӱ���buff
            m.AddUniqueStatusAbility(GetUniqueStatusName(), new ChangeVelocityStatusAbility(m, changePercent));
            unitList.Add(m);
        }
    }

    /// <summary>
    /// ��ײ�¼�
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnCollision(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        OnCollision(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // ��������ʱ�������κ���ײ�¼�
        if (isDeathState)
        {
            return;
        }
        if (collision.tag.Equals("Mouse"))
        {
            MouseUnit m = collision.GetComponent<MouseUnit>();
            if (m.GetRowIndex() != GetRowIndex() || m.GetHeight() != GetHeight())
                return;
            // �Ƴ�����buff
            unitList.Remove(m);
            m.RemoveUniqueStatusAbility(GetUniqueStatusName());
        }
    }

    public override void OnDieStateEnter()
    {
        base.OnDieStateEnter();
        // �Ƴ��Լ��������е�λ������������Ч��
        foreach (var m in unitList)
        {
            m.RemoveUniqueStatusAbility(GetUniqueStatusName());
        }
        unitList.Clear();
    }

    public override void AfterDeath()
    {
        base.AfterDeath();
        // �Ƴ��Լ��������е�λ������������Ч��
        foreach (var m in unitList)
        {
            m.RemoveUniqueStatusAbility(GetUniqueStatusName());
        }
        unitList.Clear();
    }

    /// <summary>
    /// ����shape��ȡΨһ��״̬��
    /// </summary>
    private string GetUniqueStatusName()
    {
        return "����Ч��"+mShape+ "-"+ skinType;
    }

    public override void UpdateRenderLayer(int arrayIndex)
    {
        spriteRenderer.sortingOrder = LayerManager.CalculateSortingLayer(LayerManager.UnitType.Ally, GetRowIndex(), -5, arrayIndex);
    }
}
