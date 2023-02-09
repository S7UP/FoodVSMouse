using System.Collections;

public class MainSceneState : BaseSceneState
{
    public MainSceneState(UIFacade uiFacade) : base(uiFacade)
    {

    }

    public override IEnumerator LoadScene()
    {
        yield return GameManager.Instance.StartCoroutine(AudioSourceManager.AsyncLoadBGMusic("MainScene")); // 载入主菜单BGM
        yield return GameManager.Instance.StartCoroutine(GameManager.Instance.LoadSceneAsync("MainScene"));
    }

    public override void EnterScene()
    {
        //SceneManager.LoadScene("MainScene");
        //GameManager.Instance.LoadSceneAsync("MainScene");
        // 播放主菜单BGM
        GameManager.Instance.audioSourceManager.PlayBGMusic("MainScene");
        mUIFacade.AddPanelToDict(StringManager.MainPanel);
        mUIFacade.AddPanelToDict(StringManager.ConfigPanel);
        mUIFacade.AddPanelToDict(StringManager.StaffPanel);
        base.EnterScene();
        EnterMainPanel(); // 默认进入MainPanel
    }

    public override void ExitScene()
    {
        base.ExitScene();
    }

    public void EnterMainPanel()
    {
        mUIFacade.currentScenePanelDict[StringManager.MainPanel].EnterPanel();
        mUIFacade.currentScenePanelDict[StringManager.ConfigPanel].ExitPanel();
        mUIFacade.currentScenePanelDict[StringManager.StaffPanel].ExitPanel();
    }

    public void EnterConfigPanel()
    {
        mUIFacade.currentScenePanelDict[StringManager.ConfigPanel].EnterPanel();
        mUIFacade.currentScenePanelDict[StringManager.StaffPanel].ExitPanel();
        mUIFacade.currentScenePanelDict[StringManager.MainPanel].ExitPanel();
    }

    public void EnterStaffPanel()
    {
        mUIFacade.currentScenePanelDict[StringManager.StaffPanel].EnterPanel();
        mUIFacade.currentScenePanelDict[StringManager.ConfigPanel].ExitPanel();
        mUIFacade.currentScenePanelDict[StringManager.MainPanel].ExitPanel();
    }
}
