using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// 道具控制器
/// </summary>
public class ItemController : MonoBehaviour, IGameControllerMember
{
    private GameObject[] itemListGo; // 用于存放地图物品单位的父对象
    private BaseCat[] catList; // 猫猫
    public List<BaseUnit>[] mItemList; // 存活的道具单位表

    public void Awake()
    {
        mItemList = new List<BaseUnit>[MapController.yRow];
        itemListGo = new GameObject[mItemList.Length];
        for (int i = 0; i < mItemList.Length; i++)
        {
            mItemList[i] = new List<BaseUnit>();
            GameObject go = new GameObject("i");
            itemListGo[i] = go;
            go.transform.SetParent(transform);
        }

        catList = new BaseCat[mItemList.Length];
    }

    /// <summary>
    /// 初始化工作
    /// </summary>
    public void MInit()
    {
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
        foreach (var item in mItemList)
        {
            bool flag = false;
            for (int i = 0; i < item.Count; i++)
            {
                BaseUnit unit = item[i];
                if (unit.IsValid())
                {
                    unit.MUpdate();
                }
                else
                {
                    i--;
                    item.Remove(unit);
                    flag = true;
                }
            }
            if (flag)
                for (int i = 0; i < item.Count; i++)
                {
                    BaseUnit unit = item[i];
                    unit.UpdateRenderLayer(i);
                }
        }
    }

    /// <summary>
    /// 获取特定行的道具
    /// </summary>
    /// <returns></returns>
    public List<BaseUnit> GetSpecificRowItemList(int rowIndex)
    {
        return mItemList[rowIndex];
    }

    /// <summary>
    /// 设置道具到合适的行数父对象目录
    /// </summary>
    public void SetItemRowParent(BaseUnit item, int rowIndex)
    {
        item.transform.SetParent(itemListGo[rowIndex].transform);
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

    /// <summary>
    /// 回收所有道具单位
    /// </summary>
    public void RecycleAll()
    {
        for (int i = 0; i < mItemList.Length; i++)
        {
            foreach (var item in mItemList[i])
            {
                item.ExecuteRecycle();
            }
            mItemList[i].Clear();
        }
        catList.Initialize();
    }

    public void MDestory()
    {
        
    }

    public void MPause()
    {
        // 道具帧逻辑
        foreach (var item in mItemList)
        {
            for (int i = 0; i < item.Count; i++)
            {
                item[i].MPause();
            }
        }
    }

    public void MResume()
    {
        // 道具帧逻辑
        foreach (var item in mItemList)
        {
            for (int i = 0; i < item.Count; i++)
            {
                item[i].MResume();
            }
        }
    }

    public void MPauseUpdate()
    {
        
    }
}
