[System.Serializable]
/// <summary>
/// ������Ϣ
/// </summary>
public class MusicInfo
{
    public string refenceName; // ���������൱������
    public string displayName; // ��ʾ������ʾ����Ϸ�ڵ�������
    public string author; // ����
    public string resPath; // BGMԴ�ļ���ŵ�ַ�����·����
    public float startTime; // ��һ�β������
    public float loopStartTime; // ѭ������ʱ��ʼ�㣨�룩
    public float loopEndTime = -1; // ѭ�����ŵĶϵ�(-1Ϊδ���öϵ㣬Ĭ��Ϊ��β����
    public float volume;
}
