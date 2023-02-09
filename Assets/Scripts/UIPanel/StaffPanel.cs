public class StaffPanel : BasePanel
{
    /// <summary>
    /// 当这个面板被点击时就会关闭
    /// </summary>
    public void OnClickExit()
    {
        MainSceneState main = GameManager.Instance.uiManager.mUIFacade.currentSceneState as MainSceneState;
        main.EnterMainPanel();
    }
}
