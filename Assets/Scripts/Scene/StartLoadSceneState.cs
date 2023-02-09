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
        // SceneManager.LoadScene("StartLoadScene");
        // mUIFacade.AddPanelToDict(StringManager.StartLoadPanel);
        // GameManager.Instance.startLoadPanel.gameObject.SetActive(true);
        base.EnterScene();
    }

    public override void ExitScene()
    {
        base.ExitScene();
    }
}
