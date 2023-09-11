using UnityEngine;
/// <summary>
/// �ؿ��������
/// </summary>
namespace UIPanel.StageConfigPanel
{
    public class StageConfigPanel : BasePanel
    {
        private StageInfoUI mStageInfoUI; // �ؿ���Ϣ��ʾ���
        private SelectEquipmentUI mSelectEquipmentUI; // ��Ƭ��װ��ѡ�����
        private SelectTagUI mSelectTagUI;
        private SettlementUI mSettlementUI;

        private TagFacade mTagFacade;

        protected override void Awake()
        {
            base.Awake();
            mStageInfoUI = transform.Find("StageInfoUI").GetComponent<StageInfoUI>();
            mSelectEquipmentUI = transform.Find("SelectEquipmentUI").GetComponent<SelectEquipmentUI>();
            mSelectTagUI = transform.Find("SelectTagUI").GetComponent<SelectTagUI>();
            mSettlementUI = mStageInfoUI.transform.Find("Emp_StageInfo").Find("SettlementUI").GetComponent<SettlementUI>();

            mTagFacade = new TagFacade(mSelectTagUI, mSettlementUI);
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
            mTagFacade.MInit();
        }

        public override void ExitPanel()
        {
            base.ExitPanel();
            mTagFacade.MDestory();
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
            mSelectEquipmentUI.EnterCombatScene(); // ��װ��ѡ������ж�ȡ���������ݵ���һ����
            GameManager.Instance.playerData.Save(); // ����һ���������
            GameManager.Instance.EnterComBatScene();
        }

        /// <summary>
        /// �����ر����ʱ���ɱ༭����ֵ��
        /// </summary>
        public void OnClickReturn()
        {
            Debug.Log("��յ�ǰ��̬�ؿ�����");
            PlayerData.GetInstance().SetCurrentDynamicStageInfo(null);
            mUIFacade.currentScenePanelDict[StringManager.StageConfigPanel].ExitPanel();
        }
    }
}

