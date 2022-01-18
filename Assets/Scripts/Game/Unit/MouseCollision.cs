using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ��ս�������У������������λ����ײ�߼�
/// ע���������ɳ����Զ���Ӹ�������󣬶��Ǳ༭���ֶ����ʵ�֣�
/// </summary>
public class MouseCollision : MonoBehaviour
{
    // ���ⲿ�Զ���ֵ������
    public MouseUnit mouseUnit;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.tag);
        // �����������ˣ�
        if (collision.tag.Equals("Food"))
        {
            Debug.Log("�л�Ϊ����ģʽ��");
            MouseAttackState state = new MouseAttackState(mouseUnit);
            mouseUnit.SetActionState(state);
            state.mTarget = collision.gameObject;
        }
    }
}
