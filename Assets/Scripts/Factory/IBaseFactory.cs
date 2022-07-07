using UnityEngine;

// 游戏物体工厂接口
public interface IBaseFactory
{
    GameObject GetItem(string itemName);

    void PushItem(string itemName, GameObject item);

    void Clear();
}
