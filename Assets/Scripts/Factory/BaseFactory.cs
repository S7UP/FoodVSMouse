using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// 游戏物体类型的工厂基类
public class BaseFactory : IBaseFactory
{
    // 当前拥有的gameObject类型的资源(UI,UIPanel,Game) 切记：它存放的是游戏物体资源（如预制体）
    protected Dictionary<string, GameObject> factoryDict = new Dictionary<string, GameObject>();
    // 对象池（就是我们具体存贮的游戏物体类型的对象）注：对应的是一个具体的游戏物体对象

    // 对象池字典
    protected Dictionary<string, Stack<GameObject>> objectPoolDict = new Dictionary<string, Stack<GameObject>>();

    // 加载路径
    protected string loadPath;

    public BaseFactory()
    {
        loadPath = "Prefabs/";
    }

    //放入池子
    public void PushItem(string itemName, GameObject item)
    {
        item.SetActive(false);
        item.transform.SetParent(GameManager.Instance.transform);
        if (objectPoolDict.ContainsKey(itemName))
        {
            objectPoolDict[itemName].Push(item);
        }
        else
        {
            Debug.Log("当前字典没有" +itemName+"的栈");
        }
    }

    // 取实例
    public GameObject GetItem(string itemName)
    {
        GameObject itemGo = null;
        if (objectPoolDict.ContainsKey(itemName)) //包含此对象
        {
            if (objectPoolDict[itemName].Count == 0)
            {
                // 对象池没东西了也需要自己产生实例
                GameObject go = GetResource(itemName); // 这里取出来的并非实例，只是预制体之类的
                itemGo = GameManager.Instance.CreateItem(go); // 让GameManager为预制体实例化一下，产生真正的实例
            }
            else
            {
                itemGo = objectPoolDict[itemName].Pop(); // 弹栈
                itemGo.SetActive(true); //记得激活
            }
        }
        else // 不包含此对象池
        {
            objectPoolDict.Add(itemName, new Stack<GameObject>()); // 要先建立一个这种的对象池
            GameObject go = GetResource(itemName); // 这里取出来的并非实例，只是预制体之类的
            itemGo = GameManager.Instance.CreateItem(go); // 让GameManager为预制体实例化一下，产生真正的实例
        }
        // 安全性检测
        if (itemGo == null)
        {
            Debug.Log(itemName+"的实例获取失败");
        }
        return itemGo;
    }

    // 取资源
    private GameObject GetResource(string itemName)
    {
        GameObject itemGo = null;
        string itemLoadPath = loadPath + itemName;
        if (factoryDict.ContainsKey(itemName))
        {
            itemGo = factoryDict[itemName];
        }
        else
        {
            itemGo = Resources.Load<GameObject>(itemLoadPath);
            factoryDict.Add(itemName, itemGo);
        }
        if (itemGo == null)
        {
            Debug.Log(itemName + "的资源获取失败");
            Debug.Log("失败路径：" + itemLoadPath);
        }
        return itemGo;
    }
}
