/// <summary>
/// ≥ı ºº”‘ÿ“≥√Ê
/// </summary>
public class StartLoadSceneState : BaseSceneState
{
    public StartLoadSceneState(UIFacade uiFacade) : base(uiFacade)
    {

    }

    public override void EnterScene()
    {
        //SceneManager.LoadScene("StartLoadScene");
        mUIFacade.AddPanelToDict(StringManager.StartLoadPanel);
        base.EnterScene();
    }

    public override void ExitScene()
    {
        base.ExitScene();
    }
}
