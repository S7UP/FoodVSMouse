using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;
/// <summary>
/// 关卡配置面板
/// </summary>
public class StageConfigPanel : BasePanel
{
    private StageInfoUI mStageInfoUI; // 关卡信息显示面板
    private SelectEquipmentUI mSelectEquipmentUI; // 卡片与装备选择面板

    protected override void Awake()
    {
        base.Awake();
        mStageInfoUI = transform.Find("StageInfoUI").GetComponent<StageInfoUI>();
        mStageInfoUI.SetStageConfigPanel(this);
        mSelectEquipmentUI = transform.Find("SelectEquipmentUI").GetComponent<SelectEquipmentUI>();
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
        UpdateUIByChangeStage();
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
        GameManager.Instance.EnterComBatScene();
    }

    /// <summary>
    /// 当返回被点击时（由编辑器赋值）
    /// </summary>
    public void OnClickReturn()
    {
        mUIFacade.currentScenePanelDict[StringManager.StageConfigPanel].ExitPanel();
    }
}
