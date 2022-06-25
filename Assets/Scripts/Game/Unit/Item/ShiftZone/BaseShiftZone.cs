using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �������ٵش�
/// </summary>
public class BaseShiftZone : BaseItem
{
    private float changePercent; // �������ٸı�ֵ

    public override void MInit()
    {
        base.MInit();
        changePercent = 0;
    }

    /// <summary>
    /// �����ٶȸı����
    /// </summary>
    public void SetChangePercent(float per)
    {
        changePercent = per;
    }

    /// <summary>
    /// ��ײ�¼�
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
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
            // ��ӱ���buff
            m.AddUniqueStatusAbility(GetUniqueStatusName(), new ChangeVelocityStatusAbility(m, changePercent));
        }
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
            m.RemoveUniqueStatusAbility(GetUniqueStatusName());
        }
    }

    /// <summary>
    /// ����shape��ȡΨһ��״̬��
    /// </summary>
    private string GetUniqueStatusName()
    {
        return "����Ч��"+mShape;
    }
}
