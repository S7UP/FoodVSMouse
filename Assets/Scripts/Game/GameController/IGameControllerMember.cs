public interface IGameControllerMember
{
    //��ʼ������
    public void MInit();
    //֡���·���
    public void MUpdate();
    //��ͣʱ�ķ���
    public void MPause();
    // ��ͣ�ڼ�֡����
    public void MPauseUpdate();
    //�ָ�ʱ�ķ���
    public void MResume();
    //����ʱ�ķ���
    public void MDestory();
}
