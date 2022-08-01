using UnityEngine;
using System.Collections.Generic;
/// <summary>
/// 进入武器选择UI
/// </summary>
public class SelectWeaponsUI : MonoBehaviour
{
    private SelectEquipmentUI mSelectEquipmentUI;
    private Transform Tran_WeaponsDisplay;
    private Transform Tran_CharacterDisplay;
    private List<WeaponsDisplay> weaponsDisplayList = new List<WeaponsDisplay>();
    private List<CharacterDisplay> characterDisplayList = new List<CharacterDisplay>();

    private WeaponsDisplay currentSelectedWeaponsDisplay;
    private CharacterDisplay currentSelectedCharacterDisplay;

    private void Awake()
    {
        Tran_WeaponsDisplay = transform.Find("Img_center").Find("Emp_Weapons").Find("Scr").Find("Viewport").Find("Content");
        Tran_CharacterDisplay = transform.Find("Img_center").Find("Emp_Character").Find("Scr").Find("Viewport").Find("Content");
    }

    public void Initial()
    {
        ClearWeaponsDisplayList();
        ClearCharacterDisplayList();
        FillWeaponsDisplayList();
        FillCharacterDisplayList();
        // 默认选用0 0武器
        SetWeaponsSelected(0, 0);
        // 默认选用0 0人物
        SetCharacterSelected(0, 0);
    }

    /// <summary>
    /// 清空武器表
    /// </summary>
    private void ClearWeaponsDisplayList()
    {
        foreach (var item in weaponsDisplayList)
        {
            item.ExecuteRecycle();
        }
        weaponsDisplayList.Clear();
    }

    /// <summary>
    /// 清空角色表
    /// </summary>
    private void ClearCharacterDisplayList()
    {
        foreach (var item in characterDisplayList)
        {
            item.ExecuteRecycle();
        }
        characterDisplayList.Clear();
    }

    /// <summary>
    /// 填充武器表
    /// </summary>
    private void FillWeaponsDisplayList()
    {
        Dictionary<WeaponsNameTypeMap, string> dict = WeaponsManager.GetWeaponsNameDict();
        foreach (var keyValuePair in dict)
        {
            WeaponsDisplay w = WeaponsDisplay.GetInstance();
            w.Initial();
            w.SetValues(this, (int)keyValuePair.Key, 0);
            w.transform.SetParent(Tran_WeaponsDisplay);
            w.transform.localScale = Vector3.one;
            weaponsDisplayList.Add(w);
        }
    }

    /// <summary>
    /// 填充角色表
    /// </summary>
    private void FillCharacterDisplayList()
    {
        Dictionary<CharacterNameShapeMap, List<string>> dict = CharacterManager.GetCharacterNameDict();
        foreach (var keyValuePair in dict)
        {
            List<string> list = keyValuePair.Value;
            for (int i = 0; i < list.Count; i++)
            {
                CharacterDisplay c = CharacterDisplay.GetInstance();
                c.Initial();
                c.SetValues(this, i, (int)keyValuePair.Key);
                c.transform.SetParent(Tran_CharacterDisplay);
                c.transform.localScale = Vector3.one;
                characterDisplayList.Add(c);
            }
        }
    }

    /// <summary>
    /// 设置某个武器被选中
    /// </summary>
    public void SetWeaponsSelected(int type, int shape)
    {
        currentSelectedWeaponsDisplay = null;
        foreach (var item in weaponsDisplayList)
        {
            item.CancelSelected();
            if (item.type == type && item.shape == shape)
            {
                currentSelectedWeaponsDisplay = item;
            }
        }
        if (currentSelectedWeaponsDisplay != null)
            currentSelectedWeaponsDisplay.SetSelected();
    }

    /// <summary>
    /// 设置某个角色被选中
    /// </summary>
    /// <param name="type"></param>
    /// <param name="shape"></param>
    public void SetCharacterSelected(int type, int shape)
    {
        currentSelectedCharacterDisplay = null;
        foreach (var item in characterDisplayList)
        {
            item.CancelSelected();
            if (item.type == type && item.shape == shape)
            {
                currentSelectedCharacterDisplay = item;
            }
        }
        if (currentSelectedCharacterDisplay != null)
            currentSelectedCharacterDisplay.SetSelected();
    }

    /// <summary>
    /// 获取当前选中武器的信息
    /// </summary>
    /// <returns></returns>
    public WeaponsInfo GetSelctedWeaponsInfo()
    {
        return new WeaponsInfo(currentSelectedWeaponsDisplay.type, currentSelectedWeaponsDisplay.shape);
    }

    /// <summary>
    /// 获取当前选中的角色信息
    /// </summary>
    /// <returns></returns>
    public CharacterInfo GetSelctedCharacterInfo()
    {
        Debug.Log("currentSelectedCharacterDisplay.type="+ currentSelectedCharacterDisplay.type);
        return new CharacterInfo(currentSelectedCharacterDisplay.type, currentSelectedCharacterDisplay.shape);
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
