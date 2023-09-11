using System.Collections;
public class GameNormalSceneState : BaseSceneState
{
    public GameNormalSceneState(UIFacade uiFacade) : base(uiFacade)
    {

    }
    public override IEnumerator LoadScene()
    {
        PlayerData data = PlayerData.GetInstance();
        // 载入本关的资源
        yield return GameManager.Instance.StartCoroutine(data.GetCurrentStageInfo().LoadResWhenEnterCombatScene());
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
        // 卸载本关加载的资源
        PlayerData.GetInstance().GetCurrentStageInfo().UnLoadResWhenExitCombatScene();
        // 离开战斗场景后动态关卡信息包自然结束了它的使命
        PlayerData.GetInstance().SetCurrentDynamicStageInfo(null);
        base.ExitScene();
    }
}