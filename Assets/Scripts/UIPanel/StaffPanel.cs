public class StaffPanel : BasePanel
{
    /// <summary>
    /// �������屻���ʱ�ͻ�ر�
    /// </summary>
    public void OnClickExit()
    {
        MainSceneState main = GameManager.Instance.uiManager.mUIFacade.currentSceneState as MainSceneState;
        main.EnterMainPanel();
    }
}
