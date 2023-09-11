using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// ���߿�����
/// </summary>
public class ItemController : MonoBehaviour, IGameControllerMember
{
    private GameObject itemListGo; // ���ڴ�ŵ�ͼ��Ʒ��λ�ĸ�����
    private BaseCat[] catList; // èè
    public List<BaseUnit> itemList = new List<BaseUnit>(); // ���ĵ��ߵ�λ��

    public void Awake()
    {
        itemListGo = new GameObject("itemList");
        catList = new BaseCat[7];
    }

    /// <summary>
    /// ��ʼ������
    /// </summary>
    public void MInit()
    {
        foreach (var item in itemList)
            item.MDestory();
        itemList.Clear();
        RemoveAllCats();
        catList.Initialize();
        // ����èè
        for (int i = 0; i < catList.Length; i++)
        {
            BaseCat cat = (BaseCat)GameController.Instance.CreateItem(new Vector3(MapManager.GetColumnX(-1), MapManager.GetRowY(i), 0), (int)ItemNameTypeMap.Cat, 0);
            cat.SetRowIndex(i);
            catList[i] = cat;
        }
    }

    public void MUpdate()
    {
        // ����֡�߼�
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
        // ����֡�߼�
        foreach (var item in itemList)
        {
            item.MPause();
        }
    }

    public void MResume()
    {
        // ����֡�߼�
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
    #region �������õķ���
    /// <summary>
    /// �Ƴ����е�èè
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
    /// ��ȡ�ض��е�è
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
