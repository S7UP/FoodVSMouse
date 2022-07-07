using UnityEngine;

/// <summary>
/// Ŀ���ܻҽ�Ч������ʱ������ʱ����duringDeath
/// </summary>
public class BurnState : BaseActionState
{
    int totalTime = ConfigManager.fps * 2; // ��������ʱ�䣨֡��������Ϊ��
    int currentTime = 0;

    public BurnState(BaseUnit baseUnit) : base(baseUnit)
    {
    }

    public override void OnEnter()
    {
        mBaseUnit.OnBurnStateEnter();
    }

    public override void OnUpdate()
    {
        currentTime++;
        mBaseUnit.DuringBurn((float)currentTime/totalTime);
    }

    public override void OnExit()
    {
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
