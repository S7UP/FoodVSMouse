using UnityEngine;

/// <summary>
/// �����
/// </summary>
public class MainPanel : BasePanel
{
    /// <summary>
    /// ����ѡ���������
    /// </summary>
    public void EnterSelcetScene()
    {
        GameManager.Instance.EnterSelectScene();
    }

    /// <summary>
    /// ����༭�����
    /// </summary>
    public void EnterEditorScene()
    {
        GameManager.Instance.EnterEditorScene();
    }

    public override void EnterPanel()
    {

    }

    public override void ExitPanel()
    {

    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
