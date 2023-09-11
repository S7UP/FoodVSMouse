using System.Collections;

using UnityEngine;

public class MainSceneState : BaseSceneState
{
    private bool isFirstLoad = true;

    public MainSceneState(UIFacade uiFacade) : base(uiFacade)
    {

    }

    public override IEnumerator LoadScene()
    {
        if (isFirstLoad)
        {
            Debug.Log("MainSceneFirstLoad!");
            yield return GameManager.Instance.StartCoroutine(GameManager.Instance.LoadWhenBeforeEnterMainScene());
            isFirstLoad = false;
        }

        yield return GameManager.Instance.StartCoroutine(AudioSourceController.AsyncLoadBGMusic("MainScene")); // �������˵�BGM
        yield return GameManager.Instance.StartCoroutine(GameManager.Instance.LoadSceneAsync("MainScene"));
    }

    public override void EnterScene()
    {
        // �������˵�BGM
        GameManager.Instance.audioSourceController.PlayBGMusic("MainScene");
        mUIFacade.AddPanelToDict(StringManager.MainPanel);
        mUIFacade.AddPanelToDict(StringManager.ConfigPanel);
        mUIFacade.AddPanelToDict(StringManager.StaffPanel);
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
