using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Emp_ApartAdd : MonoBehaviour
{
    private List<Emp_ApartAddItem> itemList;
    private GameObject InfoDisplayer;
    public Button button;

    private int index; // 当前显示编辑的下标

    private void Awake()
    {
        itemList = new List<Emp_ApartAddItem>();
        for (int i = 0; i < transform.childCount; i++)
        {
            Emp_ApartAddItem item = transform.GetChild(i).GetComponent<Emp_ApartAddItem>();
            item.index = i;
            item.master = this;
            itemList.Add(item);
        }
        InfoDisplayer = itemList[0].transform.Find("InfoDisplayer").gameObject;
        // 按钮的监听由外部EditorPanel添加实现
        button = InfoDisplayer.transform.Find("Button").GetComponent<Button>();
        index = -1;
    }

    public void SetIndex(int index)
    {
        this.index = index;
        // 先隐藏全部
        foreach (var item in itemList)
        {
            item.Hide();
        }
        if (index >= 0 && index < itemList.Count)
        {
            InfoDisplayer.SetActive(true);
            itemList[index].Show();
            InfoDisplayer.transform.SetParent(itemList[index].transform);
            InfoDisplayer.transform.transform.localPosition = Vector3.zero;
        }
        else
        {
            InfoDisplayer.SetActive(false);
        }
    }

    public int GetIndex()
    {
        return index;
    }

    // Start is called before the first frame update
    void Start()
    {
        SetIndex(0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
