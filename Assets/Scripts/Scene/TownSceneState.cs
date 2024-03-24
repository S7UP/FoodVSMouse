
using System.Collections;

public class TownSceneState : BaseSceneState
{
    public TownSceneState(UIFacade uiFacade) : base(uiFacade)
    {

    }

    public override IEnumerator LoadScene()
    {
        yield return GameManager.Instance.StartCoroutine(AudioSourceController.AsyncLoadBGMusic("SelectScene"));
        yield return GameManager.Instance.StartCoroutine(GameManager.Instance.LoadSceneAsync("TownScene"));
    }

    public override void EnterScene()
    {
        GameManager.Instance.audioSourceController.PlayBGMusic("SelectScene");
        mUIFacade.AddPanelToDict(StringManager.PlayerInfoPanel);
        mUIFacade.AddPanelToDict(StringManager.EncyclopediaPanel);
        mUIFacade.AddPanelToDict(StringManager.BigChapterPanel);
        mUIFacade.AddPanelToDict(StringManager.LocalStagePanel);
        mUIFacade.AddPanelToDict(StringManager.StageConfigPanel);
        //mUIFacade.AddPanelToDict(StringManager.RankSelectPanel);
        base.EnterScene();
        // 一开始要隐藏的面板
        mUIFacade.currentScenePanelDict[StringManager.EncyclopediaPanel].ExitPanel();
        mUIFacade.currentScenePanelDict[StringManager.BigChapterPanel].ExitPanel();
        mUIFacade.currentScenePanelDict[StringManager.LocalStagePanel].ExitPanel();
        mUIFacade.currentScenePanelDict[StringManager.StageConfigPanel].ExitPanel();
        //mUIFacade.currentScenePanelDict[StringManager.RankSelectPanel].ExitPanel();
    }

    public override void ExitScene()
    {
        base.ExitScene();
    }
}
