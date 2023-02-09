
using System.Collections;

public class TownSceneState : BaseSceneState
{
    public TownSceneState(UIFacade uiFacade) : base(uiFacade)
    {

    }

    public override IEnumerator LoadScene()
    {
        yield return GameManager.Instance.StartCoroutine(AudioSourceManager.AsyncLoadBGMusic("SelectScene"));
        yield return GameManager.Instance.StartCoroutine(GameManager.Instance.LoadSceneAsync("TownScene"));
    }

    public override void EnterScene()
    {
        // SceneManager.LoadScene("TownScene");
        // GameManager.Instance.LoadSceneAsync("TownScene");
        GameManager.Instance.audioSourceManager.PlayBGMusic("SelectScene");
        mUIFacade.AddPanelToDict(StringManager.PlayerInfoPanel);
        mUIFacade.AddPanelToDict(StringManager.EncyclopediaPanel);
        mUIFacade.AddPanelToDict(StringManager.MainlinePanel);
        mUIFacade.AddPanelToDict(StringManager.SpurlinePanel);
        mUIFacade.AddPanelToDict(StringManager.WarriorChallengePanel);
        mUIFacade.AddPanelToDict(StringManager.StageConfigPanel);
        mUIFacade.AddPanelToDict(StringManager.RankSelectPanel);
        base.EnterScene();
        // 一开始要隐藏的面板
        mUIFacade.currentScenePanelDict[StringManager.EncyclopediaPanel].ExitPanel();
        mUIFacade.currentScenePanelDict[StringManager.MainlinePanel].ExitPanel();
        mUIFacade.currentScenePanelDict[StringManager.SpurlinePanel].ExitPanel();
        mUIFacade.currentScenePanelDict[StringManager.WarriorChallengePanel].ExitPanel();
        mUIFacade.currentScenePanelDict[StringManager.StageConfigPanel].ExitPanel();
        mUIFacade.currentScenePanelDict[StringManager.RankSelectPanel].ExitPanel();
    }

    public override void ExitScene()
    {
        base.ExitScene();
    }
}
