using UnityEngine.SceneManagement;

public class GameNormalSceneState : BaseSceneState
{
    public GameNormalSceneState(UIFacade uiFacade) : base(uiFacade)
    {

    }

    public override void EnterScene()
    {
        SceneManager.LoadScene("CombatScene");
        mUIFacade.AddPanelToDict(StringManager.GameNormalPanel);
        base.EnterScene();
    }

    public override void ExitScene()
    {
        // 清空游戏对象工厂的所有对象
        GameManager.Instance.ClearGameObjectFactory(FactoryType.GameFactory);
        base.ExitScene();
    }
}