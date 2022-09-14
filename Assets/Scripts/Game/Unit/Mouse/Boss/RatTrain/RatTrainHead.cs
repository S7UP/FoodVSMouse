/// <summary>
/// ����г���ͷ
/// </summary>
public class RatTrainHead : RatTrainComponent
{
    /// <summary>
    /// ת��Ϊ����̬ʱ�¼�
    /// </summary>
    public override void OnTurnToAppear()
    {
        // ֪ͨ���������µ�·����Ϣ���복����������Ա�������������
        GetMaster().AddNextRouteToManager();
    }

    /// <summary>
    /// ���ִ����һ��·����ʱ���ⲿ���ɳ�ͷ��д��
    /// </summary>
    public override void OnOutOfBound()
    {
        // ������ʣ��·��ʱ����������Ч��������֪ͨBOSS��������ѵִ�Ŀ�ĵ�
        if (GetRouteQueue().Count > 0)
            base.OnOutOfBound();
        else
        {
            GetMaster().MarkMoveToDestination();
            GetMaster().SetActionState(new IdleState(GetMaster()));
        }
            
    }
}
