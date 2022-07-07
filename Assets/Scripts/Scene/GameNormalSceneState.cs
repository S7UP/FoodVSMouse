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
        // �����Ϸ���󹤳������ж���
        GameManager.Instance.ClearGameObjectFactory(FactoryType.GameFactory);
        base.ExitScene();
    }
}