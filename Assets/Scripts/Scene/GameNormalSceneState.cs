
using System.Collections;

public class GameNormalSceneState : BaseSceneState
{
    public GameNormalSceneState(UIFacade uiFacade) : base(uiFacade)
    {

    }
    public override IEnumerator LoadScene()
    {
        // 载入本关的BGM
        yield return GameManager.Instance.StartCoroutine(PlayerData.GetInstance().GetCurrentStageInfo().LoadResWhenEnterCombatScene());
        yield return GameManager.Instance.StartCoroutine(GameManager.Instance.LoadSceneAsync("CombatScene"));
        //mUIFacade.AddPanelToDict(StringManager.GameNormalPanel);
    }

    public override void EnterScene()
    {
        //SceneManager.LoadScene("CombatScene");
        mUIFacade.AddPanelToDict(StringManager.GameNormalPanel);
        base.EnterScene();
    }

    public override void ExitScene()
    {
        // 清空游戏对象工厂的所有对象
        GameManager.Instance.ClearGameObjectFactory(FactoryType.GameFactory);
        // 清空通关后的奖励设置
        PlayerData.GetInstance().SetCurrentStageID(null);
        base.ExitScene();
    }
}