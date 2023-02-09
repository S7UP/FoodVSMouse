/// <summary>
/// �ؿ��������
/// </summary>
public class StageConfigPanel : BasePanel
{
    private StageInfoUI mStageInfoUI; // �ؿ���Ϣ��ʾ���
    private SelectEquipmentUI mSelectEquipmentUI; // ��Ƭ��װ��ѡ�����

    protected override void Awake()
    {
        base.Awake();
        mStageInfoUI = transform.Find("StageInfoUI").GetComponent<StageInfoUI>();
        mStageInfoUI.SetStageConfigPanel(this);
        mSelectEquipmentUI = transform.Find("SelectEquipmentUI").GetComponent<SelectEquipmentUI>();
    }

    /// <summary>
    /// ��ʼ��
    /// </summary>
    public override void InitPanel()
    {
        mStageInfoUI.Initial();
        mSelectEquipmentUI.Initial();
    }

    public override void EnterPanel()
    {
        base.EnterPanel();
        mStageInfoUI.Initial();
        UpdateUIByChangeStage();
    }

    /// <summary>
    /// ���л��������ؿ�ʱ��Ӧ������һ�����UI
    /// </summary>
    public void UpdateUIByChangeStage()
    {
        mSelectEquipmentUI.LoadAndFixUI(); // ���ݵ�ǰѡ�йظ�������
        mStageInfoUI.UpdateInfo(); // �����Ҳ�ؿ���Ϣ
    }

    /// <summary>
    /// ����ս����������ʼ��Ϸ�����ɱ༭����ֵ��
    /// </summary>
    public void OnClickStartGame()
    {
        // ����Ǵ�������ս������Ĺؿ�������ȡ��ǰ������ս��ͨ��������¼�뵽playerData��
        if (mUIFacade.currentScenePanelDict.ContainsKey(StringManager.MainlinePanel))
        {
            // ��ͨͼ��¼
            {
                MainlinePanel panel = mUIFacade.currentScenePanelDict[StringManager.MainlinePanel] as MainlinePanel;
                if (panel.isActiveAndEnabled)
                    PlayerData.GetInstance().SetCurrentStageSuccessRewardFunc(panel.SuccessReward);
            }
            // ��ʿ��ս��¼
            {
                WarriorChallengePanel panel = mUIFacade.currentScenePanelDict[StringManager.WarriorChallengePanel] as WarriorChallengePanel;
                if (panel.isActiveAndEnabled)
                    PlayerData.GetInstance().SetCurrentStageSuccessRewardFunc(panel.SuccessReward);
            }
            // ֧����ս��¼
            {
                SpurlinePanel panel = mUIFacade.currentScenePanelDict[StringManager.SpurlinePanel] as SpurlinePanel;
                if (panel.isActiveAndEnabled)
                    PlayerData.GetInstance().SetCurrentStageSuccessRewardFunc(panel.SuccessReward);
            }
        }

        mSelectEquipmentUI.EnterCombatScene(); // ��װ��ѡ������ж�ȡ���������ݵ���һ����
        GameManager.Instance.EnterComBatScene();
    }

    /// <summary>
    /// �����ر����ʱ���ɱ༭����ֵ��
    /// </summary>
    public void OnClickReturn()
    {
        mUIFacade.currentScenePanelDict[StringManager.StageConfigPanel].ExitPanel();
    }
}
