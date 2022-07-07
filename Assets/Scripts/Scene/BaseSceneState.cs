public class BaseSceneState : IBaseSceneState
{
    protected UIFacade mUIFacade;

    public BaseSceneState(UIFacade uiFacade)
    {
        mUIFacade = uiFacade;
    }

    public virtual void EnterScene()
    {
        mUIFacade.InitDict();
    }

    public virtual void ExitScene()
    {
        mUIFacade.ClearDict();
    }
}