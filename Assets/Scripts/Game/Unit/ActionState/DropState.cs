using UnityEngine;
/// <summary>
/// ˤ��״̬��������
/// </summary>
public class DropState : BaseActionState
{
    int totalTime = 120; // ��������ʱ�䣨֡��������Ϊ��
    int currentTime = 0;

    public DropState(BaseUnit baseUnit) : base(baseUnit)
    {
    }

    public override void OnEnter()
    {
        mBaseUnit.OnDropStateEnter();
    }

    public override void OnUpdate()
    {
        currentTime++;
        mBaseUnit.OnDropState((float)currentTime / totalTime);
    }

    public override void OnExit()
    {
        mBaseUnit.OnDropStateExit();
        // ������뿪������ζ��ʲô�����
        // ������ʵ�ϣ���ԶҲ�޷��ﵽ�뿪��������ʵ����������������²�Ӧ�õ��õ�����
        Debug.LogWarning("���棺�ж��������״̬ת��������״̬�ˣ�����");
    }

    public override void OnInterrupt()
    {

    }

    public override void OnContinue()
    {

    }
}
