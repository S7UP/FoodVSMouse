using UnityEngine;
/// <summary>
/// 进入武器选择UI
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
    /// 给它的创建者使用
    /// </summary>
    /// <param name="ui"></param>
    public void SetSelectEquipmentUI(SelectEquipmentUI ui)
    {
        mSelectEquipmentUI = ui;
    }

    /// <summary>
    /// 进入战斗场景
    /// </summary>
    public void EnterComBatScene()
    {
        mSelectEquipmentUI.EnterCombatScene();
    }
}
