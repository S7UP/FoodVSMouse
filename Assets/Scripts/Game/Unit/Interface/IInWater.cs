/// <summary>
/// ��λ��ˮ��Ϊ�ӿ�
/// </summary>
public interface IInWater
{
    /// <summary>
    /// ��ˮʱ�¼�
    /// </summary>
    public void OnEnterWater();

    /// <summary>
    /// ͣ����ˮ���¼�
    /// </summary>
    public void OnStayWater();

    /// <summary>
    /// �뿪ˮʱ�¼�
    /// </summary>
    public void OnExitWater();
}
