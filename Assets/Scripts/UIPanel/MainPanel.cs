using UnityEngine;

/// <summary>
/// 主面板
/// </summary>
public class MainPanel : BasePanel
{
    /// <summary>
    /// 进入选择配置面板
    /// </summary>
    public void EnterSelcetScene()
    {
        GameManager.Instance.EnterSelectScene();
    }

    /// <summary>
    /// 进入编辑器面板
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
