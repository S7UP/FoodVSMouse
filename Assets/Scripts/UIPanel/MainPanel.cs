using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 主面板
/// </summary>
public class MainPanel : BasePanel
{
    private Button Btn_EditorMode;
    private Button Btn_Staff;

    protected override void Awake()
    {
        base.Awake();
        Btn_EditorMode = transform.Find("Btn_EditorMode").GetComponent<Button>();
        Btn_Staff = transform.Find("Btn_Staff").GetComponent<Button>();
    }

    private void Initial()
    {
        if (ConfigManager.IsDeveloperMode())
        {
            // 开发者模式显示编辑器面板
            Btn_EditorMode.gameObject.SetActive(true);
            Btn_Staff.gameObject.SetActive(false);
        }
        else
        {
            // 否则显示staff
            Btn_EditorMode.gameObject.SetActive(false);
            Btn_Staff.gameObject.SetActive(true);
        }
    }

    public override void InitPanel()
    {
        base.InitPanel();
        Initial();
    }

    public override void EnterPanel()
    {
        base.EnterPanel();
        Initial();
    }

    /// <summary>
    /// 进入选择配置面板
    /// </summary>
    public void EnterTownScene()
    {
        GameManager.Instance.EnterTownScene();
    }

    /// <summary>
    /// 进入编辑器面板
    /// </summary>
    public void EnterEditorScene()
    {
        GameManager.Instance.EnterEditorScene();
    }

    /// <summary>
    /// 进入设置面板
    /// </summary>
    public void EnterConfigPanel()
    {
        MainSceneState main = GameManager.Instance.uiManager.mUIFacade.currentSceneState as MainSceneState;
        main.EnterConfigPanel();
    }

    /// <summary>
    /// 进入Staff面板
    /// </summary>
    public void EnterStaffPanel()
    {
        MainSceneState main = GameManager.Instance.uiManager.mUIFacade.currentSceneState as MainSceneState;
        main.EnterStaffPanel();
    }

    /// <summary>
    /// 退出游戏
    /// </summary>
    public void ExitGame()
    {
        Application.Quit();
    }

    public void EnterEncyclopediaPanel()
    {
        mUIFacade.currentScenePanelDict[StringManager.EncyclopediaPanel].EnterPanel();
    }
}
