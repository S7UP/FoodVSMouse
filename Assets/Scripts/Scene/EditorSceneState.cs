using UnityEngine.SceneManagement;
public class EditorSceneState : BaseSceneState
{
    public EditorSceneState(UIFacade uiFacade) : base(uiFacade)
    {

    }

    public override void EnterScene()
    {
        SceneManager.LoadScene("EditorScene");
        mUIFacade.AddPanelToDict(StringManager.EditorPanel);
        base.EnterScene();
    }

    public override void ExitScene()
    {
        base.ExitScene();
    }
}
