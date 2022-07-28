using UnityEngine;
/// <summary>
/// ��������ѡ��UI
/// </summary>
public class SelectWeaponsUI : MonoBehaviour
{
    private SelectEquipmentUI mSelectEquipmentUI;

    public void Initial()
    {
        
    }

    private void Awake()
    {
        
    }

    /// <summary>
    /// �����Ĵ�����ʹ��
    /// </summary>
    /// <param name="ui"></param>
    public void SetSelectEquipmentUI(SelectEquipmentUI ui)
    {
        mSelectEquipmentUI = ui;
    }

    /// <summary>
    /// ����ս������
    /// </summary>
    public void EnterComBatScene()
    {
        mSelectEquipmentUI.EnterCombatScene();
    }
}
