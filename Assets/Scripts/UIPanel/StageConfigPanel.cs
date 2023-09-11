using UnityEngine;
/// <summary>
/// 关卡配置面板
/// </summary>
namespace UIPanel.StageConfigPanel
{
    public class StageConfigPanel : BasePanel
    {
        private StageInfoUI mStageInfoUI; // 关卡信息显示面板
        private SelectEquipmentUI mSelectEquipmentUI; // 卡片与装备选择面板
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
        /// 初始化
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
        /// 当切换到其他关卡时，应当更新一下相关UI
        /// </summary>
        public void UpdateUIByChangeStage()
        {
            mSelectEquipmentUI.LoadAndFixUI(); // 根据当前选中关更新配置
            mStageInfoUI.UpdateInfo(); // 更新右侧关卡信息
        }

        /// <summary>
        /// 进入战斗场景（开始游戏）（由编辑器赋值）
        /// </summary>
        public void OnClickStartGame()
        {
            mSelectEquipmentUI.EnterCombatScene(); // 从装备选择面板中读取并传入数据到下一场景
            GameManager.Instance.playerData.Save(); // 保存一次玩家配置
            GameManager.Instance.EnterComBatScene();
        }

        /// <summary>
        /// 当返回被点击时（由编辑器赋值）
        /// </summary>
        public void OnClickReturn()
        {
            Debug.Log("清空当前动态关卡数据");
            PlayerData.GetInstance().SetCurrentDynamicStageInfo(null);
            mUIFacade.currentScenePanelDict[StringManager.StageConfigPanel].ExitPanel();
        }
    }
}

