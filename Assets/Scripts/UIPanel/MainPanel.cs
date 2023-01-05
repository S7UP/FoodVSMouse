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

    /// <summary>
    /// 进入设置面板
    /// </summary>
    public void EnterConfigPanel()
    {
        MainSceneState main = GameManager.Instance.uiManager.mUIFacade.currentSceneState as MainSceneState;
        main.EnterConfigPanel();
    }

    /// <summary>
    /// 退出游戏
    /// </summary>
    public void ExitGame()
    {
        Application.Quit();
    }

    public void EnterEncyclopediaPanel()
    {
        mUIFacade.currentScenePanelDict[StringManager.EncyclopediaPanel].EnterPanel();
    }
}
