using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// �����
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
            // ������ģʽ��ʾ�༭�����
            Btn_EditorMode.gameObject.SetActive(true);
            Btn_Staff.gameObject.SetActive(false);
        }
        else
        {
            // ������ʾstaff
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
    /// ����ѡ���������
    /// </summary>
    public void EnterTownScene()
    {
        GameManager.Instance.EnterTownScene();
    }

    /// <summary>
    /// ����༭�����
    /// </summary>
    public void EnterEditorScene()
    {
        GameManager.Instance.EnterEditorScene();
    }

    /// <summary>
    /// �����������
    /// </summary>
    public void EnterConfigPanel()
    {
        MainSceneState main = GameManager.Instance.uiManager.mUIFacade.currentSceneState as MainSceneState;
        main.EnterConfigPanel();
    }

    /// <summary>
    /// ����Staff���
    /// </summary>
    public void EnterStaffPanel()
    {
        MainSceneState main = GameManager.Instance.uiManager.mUIFacade.currentSceneState as MainSceneState;
        main.EnterStaffPanel();
    }

    /// <summary>
    /// �˳���Ϸ
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
