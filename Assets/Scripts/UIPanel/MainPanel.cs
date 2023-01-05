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

    /// <summary>
    /// �����������
    /// </summary>
    public void EnterConfigPanel()
    {
        MainSceneState main = GameManager.Instance.uiManager.mUIFacade.currentSceneState as MainSceneState;
        main.EnterConfigPanel();
    }

    /// <summary>
    /// �˳���Ϸ
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
