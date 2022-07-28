using UnityEngine.SceneManagement;

public class MainSceneState : BaseSceneState
{
    public MainSceneState(UIFacade uiFacade) : base(uiFacade)
    {

    }

    public override void EnterScene()
    {
        SceneManager.LoadScene("MainScene");
        mUIFacade.AddPanelToDict(StringManager.MainPanel);
        base.EnterScene();
    }

    public override void ExitScene()
    {
        base.ExitScene();
    }
}
