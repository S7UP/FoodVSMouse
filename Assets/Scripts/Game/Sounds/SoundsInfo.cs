[System.Serializable]
/// <summary>
/// ��Ч��Ϣ
/// </summary>
public class SoundsInfo
{
    public string refenceName; // ���������൱������
    public string displayName; // ��ʾ������ʾ����Ϸ�ڵ���Ч��
    public string author; // ����
    public string resPath; // SEԴ�ļ���ŵ�ַ�����·����
    public float volume;

    public SoundsInfo(string refenceName, string displayName, string author, string resPath, float volume)
    {
        this.refenceName = refenceName;
        this.displayName = displayName;
        this.author = author;
        this.resPath = resPath;
        this.volume = volume;
    }
}
