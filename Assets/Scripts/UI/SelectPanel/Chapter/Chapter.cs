using UnityEngine;
/// <summary>
/// �½ڣ����ͼ��
/// </summary>
public class Chapter : MonoBehaviour
{
    // ����Ҫ��ʼ�����ɴ���ʱ�Զ���������������
    private SelectPanel mSelectPanel; // ѡ�����
    public ChapterType chapterType; // �½�����
    // ����
    public int selectedSceneIndex = -1; // ��ǰѡ��ؿ��±꣬-1Ϊδѡ�� 

    public void Initial()
    {
        selectedSceneIndex = -1;

    }

    /// <summary>
    /// ���뱾�½�ĳ������
    /// </summary>
    /// <param name="index">�������</param>
    public void EnterScene(int index)
    {
        selectedSceneIndex = index;
        mSelectPanel.LoadcurrentSceneStageList();
        // ֪ͨѡ�������ʾ����UI
        mSelectPanel.EnterSelectStageUIAndStageInfoUI();
    }

    ////////////////////////////////////////////////////////////////////////���·����������ɱ�ʵ����SelectPanel����//////////////////////////////
    /// <summary>
    /// �����������
    /// </summary>
    /// <param name="panel"></param>
    public void SetInfo(SelectPanel panel, ChapterType type)
    {
        mSelectPanel = panel;
        chapterType = type;
    }
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
}
