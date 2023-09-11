using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// 道具控制器
/// </summary>
public class ItemController : MonoBehaviour, IGameControllerMember
{
    private GameObject itemListGo; // 用于存放地图物品单位的父对象
    private BaseCat[] catList; // 猫猫
    public List<BaseUnit> itemList = new List<BaseUnit>(); // 存活的道具单位表

    public void Awake()
    {
        itemListGo = new GameObject("itemList");
        catList = new BaseCat[7];
    }

    /// <summary>
    /// 初始化工作
    /// </summary>
    public void MInit()
    {
        foreach (var item in itemList)
            item.MDestory();
        itemList.Clear();
        RemoveAllCats();
        catList.Initialize();
        // 产生猫猫
        for (int i = 0; i < catList.Length; i++)
        {
            BaseCat cat = (BaseCat)GameController.Instance.CreateItem(new Vector3(MapManager.GetColumnX(-1), MapManager.GetRowY(i), 0), (int)ItemNameTypeMap.Cat, 0);
            cat.SetRowIndex(i);
            catList[i] = cat;
        }
    }

    public void MUpdate()
    {
        // 道具帧逻辑
        List<BaseUnit> delList = new List<BaseUnit>();
        foreach (var unit in itemList)
        {
            if (unit.IsValid())
            {
                unit.MUpdate();
            }
            else
                delList.Add(unit);
        }
        foreach (var unit in delList)
        {
            itemList.Remove(unit);
        }
    }

    public void MPause()
    {
        // 道具帧逻辑
        foreach (var item in itemList)
        {
            item.MPause();
        }
    }

    public void MResume()
    {
        // 道具帧逻辑
        foreach (var item in itemList)
        {
            item.MResume();
        }
    }

    public void MPauseUpdate()
    {
        
    }

    public void MDestory()
    {
        foreach (var item in itemList)
            item.MDestory();
        itemList.Clear();
        catList.Initialize();
    }
    #region 供外界调用的方法
    /// <summary>
    /// 移除所有的猫猫
    /// </summary>
    public void RemoveAllCats()
    {
        for (int i = 0; i < 7; i++)
        {
            BaseCat cat = GetSpecificRowCat(i);
            if (cat != null)
                cat.MDestory();
        }
    }

    public void AddItem(BaseUnit unit)
    {
        unit.transform.SetParent(itemListGo.transform);
        itemList.Add(unit);
    }

    /// <summary>
    /// 获取特定行的猫
    /// </summary>
    /// <returns></returns>
    public BaseCat GetSpecificRowCat(int rowIndex)
    {
        if (catList == null)
            return null;
        if (rowIndex >= catList.Length)
            return null;
        return catList[rowIndex];
    }
    #endregion
}
