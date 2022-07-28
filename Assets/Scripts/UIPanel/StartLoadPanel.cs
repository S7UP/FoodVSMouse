public class StartLoadPanel : BasePanel
{
    protected override void Awake()
    {
        base.Awake();
        Invoke("LoadNextScene", 1);
    }

    private void LoadNextScene()
    {
        mUIFacade.ChangeSceneState(new MainSceneState(mUIFacade));
    }
}
