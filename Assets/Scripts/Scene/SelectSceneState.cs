using UnityEngine.SceneManagement;

public class SelectSceneState : BaseSceneState
{
    public SelectSceneState(UIFacade uiFacade) : base(uiFacade)
    {

    }

    public override void EnterScene()
    {
        SceneManager.LoadScene("SelectScene");
        GameManager.Instance.audioSourceManager.PlayBGMusic("SelectScene");
        mUIFacade.AddPanelToDict(StringManager.SelectPanel);
        mUIFacade.AddPanelToDict(StringManager.PlayerInfoPanel);
        mUIFacade.AddPanelToDict(StringManager.EncyclopediaPanel);
        mUIFacade.AddPanelToDict(StringManager.MainlinePanel);
        mUIFacade.AddPanelToDict(StringManager.StageConfigPanel);
        mUIFacade.AddPanelToDict(StringManager.RankSelectPanel);
        base.EnterScene();
        // һ��ʼҪ���ص����
        mUIFacade.currentScenePanelDict[StringManager.EncyclopediaPanel].ExitPanel();
        mUIFacade.currentScenePanelDict[StringManager.MainlinePanel].ExitPanel();
        mUIFacade.currentScenePanelDict[StringManager.StageConfigPanel].ExitPanel();
        mUIFacade.currentScenePanelDict[StringManager.RankSelectPanel].ExitPanel();
    }

    public override void ExitScene()
    {
        base.ExitScene();
    }
}
