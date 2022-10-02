using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// ���߿�����
/// </summary>
public class ItemController : MonoBehaviour, IGameControllerMember
{
    private GameObject[] itemListGo; // ���ڴ�ŵ�ͼ��Ʒ��λ�ĸ�����
    private BaseCat[] catList; // èè
    public List<BaseUnit>[] mItemList; // ���ĵ��ߵ�λ��

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
    /// ��ʼ������
    /// </summary>
    public void MInit()
    {
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
    /// ��ȡ�ض��еĵ���
    /// </summary>
    /// <returns></returns>
    public List<BaseUnit> GetSpecificRowItemList(int rowIndex)
    {
        return mItemList[rowIndex];
    }

    /// <summary>
    /// ���õ��ߵ����ʵ�����������Ŀ¼
    /// </summary>
    public void SetItemRowParent(BaseUnit item, int rowIndex)
    {
        item.transform.SetParent(itemListGo[rowIndex].transform);
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

    /// <summary>
    /// �������е��ߵ�λ
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
        // ����֡�߼�
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
        // ����֡�߼�
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
