using UnityEngine.SceneManagement;

public class SelectSceneState : BaseSceneState
{
    public SelectSceneState(UIFacade uiFacade) : base(uiFacade)
    {

    }

    public override void EnterScene()
    {
        SceneManager.LoadScene("SelectScene");
        mUIFacade.AddPanelToDict(StringManager.SelectPanel);
        base.EnterScene();
    }

    public override void ExitScene()
    {
        base.ExitScene();
    }
}
