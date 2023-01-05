using UnityEngine.SceneManagement;

public class MainSceneState : BaseSceneState
{
    public MainSceneState(UIFacade uiFacade) : base(uiFacade)
    {

    }

    public override void EnterScene()
    {
        SceneManager.LoadScene("MainScene");
        // �������˵�BGM
        GameManager.Instance.audioSourceManager.PlayBGMusic("MainScene");
        mUIFacade.AddPanelToDict(StringManager.MainPanel);
        mUIFacade.AddPanelToDict(StringManager.ConfigPanel);
        base.EnterScene();
        EnterMainPanel(); // Ĭ�Ͻ���MainPanel
    }

    public override void ExitScene()
    {
        base.ExitScene();
    }

    public void EnterMainPanel()
    {
        mUIFacade.currentScenePanelDict[StringManager.MainPanel].EnterPanel();
        mUIFacade.currentScenePanelDict[StringManager.ConfigPanel].ExitPanel();
    }

    public void EnterConfigPanel()
    {
        mUIFacade.currentScenePanelDict[StringManager.MainPanel].ExitPanel();
        mUIFacade.currentScenePanelDict[StringManager.ConfigPanel].EnterPanel();
    }
}
