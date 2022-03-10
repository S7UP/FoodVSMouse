using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ŀ������ʱ������ʱ����duringDeath
/// </summary>
public class DieState : BaseActionState
{
    public DieState(BaseUnit baseUnit) : base(baseUnit)
    {
    }

    public override void OnEnter()
    {
        mBaseUnit.OnDieStateEnter();
    }

    public override void OnUpdate()
    {
        mBaseUnit.DuringDeath();
    }

    public override void OnExit()
    {
        // ������뿪������ζ��ʲô�����
        // ������ʵ�ϣ���ԶҲ�޷��ﵽ�뿪��������ʵ����������������²�Ӧ�õ��õ�����
        Debug.LogWarning("���棺�ж��������״̬ת��������״̬�ˣ�����");
    }
}
