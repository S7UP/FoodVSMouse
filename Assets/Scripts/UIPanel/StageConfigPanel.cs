using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;
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
